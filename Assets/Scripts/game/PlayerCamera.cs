using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    public NucleonNode NucleonNode;
    public Camera Cam;
    public KeyCode ZoomKey;
    public float Sensitivity = 7f;

    private float rotX = 90;
    private float rotY = 0;
    public float MinX = -360;
    public float MaxX = 360;
    public float MinY = -90;
    public float MaxY = 90;

    private bool zoomPressed = false;

    // Use this for initialization
    void Start () {
		transform.rotation = Quaternion.AngleAxis(rotX, Vector3.up) * Quaternion.AngleAxis(rotY, Vector3.left);
	}
	
	// Update is called once per frame
	void Update () {
        if (!Data.Paused)
        {
            rotX = Clamp(rotX + Input.GetAxis("Mouse X") * Sensitivity, MinX, MaxX);
            rotY = Clamp(rotY + Input.GetAxis("Mouse Y") * Sensitivity, MinY, MaxY);
            transform.rotation = Quaternion.AngleAxis(rotX, Vector3.up) * Quaternion.AngleAxis(rotY, Vector3.left);

            var pos = Cam.transform.localPosition;
            pos.Normalize();
            Cam.transform.localPosition = pos * NucleonNode.Size * (Input.GetKey(ZoomKey) && !zoomPressed ? -2 : (!Input.GetKey(ZoomKey) && zoomPressed ? -0.5f : 1));
            zoomPressed = Input.GetKey(ZoomKey);
        }
    }

    private float Clamp(float angle, float min, float max)
    {
        return Mathf.Clamp(angle + (angle < -360 ? 360 : (angle > 360 ? -360 : 0)), min, max);
    }
}
