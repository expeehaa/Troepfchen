using UnityEngine;
using UnityEngine.UI;

public class ChangeSpeedScript : MonoBehaviour {

    public float change = 0f;
    public KeyCode key;
	
	// Update is called once per frame
	void Start() {
        this.gameObject.GetComponent<Button>().onClick.AddListener(changeSpeedValue);
	}

    private void Update()
    {
        if (Input.GetKeyDown(key)) NukleonScript.speed += change;
    }

    void changeSpeedValue()
    {
        NukleonScript.speed += change;
    }
}
