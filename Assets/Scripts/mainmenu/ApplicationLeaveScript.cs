using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationLeaveScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(onClick);
	}

    void onClick(){
        Application.Quit();
    }
}
