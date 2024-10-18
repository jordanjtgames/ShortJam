using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR_OSX
using RenderHeads.MacOSSupport.AppKit;
#endif

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	/// An editor window that can do two things the regular GameView cannot:
	/// 1) display truely fullscreen
	/// 2) display RenderTextures > 8K
	///
	/// Weirdness notes:
	/// This script does handle having multiple montiors at different resolutions and DPI scalings, however
	/// in certain cases this code doesn't work.  It seems to be a Unity issue.  The EditorWindow.position variable
	/// is ambigious and returns the same value for actual different window positions on multiple monitors.
	/// On Windows we get around this by using the Win32 native methods to set the window position.
	public partial class ExternalGameView : EditorWindow
	{ 
		[SerializeField] Vector2 _textureSize = Vector2.zero;
		[SerializeField] Vector2 _croppedSize = Vector2.zero;
		[SerializeField] Rect _windowRect = Rect.zero;

		[System.NonSerialized] bool _isCreated = false;
		[System.NonSerialized] bool _queueClose = false;
		[System.NonSerialized] bool _isDirtyWindow = false;
		[System.NonSerialized] RenderTexture _texture = null;
		#if TESTING_MOUSE_ZOOM
		[System.NonSerialized] Vector2 _startPos;
		#endif

		internal static ExternalGameView Instance { get; private set; }
		internal RenderTexture Texture { get { return _texture; } }
		internal Rect WindowRect { get { return _windowRect; } }

		private static ExternalGameView FindOpenWindow()
		{
			// NOTE: We could use EditorWindow.HasOpenInstances<ExternalGameView>() in 2019.3 and above, but it just uses FindObjectsOfTypeAll() anyway
			ExternalGameView result = null;
			ExternalGameView[] windows = Resources.FindObjectsOfTypeAll<ExternalGameView>();
			if (windows != null && windows.Length > 0)
			{
				result = windows[0];
			}
			return result;
		}

		internal static void OpenWindow()
		{
			if (Instance == null)
			{
				CloseWindows();
			}
			
			ExternalGameView window = CreateInstance<ExternalGameView>();
			if (window != null)
			{
				window.SetupWindow();
				window.ShowPopup();
				Debug.Log("[ExternalGameView] Press ESC to close window");
			}
		}

		internal static void CloseWindows()
		{
			if (Instance != null)
			{
				Instance.Close();
			}

			// Close any other open windows
			var window = FindOpenWindow();
			if (window != null)
			{
				window.Close();
			}
		}

		void OnEnable()
		{
			this.autoRepaintOnSceneChange = true;
			if (!_isCreated)
			{
				SetupWindow();
			}
		}

		private void OnDisable()
		{
			_queueClose = false;
		}

		void OnDestroy()
		{
			_isCreated = false;
			Instance = null;
		}

		public void SetupWindow()
		{
			if (!_isCreated)
			{
				Debug.Assert(Instance == null);
				_isDirtyWindow = true;
				_isCreated = true;
				Instance = this;
			}
		}

		void Update()
		{
			if (_queueClose)
			{
				CloseWindows();
				return;
			}
			
			if (_texture == null)
			{
				SearchTexture();
			}

			// Detect texture size changes
			if (_texture != null)
			{
				if (_texture.width != _textureSize.x || _texture.height != _textureSize.y)
				{
					ChangeTexture(_texture);
				}
			}
			else
			{
				if (PreserveTexture.PreservedTexture != null && Settings.PreserveTextureOnExitingPlayMode)
				{
					ChangeTexture(PreserveTexture.PreservedTexture);
				}
			}

			if (_isDirtyWindow)
			{
				_isDirtyWindow = false;
				UpdateWindowPosition();
			}
		}

		internal void SearchTexture()
		{
			RenderTexture rt = Utils.FindRenderTexture(Settings.TextureName);
			if (rt != null)
			{
				ChangeTexture(rt);
			}
		}

		private void ChangeTexture(RenderTexture texture)
		{
			_texture = texture;
			if (_texture != null)
			{
				// Texture has changed, so resize window
				_textureSize = new Vector2(_texture.width, _texture.height);
				UpdateWindowPosition();
			}
		}

		internal void UpdateWindowPosition()
		{
			SetPositionToMonitor(DisplayInfo.GetByNameOrDefault(Settings.PinnedDisplayName));
		}

		#if UNITY_2021_2_OR_NEWER
		/*void OnBackingScaleFactorChanged()
		{
			Debug.Log("DPI changed?");
		}*/
		#endif

		private Rect CropWindowRectToMonitors(Rect windowRect)
		{
			Rect result = Rect.zero;
			foreach (DisplayInfo display in DisplayInfo.Displays)
			{
				if (display.Area.Overlaps(windowRect))
				{
					//Debug.Log("overlap" + display.DeviceName + " " + display.Area + " " + windowRect);
					// Expand extends to include this monitor
					result.xMin = Mathf.Min(result.xMin, display.Area.xMin);
					result.yMin = Mathf.Min(result.yMin, display.Area.yMin);
					result.xMax = Mathf.Max(result.xMax, display.Area.xMax);
					result.yMax = Mathf.Max(result.yMax, display.Area.yMax);
				}
			}
			// Crop
			result.xMin = Mathf.Max(result.xMin, windowRect.xMin);
			result.xMax = Mathf.Min(result.xMax, windowRect.xMax);
			result.yMin = Mathf.Max(result.yMin, windowRect.yMin);
			result.yMax = Mathf.Min(result.yMax, windowRect.yMax);
			//Debug.Log("cropped: " + windowRect + " to " + result);
			return result;
		}

#if UNITY_EDITOR_OSX
		internal void SetPositionToMonitor(DisplayInfo display)
		{
			if (display == null) { return; }

			Rect newWindow = new Rect();
			newWindow.position = display.Area.position + Settings.WindowOffset / display.ScaleFactor;

			switch (Settings.WindowSizeMode)
			{
				case WindowSizeMode.MatchTexture:
					newWindow.size = (_texture != null ? new Vector2(_texture.width, _texture.height) : display.Area.size) / display.ScaleFactor;
					break;
				case WindowSizeMode.FitSingleDisplay:
					newWindow.size = display.Area.size;
					break;
				case WindowSizeMode.FitAllDisplays:
					newWindow.position = DisplayInfo.VirtualArea.position;
					newWindow.size = DisplayInfo.VirtualArea.size;
					break;
				case WindowSizeMode.Custom:
					newWindow.size = Settings.CustomWindowSize;
					break;
			}

			//Debug.Log("newWindow: " + newWindow);

			this.minSize = newWindow.size;
			this.maxSize = newWindow.size;
			this.position = newWindow;

			_windowRect = newWindow;
			_croppedSize = newWindow.size;

			// Unity's editor window positioning does not work on non-primary displays and adjusts for the menu bar on
			// the primary display so we'll simply override it.

			Rect primaryArea = DisplayInfo.Primary.Area;
			Rect nativePosition = newWindow;

			// Need to convert to macOS coordinates with 0,0 bottom left on the primary monitor
			nativePosition.y = -(newWindow.y + newWindow.size.y) + primaryArea.y + primaryArea.height;
			
			//Debug.Log("primaryArea: " + primaryArea);
			//Debug.Log("nativePosition: " + nativePosition);

			EditorWindow oldFocus = EditorWindow.focusedWindow;
			if (oldFocus != this) { this.Focus(); }

			NSWindow window = NSApplication.SharedApplication.KeyWindow;
			window.SetFrame(nativePosition, true, false);

			if (oldFocus != null && oldFocus != this) { oldFocus.Focus(); }

			Settings.PinnedDisplayName = display.DeviceName;
			Settings.SaveSettingsIfModified();
		}
#else
		internal void SetPositionToMonitor(DisplayInfo display)
		{
			if (display == null) return;

			//Debug.Log("Pinning to " + display.DeviceName + " " + display.Area + " offset: " + Settings.WindowOffset);

			// Work out the scale factor based on which monitors we have to traverse.  This is a simplification at the moment that only accounts for 1 monitor traversal (primary to target and nothing in between)
			float scaleFactorX =  DisplayInfo.Primary.ScaleFactor;
			float scaleFactorY =  DisplayInfo.Primary.ScaleFactor;
			if (display.Area.xMin < DisplayInfo.Primary.Area.xMin)
			{
				scaleFactorX = display.ScaleFactor;
			}
			if (display.Area.yMin <  DisplayInfo.Primary.Area.yMin)
			{
				scaleFactorY = display.ScaleFactor;
			}

			//scaleFactorX = scaleFactorY = EditorGUIUtility.pixelsPerPoint;
			//scaleFactorX = scaleFactorY = 1f;

			float textureWidth = 640f;
			float textureHeight = 360f;
			if (_texture != null)
			{
				textureWidth = _texture.width;
				textureHeight = _texture.height;
			}

			Rect newWindow = new Rect(display.Area.xMin + Settings.WindowOffset.x, display.Area.yMin + Settings.WindowOffset.y, textureWidth, textureHeight);
			if (Settings.WindowSizeMode == WindowSizeMode.FitAllDisplays)
			{
				newWindow = CropWindowRectToMonitors(newWindow);
				_croppedSize = newWindow.size;
			}
			else if (Settings.WindowSizeMode == WindowSizeMode.FitSingleDisplay)
			{
				_croppedSize = display.Area.size;
				newWindow.size = _croppedSize;
			}
			else if (Settings.WindowSizeMode == WindowSizeMode.Custom)
			{
				_croppedSize = Settings.CustomWindowSize;
				newWindow.size = _croppedSize;
			}

			newWindow.x /= scaleFactorX;
			newWindow.y /= scaleFactorY;
			newWindow.width /= display.ScaleFactor;
			newWindow.height /= display.ScaleFactor;

			//Debug.Log("scale: " + scaleFactorX + " " + scaleFactorY + " " + EditorGUIUtility.pixelsPerPoint);

			//newWindow.width = Mathf.Max(newWindow.width, this.minSize.x);
			//newWindow.height = Mathf.Max(newWindow.height, this.minSize.y);

			float scaleFactor = display.ScaleFactor;
			/*float scaleFactor = EditorGUIUtility.pixelsPerPoint;
			if (scaleFactor == 1f)
			{
				scaleFactor = display.ScaleFactor;
			}
			newWindow.width /= scaleFactor;
			newWindow.height /= scaleFactor;*/


			//this.minSize = new Vector2(640f, 360f);
			this.minSize = this.maxSize = newWindow.size;
			
			// NOTE: for some reason we have to call this twice,
			// otherwise the window size is not set correctly.  This must some DPI correction...
			this.position = newWindow; 	this.position = newWindow;

			//this.ShowPopup();
			//this.Focus();

			//Debug.Log(this.position + " "+ newWindow + " " + scaleFactorX + " " + scaleFactorY + " " + textureWidth + "x" + textureHeight);


#if UNITY_EDITOR_WIN
			{
				EditorWindow oldFocus = EditorWindow.focusedWindow;
				if (oldFocus != this)
				{
					this.Focus();
				}

				IntPtr hwnd = WindowsNative.GetActiveWindow();
				//newWindow = CropWindowRectToMonitors(new Rect(display.MonitorLeft + _windowOffsetX, display.MonitorTop + _windowOffsetY));
				_windowRect = new Rect(Mathf.FloorToInt(display.Area.xMin + Settings.WindowOffset.x), Mathf.FloorToInt(display.Area.yMin + Settings.WindowOffset.y), newWindow.width * scaleFactor, newWindow.height * scaleFactor);
				WindowsNative.SetWindowPos(hwnd, IntPtr.Zero, (int)_windowRect.position.x, (int)_windowRect.position.y, 0, 0, WindowsNative.SetWindowPosFlags.SWP_NOZORDER | WindowsNative.SetWindowPosFlags.SWP_NOSIZE | WindowsNative.SetWindowPosFlags.SWP_NOACTIVATE);

				Settings.CustomWindowSize = _windowRect.size;

				if (oldFocus != null && oldFocus != this)
				{
					oldFocus.Focus();
				}
			}
#endif

			Settings.PinnedDisplayName = display.DeviceName;
			Settings.SaveSettingsIfModified();
		}
#endif
	}
}
