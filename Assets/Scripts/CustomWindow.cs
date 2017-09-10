using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomWindow : MonoBehaviour {

    public GameObject Panel;
    public GraphicRaycaster Raycaster;
    public EscapeMenuScript EscapeMenuScript;

    [SerializeField]
    private List<WindowPanelBase> panels = new List<WindowPanelBase>();
    private int _selectedIndex = -1;
    private int selectedIndex
    {
        get
        {
            return _selectedIndex;
        }
        set
        {
            _selectedIndex = value < 0 ? -1 : (value >= panels.Count ? -1 : value);
            if (value == -1) panels.ForEach(p => p.gameObject.SetActive(false));
            else
            {
                panels.Where(p => p != panels.ElementAt(_selectedIndex)).ToList().ForEach(p => p.gameObject.SetActive(false));
                panels.ElementAt(_selectedIndex).gameObject.SetActive(true);
                Panel.transform.localPosition = panels.ElementAt(_selectedIndex).Position;
            }
        }
    }

    private RectTransform rectTransform;
    private bool followMouse = false;
    private Vector3 lastMousePos = Vector3.zero;

	void Awake () {
        rectTransform = Panel.GetComponent<RectTransform>();

        var list = new List<WindowPanelBase>();
        foreach (var prefab in panels)
        {
            var panel = Instantiate(prefab);
            panel.transform.SetParent(Panel.transform, false);
            panel.gameObject.SetActive(false);
            panel.callOnDeactivate = onWindowSelfDeactivate;
            list.Add(panel);
        }
        panels = list;
        selectedIndex = -1;
    }
	

	void Update () {
        if (selectedIndex != -1) {
            var rt = panels.ElementAt(selectedIndex).GetComponent<RectTransform>();
            if(rt.sizeDelta != rectTransform.sizeDelta) rectTransform.sizeDelta = rt.sizeDelta;
            if (!Panel.GetComponent<Image>().enabled) Panel.GetComponent<Image>().enabled = true;
            if (!GetComponent<Image>().enabled) GetComponent<Image>().enabled = true;
            Panel.GetComponent<Image>().color = panels.ElementAt(selectedIndex).BackgroundColor;
        }

        if(selectedIndex == -1)
        {
            if(GetComponent<Image>().enabled) GetComponent<Image>().enabled = false;
            if (Panel.GetComponent<Image>().enabled) Panel.GetComponent<Image>().enabled = false;
        }

        if (EscapeMenuScript.GetFirstOpenWindow() == UIWindow.CustomWindow)
        {
            if (Input.GetMouseButtonUp(0)) followMouse = false;

            var pe = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var hits = new List<RaycastResult>();
            Raycaster.Raycast(pe, hits);
            WindowPanelBase hit = null;
            hits.ForEach(res => hit = res.gameObject.GetComponentInParent<WindowPanelBase>() ?? hit);
            if (hit != null && panels.Contains(hit))
            {
                if (Input.GetMouseButtonDown(0)) followMouse = true;
            }

            if (followMouse)
            {
                var mouseDelta = Input.mousePosition - lastMousePos;
                Panel.transform.localPosition = panels[selectedIndex].Position += mouseDelta;
            }
        }

        lastMousePos = Input.mousePosition;
    }

    public T Activate<T>() where T : WindowPanelBase
    {
        if (selectedIndex != -1) return null;
        if (panels.Find(wpb => wpb.GetComponent<T>() != null) == null) return null;
        //print(panels.IndexOf(panels.FirstOrDefault(wpb => wpb.GetComponent<T>())));
        selectedIndex = panels.IndexOf(panels.FirstOrDefault(wpb => wpb.GetComponent<T>()));
        //print(selectedIndex);
        //print(panels.ElementAt(selectedIndex).ToString() + " | " + panels.ElementAt(selectedIndex).GetComponent<T>().ToString());
        return panels.ElementAt(selectedIndex).GetComponent<T>();
    }

    public void Deactivate()
    {
        if (selectedIndex == -1) return;
        panels[selectedIndex].ForceDeactivate();
        selectedIndex = -1;
    }

    private void onWindowSelfDeactivate(WindowPanelBase wpb)
    {
        var i = panels.IndexOf(wpb);
        if (selectedIndex == i) selectedIndex = -1;
    }

    public bool IsWindowOpen()
    {
        return selectedIndex >= 0;
    }

    public void ResetPosition()
    {
        rectTransform.sizeDelta = new Vector2(0, 0);
    }
}
