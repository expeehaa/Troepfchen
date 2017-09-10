using Assets.scripts;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NuclideCard
{
    private static List<NuclideEntry> nuclides = new List<NuclideEntry>();
    public static List<NuclideEntry> Nuclides
    {
        get
        {
            return nuclides;
        }
        set
        {
            nuclides = value;
        }
    }

    public static void RetrieveData()
    {
        var content = new FileInfo(Application.streamingAssetsPath + "/nuclides.txt").OpenText().ReadToEnd();
        Nuclides = new List<NuclideEntry>();
        foreach (var nuclide in content.Split('\n'))
        {
            if (nuclide.Contains(";;")) Nuclides.Add(new NuclideEntry(nuclide));
        }
    }
}
