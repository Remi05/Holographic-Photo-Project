using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildTool : MonoBehaviour {

    public const string VS_BUILD_DIR_NAME = "VSBuild";

    [MenuItem("Hololens Build With Postprocess")]
    public static void BuildGame() {

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WSAPlayer);
        EditorUserBuildSettings.wsaSDK = WSASDK.UWP;
        EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.XAML;
        EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;
        EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
        EditorUserBuildSettings.wsaGenerateReferenceProjects = true;
        
        EditorBuildSettingsScene[] projectScenes =  EditorBuildSettings.scenes;
        string outPath = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath)) + "/" + VS_BUILD_DIR_NAME;

        BuildPipeline.BuildPlayer(projectScenes, outPath, BuildTarget.WSAPlayer, BuildOptions.Development);
    }
}
