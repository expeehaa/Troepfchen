using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NodelistPanelScript : MonoBehaviour {

    public InputField inputN;
    public InputField inputP;
    public InputField inputX;
    public InputField inputY;
    public InputField inputZ;

    public Button btnCreate;
    public Button btnDecay;
    public Button btnDecayChoice;
    public Button btnOpenNuclideCard;
    public Button btnNeutronFission;

    public GameObject nukleonNode;
	public GameObject NuclideCardPanel;
    public CustomWindow CustomWindow;

    private NuclideContainer nuclideContainer;

    void Start () {
        nuclideContainer = GameObject.Find("EventSystem").GetComponent<NuclideContainer>();
        btnCreate.onClick.AddListener(CreateNuclide);
        btnDecay.onClick.AddListener(OnMakeDecay);
        btnOpenNuclideCard.onClick.AddListener(OpenNuclideCard);
        btnDecayChoice.onClick.AddListener(OnDecayChoice);
        btnNeutronFission.onClick.AddListener(OnMakeNeutronFission);
    }

    void Update()
    {
        var pos = new Vector3(inputX.text != "" ? (inputX.text.StartsWith("-") ? (inputX.text.Length == 1 ? 0F : -float.Parse(inputX.text.Substring(1))) : float.Parse(inputX.text)) : 0F, inputY.text != "" ? (inputY.text.StartsWith("-") ? (inputY.text.Length == 1 ? 0F : -float.Parse(inputY.text.Substring(1))) : float.Parse(inputY.text)) : 0F, inputZ.text != "" ? (inputZ.text.StartsWith("-") ? (inputZ.text.Length == 1 ? 0F : -float.Parse(inputZ.text.Substring(1))) : float.Parse(inputZ.text)) : 0F);
        if (nukleonNode.transform.position != pos) nukleonNode.GetComponent<NukleonNodeScript>().ChangeWorldPosition(pos, true);

        var state = nukleonNode.GetComponent<NukleonNodeScript>().Gamestate;
        btnDecay.GetComponentInChildren<Text>().text = state == NukleonNodeScript.GameState.Normal ? "Zerfall" : "Reset";
        btnDecay.interactable = state != NukleonNodeScript.GameState.Fission;
        btnNeutronFission.interactable = nukleonNode.GetComponent<NukleonNodeScript>().CanDoNeutronInducedFission() || (nukleonNode.GetComponent<NukleonNodeScript>().Gamestate == NukleonNodeScript.GameState.Fission);
    }

    public void CreateNuclide()
    {
        nukleonNode.GetComponent<NukleonNodeScript>().Nuclide = new Nuclide(int.Parse(inputP.text), int.Parse(inputN.text));
    }

    public void OpenNuclideCard()
    {
		NuclideCardPanel.GetComponent<NuclideCardScript>().Activate(OnNuclideCardDeactivate);
    }

    public void OnNuclideCardDeactivate(int p, int n)
    {
        inputN.text = n.ToString();
        inputP.text = p.ToString();
    }

    public void OnDecayChoice()
    {
        var cdcwp = CustomWindow.Activate<CreativeDecayChoiceWindowPanel>();
        if(cdcwp != null) cdcwp.Activate(OnGetCreativeDecay);
    }

    public void OnGetCreativeDecay(string s)
    {
        //print(s);
        nukleonNode.GetComponent<NukleonNodeScript>().Decay = s;
    }

    public void OnMakeDecay()
    {
        if (nukleonNode.GetComponent<NukleonNodeScript>().Gamestate == NukleonNodeScript.GameState.Normal) nukleonNode.GetComponent<NukleonNodeScript>().CreateRandomPredefinedDecay();
        else if (nukleonNode.GetComponent<NukleonNodeScript>().Gamestate == NukleonNodeScript.GameState.Decay) nukleonNode.GetComponent<NukleonNodeScript>().Decay = string.Empty;
    }

    public void OnMakeNeutronFission()
    {
        if (!nukleonNode.GetComponent<NukleonNodeScript>().CanDoNeutronInducedFission()) return;

        if (nukleonNode.GetComponent<NukleonNodeScript>().Gamestate == NukleonNodeScript.GameState.Fission) nukleonNode.GetComponent<NukleonNodeScript>().ResetNormal();
        else
        {
            var fwp = CustomWindow.Activate<FissionWindowPanel>();
            if (fwp != null) fwp.Activate(fd => nukleonNode.GetComponent<NukleonNodeScript>().CreateNeutronInducedFission(fd), NeutronFissionProductYield.FissionNuclides.FirstOrDefault(fn => GetNuclide().ProtonCount == fn.Protons && GetNuclide().NucleonCount == fn.Mass));
        }
    }

    public void SetNodePosition(Vector3 pos)
    {
        inputX.text = pos.x.ToString();
        inputY.text = pos.y.ToString();
        inputZ.text = pos.z.ToString();
    }

    public Vector3 GetNodePosition()
    {
        return nukleonNode.transform.localPosition;
    }

    public void ChangeNuclideType(Nuclide nuclide)
    {
        inputN.text = nuclide.NeutronCount.ToString();
        inputP.text = nuclide.ProtonCount.ToString();
    }

    public Nuclide GetNuclide()
    {
        return nukleonNode.GetComponent<NukleonNodeScript>().Nuclide;
    }

    public void Remove()
    {
        nuclideContainer.RemoveNodePanel(gameObject);
        Destroy(nukleonNode);
        Destroy(gameObject);
    }
}
