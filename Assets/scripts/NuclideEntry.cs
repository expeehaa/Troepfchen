using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public class NuclideEntry
    {
        public string ShortName { get; set; }
        public int ProtonCount { get; set; }
        public int NeutronCount { get; set; }
        public float Level { get; set; }
        public float Mass { get; set; }
        public float? Spin { get; set; }
        public float? NatOccurence { get; set; }
        public float? Halflife { get; set; }
        public List<string> Decays { get; set; }
        public int NucleonCount
        {
            get
            {
                return ProtonCount + NeutronCount;
            }
        }
        
        public NuclideEntry(string shortName, int protonCount, int neutronCount, float level, float mass, float? spin, float? natOccurence, float? halflife, List<string> decays)
        {
            ShortName = shortName;
            ProtonCount = protonCount;
            NeutronCount = neutronCount;
            Level = level;
            Mass = mass;
            Spin = spin;
            NatOccurence = natOccurence;
            Halflife = halflife;
            Decays = decays;
        }

        public NuclideEntry(string row)
        {
            //Debug.Log(row);
            var data = row.Split(new string[] { ";;" }, StringSplitOptions.None);
            if (data[0].EndsWith("m")) data[0] = data[0].Substring(0, data[0].Length - 1);
            ShortName = data[1].Substring(0, 1).ToUpper() + data[1].Substring(1).ToLower();
            ProtonCount = int.Parse(data[0]);
            NeutronCount = int.Parse(data[2])-ProtonCount;
            Level = float.Parse(data[3]);
            Mass = float.Parse(data[4]);
            Spin = data[5] == "null" ? null : (float?)float.Parse(data[5]);
            NatOccurence = data[6] == "null" ? null : (float?)float.Parse(data[6]);
            Halflife = data[7] == "null" ? null : (data[7] == "stable" ? null : (float?)float.Parse(data[7]));
            if (data.Length >= 9) Decays = (data[7] == "stable" ? null : data[8].Split(',').ToList()) ?? new List<string>();
        }
    }
}
