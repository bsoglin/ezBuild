#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEditor;

// main
public class EzBuild : MonoBehaviour
{
    public string buildLocation, gameName;

    public bool overrideChildSettings;

    public EzBuildTargetSettings settings;

    public void PrepBuild()
    {
        // do the given folder(s) exist? if not, create em
        foreach (string dir in new List<string>() { "old" })
        {
            string path = Path.Combine(buildLocation, dir);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    public void BuildAll()
    {
        List<string> buildTargetNamesByTargetGroup = (from buildTarget in GetComponentsInChildren<EzBuildTarget>()
                                                         orderby buildTarget.buildTarget
                                                         select buildTarget.name).ToList();

        // the scenes are reloaded between each build, so have to refind the targets
        foreach(string childName in buildTargetNamesByTargetGroup)
        {
            EzBuild ezBuild = FindObjectOfType<EzBuild>();

            EzBuildTarget ezbt = (from bt in ezBuild.GetComponentsInChildren<EzBuildTarget>()
                                  where bt.name == childName
                                  select bt).First();

            if (ezBuild.overrideChildSettings)
            {
                ezbt.Build(ezBuild.settings);
            }
            else
            {
                ezbt.Build(ezbt.settings);
            }
        }
    }
}
#endif