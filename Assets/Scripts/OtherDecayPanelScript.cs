using UnityEngine;
using UnityEngine.UI;

public class OtherDecayPanelScript : MonoBehaviour {

    public InputField InputMultiplier;
    public Toggle ToggleAlpha;
    public Toggle ToggleElectron;
    public Toggle ToggleBetaPlus;
    public Toggle ToggleBetaMinus;
    public Toggle ToggleNeutrons;
    public Toggle ToggleProtons;

	public void ResetFields()
    {
        InputMultiplier.text = string.Empty;
        ToggleAlpha.isOn = false;
        ToggleElectron.isOn = false;
        ToggleBetaPlus.isOn = false;
        ToggleBetaMinus.isOn = false;
        ToggleNeutrons.isOn = false;
        ToggleProtons.isOn = false;
    }

    public string GetDecayString()
    {
        var multiplier = InputMultiplier.text == null || InputMultiplier.text.Equals(string.Empty) ? 1 : (int.Parse(InputMultiplier.text) < 0 ? 0 : int.Parse(InputMultiplier.text));
        var alpha = ToggleAlpha.isOn ? "a" : string.Empty;
        var electron = ToggleElectron.isOn ? "e" : string.Empty;
        var betaplus = ToggleBetaPlus.isOn ? "b+" : string.Empty;
        var betaminus = ToggleBetaMinus.isOn ? "b-" : string.Empty;
        var neutrons = ToggleNeutrons.isOn ? "n" : string.Empty;
        var protons = ToggleProtons.isOn ? "p" : string.Empty;
        return multiplier + alpha + electron + betaplus + betaminus + neutrons + protons;
    }
}
