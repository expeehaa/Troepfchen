using UnityEngine;

public class ArenaBoundaries : MonoBehaviour {

    public GameObject Bottom;
    public GameObject WallXp;
    public GameObject WallXm;
    public GameObject WallZp;
    public GameObject WallZm;

    private Vector3 oldDimensions;
    public Vector3 Dimensions;

    private float oldThickness;
    public float Thickness;
    
    void Start () {
        updateBoundaries(Dimensions, Thickness);
	}

    void Update()
    {
        if (Dimensions != oldDimensions || Thickness != oldThickness) updateBoundaries(Dimensions, Thickness);
        oldDimensions = Dimensions;
        oldThickness = Thickness;
    }

    void updateBoundaries(Vector3 dimensions, float thickness)
    {
        updateBoundaries(dimensions.x, dimensions.z, dimensions.y, thickness);
    }

    void updateBoundaries(float width, float length, float height, float thickness)
    {
        Bottom.transform.localScale = new Vector3(width, thickness, length);
        Bottom.transform.localPosition = new Vector3(0, -thickness/2, 0);

        WallXp.transform.localScale = new Vector3(thickness, height, width);
        WallXp.transform.localPosition = new Vector3((width / 2) - thickness/2, height / 2, 0);

        WallXm.transform.localScale = new Vector3(thickness, height, width);
        WallXm.transform.localPosition = new Vector3(-(width / 2) + thickness/2, height / 2, 0);

        WallZp.transform.localScale = new Vector3(length, height, thickness);
        WallZp.transform.localPosition = new Vector3(0, height / 2, (length / 2) - thickness/2);

        WallZm.transform.localScale = new Vector3(length, height, thickness);
        WallZm.transform.localPosition = new Vector3(0, height / 2, -(length / 2) + thickness/2);
    }
}
