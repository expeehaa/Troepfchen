using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class NeutronFissionProductYield
{
    private static List<FissionNuclide> fissionNuclides;
    public static List<FissionNuclide> FissionNuclides
    {
        get
        {
            return fissionNuclides;
        }
        set
        {
            fissionNuclides = value;
        }
    }

    public static void RetrieveData()
    {
        var content = new FileInfo(Application.streamingAssetsPath + "/neutroninducedProductYields.json").OpenText().ReadToEnd();
        FissionNuclides = new List<FissionNuclide>();
        var settings = new JsonSerializerSettings()
        {
            MaxDepth = 100,
            Formatting = Formatting.None,
            FloatParseHandling = FloatParseHandling.Double
        };
        FissionNuclides.AddRange(JsonConvert.DeserializeObject<List<FissionNuclide>>(content, settings));
        //Debug.Log(JsonConvert.SerializeObject(FissionNuclides).Substring(0, 500));
    }
}

public class FissionNuclide
{
    public string Symbol;
    public int Protons;
    public int Mass;
    public bool Metastable;
    public List<YieldEntry> ProductYields;

    public FissionNuclide(string symbol, int mass, bool metastable)
    {
        Symbol = symbol;
        Mass = mass;
        Metastable = metastable;
        ProductYields = new List<YieldEntry>();
        Protons = NuclideCard.Nuclides.Find(ne => ne.ShortName.ToLower().Equals(Symbol.ToLower())).ProtonCount;
    }
}

public class YieldEntry
{
    public double NeutronEnergy;
    public List<FissionProduct> ProductYield;

    public YieldEntry(double neutronEnergy)
    {
        NeutronEnergy = neutronEnergy;
        ProductYield = new List<FissionProduct>();
    }
}

public class FissionProduct
{
    public int Protons;
    public int Mass;
    public string Symbol;
    public bool Metastable;
    public double Y;
    public double DY;

    public FissionProduct(int protons, int mass, string symbol, bool metastable, double y, double dy)
    {
        Protons = protons;
        Mass = mass;
        Symbol = symbol;
        Metastable = metastable;
        Y = y;
        DY = dy;
    }

    public override string ToString()
    {
        return Symbol.Trim() + "-" + Mass + (Metastable ? "M" : "");
    }

    public bool Equals(string s)
    {
        return s == this.ToString();
    }
}