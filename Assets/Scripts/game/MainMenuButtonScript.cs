using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(onClick);
	}
	
    void onClick(){
        SceneManager.LoadScene("mainmenu");
    }
}
