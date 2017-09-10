using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class NukleonNodeScript : MonoBehaviour {

    public GameObject Sphere;
    public GameObject Text;

    public Material MatN;
    public Material MatZ;
    public Material MatOutlineShader;
    public Material MatOutlineShaderFake;

    public Vector3 MassPointNormal = Vector3.zero;
    public Vector3 MassPointAlpha = new Vector3(0, 0, 40);
    public Vector3 MassPointProton = new Vector3(0, 0, 30);
    public Vector3 MassPointNeutron = new Vector3(0, 0, 20);

    public float NeutronDistance = 20f;

    private Nuclide nuclide = new Nuclide(0, 0);

    public Nuclide Nuclide
    {
        get
        {
            return nuclide;
        }
        set
        {
            nuclide = value;
            SpawnNukleons(value);
        }
    }

    private string decay = string.Empty;
    public string Decay
    {
        get
        {
            return decay;
        }
        set
        {
            decay = value ?? string.Empty;
            if (!CreateDecay(decay))
            {
                ResetNormal();
                Gamestate = GameState.Normal;
            }
            else Gamestate = GameState.Decay;
        }
    }

    public enum GameState { Normal, Decay, Fission }
    private GameState _gamestate = GameState.Normal;
    public GameState Gamestate {
        get
        {
            return _gamestate;
        }
        private set
        {
            _gamestate = value;
        }
    }
    
    private List<GameObject> nucleons = new List<GameObject>();
    public List<GameObject> Nucleons
    {
        get
        {
            return nucleons;
        }
    }
    private System.Random rnd = new System.Random();
    private bool hasOutlineShader = false;
    private NuclideContainer nuclideContainer;
    private Nuclide outerDecayNucleons = new Nuclide(0,0);
    private FissionData fissionData;
    private float spawnDistance = 0;

    // Use this for initialization
    void Start()
    {
        Text.GetComponent<TextMesh>().alignment = TextAlignment.Center;
        Text.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
        Text.GetComponent<TextMesh>().richText = true;
        nuclideContainer = GameObject.Find("EventSystem").GetComponent<NuclideContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        Text.transform.localPosition = new Vector3(0, Mathf.Sqrt(spawnDistance) * 2, 0);
        var n = NuclideCard.Nuclides.Find(k => k.ProtonCount == Nuclide.ProtonCount);
        var name = n == null ? "El" : n.ShortName;
        var dn = NuclideCard.Nuclides.Find(k => k.ProtonCount == Nuclide.ProtonCount - outerDecayNucleons.ProtonCount);
        var dname = dn == null ? "El" : dn.ShortName;
        name = Nuclide.NeutronCount == 1 && Nuclide.ProtonCount == 0 ? name.ToLower() : name;
        n = NuclideCard.Nuclides.Find(k => k.ProtonCount == Nuclide.ProtonCount && k.NeutronCount == Nuclide.NeutronCount);
        var dm = n == null ? -1 : (n.ProtonCount * 1.007825 + n.NeutronCount * 1.008665 - n.Mass);
        if (nuclide.NucleonCount != 0)
        {
            Text.GetComponent<TextMesh>().text = "<color=black><b>" + name + (dname != name ? " (" + dname + ")" : "") + "\nA</b>: " + Nuclide.NucleonCount + (outerDecayNucleons.NucleonCount != 0 ? " (" + (nuclide.NucleonCount - outerDecayNucleons.NucleonCount) + ")" : "") + "\n<b>Z</b>: " + Nuclide.ProtonCount + (outerDecayNucleons.ProtonCount != 0 ? " (" + (nuclide.ProtonCount - outerDecayNucleons.ProtonCount) + ")" : "") + "\n" + (n == null ? "" : "<b>m = </b>" + String.Format("{0:0.#######}", n.Mass) + "u\n") + (dm <= 0 ? "" : "<b>Δm =</b> " + String.Format("{0:0.#######}", dm) + "u\n") + (Gamestate == GameState.Normal ? "Normal" : (Gamestate == GameState.Decay ? "Zerfall (" + Decay + ")" : (Gamestate == GameState.Fission ? "Kernspaltung (" + fissionData.product.ToString() + ")" : ""))) + "</color>";
        }
        else Text.GetComponent<TextMesh>().text = "";
    }

    private void SpawnNukleons(Nuclide nuclide)
    {
        ResetAll();

        spawnDistance = CalculateSpawnDistance(nuclide.NucleonCount);

        for (int i = 0; i < nuclide.NucleonCount; i++)
        {
            var obj = Instantiate(Sphere);
            var nukleonscript = obj.GetComponent<NukleonScript>();
            nukleonscript.massPoint = MassPointNormal;
            if (i < nuclide.NeutronCount)
            {
                obj.GetComponent<Renderer>().material = MatN;
                nukleonscript.nucleonType = NukleonScript.NucleonType.Neutron;
                nukleonscript.fakeType = NukleonScript.NucleonType.Neutron;
            }
            else
            {
                obj.GetComponent<Renderer>().material = MatZ;
                nukleonscript.nucleonType = NukleonScript.NucleonType.Proton;
                nukleonscript.fakeType = NukleonScript.NucleonType.Proton;
            }

            nukleonscript.fast = false;

            obj.transform.SetParent(transform, false);

            var vec = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
            vec.Normalize();

            vec *= spawnDistance;
            obj.transform.localPosition = vec + MassPointNormal;

            nucleons.Add(obj);
        }
    }

    public bool CreateRandomPredefinedDecay()
    {
		var n = NuclideCard.Nuclides.FindAll(k => k.NeutronCount == Nuclide.NeutronCount && k.ProtonCount == Nuclide.ProtonCount);
        var decays = new List<string>();
        foreach (var item in n)
        {
            if (item != null && item.Decays != null && item.Decays.Count != 0) decays.AddRange(item.Decays.FindAll(s => !Regex.IsMatch(s, "(fe|sf|it|f)")));
        }
        if (decays.Count == 0) return false;
        var d = decays[rnd.Next(0, decays.Count)];
        if (Regex.IsMatch(d, "(=)")) d = d.Split('=')[0];
        Decay = d;
        return true;
    }

    private bool CreateDecay(string type)
    {
        if(type == null || type.Equals(string.Empty))
        {
            this.ResetNormal();
            outerDecayNucleons = new Nuclide(0, 0);
            return false;
        }
        else
        {
            if(Gamestate == GameState.Decay)
            {
                this.ResetNormal();
            }

            if(Nuclide.IsNuclideString(type.ToLower()))
            {
                var nuclide = Nuclide.FromString(type.ToLower());
                if (this.Nuclide.NeutronCount <= nuclide.NeutronCount && this.Nuclide.ProtonCount <= nuclide.ProtonCount) return false;

                var position = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
                position.Normalize();
                position *= MassPointAlpha.magnitude;

                var neutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                neutrons.Sort(new PositionDistanceComparer(position));
                foreach (var sphere in neutrons.Take(nuclide.NeutronCount))
                {
                    sphere.GetComponent<NukleonScript>().massPoint = position;
                }

                var protons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                protons.Sort(new PositionDistanceComparer(position));
                foreach (var sphere in protons.Take(nuclide.ProtonCount))
                {
                    sphere.GetComponent<NukleonScript>().massPoint = position;
                }
            }
            else if(Regex.IsMatch(type.ToLower(), "(it|fe|f)"))
            {
                return false;
            }
            else if (type.ToLower().Equals("sf"))
            {
                return false;
            }
            else
            {
                int pos = 0;
                int multiplier = 1;

                if (Regex.IsMatch(type.ToLower(), "^(\\d+)"))
                {
                    var match = Regex.Match(type.ToLower(), "^(\\d+)");
                    multiplier = int.Parse(match.Value);
                    pos += match.Length;
                }

                while (pos < type.ToLower().Length)
                {
                    var sub = type.Substring(pos);
                    //print("Substring: " + sub);

                    //print((sub.StartsWith("e") || sub.StartsWith("b+")) + "|" + sub.StartsWith("a") + "|" + sub.StartsWith("b-") + "|" + sub.StartsWith("n") + "|" + sub.StartsWith("p"));

                    if (sub.StartsWith("e") || sub.StartsWith("b+"))
                    {
                        if (nucleons.Count(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal) < multiplier) return false;

                        var protons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                        protons.Sort(new MassPointDistanceComparer());
                        foreach (var proton in protons.Take(multiplier))
                        {
                            proton.GetComponent<Renderer>().material = MatN;
                            proton.GetComponent<NukleonScript>().fakeType = NukleonScript.NucleonType.Neutron;
                        }

                        pos += sub.StartsWith("e") ? 1 : 2;
                    }
                    else if(sub.StartsWith("a"))
                    {
                        if (nucleons.Count(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal) <= 2 * multiplier && nucleons.Count(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal) <= 2 * multiplier) return false;

                        for (int i = 0; i < multiplier; i++)
                        {
                            var position = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
                            position.Normalize();
                            position *= MassPointAlpha.magnitude;

                            var neutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                            neutrons.Sort(new PositionDistanceComparer(position));
                            foreach (var sphere in neutrons.Take(2))
                            {
                                sphere.GetComponent<NukleonScript>().massPoint = position;
                            }

                            var protons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                            protons.Sort(new PositionDistanceComparer(position));
                            foreach (var sphere in protons.Take(2))
                            {
                                sphere.GetComponent<NukleonScript>().massPoint = position;
                            }
                        }
                        
                        pos++;
                    }
                    else if (sub.StartsWith("b-"))
                    {
                        if (nucleons.Count(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal) < multiplier) return false;

                        var neutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                        neutrons.Sort(new MassPointDistanceComparer());
                        foreach (var neutron in neutrons.Take(multiplier))
                        {
                            neutron.GetComponent<Renderer>().material = MatZ;
                            neutron.GetComponent<NukleonScript>().fakeType = NukleonScript.NucleonType.Proton;
                        }

                        pos += 2;
                    }
                    else if (sub.StartsWith("n"))
                    {
                        if (nucleons.Count(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal) < multiplier) return false;
                        
                        for (int i = 0; i < multiplier; i++)
                        {
                            var position = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
                            position.Normalize();
                            position *= MassPointNeutron.magnitude;

                            var neutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                            neutrons.Sort(new PositionDistanceComparer(position));
                            neutrons[0].GetComponent<NukleonScript>().massPoint = position;
                        }

                        pos++;
                    }
                    else if (sub.StartsWith("p"))
                    {
                        if (nucleons.Count(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal) < multiplier) return false;

                        for (int i = 0; i < multiplier; i++)
                        {
                            var position = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
                            position.Normalize();
                            position *= MassPointProton.magnitude;

                            var protons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                            protons.Sort(new PositionDistanceComparer(position));
                            protons[0].GetComponent<NukleonScript>().massPoint = position;
                        }

                        pos++;
                    }
                    else
                    {
                        pos++;
                    }
                }
                
            }

            CalculateShownInnerNuclide();

            return true;
        }
    }

    public void ResetDecay()
    {
        Decay = string.Empty;
    }

    public void ResetNormal(bool removeNeutron = true)
    {
        if (Gamestate == GameState.Fission && removeNeutron) nucleons.Where(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron).Take(1).ToList().ForEach(n => { nucleons.Remove(n); Destroy(n); });
        foreach (var nukleon in nucleons)
        {
            nukleon.GetComponent<NukleonScript>().massPoint = MassPointNormal;
            if (nukleon.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron) nukleon.GetComponent<Renderer>().material = MatN;
            if (nukleon.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton) nukleon.GetComponent<Renderer>().material = MatZ;
            nukleon.GetComponent<NukleonScript>().fakeType = nukleon.GetComponent<NukleonScript>().nucleonType;
        }
        CalculateNuclideCombination(true);
        outerDecayNucleons = new Nuclide(0, 0);
        Gamestate = GameState.Normal;
        spawnDistance = CalculateSpawnDistance(Nuclide.NucleonCount);
    }

    private void ResetAll(bool respawn = false)
    {
        foreach (var nukleon in nucleons)
        {
            Destroy(nukleon);
        }
        nucleons = new List<GameObject>();
        outerDecayNucleons = new Nuclide(0, 0);
        Gamestate = GameState.Normal;
        if(respawn) SpawnNukleons(Nuclide);
    }

    public void AddOutlineShader()
    {
        if (!hasOutlineShader) nucleons.ForEach(n => n.GetComponent<Renderer>().materials = n.GetComponent<Renderer>().materials.Concat(new List<Material>() { n.GetComponent<NukleonScript>().nucleonType != n.GetComponent<NukleonScript>().fakeType && n.GetComponent<NukleonScript>().massPoint == MassPointNormal ? MatOutlineShaderFake : MatOutlineShader }).ToArray());
        hasOutlineShader = true;
    }

    public void AddOutlineShaderAtMassPoint(Vector3 massPoint)
    {
        if (!hasOutlineShader) nucleons.ForEach(n => {
            if (n.GetComponent<NukleonScript>().massPoint == massPoint) n.GetComponent<Renderer>().materials = n.GetComponent<Renderer>().materials.Concat(new List<Material>() { MatOutlineShader }).ToArray();
            else n.GetComponent<Renderer>().materials = new List<Material>() { n.GetComponent<Renderer>().materials.First() }.ToArray();
        });
    }

    public void RemoveOutlineShader()
    {
        if (hasOutlineShader) nucleons.ForEach(n => n.GetComponent<Renderer>().materials = new List<Material>() { n.GetComponent<Renderer>().materials.First() }.ToArray());
        hasOutlineShader = false;
    }

    public void AdoptNucleons(List<GameObject> nucleons)
    {
        ResetDecay();
        bool fail = false;
        nucleons.ForEach(n => fail = !fail ? n.GetComponent<NukleonScript>() == null : fail);
        if (fail)
        {
            print("Adopting Nucleons failed!");
            return;
        }

        nucleons.ForEach(n =>
        {
            var nucleon = n.GetComponent<NukleonScript>();
            nucleon.massPoint = MassPointNormal;
            nucleon.fast = false;
            n.GetComponent<Renderer>().materials = new List<Material>() { n.GetComponent<Renderer>().materials.First() }.ToArray();
            nucleon.transform.SetParent(transform, true);
        });

        nuclide.NeutronCount += nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron);
        nuclide.ProtonCount += nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton);
        this.nucleons.AddRange(nucleons);

        CalculateShownInnerNuclide();

        var nps = nuclideContainer.NucleonNodePanels.FirstOrDefault(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode == gameObject);
        if (nps == null) return;
        nps.GetComponent<NodelistPanelScript>().ChangeNuclideType(Nuclide);
    }

    public List<GameObject> RemoveNucleonsFromNuclide(Vector3 massPoint)
    {
        if (massPoint == MassPointNormal) return null;
        var nucleons = this.nucleons.Where(n => n.GetComponent<NukleonScript>().massPoint == massPoint).ToList();
        nucleons.ForEach(n =>
        {
            this.nucleons.Remove(n);
            n.GetComponent<Renderer>().materials = new List<Material>() { n.GetComponent<Renderer>().materials.First() }.ToArray();
        });
        CalculateNuclideCombination(true);
        CalculateShownInnerNuclide();
        return nucleons;
    }

    public void AcceptInnerNucleons()
    {
        nucleons.Where(n => n.GetComponent<NukleonScript>().massPoint == MassPointNormal).ToList().ForEach(n => n.GetComponent<NukleonScript>().nucleonType = n.GetComponent<NukleonScript>().fakeType);
        nuclide.NeutronCount = nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron);
        nuclide.ProtonCount = nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton);
        var nps = nuclideContainer.NucleonNodePanels.FirstOrDefault(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode == gameObject);
        if (nps == null) return;
        nps.GetComponent<NodelistPanelScript>().ChangeNuclideType(Nuclide);
        CalculateShownInnerNuclide();
    }

    public void ChangeWorldPosition(Vector3 pos, bool nucleonsStay)
    {
        var dpos = transform.localPosition - pos;
        if (nucleonsStay) nucleons.ForEach(n => n.transform.localPosition += dpos);
        transform.localPosition = pos;
    }

    public IEnumerable<Vector3> GetMassPoints()
    {
        var massPoints = new List<Vector3>();
        nucleons.ForEach(n => {
            if (!massPoints.Contains(n.GetComponent<NukleonScript>().massPoint)) massPoints.Add(n.GetComponent<NukleonScript>().massPoint);
        });
        return massPoints;
    }

    private void CalculateShownInnerNuclide()
    {
        var deltaProtons = nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton) - nucleons.Count(n => n.GetComponent<NukleonScript>().massPoint == MassPointNormal && n.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Proton);
        var deltaNeutrons = nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron) - nucleons.Count(n => n.GetComponent<NukleonScript>().massPoint == MassPointNormal && n.GetComponent<NukleonScript>().fakeType == NukleonScript.NucleonType.Neutron);
        outerDecayNucleons = new Nuclide(deltaProtons, deltaNeutrons);
    }

    private void CalculateNuclideCombination(bool updateNodelistPanel)
    {
        Nuclide.NeutronCount = nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron);
        Nuclide.ProtonCount = nucleons.Count(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton);
        if (updateNodelistPanel)
        {
            var nps = nuclideContainer.NucleonNodePanels.FirstOrDefault(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode == gameObject);
            if (nps != null) nps.GetComponent<NodelistPanelScript>().ChangeNuclideType(Nuclide);
        }
    }

    public bool CanDoNeutronInducedFission()
    {
        return NeutronFissionProductYield.FissionNuclides.FindAll(fn => fn.Protons == Nuclide.ProtonCount && fn.Mass == Nuclide.NucleonCount).Count > 0;
    }

    public bool CreateNeutronInducedFission(FissionData fd)
    {
        if (Gamestate == GameState.Fission || !CanDoNeutronInducedFission() || fd.Equals(null) || (Nuclide.NeutronCount < (fd.product.Mass - fd.product.Protons) + fissionData.neutronCount || Nuclide.ProtonCount < fd.product.Protons)) return false;
        ResetNormal();
        Gamestate = GameState.Fission;

        var neutron = Instantiate(Sphere);
        var nukleonscript = neutron.GetComponent<NukleonScript>();
        nukleonscript.massPoint = MassPointNormal;
        neutron.GetComponent<Renderer>().material = MatN;
        nukleonscript.nucleonType = NukleonScript.NucleonType.Neutron;
        nukleonscript.fakeType = NukleonScript.NucleonType.Neutron;
        nukleonscript.onCollisionEnterFunc = OnFissionNeutronCollisionEnter;

        neutron.transform.SetParent(transform, false);
        neutron.transform.localPosition = Vector3.left * Sphere.transform.localScale.x * 30 + MassPointNormal;
        nucleons.Add(neutron);

        nuclide.NeutronCount++;
        CalculateShownInnerNuclide();
        CalculateNuclideCombination(false);

        fissionData = fd;

        return true;
    }

    private void OnFissionNeutronCollisionEnter(NukleonScript nukleon, Collision coll)
    {
        if (coll.gameObject.GetComponent<NukleonScript>() == null || !nucleons.Contains(coll.gameObject) || !nucleons.Contains(nukleon.gameObject) || fissionData.product == null || fissionData.neutronCount < 0) return;
        nukleon.onCollisionEnterFunc = null;
        StartCoroutine(PreFissionEnd());
    }

    private IEnumerator PreFissionEnd()
    {
        var dir = new Vector3(0f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
        var n1 = new Nuclide(fissionData.product.Protons, fissionData.product.Mass - fissionData.product.Protons);
        var n2 = new Nuclide(Nuclide.ProtonCount - fissionData.product.Protons, Nuclide.NeutronCount - fissionData.neutronCount - (fissionData.product.Mass - fissionData.product.Protons));
        var distance1 = CalculateSpawnDistance(n1.NucleonCount) * 4;
        var distance2 = CalculateSpawnDistance(n2.NucleonCount) * 4;
        var pos1 = dir * distance1;
        var pos2 = -dir * distance2;

        var counter = 5;
        while(counter > 0)
        {
            if (counter % 2 == 1)
            {
                {
                    var protons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                    protons.Sort(new PositionDistanceComparer(pos1 / counter));
                    protons.Take(n1.ProtonCount).ToList().ForEach(n => n.GetComponent<NukleonScript>().massPoint = pos1 / counter);

                    var neutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                    neutrons.Sort(new PositionDistanceComparer(pos1 / counter));
                    neutrons.Take(n1.NeutronCount + Mathf.FloorToInt(fissionData.neutronCount / 2f)).ToList().ForEach(n => n.GetComponent<NukleonScript>().massPoint = pos1 / counter);
                }
                {
                    var protons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Proton && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                    protons.Sort(new PositionDistanceComparer(pos2 / counter));
                    protons.Take(n2.ProtonCount).ToList().ForEach(n => n.GetComponent<NukleonScript>().massPoint = pos2 / counter);

                    var neutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && go.GetComponent<NukleonScript>().massPoint == MassPointNormal);
                    neutrons.Sort(new PositionDistanceComparer(pos2 / counter));
                    neutrons.Take(n2.NeutronCount + Mathf.CeilToInt(fissionData.neutronCount / 2f)).ToList().ForEach(n => n.GetComponent<NukleonScript>().massPoint = pos2 / counter);
                }
            }
            else
            {
                nucleons.ForEach(go => go.GetComponent<NukleonScript>().massPoint = MassPointNormal);
            }

            counter--;
            if(counter > 0) yield return new WaitForSeconds(1f/(counter*2));
        }
        

        for (int i = 0; i < fissionData.neutronCount; i++)
        {
            var freeNeutrons = nucleons.FindAll(go => go.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && (go.GetComponent<NukleonScript>().massPoint == pos1 || go.GetComponent<NukleonScript>().massPoint == pos2));
            var pos = new Vector3((float)(rnd.NextDouble() + 1) / 2, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
            pos.Normalize();
            freeNeutrons.Sort(new PositionDistanceComparer(pos));
            var n = freeNeutrons.FirstOrDefault();
            n.GetComponent<NukleonScript>().massPoint = pos * (distance1 + distance2)/2;
            n.GetComponent<NukleonScript>().fast = true;
        }

        yield return new WaitForSeconds(2);
        nucleons.FindAll(n => n.GetComponent<NukleonScript>().nucleonType == NukleonScript.NucleonType.Neutron && (n.GetComponent<NukleonScript>().massPoint != pos1 && n.GetComponent<NukleonScript>().massPoint != pos1)).ForEach(n => n.GetComponent<NukleonScript>().fast = false);

        if (fissionData.seperateNuclei)
        {
            var nps = nuclideContainer.NucleonNodePanels.FirstOrDefault(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode == gameObject);
            var cfdpanel = GameObject.FindGameObjectWithTag("cfdpanel");

            if (cfdpanel != null && nps != null)
            {
                var masspoints = GetMassPoints().ToList();
                masspoints.Remove(pos1);
                foreach (var mp in masspoints)
                {
                    cfdpanel.GetComponent<CFDPanel>().AddNuclide(gameObject.transform.localPosition + mp, RemoveNucleonsFromNuclide(mp));
                }
                nucleons.ForEach(n => n.GetComponent<NukleonScript>().massPoint = MassPointNormal);
                nps.GetComponent<NodelistPanelScript>().SetNodePosition(gameObject.transform.localPosition + pos1);
                ResetNormal(false);
                spawnDistance = CalculateSpawnDistance(Nuclide.NucleonCount);
            }
        }
    }

    private float CalculateSpawnDistance(int nucleons)
    {
        return Mathf.Sqrt((Sphere.transform.localScale.x * 10 * nucleons) / (4 * Mathf.PI));
    }

    private class MassPointDistanceComparer : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            var dx = (x.GetComponent<NukleonScript>().massPoint - x.transform.localPosition).magnitude;
            var dy = (y.GetComponent<NukleonScript>().massPoint - y.transform.localPosition).magnitude;
            return dx > dy ? -1 : (dx < dy ? 1 : 0);
        }
    }

    private class PositionDistanceComparer : IComparer<GameObject>
    {
        private Vector3 point;

        public PositionDistanceComparer(Vector3 point)
        {
            this.point = point;
        }

        public int Compare(GameObject x, GameObject y)
        {
            var dx = (x.transform.localPosition - point).magnitude;
            var dy = (y.transform.localPosition - point).magnitude;
            return dx > dy ? 1 : (dx < dy ? -1 : 0);
        }
    }
}
