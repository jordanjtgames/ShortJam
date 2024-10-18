using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	internal static class Utils
	{
		internal const string GameViewTextureName = "GameView RT";

		internal static RenderTexture FindRenderTexture(string name)
		{
			// Ignore unnamed RenderTextures
			if (!string.IsNullOrEmpty(name))
			{
				RenderTexture[] rts = Resources.FindObjectsOfTypeAll<RenderTexture>();
				foreach (RenderTexture rt in rts)
				{
					if (rt.name == name)
					{
						return rt;
					}
				}
			}
			return null;
		}

		internal static Rect ScaleToFit(Rect sourceRect, Rect destRect) 
		{
			float sourceRatio = sourceRect.width / sourceRect.height;
			float destRatio = destRect.width / destRect.height;
			if (destRatio > sourceRatio)
			{
				float adjust = sourceRatio / destRatio;
				destRect = new Rect(destRect.xMin + destRect.width * (1f - adjust) * 0.5f, destRect.yMin, adjust * destRect.width, destRect.height);
			}
			else
			{
				float adjust = destRatio / sourceRatio;
				destRect = new Rect(destRect.xMin, destRect.yMin + destRect.height * (1f - adjust) * 0.5f, destRect.width, adjust * destRect.height);
			}
			return destRect;
		}

		internal static bool IsIMGUIKeyDown(KeyCode keyCode)
		{
			bool result = false;
			if (Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == keyCode)
				{
					result = true;
				}
			}
			return result;
		}

		internal static EditorWindow GetMainGameView()
		{
			var assembly = typeof(EditorWindow).Assembly;
			var type = assembly.GetType("UnityEditor.GameView");
			var gameview = EditorWindow.GetWindow(type);
			return gameview;
		}

		internal static void OpenPreferencesWindow()
		{
			//var assembly= Assembly.GetAssembly(typeof(EditorWindow));
			var assembly = typeof(EditorWindow).Assembly;
			var type = assembly.GetType("UnityEditor.PreferencesWindow");
			var method = type.GetMethod("ShowPreferencesWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			method.Invoke(null, null);
		}
	}
}