using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FissionWindowPanel : WindowPanelBase {

    public Toggle ToggleRandom;
    public Dropdown DropdownNeutrons;
    public GameObject PanelProductSelection;
    public Dropdown DropdownNeutronEnergy;
    public Dropdown DropdownProduct;
    public Toggle ToggleSeperateNuclei;
    public Button BtnDeny;
    public Button BtnAccept;

    private UnityAction<FissionData> callOnEnd = null;
    private FissionNuclide fissionNuclide = null;
    private System.Random rnd = new System.Random();
    private Dictionary<YieldEntry, string> neutronEnergiesDic = new Dictionary<YieldEntry, string>();

    void Start()
    {
        ToggleRandom.onValueChanged.AddListener(onToggleRandomProductChanged);
        DropdownNeutronEnergy.onValueChanged.AddListener(onSelectedNeutronEnergyChanged);
        BtnDeny.onClick.AddListener(() => Deactivate(false));
        BtnAccept.onClick.AddListener(() => Deactivate(true));
    }

    public override void ResetPanel()
    {
        neutronEnergiesDic.Clear();
        ToggleRandom.isOn = true;
        ToggleSeperateNuclei.isOn = true;
        PanelProductSelection.SetActive(!ToggleRandom.isOn);

        DropdownNeutrons.ClearOptions();
        DropdownNeutronEnergy.ClearOptions();
        DropdownProduct.ClearOptions();

        DropdownNeutrons.Hide();
        DropdownNeutronEnergy.Hide();
        DropdownProduct.Hide();

        DropdownNeutrons.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("2-3"), new Dropdown.OptionData("2"), new Dropdown.OptionData("3") });
        DropdownNeutrons.value = 0;
        if (fissionNuclide != null)
        {
            var neutronEnergyOptions = new List<Dropdown.OptionData>();
            foreach (var ye in fissionNuclide.ProductYields)
            {
                var s = ye.NeutronEnergy == 0 ? "Unabhängig" : ye.NeutronEnergy.ToString();
                neutronEnergiesDic.Add(ye, s);
                neutronEnergyOptions.Add(new Dropdown.OptionData(s));
            }
            //fissionNuclide.ProductYields.ForEach(ye => neutronEnergiesDic.Add(ye, ye.NeutronEnergy == 0 ? "Unabhängig" : ye.NeutronEnergy.ToString()));
            //neutronEnergiesDic.Values.ToList().ForEach(s => neutronEnergyOptions.Add(new Dropdown.OptionData(s)));
            DropdownNeutronEnergy.AddOptions(neutronEnergyOptions);
            DropdownNeutronEnergy.value = 0;
            onSelectedNeutronEnergyChanged(0);
        }
    }

    public void Activate(UnityAction<FissionData> callOnDeactivate, FissionNuclide fissionNuclide)
    {
        callOnEnd = callOnDeactivate;
        this.fissionNuclide = fissionNuclide;
        ResetPanel();
    }

    public void Deactivate(bool doCall)
    {
        if (doCall) callOnEnd.Invoke(getFissionProduct());
        fissionNuclide = null;
        ResetPanel();
        if (callOnDeactivate != null) callOnDeactivate.Invoke(this);
    }

    private void onToggleRandomProductChanged(bool on)
    {
        PanelProductSelection.SetActive(!on);
    }

    private void onSelectedNeutronEnergyChanged(int value)
    {
        DropdownNeutronEnergy.Hide();
        var text = DropdownNeutronEnergy.options[DropdownNeutronEnergy.value].text;
        var yieldentry = neutronEnergiesDic.Where(ne => ne.Value == text).FirstOrDefault().Key;
        var options = new List<Dropdown.OptionData>();
        yieldentry.ProductYield.ForEach(fp => options.Add(new Dropdown.OptionData(fp.ToString())));
        DropdownProduct.ClearOptions();
        DropdownProduct.AddOptions(options);
    }

    private FissionData getFissionProduct()
    {
        var fd = new FissionData()
        {
            neutronCount = DropdownNeutrons.options[DropdownNeutrons.value].text == "2-3" ? rnd.Next(2, 3) : (DropdownNeutrons.options[DropdownNeutrons.value].text == "2" ? 2 : 3),
            seperateNuclei = ToggleSeperateNuclei.isOn
        };

        if (ToggleRandom.isOn)
        {
            var ye = NeutronFissionProductYield.FissionNuclides.FirstOrDefault(fn => fn.ToString() == fissionNuclide.ToString()).ProductYields[rnd.Next(0, fissionNuclide.ProductYields.Count - 2)];
            fd.product = ye.ProductYield[rnd.Next(0, ye.ProductYield.Count - 1)];
        }
        else
        {
            var text1 = DropdownNeutronEnergy.options[DropdownNeutronEnergy.value].text;
            var text2 = DropdownProduct.options[DropdownProduct.value].text;
            fd.product = neutronEnergiesDic.FirstOrDefault(kv => kv.Value == text1).Key.ProductYield.FirstOrDefault(fp => fp.Equals(text2));
        }
        return fd;
    }
}
