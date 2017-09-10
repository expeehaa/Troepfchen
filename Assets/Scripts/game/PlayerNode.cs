using UnityEngine;
using UnityEngine.UI;

public class PlayerNode : MonoBehaviour {
    
    public Camera PlayerCamera;
    public ArenaBoundaries ArenaBoundaries;
    public Scrollbar SelectionScrollbar;
    public GameObject CameraNode;
    public KeyCode SprintKey;
    public KeyCode ResetKey;
    public KeyCode FireKey;

    public float Speed = 2f;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

        if (!Data.Paused)
        {
            if (Input.GetKeyDown(ResetKey)) GetComponent<NucleonNode>().spawnNukleons(GetComponent<NucleonNode>().GameAtomData);

            var number = Mathf.RoundToInt(SelectionScrollbar.value * (SelectionScrollbar.numberOfSteps - 1));
            if (Input.GetKey(FireKey))
            {
                GetComponent<NucleonNode>().Shoot((Throwable)number, PlayerCamera.transform.forward);
            }

            var moveSidewards = Input.GetAxis("Horizontal");
            var moveUp = Input.GetAxis("Jump");
            var moveForwards = Input.GetAxis("Vertical");

            var forward = PlayerCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            var sidewards = Vector3.Cross(Vector3.up, forward);
            sidewards.Normalize();
            var dir = forward * moveForwards + sidewards * moveSidewards + Vector3.up * moveUp;
            dir.Normalize();
            transform.localPosition = fixPositionInBoundaries(transform.localPosition + dir * Speed * (Input.GetKey(SprintKey) ? 2 : 1));
            

            var scrollwheel = Input.GetAxis("Mouse ScrollWheel");
            var value = number + (scrollwheel < 0 ? 1 : scrollwheel > 0 ? -1 : 0);
            //if ((value < 0 ? (SelectionScrollbar.numberOfSteps - 1) : (value > (SelectionScrollbar.numberOfSteps - 1) ? 0 : value)) != value) print(value + " -> " + (value < 0 ? (SelectionScrollbar.numberOfSteps - 1) : (value > (SelectionScrollbar.numberOfSteps - 1) ? 0 : value)));
            value = value < 0 ? (SelectionScrollbar.numberOfSteps - 1) : (value > (SelectionScrollbar.numberOfSteps - 1) ? 0 : value);
            SelectionScrollbar.value = value / (float)(SelectionScrollbar.numberOfSteps - 1);
        }
    }

    #region Helper
    
    private Vector3 fixPositionInBoundaries(Vector3 pos)
    {
        var size = GetComponent<NucleonNode>().Size;
        pos.x = pos.x + size > ArenaBoundaries.Dimensions.x / 2 - ArenaBoundaries.Thickness / 2 ? ArenaBoundaries.Dimensions.x / 2 - ArenaBoundaries.Thickness / 2 - size : (pos.x - size < -ArenaBoundaries.Dimensions.x / 2 + ArenaBoundaries.Thickness / 2 ? -ArenaBoundaries.Dimensions.x / 2 + ArenaBoundaries.Thickness / 2 + size : pos.x);
        pos.y = pos.y - size < ArenaBoundaries.Thickness / 2 ? size + ArenaBoundaries.Thickness / 2 : pos.y;
        pos.z = pos.z + size > ArenaBoundaries.Dimensions.z / 2 - ArenaBoundaries.Thickness / 2 ? ArenaBoundaries.Dimensions.z / 2 - ArenaBoundaries.Thickness / 2 - size : (pos.z - size < -ArenaBoundaries.Dimensions.z / 2 + ArenaBoundaries.Thickness / 2 ? -ArenaBoundaries.Dimensions.z / 2 + ArenaBoundaries.Thickness / 2 + size : pos.z);
        return pos;
    }

    #endregion
}
