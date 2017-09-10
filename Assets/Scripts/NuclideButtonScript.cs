using UnityEngine;
using UnityEngine.UI;

public class NuclideButtonScript : MonoBehaviour {

    public Nuclide nuclide;

    void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(onClick);
    }

    void onClick()
    {
        this.gameObject.GetComponentInParent<NuclideCardScript>().Deactivate(nuclide);
    }
}
