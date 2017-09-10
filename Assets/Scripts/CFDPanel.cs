using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CFDPanel : DelayedActionScript {

    public Button BtnExpand;
    public Button BtnCreateNuclide;
    public Button BtnFusion;
    public Button BtnDivide;
    public Button BtnAcceptInner;
    public Button BtnDecayChain;
    public Button BtnReset;

    public GameObject PanelButtons;
    public GameObject ScrollViewerContent;
    public GameObject panelPrefab;
    public GameObject nukleonNodePrefab;
    public GameObject NuclideCardPanel;
    public CustomWindow CustomWindow;
    public CameraScript CamScript;
    public GameObject NucleiNode;
    public GraphicRaycaster Raycaster;
    public Image ImgExpander;

    public Sprite SpriteArrowLeft;
    public Sprite SpriteArrowRight;

    public KeyCode KeyDelete;

    private NuclideContainer nuclideContainer;
    private SelectedNode[] selectedNodes = null;
    private int currentPos = 0;
    private Camera cam;
    private bool expanded = true;
    
    private NuclideChangeStates state = NuclideChangeStates.Nothing;

    // Use this for initialization
    void Start()
    {
        nuclideContainer = GameObject.Find("EventSystem").GetComponent<NuclideContainer>();
        cam = CamScript.GetComponent<Camera>();
        BtnExpand.onClick.AddListener(onExpanderClicked);
        BtnCreateNuclide.onClick.AddListener(addNuclideHandler);
        BtnFusion.onClick.AddListener(fusionNodes1);
        BtnDivide.onClick.AddListener(divideNodes1);
        BtnAcceptInner.onClick.AddListener(acceptInner1);
        BtnDecayChain.onClick.AddListener(activateDecayChainPanel);
        BtnReset.onClick.AddListener(removeAllNuclideNodes);
        SetButtonsActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyDelete))
        {
            var pe = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var hits = new List<RaycastResult>();
            Raycaster.Raycast(pe, hits);
            if(hits.Count > 0)
            {
                var nps = hits[0].gameObject.GetComponentInParent<NodelistPanelScript>();
                if (nps != null) nps.Remove();
            }
        }

        if (selectedNodes != null && selectedNodes.Length > 0 && currentPos < selectedNodes.Length && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)))
        {
            var CIray = cam.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(CIray);
            GameObject hit = null;
            var massPoint = Vector3.zero;
            foreach (var h in hits)
            {
                if (h.collider.gameObject.GetComponentInParent<NukleonNodeScript>() != null && h.collider.gameObject.GetComponent<NukleonScript>() != null)
                {
                    hit = h.collider.gameObject.GetComponentInParent<NukleonNodeScript>().gameObject;
                    massPoint = h.collider.gameObject.GetComponent<NukleonScript>().massPoint;
                    break;
                }
            }
            if (hit != null)
            {
                selectedNodes[currentPos] = new SelectedNode(nuclideContainer.NucleonNodePanels.First(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode == hit.GetComponent<NukleonNodeScript>().gameObject), massPoint);
                currentPos++;
            }
            if (currentPos >= selectedNodes.Length)
            {
                if (state == NuclideChangeStates.Fusion) fusionNodes2();
                else if (state == NuclideChangeStates.Division) divideNodes2();
                else if(state == NuclideChangeStates.AcceptInner)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0)) acceptInner2(false);
                    else if (Input.GetKeyDown(KeyCode.Mouse1)) acceptInner2(true);
                }
            }
        }
    }

    public NodelistPanelScript AddNuclide()
    {
        var obj = Instantiate(panelPrefab);
        var node = Instantiate(nukleonNodePrefab);
        node.GetComponentInChildren<NodeTextRotation>().CamScript = CamScript;
        node.transform.SetParent(NucleiNode.transform, false);
        obj.GetComponent<NodelistPanelScript>().CustomWindow = CustomWindow;
        obj.GetComponent<NodelistPanelScript>().nukleonNode = node;
        obj.GetComponent<NodelistPanelScript>().NuclideCardPanel = NuclideCardPanel;
        obj.transform.SetParent(ScrollViewerContent.transform, false);
        nuclideContainer.AddNodePanel(obj);
        return obj.GetComponent<NodelistPanelScript>();
    }

    public NodelistPanelScript AddNuclide(Vector3 pointInWorldSpace, List<GameObject> nucleons = null)
    {
        var node = Instantiate(nukleonNodePrefab);
        node.GetComponentInChildren<NodeTextRotation>().CamScript = CamScript;
        node.transform.SetParent(NucleiNode.transform, false);
        var obj = Instantiate(panelPrefab);
        obj.transform.SetParent(ScrollViewerContent.transform, false);
        var nps = obj.GetComponent<NodelistPanelScript>();
        nps.CustomWindow = CustomWindow;
        nps.NuclideCardPanel = NuclideCardPanel;
        nps.nukleonNode = node;
        nuclideContainer.AddNodePanel(obj);
        InvokeLater(() =>
        {
            if (nucleons != null) node.GetComponent<NukleonNodeScript>().AdoptNucleons(nucleons);
            nps.SetNodePosition(pointInWorldSpace);
        }, Time.fixedDeltaTime);
        return obj.GetComponent<NodelistPanelScript>();
    }

    #region handlers

    private void addNuclideHandler()
    {
        AddNuclide();
    }

    private void removeAllNuclideNodes()
    {
        var panels = nuclideContainer.NucleonNodePanels.ToArray();
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].GetComponent<NodelistPanelScript>().Remove();
        }
    }

    private void fusionNodes1()
    {
        currentPos = 0;
        selectedNodes = new SelectedNode[2];
        SetButtonsActive(false);
        nuclideContainer.shaderWithMassPoint = false;
        state = NuclideChangeStates.Fusion;
    }

    private void fusionNodes2()
    {
        var l = selectedNodes.Length;
        if (l == 1) return;
        else if (l == 2)
        {
            ProcessFusion(selectedNodes[0], selectedNodes[1]);
        }
        ResetNodeSearch();
    }

    private void divideNodes1()
    {
        currentPos = 0;
        selectedNodes = new SelectedNode[1];
        SetButtonsActive(false);
        nuclideContainer.shaderWithMassPoint = true;
        state = NuclideChangeStates.Division;
    }

    private void divideNodes2()
    {
        if(selectedNodes[0].MassPoint == selectedNodes[0].Node.GetComponent<NodelistPanelScript>().nukleonNode.GetComponent<NukleonNodeScript>().MassPointNormal)
        {
            ResetNodeSearch();
            divideNodes1();
        }
        else
        {
            ProcessDivision(selectedNodes[0]);
            ResetNodeSearch();
        }
    }

    private void acceptInner1()
    {
        currentPos = 0;
        selectedNodes = new SelectedNode[1];
        SetButtonsActive(false);
        nuclideContainer.shaderWithMassPoint = false;
        state = NuclideChangeStates.AcceptInner;
    }

    private void acceptInner2(bool deleteOuterNucleons)
    {
        ProcessAcceptInner(selectedNodes[0], deleteOuterNucleons);
        ResetNodeSearch();
    }

    private void activateDecayChainPanel()
    {
        var dcwp = CustomWindow.Activate<DecayChainWindowPanel>();
        if (dcwp != null) dcwp.Activate();
    }

    private void onExpanderClicked()
    {
        expanded = !expanded;
        ImgExpander.sprite = expanded ? SpriteArrowLeft : SpriteArrowRight;
        PanelButtons.SetActive(expanded);
    }

    #endregion

    #region Processes

    public void ProcessFusion(SelectedNode node1, SelectedNode node2)
    {
        var panel1 = node1.Node.GetComponent<NodelistPanelScript>();
        var panel2 = node2.Node.GetComponent<NodelistPanelScript>();
        var pos1 = panel1.GetNodePosition();
        var pos2 = panel2.GetNodePosition();
        var nukleonNode1 = panel1.nukleonNode.GetComponent<NukleonNodeScript>();
        var nukleonNode2 = panel2.nukleonNode.GetComponent<NukleonNodeScript>();
        if (node1 != node2)
        {
            nukleonNode1.ResetDecay();
            nukleonNode2.ResetDecay();
            var pos = (((pos2 - pos1) * nukleonNode2.Nuclide.NucleonCount) / (nukleonNode1.Nuclide.NucleonCount + nukleonNode2.Nuclide.NucleonCount)) + pos1;
            panel1.SetNodePosition(pos);
            nukleonNode1.AdoptNucleons(nukleonNode2.Nucleons);
            panel2.Remove();
        }
    }
    
    public void ProcessDivision(SelectedNode node)
    {
        var panel = node.Node.GetComponent<NodelistPanelScript>();
        var massPoint = node.MassPoint;
        var nucleons = panel.nukleonNode.GetComponent<NukleonNodeScript>().RemoveNucleonsFromNuclide(massPoint);
        AddNuclide(panel.nukleonNode.transform.localPosition + massPoint, nucleons);
    }

    public void ProcessAcceptInner(SelectedNode node, bool deleteOuterNucleons)
    {
        var panel = node.Node.GetComponent<NodelistPanelScript>();
        var nukleonNode = panel.nukleonNode.GetComponent<NukleonNodeScript>();
        var massPoints = nukleonNode.GetMassPoints();
        foreach (var massPoint in massPoints.Where(mp => mp != Vector3.zero))
        {
            var nucleons = nukleonNode.RemoveNucleonsFromNuclide(massPoint);
            if (!deleteOuterNucleons) AddNuclide(nukleonNode.transform.localPosition + massPoint, nucleons);
            else nucleons.ForEach(n => Destroy(n));
        }
        nukleonNode.AcceptInnerNucleons();
        nukleonNode.ResetDecay();
    }

    public void ProcessDecayChain(Nuclide nuclide, Vector3 pos, float time, bool deleteOuterNucleons)
    {
        var nps = AddNuclide(pos);
        nps.ChangeNuclideType(nuclide);
        StartCoroutine("decayChainCoroutine", new DecayChainArguments(nps, pos, time, deleteOuterNucleons));
    }

    private IEnumerator decayChainCoroutine(object dcargs)
    {
        var args = (DecayChainArguments)dcargs;
        args.NodelistPanelScript.CreateNuclide();
        yield return new WaitForSeconds(args.Time);
        while (args.NodelistPanelScript.nukleonNode.GetComponent<NukleonNodeScript>().CreateRandomPredefinedDecay())
        {
            yield return new WaitForSeconds(args.Time);
            ProcessAcceptInner(new SelectedNode(args.NodelistPanelScript.gameObject, Vector3.zero), args.DeleteOuterNucleons);
            yield return new WaitForSeconds(args.Time);
        }
    }

    #endregion

    private void SetButtonsActive(bool active)
    {
        BtnFusion.GetComponent<Button>().interactable = active;
        BtnDivide.GetComponent<Button>().interactable = active;
        BtnAcceptInner.GetComponent<Button>().interactable = active;
    }

    public bool IsSearchModeActive()
    {
        return selectedNodes != null;
    }

    public void ResetNodeSearch()
    {
        selectedNodes = null;
        SetButtonsActive(true);
        nuclideContainer.shaderWithMassPoint = false;
        state = NuclideChangeStates.Nothing;
    }

    public class SelectedNode
    {
        public GameObject Node;
        public Vector3 MassPoint;

        public SelectedNode(GameObject node, Vector3 massPoint)
        {
            this.Node = node;
            this.MassPoint = massPoint;
        }
    }

    private class DecayChainArguments
    {
        public NodelistPanelScript NodelistPanelScript;
        public Vector3 Position;
        public float Time;
        public bool DeleteOuterNucleons;

        public DecayChainArguments(NodelistPanelScript nps, Vector3 pos, float time, bool deleteOuterNucleons)
        {
            NodelistPanelScript = nps;
            Position = pos;
            Time = time;
            DeleteOuterNucleons = deleteOuterNucleons;
        }
    }
}
