using UnityEngine;
using UnityEngine.UI;

public class DecayChainWindowPanel : WindowPanelBase {

    public InputField InputProtons;
    public InputField InputNeutrons;
    public InputField InputDecayTime;
    public InputField InputX;
    public InputField InputY;
    public InputField InputZ;
    public Toggle ToggleDeleteOuterNucleons;
    public Button BtnCancel;
    public Button BtnAccept;
    public Button BtnNuclideCard;

    private NuclideCardScript nuclideCard;
    private CFDPanel cfdpanel;
    
    void Start () {
        nuclideCard = GameObject.Find("EventSystem").GetComponent<PublicData>().NuclideCardScript;
        cfdpanel = GameObject.Find("EventSystem").GetComponent<PublicData>().CFDPanel;
        ResetPanel();
        BtnCancel.onClick.AddListener(() => { Deactivate(false); });
        BtnAccept.onClick.AddListener(() => { Deactivate(true); });
        BtnNuclideCard.onClick.AddListener(openNuclideCard);
	}

    public void Activate()
    {
        ResetPanel();
    }

    public void Deactivate(bool startDecayChain)
    {
        if (startDecayChain)
        {
            var pos = new Vector3(InputX.text != "" ? (InputX.text.StartsWith("-") ? (InputX.text.Length == 1 ? 0F : -float.Parse(InputX.text.Substring(1))) : float.Parse(InputX.text)) : 0F, InputY.text != "" ? (InputY.text.StartsWith("-") ? (InputY.text.Length == 1 ? 0F : -float.Parse(InputY.text.Substring(1))) : float.Parse(InputY.text)) : 0F, InputZ.text != "" ? (InputZ.text.StartsWith("-") ? (InputZ.text.Length == 1 ? 0F : -float.Parse(InputZ.text.Substring(1))) : float.Parse(InputZ.text)) : 0F);
            var decayTime = InputDecayTime.text != "" ? (float.Parse(InputDecayTime.text) < 0 ? 0F : float.Parse(InputDecayTime.text)) : 0F;
            var nuclide = new Nuclide(InputProtons.text != "" ? (int.Parse(InputProtons.text) < 0 ? 0 : int.Parse(InputProtons.text)) : 0, InputNeutrons.text != "" ? (int.Parse(InputNeutrons.text) < 0 ? 0 : int.Parse(InputNeutrons.text)) : 0);
            cfdpanel.ProcessDecayChain(nuclide, pos, decayTime, !ToggleDeleteOuterNucleons.isOn);
        }
        if (callOnDeactivate != null) callOnDeactivate.Invoke(this);
    }

    private void openNuclideCard()
    {
        nuclideCard.Activate((p,n) => {
            InputProtons.text = p.ToString();
            InputNeutrons.text = n.ToString();
        });
    }

    public override void ResetPanel()
    {
        InputProtons.text = 92.ToString();
        InputNeutrons.text = 143.ToString();
        InputDecayTime.text = 5.ToString();
        InputX.text = 0.ToString();
        InputY.text = 0.ToString();
        InputZ.text = 0.ToString();
        ToggleDeleteOuterNucleons.isOn = true;
    }
}
