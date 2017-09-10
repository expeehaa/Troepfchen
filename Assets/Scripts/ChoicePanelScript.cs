using UnityEngine;

public class ChoicePanelScript : MonoBehaviour {

    public GameObject PanelCluster;
    public GameObject PanelOther;

    private ChosenChoicePanel _chosenChoicePanel;
    public ChosenChoicePanel chosenChoicePanel
    {
        get
        {
            return _chosenChoicePanel;
        }
        set
        {
            if(value != _chosenChoicePanel)
            {
                if (value == ChosenChoicePanel.Cluster)
                {
                    PanelCluster.gameObject.SetActive(true);
                    PanelOther.gameObject.SetActive(false);
                }
                else if(value == ChosenChoicePanel.Other)
                {
                    PanelCluster.gameObject.SetActive(false);
                    PanelOther.gameObject.SetActive(true);
                }
            }
            _chosenChoicePanel = value;
            
        }
    }

    public string DecayString
    {
        get
        {
            if (chosenChoicePanel == ChosenChoicePanel.Cluster)
            {
                return PanelCluster.GetComponent<ClusterPanelScript>().GetDecayString();
            }
            else if (chosenChoicePanel == ChosenChoicePanel.Other)
            {
                return PanelOther.GetComponent<OtherDecayPanelScript>().GetDecayString();
            }
            else return string.Empty;
        }
    }

    public void ResetInputs()
    {
        PanelCluster.GetComponent<ClusterPanelScript>().ResetFields();
        PanelOther.GetComponent<OtherDecayPanelScript>().ResetFields();
    }
}
