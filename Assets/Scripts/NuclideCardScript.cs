using Assets.scripts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NuclideCardScript : MonoBehaviour {

    public Button BtnPrefab;
    public GameObject Panel;
    public float ZoomFactor = 3;
    public EscapeMenuScript EscapeMenuScript;
    
    private float _zoom = 0;
    private float zoom
    {
        get
        {
            return _zoom;
        }
        set
        {
            if (value == _zoom) return;
            var oldZoom = _zoom;
            if (value < 0) _zoom = 0;
            else _zoom = value;
            nuclidebuttons.ForEach(btn =>
             {
                 var pos = btn.transform.localPosition;
                 btn.transform.localPosition = new Vector3((pos.x) * ((zoom + 1) / (oldZoom + 1)), (pos.y) * ((zoom + 1) / (oldZoom + 1)), pos.z);
                 btn.GetComponent<RectTransform>().sizeDelta *= (zoom + 1) / (oldZoom + 1);
             });
        }
    }

    private float baseHeight, baseWidth;
    private int maxP, maxA;
    private Vector3 lastMousePos = Vector3.zero;
    private bool followMouse = false;
    private List<Button> nuclidebuttons = new List<Button>();
    private UnityAction<int, int> call = null;

    void Awake () {
        NuclideCard.RetrieveData();
        NeutronFissionProductYield.RetrieveData();
        maxP = NuclideCard.Nuclides.OrderByDescending(n => n.ProtonCount).First().ProtonCount;
        maxA = NuclideCard.Nuclides.OrderByDescending(n => n.NucleonCount).First().NucleonCount;
        baseHeight = this.GetComponent<RectTransform>().rect.height / (maxP + 1);
        baseWidth = this.GetComponent<RectTransform>().rect.width / maxA;

        foreach (var nuclide in NuclideCard.Nuclides)
        {
            var btn = Instantiate(BtnPrefab);

            btn.GetComponentInChildren<Text>().text = nuclide.ShortName + "\nA:" + nuclide.NucleonCount + "\nZ:" + nuclide.ProtonCount;
            btn.transform.SetParent(Panel.transform);

            btn.transform.localPosition = new Vector3((float)(nuclide.NucleonCount - 0.5) * baseWidth * (zoom + 1), (float)(nuclide.ProtonCount + 0.5) * baseHeight * (zoom + 1), 0);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(baseWidth * (zoom + 1), baseHeight * (zoom + 1));

            var nbtnScript = btn.GetComponent<NuclideButtonScript>();
            nbtnScript.nuclide = new Nuclide(nuclide.ProtonCount, nuclide.NeutronCount);
            nuclidebuttons.Add(btn);
        }
		Deactivate();
	}

    void OnRectTransformDimensionsChange()
    {
        changeSize();
    }

    private void OnPreRender()
    {
        changeSize();
    }

    private void changeSize()
    {
        var h = this.GetComponent<RectTransform>().rect.height / (maxP + 1);
        var w = this.GetComponent<RectTransform>().rect.width / maxA;
        if (h != baseHeight || w != baseWidth) nuclidebuttons.ForEach(btn =>
            {
                btn.transform.localPosition = new Vector3((float)(btn.GetComponent<NuclideButtonScript>().nuclide.NucleonCount - 0.5) * baseWidth * (zoom + 1), (float)(btn.GetComponent<NuclideButtonScript>().nuclide.ProtonCount + 0.5) * baseHeight * (zoom + 1), 0);
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(baseWidth * (zoom + 1), baseHeight * (zoom + 1));
            });
    }

    void Update()
    {
        if(EscapeMenuScript.GetFirstOpenWindow() == UIWindow.NuclideCard)
        {
            if (Input.GetMouseButtonDown(0)) followMouse = true;
            if (Input.GetMouseButtonUp(0)) followMouse = false;

            zoom += Input.GetAxis("Mouse ScrollWheel") * ZoomFactor;
            var mouseDelta = followMouse ? Input.mousePosition - lastMousePos : Vector3.zero;
            Panel.transform.localPosition += mouseDelta;
        }
        
        lastMousePos = Input.mousePosition;
    }

    public void Activate(UnityAction<int, int> call)
    {
        if (gameObject.activeSelf) return;
        this.gameObject.SetActive(true);
        this.call = call;
        lastMousePos = Input.mousePosition;
        followMouse = false;
    }

    public void Deactivate(Nuclide nuclide = null)
    {
        this.gameObject.SetActive(false);
        if (nuclide != null && call != null) call.Invoke(nuclide.ProtonCount, nuclide.NeutronCount);

        zoom = 0;
        Panel.transform.localPosition = Vector3.zero;
        foreach (var btn in nuclidebuttons)
        {
            var script = btn.GetComponent<NuclideButtonScript>();
            btn.transform.localPosition = new Vector3((float)(script.nuclide.NucleonCount - 0.5) * baseWidth * (zoom + 1), (float)(script.nuclide.ProtonCount + 0.5) * baseHeight * (zoom + 1), 0);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(baseWidth * (zoom + 1), baseHeight * (zoom + 1));
        }
    }

    public void ResetPanel()
    {
        zoom = 0;
        Panel.transform.localPosition = Vector3.zero;
        nuclidebuttons.ForEach(btn =>
        {
            var script = btn.GetComponent<NuclideButtonScript>();
            btn.transform.localPosition = new Vector3((float)(script.nuclide.NucleonCount - 0.5) * baseWidth * (zoom + 1), (float)(script.nuclide.ProtonCount + 0.5) * baseHeight * (zoom + 1), 0);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(baseWidth * (zoom + 1), baseHeight * (zoom + 1));
        });
    }

    public bool IsNuclideCardActive()
    {
        return this.gameObject.activeSelf;
    }
}
