using UnityEngine;
using UnityEngine.UI;

public class SetSpeedTextScript : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        gameObject.GetComponent<Text>().text = (Mathf.Round(NukleonScript.speed * 100)/100).ToString();
	}
}
