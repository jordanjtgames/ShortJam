//#define TESTING_MOUSE_ZOOM
using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	public partial class ExternalGameView
	{
		private void OnGUI()
		{
			//Debug.Assert(_isCreated);
			//if (!_isCreated) { this.minSize = this.maxSize = Vector2.zero; this.position = new Rect(0f, 0f, 0f, 0f); return; }

			// Close the window when ESC is pressed
			if (Utils.IsIMGUIKeyDown(KeyCode.Escape))
			{
				CloseWindows();
				GUIUtility.ExitGUI();
			}

#if TESTING_MOUSE_ZOOM
			if (Event.current.type == EventType.MouseDown)
			{
				_startPos = Event.current.mousePosition;
			}
			if (Event.current.type == EventType.ScrollWheel)
			{
				float newZoom = Mathf.Clamp(Settings.TextureZoom - (Event.current.delta.y / 100f), 0.25f, 16f);

				/*float factor = 1f;
				float delta = (newZoom / Settings.TextureZoom);
				Settings.TextureOffset -= new Vector2((Event.current.mousePosition.x - Settings.TextureOffset.x) - delta * (Event.current.mousePosition.x - Settings.TextureOffset.x), 0f);
				Settings.TextureOffset -= new Vector2(0f, (Event.current.mousePosition.y - Settings.TextureOffset.y) - delta * (Event.current.mousePosition.y - Settings.TextureOffset.y));
				*/

				float ZoomP_x = Event.current.mousePosition.x - _windowRect.width / 2f;
				float ZoomP_y = Event.current.mousePosition.y - _windowRect.height / 2f;
				float oldZoom_x = Settings.TextureZoom;
				float oldZoom_y = Settings.TextureZoom;
				float newZoom_x = newZoom;
				float newZoom_y = newZoom;
				float oldOffset_x = Settings.TextureOffset.x;
				float oldOffset_y = Settings.TextureOffset.y;
				float newOffset_x = oldOffset_x + (1f - (newZoom_x / oldZoom_x)) * (ZoomP_x - oldOffset_x);
				float newOffset_y = oldOffset_y + (1f - (newZoom_y / oldZoom_y)) * (ZoomP_y - oldOffset_y);
				Settings.TextureOffset = new Vector2(newOffset_x, newOffset_y);

				//Debug.Log(newZoom + " " + delta);

				Settings.TextureZoom = newZoom;
				Repaint();
			}

			if (Event.current.type == EventType.MouseDrag)
			{
				Settings.TextureOffset += Event.current.delta / Settings.TextureZoom * 1.0f;//new Vector2(_startPos.x - Event.current.mousePosition.x, _startPos.y - Event.current.mousePosition.y);
				Repaint();
			}
#endif

			// Show context menu on mouse press or touch
			if (Event.current.type == EventType.MouseDown)
			{
				// Right-click only
				if (Event.current.button == 1)
				{
					ShowContextMenu();
				}
			}

			// Pass through keyboard and mouse events to game view in play mode
			if (EditorApplication.isPlaying)
			{
				if (Event.current.isKey && Settings.SendKeyboardInput)
				{
					var gameView = Utils.GetMainGameView();
					gameView.SendEvent(Event.current);
				}
				else if ((Event.current.isMouse || Event.current.isScrollWheel) && Settings.SendMouseInput)
				{
					var gameView = Utils.GetMainGameView();
					// Convert mousePosition to gameview space
					// NOTE: this implementation is incomplete and needs fixing
					// TODO: take into account the texture offsets, scaling mode etc...
					if (Event.current.isMouse)
					{
						Vector2 p = Event.current.mousePosition;
						if (p.x >= 0f && p.y >= 0f && p.x < Screen.width && p.y < Screen.height)
						{
							// Normalise
							p = Vector2.Scale(p, new Vector2(1f / Screen.width, 1f / Screen.height));
							
							//Scale to Gameview window
							p = Vector2.Scale(p, Handles.GetMainGameViewSize());
							
							Event.current.mousePosition = gameView.position.position + p;
						}
					}
					gameView.SendEvent(Event.current);
				}
				else
				{
					// TODO: pass through touch events?
				}
			}

			// Draw background color if it might be visible
			if (_texture == null || Settings.WindowSizeMode != WindowSizeMode.MatchTexture || Settings.TextureOffset != Vector2.zero || Settings.TextureZoom < 1f)
			{
				GUI.color = Settings.BackgroundColor;
				if (Settings.BackgroundPattern)
				{
					EditorGUI.DrawTextureTransparent(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.blackTexture, ScaleMode.StretchToFill);
				}
				else
				{
					GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
				}
			}

			// NOTE: We MUST set the color to WHITE here, otherwise in play mode it will render using the play mode tint
			GUI.color = Color.white;

			if (_texture == null)
			{
				GUILayout.Label("External Game View - No Texture Found", EditorStyles.boldLabel);
				EditorGUILayout.Space();
				GUILayout.Label("Right-click for options");
				GUILayout.Label("Press ESC to close");
			}
			// Draw the texture
			else if (Event.current.type == EventType.Repaint)
			{
				DisplayInfo pinnedMonitor = DisplayInfo.GetByNameOrDefault(Settings.PinnedDisplayName);
				if (pinnedMonitor != null)
				{
					Rect r = new Rect(Settings.TextureOffset.x, Settings.TextureOffset.y, _texture.width, _texture.height);

					if (Settings.WindowSizeMode == WindowSizeMode.FitSingleDisplay ||
						Settings.WindowSizeMode == WindowSizeMode.FitAllDisplays)
					{
						r.width = _croppedSize.x;
						r.height = _croppedSize.y;
					}

#if !UNITY_EDITOR_OSX
					r.x /= pinnedMonitor.ScaleFactor;
					r.y /= pinnedMonitor.ScaleFactor;
					r.width /= pinnedMonitor.ScaleFactor;
					r.height /= pinnedMonitor.ScaleFactor;
#endif

					float zoom = Settings.TextureZoom;
					Vector2 offset = (r.size / 2f);
					r.position -= offset;
					r.xMin *= zoom;
					r.yMin *= zoom;
					r.xMax *= zoom;
					r.yMax *= zoom;
					r.position += offset;

					Matrix4x4 m = GUI.matrix;
					if (Settings.TextureFlipVertical)
					{
						GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0f, r.y + (r.height / 2f)));
					}

					FilterMode prevFilter = _texture.filterMode;
					if (Settings.TextureZoom != 1f && prevFilter != FilterMode.Point)
					{
						_texture.filterMode = FilterMode.Point;
					}
					
					if (Settings.WindowSizeMode == WindowSizeMode.FitAllDisplays ||
						Settings.WindowSizeMode == WindowSizeMode.FitSingleDisplay ||
						Settings.WindowSizeMode == WindowSizeMode.Custom)
					{
						GUI.DrawTexture(r, _texture, ScaleMode.ScaleToFit, false);
					}
					else
					{
						GUI.DrawTexture(r, _texture, ScaleMode.StretchToFill, false);
					}

					if (Settings.TextureZoom != 1f && prevFilter != _texture.filterMode)
					{
						_texture.filterMode = prevFilter;
					}

					if (Settings.TextureFlipVertical)
					{
						GUI.matrix = m;
					}
				}
			}
		}
	}
}