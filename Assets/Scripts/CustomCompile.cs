using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif

[ExecuteInEditMode]
public class CustomCompile : MonoBehaviour
{
#if UNITY_EDITOR
	public bool refresh = false;

	public void TestGame()
	{
		CompilationPipeline.RequestScriptCompilation();
		EditorApplication.EnterPlaymode();
		//UnityEngine.Debug.Log("Python Hook Printed!");
	}

	private string watcherPath = @"C:\Users\Jay\Documents\_UnityProjects\ShortJam\ShortJam\ShortJam\Assets\Scripts";
	private string watcherFile = "compile-watcher.txt";
	
	private bool changed;
	private FileSystemWatcher watcher;


	private void OnEnable()
	{
		if (!File.Exists(Path.Combine(watcherPath, watcherFile)))
		{
			return;
		}

		watcher = new FileSystemWatcher();
		watcher.Path = watcherPath;
		watcher.Filter = watcherFile;

		// Watch for changes in LastAccess and LastWrite times, and
		// the renaming of files or directories.
		watcher.NotifyFilter = NotifyFilters.LastWrite;

		// Add event handlers
		watcher.Changed += OnChanged;

		// Begin watching
		watcher.EnableRaisingEvents = true;
	}


	private void OnDisable()
	{
		if(watcher != null)
		{
			watcher.Changed -= OnChanged;
			watcher.Dispose();
		}
	}

	private void Update()
	{
		if (changed)
		{
			// Do something here…
			TestGame();
			changed = false;
		}
	}


	private void OnChanged(object source, FileSystemEventArgs e)
	{
		Debug.Log("Entering Playmode");
		//TestGame();
		changed = true;
	}
#endif
}


#if UNITY_EDITOR
// ensure class initializer is called whenever scripts recompile
[InitializeOnLoadAttribute]
public static class PlayModeStateChangedExample
{
	private static string senderFilepath = @"C:\Users\Jay\Documents\_UnityProjects\ShortJam\ShortJam\ShortJam\Assets\Scripts\compile-sender.txt";
	// register an event handler when the class is initialized
	static PlayModeStateChangedExample()
	{
		EditorApplication.playModeStateChanged += LogPlayModeState;
	}

	private static void LogPlayModeState(PlayModeStateChange state)
	{
		if (!File.Exists(senderFilepath))
		{
			return;
		}
		
		string strState = state.ToString();
		switch (strState)
		{
			case "ExitingEditMode":
				break;
			case "EnteredPlayMode":
				// Write to sender
				File.WriteAllText(senderFilepath, strState);
				break;
			case "ExitingPlayMode":
				// Write to sender
				File.WriteAllText(senderFilepath, strState);
				break;
			case "EnteredEditMode":
				break;
		}
	}
}
#endif