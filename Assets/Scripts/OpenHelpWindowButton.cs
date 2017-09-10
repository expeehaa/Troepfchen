using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenHelpWindowButton : MonoBehaviour {

    public CustomWindow CustomWindow;

	void Start () {
        GetComponent<Button>().onClick.AddListener(() => CustomWindow.Activate<HelpWindowPanel>());
	}
}
