using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class EscapeMenuScript : MonoBehaviour {

    public Button btnSimulation;
    public Button btnMainMenu;
    public GameObject panel;
    public NuclideCardScript NuclideCardScript;
    public CustomWindow CustomWindow;
    public CFDPanel CFDPanel;
    public KeyCode KeyReset;

	[SerializeField]
	private bool escaped;
	public bool Escaped {
		get {
			return escaped;
		}
		set {
			if (escaped != value) callbacks.ForEach(cb => cb.Invoke(value));
            escaped = value;
		}
	}

	private List<UnityAction<bool>> callbacks = new List<UnityAction<bool>>();
	public List<UnityAction<bool>> Callbacks {
		get {
			return callbacks;
		}
	}

	// Use this for initialization
	void Start () {
        btnSimulation.onClick.AddListener(onBtnSimulation);
        btnMainMenu.onClick.AddListener(onBtnMainMenu);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Escaped) Escaped = false;
            else if (NuclideCardScript.IsNuclideCardActive()) NuclideCardScript.Deactivate();
            else if (CustomWindow.IsWindowOpen()) CustomWindow.Deactivate();
            else if (CFDPanel.IsSearchModeActive()) CFDPanel.ResetNodeSearch();
            else Escaped = true;
        }
        if (Input.GetKeyDown(KeyReset))
        {
            if (Escaped) { }
            else if (NuclideCardScript.IsNuclideCardActive()) NuclideCardScript.ResetPanel();
            else if (CustomWindow.IsWindowOpen()) CustomWindow.ResetPosition();
        }

        if (panel.activeSelf != Escaped) panel.SetActive(Escaped);
	}

    private void onBtnSimulation()
    {
        Escaped = false;
    }

    private void onBtnMainMenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    public UIWindow GetFirstOpenWindow()
    {
        return Escaped ? UIWindow.EscapeMenu : (NuclideCardScript.IsNuclideCardActive() ? UIWindow.NuclideCard : (CustomWindow.IsWindowOpen() ? UIWindow.CustomWindow : UIWindow.None));
    }
}
