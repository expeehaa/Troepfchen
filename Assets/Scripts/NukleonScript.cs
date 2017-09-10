using UnityEngine;
using UnityEngine.Events;

public class NukleonScript : MonoBehaviour {
    
    public Vector3 massPoint;

    private static float _speed = 2f;

    public static float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            if (value < -1) _speed = -1;
            else if (value > 10) _speed = 10;
            else _speed = value;
        }
    }
    
    public enum NucleonType
    {
        Proton, Neutron
    }

    public bool fast = false;

    public NucleonType nucleonType = NucleonType.Proton;
    public NucleonType fakeType = NucleonType.Proton;

    private static bool pause = false;

    public UnityAction<NukleonScript, Collision> onCollisionEnterFunc = null;

    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = this.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.P)) pause = true;
        else pause = false;

        if (pause) rb.Sleep();
        else
        {
            var dir = massPoint - transform.localPosition;
            dir.Normalize();
            //rb.angularVelocity = Vector3.zero;
            if (massPoint == Vector3.zero) rb.AddForce(dir * speed * (fast ? 4 : 1), ForceMode.Impulse);
            else rb.AddForce(dir * speed * (fast ? 4 : 1), ForceMode.VelocityChange);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (onCollisionEnterFunc != null) onCollisionEnterFunc.Invoke(this, collision);
    }
}
