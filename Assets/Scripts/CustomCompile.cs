using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using WebSocketServer;

[ExecuteInEditMode]
public class CustomCompile : MonoBehaviour
{
    public bool trigger = false;

    void Start()
    {
        Debug.Log("Executing in edit modes");
    }

    /*
    void Update()
    {
        
    }
    */

    private void OnValidate()
    {
        if(!trigger) return;

        CompilationPipeline.RequestScriptCompilation();
        EditorApplication.EnterPlaymode();

        trigger = false;
    }
}
