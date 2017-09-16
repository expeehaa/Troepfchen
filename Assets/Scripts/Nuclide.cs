using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class Nuclide
{
    [SerializeField]
    private int protonCount;
    public int ProtonCount
    {
        get
        {
            return protonCount;
        }
        set
        {
            protonCount = value;
        }
    }
    [SerializeField]
    private int neutronCount;
    public int NeutronCount
    {
        get
        {
            return neutronCount;
        }
        set
        {
            neutronCount = value;
        }
    }
    public int NucleonCount
    {
        get
        {
            return ProtonCount + NeutronCount;
        }
    }

    public Nuclide(int p, int n)
    {
        ProtonCount = p;
        NeutronCount = n;
    }

    public static Nuclide FromString(string s)
    {
        var split = s.Split('-');
        if (Regex.IsMatch(s.ToLower(), "(\\d+\\-\\d+)"))
            return new Nuclide(int.Parse(split[0]), int.Parse(split[1]));
        if (!Regex.IsMatch(s.ToLower(), "([a-z]+\\-\\d+)")) return null;
        var nuclideEntry = NuclideCard.Nuclides.FirstOrDefault(n => n.ShortName.ToLower().Equals(split[0].ToLower()) && n.NucleonCount == int.Parse(split[1]));
        return nuclideEntry != null ? new Nuclide(nuclideEntry.ProtonCount, nuclideEntry.NeutronCount) : null;
    }

    public static bool IsNuclideString(string s)
    {
        return Regex.IsMatch(s.ToLower(), "((\\d+\\-\\d+)|([a-z]+\\-\\d+))");
    }

    /// <summary>
    /// Returns a well formed string describing the objects content.
    /// </summary>
    /// <returns>String matching the Pattern "ProtonCount-NeutronCount".</returns>
    public override string ToString()
    {
        return ProtonCount + "-" + NeutronCount;
    }

    public string ToString(bool withName)
    {
        if (!withName) return ToString();
        var entry = NuclideCard.Nuclides.FirstOrDefault(n => n.NeutronCount == NeutronCount && n.ProtonCount == ProtonCount);
        if (entry == null) return ToString();
        return entry.ShortName + "-" + entry.NucleonCount;
    }

    public override bool Equals(object obj) {
        var nuclide = obj as Nuclide;
        return nuclide != null && (nuclide.ProtonCount == ProtonCount && nuclide.NeutronCount == NeutronCount);
    }
}