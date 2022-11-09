using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using World;

[CustomEditor(typeof(WorldStreamer))]
public class WorldSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var settings = target as WorldStreamer;
        if(GUILayout.Button("Create Texture Array"))
        {
            settings.settings.CreateTextureArray();
        }
    }
}
