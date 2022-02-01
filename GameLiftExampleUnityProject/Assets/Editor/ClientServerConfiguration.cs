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
}