using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HelpWindowPanel : WindowPanelBase {

    public Text Text;

    void Awake() {
        var text = new FileInfo(Application.streamingAssetsPath + "/helptext.txt").OpenText().ReadToEnd();
        Text.text = text;
    }

    public override void ResetPanel()
    { }
}
