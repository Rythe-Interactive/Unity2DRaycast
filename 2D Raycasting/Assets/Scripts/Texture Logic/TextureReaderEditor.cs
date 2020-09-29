using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureReader))]


public class TextureReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextureReader myScript = (TextureReader)target;

        if (GUILayout.Button("Run"))
            myScript.Generate();
            
    }
}
