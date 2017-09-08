using UnityEngine;
using UnityEngine.Events;

public abstract class WindowPanelBase : MonoBehaviour {

    public UnityAction<WindowPanelBase> callOnDeactivate;
    public Color BackgroundColor;
    public Vector3 Position = Vector3.zero;
    public abstract void ResetPanel();
    public virtual void ForceDeactivate()
    {
        ResetPanel();
        if (callOnDeactivate != null) callOnDeactivate.Invoke(this);
    }
}
