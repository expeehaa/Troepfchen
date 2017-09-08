using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NuclideContainer : MonoBehaviour {

    public Camera cam;

	private List<GameObject> nucleonNodePanels = new List<GameObject>();
    public List<GameObject> NucleonNodePanels
    {
        get
        {
            return nucleonNodePanels;
        }
    }

    public bool shaderWithMassPoint = false;
    public Color NodelistPanelColorDefault;
    public Color NodelistPanelColorSelected;

    void Update()
    {
        var CIray = cam.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(CIray);
        GameObject hit = null;
        GameObject sphereHit = null;
        foreach (var h in hits)
        {
            if (h.collider.gameObject.GetComponentInParent<NukleonNodeScript>() != null && h.collider.gameObject.GetComponent<NukleonScript>() != null)
            {
                hit = h.collider.gameObject.GetComponentInParent<NukleonNodeScript>().gameObject;
                sphereHit = h.collider.gameObject;
                break;
            }
        }

        if (hit != null && sphereHit != null)
        {
            if (shaderWithMassPoint) hit.GetComponent<NukleonNodeScript>().AddOutlineShaderAtMassPoint(sphereHit.GetComponent<NukleonScript>().massPoint);
            else hit.GetComponent<NukleonNodeScript>().AddOutlineShader();
            nucleonNodePanels.Where(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode.GetInstanceID() != hit.GetInstanceID()).ToList().ForEach(nnp => { nnp.GetComponent<NodelistPanelScript>().nukleonNode.GetComponent<NukleonNodeScript>().RemoveOutlineShader(); nnp.GetComponent<Image>().color = NodelistPanelColorDefault; });
            nucleonNodePanels.FirstOrDefault(nnp => nnp.GetComponent<NodelistPanelScript>().nukleonNode.GetInstanceID() == hit.GetInstanceID()).GetComponent<Image>().color = NodelistPanelColorSelected;
        }
        else nucleonNodePanels.ForEach(nnp => { nnp.GetComponent<NodelistPanelScript>().nukleonNode.GetComponent<NukleonNodeScript>().RemoveOutlineShader(); nnp.GetComponent<Image>().color = NodelistPanelColorDefault; });
    }

    public void AddNodePanel(GameObject go)
    {
        if (go.GetComponent<NodelistPanelScript>() != null) nucleonNodePanels.Add(go);
    }

    public void RemoveNodePanel(GameObject go)
    {
        if (nucleonNodePanels.Contains(go)) nucleonNodePanels.Remove(go);
    }
}
