using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	///
	/// Allows the external game view to open or close when entering/leaving game mode.
	/// This is most useful when using external game view fullscreen on a single display system.
	///
	[InitializeOnLoad]
	internal static class PlayModeToggle
	{		
		static PlayModeToggle()
		{
			#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
			#else
			EditorApplication.playmodeStateChanged -= PlayModeStateChanged;
			EditorApplication.playmodeStateChanged += PlayModeStateChanged;
			#endif
		}

		#if UNITY_2017_2_OR_NEWER
		private static void PlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode && Settings.AutoCloseOnExitingPlayMode)
			{
				ExternalGameView.CloseWindows();
			}
			else if (state == PlayModeStateChange.EnteredPlayMode && Settings.AutoOpenOnEnteringPlayMode)
			{
				ExternalGameView.OpenWindow();
			}
		}
		#else
		private static void PlayModeStateChanged()
		{
			bool enteringPlayMode = !UnityEditor.EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode;
			bool exitingPlayMode = UnityEditor.EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode;
			if (enteringPlayMode && Settings.AutoCloseOnExitingPlayMode)
			{
				ExternalGameView.CloseWindows();
			}
			else if (exitingPlayMode && Settings.AutoOpenOnEnteringPlayMode)
			{
				ExternalGameView.OpenWindow();
			}
		}
		#endif
	}
}