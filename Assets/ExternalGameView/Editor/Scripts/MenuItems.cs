using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	internal static class MenuItems
	{
		[MenuItem("RenderHeads/External Game View/Open Window...")]
		public static void OpenWindow()
		{
			ExternalGameView.OpenWindow();
		}

		[MenuItem("RenderHeads/External Game View/Open Preferences...")]
		public static void OpenPreferencesWindow()
		{
			SettingsIMGUIRegister.OpenSettingsWindow();
		}

#if UNITY_2019_1_OR_NEWER
		[UnityEditor.ShortcutManagement.Shortcut("RenderHeads/Toggle External Game View", KeyCode.E, UnityEditor.ShortcutManagement.ShortcutModifiers.Action)]
#endif
		public static void ToggleWindow()
		{
			Debug.Log("ToggleWindow " + (ExternalGameView.Instance == null));
			if (ExternalGameView.Instance == null)
			{
				ExternalGameView.OpenWindow();
			}
			else
			{
				ExternalGameView.CloseWindows();
			}
		}
	}
}