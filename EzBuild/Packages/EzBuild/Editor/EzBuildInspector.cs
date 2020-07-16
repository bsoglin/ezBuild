using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EzBuild))]
public class EzBuildInspector : Editor
{
	EzBuild ezBuild;
        
	public override void OnInspectorGUI()
	{
		ezBuild = target as EzBuild;

        if (GUILayout.Button("Choose build location"))
        {
            Undo.RegisterFullObjectHierarchyUndo(ezBuild.gameObject, "Choose location");
            ezBuild.buildLocation = EditorUtility.OpenFolderPanel("Choose a build location", "", "");
        }

        if (GUILayout.Button("Build All"))
        {
            if (EditorUtility.DisplayDialog("Are you sure you want to build all?", "This could take a while.", "Yes", "No"))
            {
                ezBuild.BuildAll();
            }
        }

        base.OnInspectorGUI();
	}
}
