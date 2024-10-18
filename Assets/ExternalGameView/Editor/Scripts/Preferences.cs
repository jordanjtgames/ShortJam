using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	internal static class Content
	{
		internal static GUIContent WindowOffset = new GUIContent("Offset", "Offset of the window position in pixels");
		internal static GUIContent WindowOffsetReset = new GUIContent("Reset", "Reset the window position");
		internal static GUIContent TextureOffset = new GUIContent("Offset", "Offset of the texture position in pixels");
		internal static GUIContent TextureOffsetX = new GUIContent("X", "X Offset of the texture position in pixels");
		internal static GUIContent TextureOffsetY = new GUIContent("Y", "Y Offset of the texture position in pixels");
		internal static GUIContent TextureOffsetReset = new GUIContent("Reset", "Reset the texture position offset");

		internal static GUIContent[] Toolbar =
		{
			new GUIContent("Texture"),
			new GUIContent("Window"),
			new GUIContent("Other"),
			new GUIContent("About"),
		};

		internal static GUIContent[] WindowSizeModes =
		{
			new GUIContent("Match Texture 1:1", "The window size will match the texture size giving a 1:1 pixel perfect representation with no scaling"),
			new GUIContent("Fit Single Display", "The window size will fill the entire display, potentially scaling the texture"),
			new GUIContent("Fit All Display", "The window size will fill multiple displays, potentially scaling the texture"),
			new GUIContent("Custom", "Set a custom window size"),
		};
	}

	internal static class SettingsIMGUIRegister
	{
		internal static readonly string SettingsPath = "RenderHeads/External Game View";

		private const string LinkUserManual = "https://www.renderheads.com/content/docs/ExternalGameView/";
		private const string LinkAssetStore = "https://assetstore.unity.com/packages/slug/215946?aid=1101lcNgx";

		private static int MenuIndex
		{
			get
			{
				return SessionState.GetInt("Renderheads.ExternalGameView.MenuItem", 0);
			}
			set
			{
				SessionState.SetInt("Renderheads.ExternalGameView.MenuItem", value);
			}
		}

		internal static void OpenSettingsWindow()
		{
			#if UNITY_2018_3_OR_NEWER
			SettingsService.OpenUserPreferences(SettingsPath);
			#else
			Utils.OpenPreferencesWindow();
			#endif
		}

#if UNITY_2018_3_OR_NEWER
		private class MySettingsProvider : SettingsProvider
		{
			public MySettingsProvider(string path, SettingsScope scope) : base(path, scope)
			{
				this.keywords = new HashSet<string>(new[] { "GameView", "Game View", "View", "External", "RenderHeads", "Fullscreen", "Full", "Screen" });
			}

			public override void OnGUI(string searchContext)
			{
				SettingsGUI();
			}
		}

		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			return new MySettingsProvider(SettingsPath, SettingsScope.User);
		}

#elif UNITY_5_6_OR_NEWER
		// NOTE: We can't prefix with "RenderHeads/" as it becomes too long for this old UI layout
		[PreferenceItem("Ext Game View")]
#endif
		private static void SettingsGUI()
		{
			ExternalGameView window = ExternalGameView.Instance;
			DoGUI(window);
		}

		private static void DoGUI(ExternalGameView window)
		{
			//EditorGUILayout.Space();
			//GUILayout.Label(EditorGUIUtility.pixelsPerPoint.ToString());

			GUILayout.BeginHorizontal();
			//GUILayout.FlexibleSpace();
			if (window == null)
			{
				GUI.color = Color.red;
				if (!GUILayout.Toggle(true, "Disabled", GUI.skin.button))
				{
					ExternalGameView.OpenWindow();
				}
				GUI.color = Color.white;
			}
			else
			{
				GUI.color = Color.green;
				if (GUILayout.Toggle(false, "Enabled", GUI.skin.button))
				{
					ExternalGameView.CloseWindows();
					return;
				}
				GUI.color = Color.white;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			MenuIndex = GUILayout.Toolbar(MenuIndex, Content.Toolbar);
			EditorGUILayout.Space();

			RenderTexture texture = null;
			if (window != null)
			{
				texture = window.Texture;
			}
			else
			{
				texture = Utils.FindRenderTexture(Settings.TextureName);
			}

			EditorGUI.BeginChangeCheck();

			if (MenuIndex == 0)
			{
				// Texture Search Mode
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Find Texture");
					EditorGUI.BeginChangeCheck();
					Settings.TextureSearchMode = (TextureSearchMode)EditorGUILayout.EnumPopup(Settings.TextureSearchMode);
					if (EditorGUI.EndChangeCheck())
					{
						if (Settings.TextureSearchMode == TextureSearchMode.GameView)
						{
							Settings.TextureName = Utils.GameViewTextureName;
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				// Texture Name
				EditorGUI.BeginDisabledGroup(Settings.TextureSearchMode == TextureSearchMode.GameView);
				EditorGUI.BeginChangeCheck();
				Settings.TextureName = EditorGUILayout.TextField("Name", Settings.TextureName);
				if (EditorGUI.EndChangeCheck())
				{
					if (window != null)
					{
						window.SearchTexture();
					}
				}
				EditorGUI.EndDisabledGroup();

				// Texture Stats
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Preview");
				if (texture != null)
				{
					{
						float ratio = (float)texture.width / (float)texture.height;

						Rect textureRect = GUILayoutUtility.GetAspectRect(ratio);

						GUI.color = Color.gray;
						EditorGUI.DrawTextureTransparent(textureRect, Texture2D.blackTexture, ScaleMode.StretchToFill);
						GUI.color = Color.white;
					
						Rect r = new Rect(0f, 0f, 1f, 1f);
						if (Settings.TextureFlipVertical)
						{
							r.yMin = 1f;
							r.yMax = 0f;
						}
						GUI.DrawTextureWithTexCoords(textureRect, texture, r, false);
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(" ");

					// NOTE: There's a weird bug (at least in Unity 2020.3.25) when Unity first starts, GUI.skin.box.normal.textColor is black.  The correct value is set when the game is run or a script is loaded..So here we just force it to the correct value
					GUI.skin.box.normal.textColor = GUI.skin.box.active.textColor;

					GUI.color = Color.green;
					GUILayout.Box(texture.width + "x" + texture.height, GUILayout.ExpandWidth(true));
					#if UNITY_2019_1_OR_NEWER
					GUILayout.Box(texture.graphicsFormat.ToString(), GUILayout.ExpandWidth(true));
					#else
					GUILayout.Box(texture.format.ToString(), GUILayout.ExpandWidth(true));
					#endif
					GUILayout.Box(texture.antiAliasing + "x MSAA", GUILayout.ExpandWidth(true));
					GUI.color = Color.white;
				}
				else
				{
					GUI.color = Color.red;
					GUILayout.Button("Can't find texture named '" + Settings.TextureName + "'");
					GUI.color = Color.white;
				}
				EditorGUILayout.EndHorizontal();

				// Texture Flip Vertical
				Settings.TextureFlipVertical = EditorGUILayout.Toggle("Flip Vertical", Settings.TextureFlipVertical);

				// Texture Zoom
				EditorGUILayout.BeginHorizontal();
				Settings.TextureZoom = EditorGUILayout.Slider("Zoom", Settings.TextureZoom, 0.25f, 16f);
				if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
				{
					Settings.TextureZoom = 1f;
				}
				EditorGUILayout.EndHorizontal();

				// Texture Offset
				{
					const int MaxTextureExtent = 16384;
					int extentX = MaxTextureExtent;
					int extentY = MaxTextureExtent;
					if (texture != null)
					{
						extentX = Mathf.FloorToInt(texture.width);
						extentY = Mathf.FloorToInt(texture.height);
					}

					EditorGUILayout.PrefixLabel(Content.TextureOffset, EditorStyles.toolbarButton);
					EditorGUI.indentLevel++;
					{
						EditorGUILayout.BeginHorizontal();
						int x = EditorGUILayout.IntSlider(Content.TextureOffsetX, (int)Settings.TextureOffset.x, -extentX, extentX);
						if (GUILayout.Button(Content.TextureOffsetReset, GUILayout.ExpandWidth(false)))
						{
							x = 0;
						}
						if (x != (int)Settings.TextureOffset.x)
						{
							Settings.TextureOffset = new Vector2(x, Settings.TextureOffset.y);
						}
						EditorGUILayout.EndHorizontal();
					}
					{
						EditorGUILayout.BeginHorizontal();
						int y = EditorGUILayout.IntSlider(Content.TextureOffsetY, (int)Settings.TextureOffset.y, -extentY, extentY);
						if (GUILayout.Button(Content.TextureOffsetReset, GUILayout.ExpandWidth(false)))
						{
							y = 0;
						}
						if (y != (int)Settings.TextureOffset.y)
						{
							Settings.TextureOffset = new Vector2(Settings.TextureOffset.x, y);
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUI.indentLevel--;
				}
				EditorGUILayout.Space();
			}
			// Window
			else if (MenuIndex == 1)
			{
				/*EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Window", EditorStyles.toolbarButton, EditorStyles.boldLabel);
				DisplayInfo pinnedMonitor = DisplayInfo.GetByNameOrDefault(_pinnedMonitorName);
				if (pinnedMonitor != null)
				{
					GUI.color = Color.yellow;
					GUILayout.Button((int)(this.position.x * pinnedMonitor.ScaleFactor) + "," + (int)(this.position.y * pinnedMonitor.ScaleFactor) + " " + (int)(this.position.width * pinnedMonitor.ScaleFactor) + "x" + (int)(this.position.height * pinnedMonitor.ScaleFactor), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
					GUI.color = Color.white;
				}
				EditorGUILayout.EndHorizontal();*/

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Pin to Display", EditorStyles.toolbarButton);
				if (GUILayout.Button("Refresh Displays", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true)))
				{
					DisplayInfo.Refresh();
				}
				EditorGUILayout.EndHorizontal();
			
				{
if (true)
						{
							//Rect areaRect = GUILayoutUtility.GetRect(256f, 256f);

							Rect virtualArea = DisplayInfo.VirtualArea;
							float virtualAspect = virtualArea.width / virtualArea.height;

							GUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel(" ");
							Rect areaRect = GUILayoutUtility.GetRect(128f * virtualAspect, 128f);
							GUILayout.EndHorizontal();

							// Scale virtualArea to fit into areaRect
							//Debug.Log("MM1 " + areaRect + " " + virtualArea);
							Rect scaledVirtualArea = Utils.ScaleToFit(virtualArea, areaRect);
							//Debug.Log("MM2 " + areaRect + " " + virtualArea);

							//GUI.Box(areaRect, GUIContent.none);
							if (Event.current.type != EventType.Layout)
							{
								GUI.color = Color.gray;
								GUI.Box(scaledVirtualArea, GUIContent.none, GUI.skin.button);
								GUI.color = Color.white;
								foreach (DisplayInfo display in DisplayInfo.Displays)
								{
									bool isDisplay = (display.DeviceName == Settings.PinnedDisplayName);

									GUI.color = Color.white;
									if (isDisplay)
									{
										GUI.color = Color.green;
									}
									//break;
									Vector2 scale = new Vector2(scaledVirtualArea.size.x / virtualArea.size.x, scaledVirtualArea.size.y / virtualArea.size.y);
									Vector2 offset = scaledVirtualArea.position - Vector2.Scale(scale, virtualArea.position);
									Rect displayArea = display.Area;
									//Debug.Log(display.DeviceName + " " + display.MonitorArea + " " + displayArea.position + " " + displayArea.size + " " + offset + " " + scale);
									displayArea.position = offset + Vector2.Scale(displayArea.position, scale);
									displayArea.size = Vector2.Scale(displayArea.size, scale);//ScaleToFit(virtualArea, areaRect);

									string desc = (display.DeviceName + "\n" + display.Area.width + "x" + display.Area.height + (display.IsPrimary?"\nPRIMARY":"\n"));
									if (GUI.Button(displayArea, desc))
									{
										if (!isDisplay)
										{
											if (window != null)
											{
												window.SetPositionToMonitor(display);
												window.UpdateWindowPosition();
											}
											else
											{
												Settings.PinnedDisplayName = display.DeviceName;
											}
										}
									}
								}
								GUI.color = Color.white;
							}
						}
				}
				// Pinned Display
				{
							EditorGUI.indentLevel++;
							foreach (DisplayInfo display in DisplayInfo.Displays)
							{
								GUI.color = Color.white;
								bool isDisplay = (display.DeviceName == Settings.PinnedDisplayName);
								if (isDisplay)
								{
									GUI.color = Color.green;
								}
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.PrefixLabel(display.DeviceName, EditorStyles.toggle);
								string desc = ("(" + display.Area.xMin + "," + display.Area.yMin + ") (" + display.Area.width + "x" + display.Area.height + ") (DPI Scale " + display.ScaleFactor + "x)");
								if (GUILayout.Toggle(isDisplay, desc, GUI.skin.button, GUILayout.ExpandWidth(true)))
								{
									if (!isDisplay)
									{
										if (window != null)
										{
											window.SetPositionToMonitor(display);
											window.UpdateWindowPosition();
										}
										else
										{
											Settings.PinnedDisplayName = display.DeviceName;
										}
									}
								}
								EditorGUILayout.EndHorizontal();
							}
							EditorGUI.indentLevel--;
							GUI.color = Color.white;
				}
				// Window Offset
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(Content.WindowOffset);

					{
						int x = EditorGUILayout.DelayedIntField((int)Settings.WindowOffset.x, GUILayout.ExpandWidth(false));
						int y = EditorGUILayout.DelayedIntField((int)Settings.WindowOffset.y, GUILayout.ExpandWidth(false));
						if (GUILayout.Button(Content.WindowOffsetReset, GUILayout.ExpandWidth(false)))
						{
							x = y = 0;
						}

						const int MaxWindowExtent = 16384;
						int extentX = MaxWindowExtent;
						int extentY = MaxWindowExtent;
						x = Mathf.Clamp(x, -extentX, extentX);
						y = Mathf.Clamp(y, -extentY, extentY);

						if (x != (int)Settings.WindowOffset.x || y != (int)Settings.WindowOffset.y)
						{
							Settings.WindowOffset = new Vector2(x, y);
						}
					}
					EditorGUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						if (window != null)
						{
							window.UpdateWindowPosition();
						}
					}

					// Scaling Mode
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel("Size");
						Settings.WindowSizeMode = (WindowSizeMode)EditorGUILayout.Popup((int)Settings.WindowSizeMode, Content.WindowSizeModes);
						EditorGUILayout.EndHorizontal();
						//if (Settings.WindowSizeMode == WindowSizeMode.Custom)
						{
							EditorGUI.BeginDisabledGroup(Settings.WindowSizeMode != WindowSizeMode.Custom);
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel(" ");
							int x = EditorGUILayout.DelayedIntField((int)Settings.CustomWindowSize.x, GUILayout.ExpandWidth(false));
							int y = EditorGUILayout.DelayedIntField((int)Settings.CustomWindowSize.y, GUILayout.ExpandWidth(false));
							x = Mathf.Clamp(x, 0, 16384);
							y = Mathf.Clamp(y, 0, 16384);
							if (x != (int)Settings.CustomWindowSize.x || y != (int)Settings.CustomWindowSize.y)
							{
								Settings.CustomWindowSize = new Vector2(x, y);
							}
							EditorGUILayout.EndHorizontal();
							EditorGUI.EndDisabledGroup();
						}
						if (EditorGUI.EndChangeCheck())
						{
							if (window != null)
							{
								window.UpdateWindowPosition();
							}
						}
					}

					/*if (window != null)
					{
						GUILayout.Label(window.WindowRect.position + "-" + window.WindowRect.size);
					}*/

					EditorGUILayout.Space();
				}
			}
			else if (MenuIndex == 2)
			{
				{
					Settings.BackgroundColor = EditorGUILayout.ColorField(new GUIContent("Background Color"), Settings.BackgroundColor, false, false, false
					#if UNITY_2018_1_OR_NEWER
					);
					#else
					, null);
					#endif
				}
				{
					int index = Settings.BackgroundPattern ? 1 : 0;
					Settings.BackgroundPattern = (1 == EditorGUILayout.Popup("Background Pattern", index, new[] { "None", "Checker" }));
				}
				{
					int index = Settings.SendKeyboardInput ? 1 : 0;
					Settings.SendKeyboardInput = (1 == EditorGUILayout.Popup("Keyboard Input", index, new[] { "Ignore", "Send to Game View" }));
				}
				{
					int index = Settings.SendMouseInput ? 1 : 0;
					Settings.SendMouseInput = (1 == EditorGUILayout.Popup("Mouse Input", index, new[] { "Ignore", "Send to Game View" }));
				}
				{
					int index = Settings.AutoOpenOnEnteringPlayMode ? 1 : 0;
					Settings.AutoOpenOnEnteringPlayMode = (1 == EditorGUILayout.Popup("Entering Play Mode", index, new[] { "Do Nothing", "Open External Game View" }));
				}
				{
					int index = Settings.AutoCloseOnExitingPlayMode ? 1 : 0;
					Settings.AutoCloseOnExitingPlayMode = (1 == EditorGUILayout.Popup("Exiting Play Mode", index, new[] { "Do Nothing", "Close External Game View" }));
				}
				{
					int index = Settings.PreserveTextureOnExitingPlayMode ? 1 : 0;
					Settings.PreserveTextureOnExitingPlayMode = (1 == EditorGUILayout.Popup("Exiting Play Mode", index, new[] { "Don't Preserve Texture", "Preserve Texture" }));
				}
				EditorGUILayout.Space();
				{
					#if UNITY_2019_1_OR_NEWER
					UnityEditor.ShortcutManagement.ShortcutBinding toggleKeyBinding;
					try
					{
						toggleKeyBinding = UnityEditor.ShortcutManagement.ShortcutManager.instance.GetShortcutBinding("RenderHeads/Toggle External Game View");
					}
					catch (System.ArgumentException) {}

					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Window Toggle Shortcut", toggleKeyBinding.ToString());
					if (GUILayout.Button("Modify", GUILayout.ExpandWidth(false)))
					{
						EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
					}
					GUILayout.EndHorizontal();
					#endif
					EditorGUILayout.LabelField("Window Close Shortcut", "ESC");
				}

				EditorGUILayout.Space();
				if (GUILayout.Button("Reset All"))
				{
					Settings.ResetSettings();
					if (window != null)
					{
						window.UpdateWindowPosition();
						window.Repaint();
					}
				}
			}
			else if (MenuIndex == 3)
			{
				GUILayout.Label("External Game View by RenderHeads Ltd", EditorStyles.boldLabel);
				GUILayout.Label("version " + Version.VersionString);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Documentation", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkUserManual);
				}
				if (GUILayout.Button("Rate ★ | Like ♥ | Review", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkAssetStore);
				}
				GUILayout.EndHorizontal();
			}

			if (EditorGUI.EndChangeCheck())
			{
				Settings.SaveSettingsIfModified();
				if (window != null)
				{
					window.Repaint();
				}
			}
		}
	}
}