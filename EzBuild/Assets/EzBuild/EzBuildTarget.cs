#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class EzBuildTarget : MonoBehaviour
{
    EzBuild ezb;
    public EzBuildTargetSettings settings;
    public BuildTarget buildTarget;

    EzBuildTargetSettings currentSettings;

    public void Build(EzBuildTargetSettings curSettings)
    {
        currentSettings = curSettings;

        ezb = GetComponentInParent<EzBuild>();
        ezb.PrepBuild();

        string buildLocation = ezb.buildLocation;
        string prefix = GetBuildPrefix(buildTarget);
        string gameNameWithDetails = $"{ezb.gameName}-{prefix}-{Application.version}";

        // move old versions of the game into the /old/ directory
        // first check: is there already an object with that name in this location?
        // if so, move them all to old
        foreach (string directoryPath in Directory.GetDirectories(buildLocation))
        {
            string directoryName = Path.GetFileName(directoryPath);

            // does the cur folder already contain a build w this name? if so move it
            if (directoryPath.Contains(prefix))
            {
                string moveLocation = Path.Combine(buildLocation, "old", $"{directoryName}");
                int number = 0;
                // also the old dir might contain this build too
                while (Directory.Exists(moveLocation))
                {
                    number++;

                    moveLocation = Path.Combine(buildLocation, "old", $"{directoryName}-{number}");
                }

                Directory.Move(directoryPath, moveLocation);
            }
        }

        // similar to the above, are there zip files with the given names in here? move them too
        foreach (string zipFilePath in Directory.GetFiles(buildLocation).ToList())
        {

            string fileName = Path.GetFileName(zipFilePath);

            Debug.Log(fileName);

            // does the cur folder already contain a build w this name? if so move it?
            if (fileName.Contains(prefix))
            {
                string moveLocation = Path.Combine(buildLocation, "old", $"{fileName}.zip");

                int number = 0;

                // also the old dir might contain this build too
                while (File.Exists(moveLocation))
                {
                    number++;

                    moveLocation = Path.Combine(buildLocation, "old", $"{fileName}-{number}.zip");
                }

                Debug.Log(zipFilePath + " " + moveLocation);

                File.Move(zipFilePath, moveLocation);
            }
        }

        string gamePath, gameFolderPath;

        // does the target need a subfolder made for it? or will unity handle it?
        if (OSXBuild())
        {
            gamePath = Path.Combine(buildLocation, $"{gameNameWithDetails}/{ezb.gameName}.app");

            gameFolderPath = $"{gamePath}";

            Directory.CreateDirectory(gameFolderPath);
        }
        else
        {
            gameFolderPath = Path.Combine(buildLocation, gameNameWithDetails);

            Directory.CreateDirectory(gameFolderPath);

            if (WindowsBuild())
            {
                gamePath = Path.Combine(gameFolderPath, $"{ezb.gameName}.exe");
            }
            else if (WebGLBuild())
            {
                gamePath = gameFolderPath;
            }
            else 
            {
                gamePath = Path.Combine(gameFolderPath, ezb.gameName);
            }
        }

        //once that's done, build into the location
        DoBuild(gamePath);

        if (currentSettings.zip)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(gameFolderPath, $"{gameFolderPath}.zip", System.IO.Compression.CompressionLevel.Fastest, OSXBuild());
        }

        Debug.Log($"Succesfully built for {prefix}");
    }

    public void DoBuild(string path)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(), buildTarget);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = (from editorScene in EditorBuildSettings.scenes
                                     where editorScene.enabled
                                     select editorScene.path).ToArray();

        buildPlayerOptions.locationPathName = path;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = currentSettings.developmentMode ? BuildOptions.Development : BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    string GetSubfolderName(string subfolderLocation)
    {
        string[] split = subfolderLocation.Split(Path.PathSeparator);
        return split[split.Length - 1];
    }

    public bool OSXBuild()
    {
        return buildTarget == BuildTarget.StandaloneOSX;
    }
       
    public bool WindowsBuild()
    {
        return buildTarget == BuildTarget.StandaloneWindows64;
    }
    public bool WebGLBuild()
    {
        return buildTarget == BuildTarget.WebGL;
    }

    public string GetBuildPrefix(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.StandaloneOSX:
                return "osx";
            case BuildTarget.StandaloneWindows64:
                return "pc";
            case BuildTarget.WebGL:
                return "webgl";
            case BuildTarget.StandaloneLinux64:
                return "linux";
            default:
				Debug.LogError("Unrecognized build target");
                return "unrecognized";
        }
    }

    BuildTargetGroup GetBuildTargetGroup()
    {
        switch (buildTarget)
        {
            case BuildTarget.StandaloneOSX:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneLinux64:
                return BuildTargetGroup.Standalone;
            case BuildTarget.WebGL:
                return BuildTargetGroup.WebGL;
            default:
				Debug.LogError("Unrecognized build target");
				return BuildTargetGroup.Unknown;
        }
    }
}

[System.Serializable]
public class EzBuildTargetSettings
{
    public bool developmentMode, zip;
}
#endif