using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	public partial class ExternalGameView
	{ 
		void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("External Game View"), false, null, null);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Open Preferences..."), false, OnMenuOpenPreferencesSelected);
			foreach (DisplayInfo display in DisplayInfo.Displays)
			{
				string displayDesc = string.Format("{0} {1}x{2} {3}", display.DeviceName, display.Area.width, display.Area.height, display.IsPrimary ? "PRIMARY":"");
				menu.AddItem(new GUIContent("Pin To Display/" + displayDesc), (Settings.PinnedDisplayName == display.DeviceName), OnMenuPinToDisplaySelected, display);
			}
			menu.AddItem(new GUIContent("Window Size/Match Texture 1:1"), (Settings.WindowSizeMode == WindowSizeMode.MatchTexture), OnMenuWindowSizeSelected, WindowSizeMode.MatchTexture);
			menu.AddItem(new GUIContent("Window Size/Fit Single Display"), (Settings.WindowSizeMode == WindowSizeMode.FitSingleDisplay), OnMenuWindowSizeSelected, WindowSizeMode.FitSingleDisplay);
			menu.AddItem(new GUIContent("Window Size/Fit All Displays"), (Settings.WindowSizeMode == WindowSizeMode.FitAllDisplays), OnMenuWindowSizeSelected, WindowSizeMode.FitAllDisplays);
			menu.AddItem(new GUIContent("Window Size/Custom"), (Settings.WindowSizeMode == WindowSizeMode.Custom), OnMenuWindowSizeSelected, WindowSizeMode.Custom);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Flip Vertical"), (Settings.TextureFlipVertical), OnMenuFlipVerticalSelected);
			menu.AddItem(new GUIContent("Zoom 1x"), (Settings.TextureZoom == 1f), OnMenuZoomSelected, 1);
			menu.AddItem(new GUIContent("Zoom 2x"), (Settings.TextureZoom == 2f), OnMenuZoomSelected, 2);
			menu.AddItem(new GUIContent("Zoom 4x"), (Settings.TextureZoom == 4f), OnMenuZoomSelected, 4);
			menu.AddItem(new GUIContent("Zoom 8x"), (Settings.TextureZoom == 8f), OnMenuZoomSelected, 8);
			menu.AddSeparator("");
			if (_texture != null)
			{
				string textureStats = string.Format("Texture: {0} {1}x{2} {3} {4}x MSAA", _texture.name, _texture.width, _texture.height, 
				#if UNITY_2019_1_OR_NEWER
				_texture.graphicsFormat.ToString(),
				#else
				_texture.format.ToString(),
				#endif
				_texture.antiAliasing);
				menu.AddItem(new GUIContent("Stats/" + textureStats), false, null, null);
			}
			
			string windowStats = string.Format("Window: {0}", _windowRect); 
			menu.AddItem(new GUIContent("Stats/" + windowStats), false, null, null);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Close Window"), false, OnMenuCloseSelected);
			menu.ShowAsContext();
		}

		void OnMenuWindowSizeSelected(object windowSizeObj)
		{
			Settings.WindowSizeMode = (WindowSizeMode)windowSizeObj;
			Settings.SaveSettingsIfModified();
			UpdateWindowPosition();
		}

		void OnMenuFlipVerticalSelected()
		{
			Settings.TextureFlipVertical = !Settings.TextureFlipVertical;
			Settings.SaveSettingsIfModified();
			Repaint();
		}

		void OnMenuZoomSelected(object zoomObj)
		{
			Settings.TextureZoom = System.Convert.ToSingle(zoomObj);
			if (Settings.TextureZoom == 1f)
			{
				Settings.TextureOffset = Vector2.zero;
			}
			Settings.SaveSettingsIfModified();
			Repaint();
		}

		void OnMenuPinToDisplaySelected(object displayObj)
		{
			SetPositionToMonitor((DisplayInfo)displayObj);
			UpdateWindowPosition();
			Repaint();
		}

		void OnMenuCloseSelected()
		{
			_queueClose = true;
		}

		void OnMenuOpenPreferencesSelected()
		{
			SettingsIMGUIRegister.OpenSettingsWindow();
		}
	}
}