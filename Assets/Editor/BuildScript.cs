using Ionic.Zip;
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    private static readonly string[] Scenes = { "Assets/Scenes/mainmenu.unity", "Assets/Scenes/mainscene.unity", "Assets/Scenes/game.unity", "Assets/Scenes/multiplayergame.unity" };

    [MenuItem("Building/Build Release")]
    public static void BuildRelease() {
        BuildWindows(true);
        BuildLinux(true);
        BuildMacOSX(true);
    }

    private static void BuildWindows(bool zip) {
        var path = GetPath();
        BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            locationPathName = path + "troepfchen_win_v" + Application.version + ".exe",
            options = BuildOptions.None,
            scenes = Scenes,
            target = BuildTarget.StandaloneWindows
        });
        if (!zip) return;
        using (var zipFile = new ZipFile()) {
            zipFile.AddDirectory(path + "troepfchen_win_v" + Application.version + "_Data", "troepfchen_win_v" + Application.version + "_Data");
            zipFile.AddFile(path + "troepfchen_win_v" + Application.version + ".exe", "/");
            zipFile.Save(path + "troepfchen_win_v" + Application.version + ".zip");
        }
    }

    private static void BuildLinux(bool zip) {
        var path = GetPath();
        BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            locationPathName = path + "troepfchen_linux_v" + Application.version + ".x86",
            options = BuildOptions.None,
            scenes = Scenes,
            target = BuildTarget.StandaloneLinuxUniversal
        });
        if (!zip) return;
        using (var zipFile = new ZipFile()) {
            zipFile.AddDirectory(path + "troepfchen_linux_v" + Application.version + "_Data", "troepfchen_linux_v" + Application.version + "_Data");
            zipFile.AddFile(path + "troepfchen_linux_v" + Application.version + ".x86", "/");
            zipFile.AddFile(path + "troepfchen_linux_v" + Application.version + ".x86_64", "/");
            zipFile.Save(path + "troepfchen_linux_v" + Application.version + ".zip");
        }
    }

    private static void BuildMacOSX(bool zip)
    {
        var path = GetPath();
        BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            locationPathName = path + "troepfchen_macosx_v" + Application.version + ".app",
            options = BuildOptions.None,
            scenes = Scenes,
            target = BuildTarget.StandaloneOSXUniversal
        });
        if (!zip) return;
        using (var zipFile = new ZipFile()) {
            zipFile.AddDirectory(path + "troepfchen_macosx_v" + Application.version + ".app", "troepfchen_macosx_v" + Application.version + ".app");
            zipFile.Save(path + "troepfchen_macosx_v" + Application.version + ".zip");
        }
    }

    [MenuItem("Building/Build Windows 32bit")]
    public static void BuildNoZipWindows() {
        BuildWindows(false);
    }

    [MenuItem("Building/Build Linux Universal")]
    public static void BuildNoZipLinux() {
        BuildLinux(false);
    }

    [MenuItem("Building/Build Mac OS X Universal")]
    public static void BuildNoZipMacOSX() {
        BuildMacOSX(false);
    }

    [MenuItem("Building/Build and Zip Windows 32bit")]
    public static void BuildAndZipWindows() {
        BuildWindows(true);
    }

    [MenuItem("Building/Build and Zip Linux Universal")]
    public static void BuildAndZipLinux() {
        BuildLinux(true);
    }

    [MenuItem("Building/Build and Zip Mac OS X Universal")]
    public static void BuildAndZipMacOSX() {
        BuildMacOSX(true);
    }

    [MenuItem("Building/Open File Path")]
    public static void OpenFilePath() {
        EditorUtility.RevealInFinder(GetPath());
    }


    public static string GetPath() {
        return "executables/v" + Application.version + "/";
    }
}
