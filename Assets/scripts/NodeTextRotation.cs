using UnityEngine;

public class NodeTextRotation : MonoBehaviour {

    public CameraScript CamScript;

	void Update () {
        this.transform.forward = CamScript.transform.forward;
	}
}
