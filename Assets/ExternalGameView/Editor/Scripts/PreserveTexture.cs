using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	///
	/// When displaying a custom texture that only exists in Play mode (eg not the game view texture),
	/// this texture will become lost when leaving Play mode, so we can preserve it by making a copy
	/// and displaying it until the texture becomes available again.
	/// This is useful if you want to have the last frame visible while in Edit mode.
	///
	[InitializeOnLoad]
	internal static class PreserveTexture
	{
		static PreserveTexture()
		{
			#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
			#else
			EditorApplication.playmodeStateChanged -= PlayModeStateChanged;
			EditorApplication.playmodeStateChanged += PlayModeStateChanged;
			#endif
		}

		internal static RenderTexture PreservedTexture
		{
			get;
			private set;
		}

		#if UNITY_2017_2_OR_NEWER
		private static void PlayModeStateChanged(PlayModeStateChange state)
		{
			ExternalGameView window = ExternalGameView.Instance;
			if (window != null && Settings.PreserveTextureOnExitingPlayMode)
			{
				if (state == PlayModeStateChange.ExitingPlayMode)
				{
					CopyTexture(window, Utils.FindRenderTexture(Settings.TextureName));
				}
				else if (state != PlayModeStateChange.EnteredEditMode)
				{
					//ReleaseTexture(window);
				}
			}
		}
		#else
		private static void PlayModeStateChanged()
		{
			ExternalGameView window = ExternalGameView.Instance;
			if (window != null && Settings.PreserveTextureOnExitingPlayMode)
			{
				bool enteringPlayMode = !UnityEditor.EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode;
				if (enteringPlayMode)
				{
					CopyTexture(window, Utils.FindRenderTexture(Settings.TextureName));
				}
				else
				{
					//ReleaseTexture(window);
				}
			}
		}
		#endif

		private static void CopyTexture(ExternalGameView window, RenderTexture texture)
		{
			ReleaseTexture(window);
			if (texture != null)
			{
				// NOTE: We can't use new RenderTexture() here as they don't survive PlayMode transitions
				#if UNITY_2019_1_OR_NEWER
				PreservedTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, texture.graphicsFormat);
				#else
				PreservedTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, texture.format);
				#endif
				if (PreservedTexture != null)
				{
					Graphics.Blit(texture, PreservedTexture);
					//window.SetPreservedTexture(PreservedTexture);
				}
			}
		}

		private static void ReleaseTexture(ExternalGameView window)
		{
			if (PreservedTexture != null)
			{
				//Debug.Log("releasing TEXTURE");
				RenderTexture.ReleaseTemporary(PreservedTexture);
				PreservedTexture = null;
				//window.SetPreservedTexture(null);
			}
		}
	}
}