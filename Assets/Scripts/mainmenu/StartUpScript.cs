using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PlayerPrefs.DeleteAll();
	}
}
