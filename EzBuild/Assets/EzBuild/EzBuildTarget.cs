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
        string gameName = $"{ezb.gameName}-{prefix}-{Application.version}";

        // move old versions of the game into the /old/ directory
        // first check: is there already an object with that name in this location?
        // if so, move them all to old
        foreach (string directoryPath in Directory.GetDirectories(buildLocation).ToList())
        {
            string directoryName = GetSubfolderName(directoryPath);

            // does the cur folder already contain a build w this name? if so move it?
            if (directoryPath.Contains(prefix))
            {
                string moveLocation = $"{buildLocation}/old/{directoryName}/";
                int number = 0;
                // also the old dir might contain this build too
                while (Directory.Exists(moveLocation))
                {
                    number++;

                    // if you're building for osx, preserve the .app
                    if (OSXBuild())
                    {
                        moveLocation = $"{buildLocation}/old/{directoryName.Replace(".app", "")}-{number}.app/";
                    }
                    else
                    {
                        moveLocation = $"{buildLocation}/old/{directoryName}-{number}/";
                    }
                }
                Directory.Move(directoryPath, moveLocation);
            }
        }

        // similar to the above, are their zip files with the given names in here? most likely they are zipped
        foreach (string zipFilePath in Directory.GetFiles(buildLocation).ToList())
        {

            string fileName = zipFilePath.Split('/').Last().Replace(".zip", "");

            // does the cur folder already contain a build w this name? if so move it?
            if (fileName.Contains(prefix))
            {
                string moveLocation = $"{buildLocation}/old/{fileName}.zip";
                int number = 0;

                // also the old dir might contain this build too
                while (File.Exists(moveLocation))
                {
                    number++;

                    moveLocation = $"{buildLocation}/old/{fileName}-{number}.zip";
                }

                File.Move(zipFilePath, moveLocation);
            }
        }

        string gamePath, gameFolderPath;

        // does the target need a subfolder made for it? or will unity handle it?
        if (OSXBuild())
        {
            gamePath = $"{buildLocation}/{gameName}.app";
            gameFolderPath = $"{gamePath}";
        }
        else
        {
            gameFolderPath = $"{buildLocation}/{gameName}";
            Directory.CreateDirectory(gameFolderPath);
            gamePath = $"{gameFolderPath}/{gameName}";
		}

        //once that's done, build into the location
        DoBuild(gamePath);

        if (currentSettings.zip)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(gameFolderPath, $"{gameFolderPath}.zip", System.IO.Compression.CompressionLevel.Fastest, OSXBuild());
        }
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
        string[] split = subfolderLocation.Split('/');
        return split[split.Length - 1];
    }

    public bool OSXBuild()
    {
        return buildTarget == BuildTarget.StandaloneOSX;
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
                return "huh";
                break;
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
                break;
        }
    }
}

[System.Serializable]
public class EzBuildTargetSettings
{
    public bool developmentMode, zip;
}
#endif