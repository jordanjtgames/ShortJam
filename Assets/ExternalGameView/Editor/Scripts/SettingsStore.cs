using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	//
	// Save/Load settings using a project-specific prefix
	// Yes a scriptable object in /UserSettings via SaveToSerializedFileAndForget(), or SessionState or ScriptableSingleton could be used,
	// but then we limit backwards compatibilty, so instead we're just using EditorPrefs to keep things simple
	//
	class SettingsStore
	{
		internal static string LoadSave(string name, string p, bool isSave)
		{
			if (isSave) { Save(name, p); return p; }
			return Load(name, p);
		}

		internal static void Save(string name, string p)
		{
			name = GetPrefName(name);
			EditorPrefs.SetString(name, p);
		}

		internal static string Load(string name, string p)
		{
			name = GetPrefName(name);
			p = EditorPrefs.GetString(name, p);
			return p;
		}

		internal static bool LoadSave(string name, bool p, bool isSave)
		{
			if (isSave) { Save(name, p); return p; }
			return Load(name, p);
		}

		internal static void Save(string name, bool p)
		{
			name = GetPrefName(name);
			EditorPrefs.SetBool(name, p);
		}

		internal static bool Load(string name, bool p)
		{
			name = GetPrefName(name);
			p = EditorPrefs.GetBool(name, p);
			return p;
		}

		internal static int LoadSave(string name, int p, bool isSave)
		{
			if (isSave) { Save(name, p); return p; }
			return Load(name, p);
		}

		internal static void Save(string name, int p)
		{
			name = GetPrefName(name);
			EditorPrefs.SetInt(name, p);
		}

		internal static int Load(string name, int p)
		{
			name = GetPrefName(name);
			p = EditorPrefs.GetInt(name, p);
			return p;
		}		

		internal static float LoadSave(string name, float p, bool isSave)
		{
			if (isSave) { Save(name, p); return p; }
			return Load(name, p);
		}

		internal static void Save(string name, float p)
		{
			name = GetPrefName(name);
			EditorPrefs.SetFloat(name, p);
		}

		internal static float Load(string name, float p)
		{
			name = GetPrefName(name);
			p = EditorPrefs.GetFloat(name, p);
			return p;
		}

		internal static Vector2 LoadSave(string name, Vector2 p, bool isSave)
		{
			if (isSave) { Save(name, p); return p; }
			return Load(name, p);
		}

		internal static void Save(string name, Vector2 p)
		{
			name = GetPrefName(name);
			EditorPrefs.SetFloat(name + ".X", p.x);
			EditorPrefs.SetFloat(name + ".Y", p.y);
		}

		internal static Vector2 Load(string name, Vector2 p)
		{
			name = GetPrefName(name);
			p.x = EditorPrefs.GetFloat(name + ".X", p.x);
			p.y = EditorPrefs.GetFloat(name + ".Y", p.y);
			return p;
		}

		internal static Color LoadSave(string name, Color p, bool isSave)
		{
			if (isSave) { Save(name, p); return p; }
			return Load(name, p);
		}

		internal static void Save(string name, Color p)
		{
			name = GetPrefName(name);
			EditorPrefs.SetFloat(name + ".R", p.r);
			EditorPrefs.SetFloat(name + ".G", p.g);
			EditorPrefs.SetFloat(name + ".B", p.b);
			EditorPrefs.SetFloat(name + ".A", p.a);
		}

		internal static Color Load(string name, Color p)
		{
			name = GetPrefName(name);
			p.r = EditorPrefs.GetFloat(name + ".R", p.r);
			p.g = EditorPrefs.GetFloat(name + ".G", p.g);
			p.b = EditorPrefs.GetFloat(name + ".B", p.b);
			p.a = EditorPrefs.GetFloat(name + ".A", p.a);
			return p;
		}

		private static string GetPrefName(string name)
		{
			// Add productGUID to make the settings project-specific
			// Yes this could be partially cached, but perf impact should be minimal
			return string.Format("RenderHeads-ExternalGameView-{0}-{1}", PlayerSettings.productGUID.ToString(), name);
		}
	}
}