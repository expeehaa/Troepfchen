using UnityEngine;

public class CameraScript : MonoBehaviour {

    public EscapeMenuScript ems;

    public float speed = 2f;

    public float sensitivity = 7f;

    float rotX = 0;
    float rotY = -16;
    public float minX = -360;
    public float maxX = 360;
    public float minY = -90;
    public float maxY = 90;

    private bool escaped = true;
	public GameObject NuclideCardPanel;

    void Start()
    {
        transform.rotation = Quaternion.AngleAxis(rotX, Vector3.up) * Quaternion.AngleAxis(rotY, Vector3.left);
    }

	void Update () {
        if (Input.mousePresent)
        {
            if (Input.GetKeyDown(KeyCode.X)) escaped = !escaped;

            if (!ems.Escaped)
            {
                if (!escaped)
                {
                    rotX = Clamp(rotX + Input.GetAxis("Mouse X") * sensitivity, minX, maxX);
                    rotY = Clamp(rotY + Input.GetAxis("Mouse Y") * sensitivity, minY, maxY);
                    transform.rotation = Quaternion.AngleAxis(rotX, Vector3.up) * Quaternion.AngleAxis(rotY, Vector3.left);
                }

                var moveSidewards = Input.GetAxis("Horizontal");
                var moveUp = Input.GetAxis("Jump");
                var moveForwards = Input.GetAxis("Vertical");

                var forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                var sidewards = Vector3.Cross(Vector3.up, forward);
                sidewards.Normalize();
                transform.position += (forward * moveForwards + sidewards * moveSidewards + Vector3.up * moveUp) * speed;
            }

            if (ems.Escaped) Cursor.lockState = CursorLockMode.None;
            else if (Cursor.lockState != CursorLockMode.Locked && !escaped) Cursor.lockState = CursorLockMode.Locked;
            else if (Cursor.lockState != CursorLockMode.None && escaped) Cursor.lockState = CursorLockMode.None;
        }

        if (Input.touchSupported)
        {
			if (!NuclideCardPanel.GetComponent<NuclideCardScript>().IsNuclideCardActive())
            {
                if (Input.touchCount == 2 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved))
                {
                    var touch1 = Input.GetTouch(0);
                    var touch2 = Input.GetTouch(1);
                    var forward = transform.forward;
                    forward.Normalize();

                    transform.position += forward * ((touch1.position - touch2.position).magnitude - ((touch1.position - touch1.deltaPosition) - (touch2.position - touch2.deltaPosition)).magnitude) / 10;
                }
            }
        }
    }

    private float Clamp(float angle, float min, float max)
    {
        return Mathf.Clamp(angle + (angle < -360 ? 360 : (angle > 360 ? -360 : 0)), min, max);
    }
}
