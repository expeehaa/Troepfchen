using UnityEngine;
using UnityEngine.UI;

public class ClusterPanelScript : MonoBehaviour {

    public InputField InputProtons;
    public InputField InputNeutrons;

    public Nuclide Cluster
    {
        get
        {
            return new Nuclide(InputProtons.text == null || InputProtons.text.Equals(string.Empty) ? 0 : (int.Parse(InputProtons.text) < 0 ? 0 : int.Parse(InputProtons.text)), InputNeutrons.text == null || InputNeutrons.text.Equals(string.Empty) ? 0 : (int.Parse(InputNeutrons.text) < 0 ? 0 : int.Parse(InputNeutrons.text)));
        }
        set
        {
            InputProtons.text = value.ProtonCount.ToString();
            InputNeutrons.text = value.NeutronCount.ToString();
        }
    }

    public string GetDecayString()
    {
        return Cluster.ToString();
    }

    public void ResetFields()
    {
        InputProtons.text = string.Empty;
        InputNeutrons.text = string.Empty;
    }
}
