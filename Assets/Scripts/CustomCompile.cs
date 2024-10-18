using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[ExecuteInEditMode]
public class CustomCompile : MonoBehaviour
{
    public bool refresh = false;

    public void TestGame()
    {
        CompilationPipeline.RequestScriptCompilation();
        EditorApplication.EnterPlaymode();
        //UnityEngine.Debug.Log("Python Hook Printed!");
    }

    [SerializeField] private string path = @"C:\Users\Jay\Documents\_UnityProjects\ShortJam\ShortJam\ShortJam\Assets\Scripts";
    [SerializeField] private string file = "compile-watcher.txt";
    private bool changed;
    private FileSystemWatcher watcher;


    private void OnEnable()
    {
        if (!File.Exists(Path.Combine(path, file)))
        {
            return;
        }

        watcher = new FileSystemWatcher();
        watcher.Path = path;
        watcher.Filter = file;

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
        //TestGame();
        changed = true;
    }
}