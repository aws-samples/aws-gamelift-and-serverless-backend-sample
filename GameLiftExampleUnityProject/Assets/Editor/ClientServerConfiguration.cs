using UnityEngine;
using System.Collections;
using UnityEditor;

public class ClientServerConfiguration : Editor 
{
    [MenuItem("GameLift/SetAsServerBuild")]
    private static void ServerBuild()
    {
        // Set scripting define symbols
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "SERVER");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "SERVER");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "SERVER");
    }

    [MenuItem("GameLift/SetAsClientBuild")]
    private static void ClientBuild()
    {
        // Set scripting define symbols
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "CLIENT");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "CLIENT");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "CLIENT");
    }

    [MenuItem("GameLift/BuildLinuxServer")]
    private static void BuildLinuxServer()
    {
        // Set scripting define symbols
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "SERVER");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "SERVER");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "SERVER");

        // Get filename
        string path = EditorUtility.SaveFolderPanel("Choose Location of Server Build", "", "");
        string[] levels = new string[] { "Assets/Scenes/GameWorld.unity"};

        // Build player
        BuildPipeline.BuildPlayer(levels, path + "/GameLiftExampleServer.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
    }

    [MenuItem("GameLift/BuildMacOSClient")]
    private static void BuildMacOSClient()
    {
        // Set scripting define symbols
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "CLIENT");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "CLIENT");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "CLIENT");

        // Get filename
        string path = EditorUtility.SaveFolderPanel("Choose Location of MacOS Build", "", "");
        string[] levels = new string[] { "Assets/Scenes/GameWorld.unity" };

        // Build player
        BuildPipeline.BuildPlayer(levels, path + "/GameClient.app", BuildTarget.StandaloneOSX, BuildOptions.None);
    }

    [MenuItem("GameLift/BuildWindowsClient")]
    private static void BuildWindowsClient()
    {
        // Set scripting define symbols
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "CLIENT");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "CLIENT");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "CLIENT");

        // Get filename
        string path = EditorUtility.SaveFolderPanel("Choose Location of Windows Build", "", "");
        string[] levels = new string[] { "Assets/Scenes/GameWorld.unity" };

        // Build player
        BuildPipeline.BuildPlayer(levels, path + "/GameClient.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }
}