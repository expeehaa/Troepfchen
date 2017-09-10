using UnityEditor;
using UnityEngine;

public class BuildScript
{
    private static readonly string[] Scenes = { "Assets/Scenes/mainmenu.unity", "Assets/Scenes/mainscene.unity", "Assets/Scenes/game.unity", "Assets/Scenes/multiplayergame.unity" };

    [MenuItem("Building/Build Release")]
    public static void BuildRelease() {
        BuildWindows();
        BuildLinux();
        BuildMacOSX();
    }

    [MenuItem("Building/Build Windows 32bit")]
    public static void BuildWindows() {
        var path = GetPath("windows");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            locationPathName = path + "troepfchen_win_v" + Application.version + ".exe",
            options = BuildOptions.None,
            scenes = Scenes,
            target = BuildTarget.StandaloneWindows
        });
    }

    [MenuItem("Building/Build Linux Universal")]
    public static void BuildLinux() {
        var path = GetPath("linux");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            locationPathName = path + "troepfchen_linux_v" + Application.version + ".x86",
            options = BuildOptions.None,
            scenes = Scenes,
            target = BuildTarget.StandaloneLinuxUniversal
        });
    }

    [MenuItem("Building/Build Mac OS X Universal")]
    public static void BuildMacOSX()
    {
        var path = GetPath("troepfchen_macosx_v" + Application.version + ".app");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            locationPathName = path,
            options = BuildOptions.None,
            scenes = Scenes,
            target = BuildTarget.StandaloneOSXUniversal
        });
    }

    public static string GetPath(string os) {
        return "executables/v" + Application.version + "/" + os + "/";
    }
}
