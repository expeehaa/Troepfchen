using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreativeDecayChoiceWindowPanel : WindowPanelBase {

    public Dropdown DropdownChoice;
    public ChoicePanelScript choicePanelScript;
    public Button BtnCancel;
    public Button BtnOk;

    private UnityAction<string> call = null;
    private Dictionary<ChosenChoicePanel, Dropdown.OptionData> optionsDic = new Dictionary<ChosenChoicePanel, Dropdown.OptionData>();

    void Start()
    {
        optionsDic.Clear();
        optionsDic.Add(ChosenChoicePanel.Cluster, new Dropdown.OptionData("Cluster"));
        optionsDic.Add(ChosenChoicePanel.Other, new Dropdown.OptionData("Weitere Arten"));

        BtnCancel.onClick.AddListener(() => Deactivate(false));
        BtnOk.onClick.AddListener(() => Deactivate(true));

        DropdownChoice.ClearOptions();
        DropdownChoice.AddOptions(optionsDic.Values.ToList());

        DropdownChoice.onValueChanged.AddListener((n) =>
        {
            choicePanelScript.chosenChoicePanel = optionsDic.Where(p => p.Value == DropdownChoice.options.ElementAt(n)).FirstOrDefault().Key;
        });

        DropdownChoice.value = 1;
        DropdownChoice.value = 0;
    }

    public void Activate(UnityAction<string> call)
    {
        this.call = call;
        choicePanelScript.ResetInputs();
        DropdownChoice.value = 0;
    }

    public void Deactivate(bool doCall)
    {
        //print(call == null ? "No method available :/" : "Let's invoke decay!");
        if (call != null && doCall) call.Invoke(choicePanelScript.DecayString);
        ResetPanel();
        //print("disable event fire");
        if (callOnDeactivate != null) callOnDeactivate.Invoke(this);
    }

    public override void ResetPanel()
    {
        choicePanelScript.ResetInputs();
    }
}
