using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	internal enum WindowSizeMode : int
	{
		MatchTexture,
		FitSingleDisplay,
		FitAllDisplays,
		Custom,
	}

	internal enum TextureSearchMode : int
	{
		GameView,
		ByName,
	}

	//
	// All the user configurable settings that need to be saved
	// Some basic logic included for dirty state checking
	//
	class Settings
	{
		private readonly static Color DefaultBackgroundColor = new Color(0.5f, 0.25f, 0.5f, 1f);
		private readonly static Vector2 DefaultCustomWindowSize = new Vector2(1280f, 720f);

		private const string SettingsName_TextureSearchMode = "TextureSearchMode";
		private const string SettingsName_TextureName = "TextureName";
		private const string SettingsName_TextureFlipVertical = "TextureFlipVertical";
		private const string SettingsName_TextureOffset = "TextureOffset";
		private const string SettingsName_TextureZoom = "TextureZoom";
		private const string SettingsName_PinnedMonitorName = "PinnedMonitorName";
		private const string SettingsName_WindowOffset = "WindowOffset";
		private const string SettingsName_WindowSizeMode = "WindowSizeMode";
		private const string SettingsName_CustomWindowSize = "CustomWindowSize";
		private const string SettingsName_BackgroundColor = "BackgroundColor";
		private const string SettingsName_BackgroundPattern = "BackgroundPattern";
		private const string SettingsName_SendKeyboardInput = "SendKeyboardInput";
		private const string SettingsName_SendMouseInput = "SendMouseInput";
		private const string SettingsName_AutoOpenOnEnteringPlayMode = "AutoOpenOnEnteringPlayMode";
		private const string SettingsName_AutoCloseOnExitingPlayMode = "AutoCloseOnExitingPlayMode";
		private const string SettingsName_PreserveTextureOnExitingPlayMode = "PreserveTextureOnExitingPlayMode";

		private static TextureSearchMode _textureSearchMode = TextureSearchMode.GameView;
		private static string _textureName = Utils.GameViewTextureName;
		private static bool _textureFlipVertical = true;
		private static Vector2 _textureOffset = Vector2.zero;
		private static float _textureZoom = 1f;
		private static string _pinnedDisplayName = string.Empty;
		private static Vector2 _windowOffset = Vector2.zero;
		private static WindowSizeMode _windowSizeMode = WindowSizeMode.MatchTexture;
		private static Vector2 _customWindowSize = DefaultCustomWindowSize;
		private static Color _backgroundColor = DefaultBackgroundColor;
		private static bool _backgroundPattern = true;
		private static bool _sendKeyboardInput = true;
		private static bool _sendMouseInput = false;
		private static bool _autoOpenOnEnteringPlayMode = false;
		private static bool _autoCloseOnExitingPlayMode = false;
		private static bool _preserveTextureOnExitingPlayMode = false;

		internal static TextureSearchMode TextureSearchMode 	{ set { SetProperty(ref _textureSearchMode, value); } 	get { return GetProperty(ref _textureSearchMode); } }
		internal static string TextureName 						{ set { SetProperty(ref _textureName, value); } 		get { return GetProperty(ref _textureName); } }
		internal static bool TextureFlipVertical				{ set { SetProperty(ref _textureFlipVertical, value); } get { return GetProperty(ref _textureFlipVertical); } }
		internal static Vector2 TextureOffset					{ set { SetProperty(ref _textureOffset, value); } 		get { return GetProperty(ref _textureOffset); } }
		internal static float TextureZoom						{ set { SetProperty(ref _textureZoom, value); } 		get { return GetProperty(ref _textureZoom); } }
		internal static string PinnedDisplayName				{ set { SetProperty(ref _pinnedDisplayName, value); } 	get { return GetProperty(ref _pinnedDisplayName); } }
		internal static Vector2 WindowOffset					{ set { SetProperty(ref _windowOffset, value); } 		get { return GetProperty(ref _windowOffset); } }
		internal static WindowSizeMode WindowSizeMode 			{ set { SetProperty(ref _windowSizeMode, value); } 		get { return GetProperty(ref _windowSizeMode); } }
		internal static Vector2 CustomWindowSize				{ set { SetProperty(ref _customWindowSize, value); } 	get { return GetProperty(ref _customWindowSize); } }
		internal static Color BackgroundColor					{ set { SetProperty(ref _backgroundColor, value); } 	get { return GetProperty(ref _backgroundColor); } }
		internal static bool BackgroundPattern					{ set { SetProperty(ref _backgroundPattern, value); } 	get { return GetProperty(ref _backgroundPattern); } }
		internal static bool SendKeyboardInput					{ set { SetProperty(ref _sendKeyboardInput, value); } 	get { return GetProperty(ref _sendKeyboardInput); } }
		internal static bool SendMouseInput						{ set { SetProperty(ref _sendMouseInput, value); } 		get { return GetProperty(ref _sendMouseInput); } }
		internal static bool AutoOpenOnEnteringPlayMode 		{ set { SetProperty(ref _autoOpenOnEnteringPlayMode, value); }			get { return GetProperty(ref _autoOpenOnEnteringPlayMode); } }
		internal static bool AutoCloseOnExitingPlayMode 		{ set { SetProperty(ref _autoCloseOnExitingPlayMode, value); }			get { return GetProperty(ref _autoCloseOnExitingPlayMode); } }
		internal static bool PreserveTextureOnExitingPlayMode	{ set { SetProperty(ref _preserveTextureOnExitingPlayMode, value); }	get { return GetProperty(ref _preserveTextureOnExitingPlayMode); } }

		// Dirty state
		private static bool _isModified;
		private static bool _isLoaded;

		private static void SetProperty<T>(ref T property, T value)
		{
			_isModified |= (!property.Equals(value));
			property = value; 
		}

		private static T GetProperty<T>(ref T property)
		{
			 LoadIfNeeded();
			 return property;
		}

		internal static void LoadIfNeeded()
		{
			if (!_isLoaded)
			{
				LoadSettings();
				_isLoaded = true;
			}
		}

		internal static void SaveSettingsIfModified()
		{
			if (_isModified)
			{
				SaveSettings();
				_isModified = false;
			}
		}

		internal static void LoadSettings()
		{
			LoadSaveSettings(false);
		}

		internal static void SaveSettings()
		{
			LoadSaveSettings(true);
		}

		internal static void LoadSaveSettings(bool isSave)
		{
			_textureSearchMode = (TextureSearchMode)SettingsStore.LoadSave(SettingsName_TextureSearchMode, (int)_textureSearchMode, isSave);
			_textureName = SettingsStore.LoadSave(SettingsName_TextureName, _textureName, isSave);
			_textureFlipVertical = SettingsStore.LoadSave(SettingsName_TextureFlipVertical, _textureFlipVertical, isSave);
			_textureOffset = SettingsStore.LoadSave(SettingsName_TextureOffset, _textureOffset, isSave);
			_textureZoom = SettingsStore.LoadSave(SettingsName_TextureZoom, _textureZoom, isSave);
			_pinnedDisplayName = SettingsStore.LoadSave(SettingsName_PinnedMonitorName, _pinnedDisplayName, isSave);
			_windowOffset = SettingsStore.LoadSave(SettingsName_WindowOffset, _windowOffset, isSave);
			_windowSizeMode = (WindowSizeMode)SettingsStore.LoadSave(SettingsName_WindowSizeMode, (int)_windowSizeMode, isSave);
			_customWindowSize = SettingsStore.LoadSave(SettingsName_CustomWindowSize, _customWindowSize, isSave);
			_backgroundColor = SettingsStore.LoadSave(SettingsName_BackgroundColor, _backgroundColor, isSave);
			_backgroundPattern = SettingsStore.LoadSave(SettingsName_BackgroundPattern, _backgroundPattern, isSave);
			_sendKeyboardInput = SettingsStore.LoadSave(SettingsName_SendKeyboardInput, _sendKeyboardInput, isSave);
			_sendMouseInput = SettingsStore.LoadSave(SettingsName_SendMouseInput, _sendMouseInput, isSave);
			_autoOpenOnEnteringPlayMode = SettingsStore.LoadSave(SettingsName_AutoOpenOnEnteringPlayMode, _autoOpenOnEnteringPlayMode, isSave);
			_autoCloseOnExitingPlayMode = SettingsStore.LoadSave(SettingsName_AutoCloseOnExitingPlayMode, _autoCloseOnExitingPlayMode, isSave);
			_preserveTextureOnExitingPlayMode = SettingsStore.LoadSave(SettingsName_PreserveTextureOnExitingPlayMode, _preserveTextureOnExitingPlayMode, isSave);
		}

		internal static void ResetSettings()
		{
			_textureSearchMode = TextureSearchMode.GameView;
			_textureName = Utils.GameViewTextureName;
			_textureFlipVertical = true;
			_textureOffset = Vector2.zero;
			_textureZoom = 1f;
			_pinnedDisplayName = string.Empty;
			_windowOffset = Vector2.zero;
			_windowSizeMode = WindowSizeMode.MatchTexture;
			_customWindowSize = DefaultCustomWindowSize;
			_backgroundColor = DefaultBackgroundColor;
			_backgroundPattern = true;
			_sendKeyboardInput = true;
			_sendMouseInput = false;
			_autoOpenOnEnteringPlayMode = false;
			_autoCloseOnExitingPlayMode = false;
			_preserveTextureOnExitingPlayMode = false;
			SaveSettings();
		}
	}
}