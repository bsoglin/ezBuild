using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(EzBuildTarget))]
public class EzBuildTargetInspector : Editor
{
    EzBuildTarget ezBuildTarget;

    public override void OnInspectorGUI()
    {
        ezBuildTarget = target as EzBuildTarget;

        base.OnInspectorGUI();

        if (GUILayout.Button("Build"))
        {
            ezBuildTarget.Build(ezBuildTarget.settings);
        }
    }
}