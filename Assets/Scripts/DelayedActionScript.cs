using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedActionScript : MonoBehaviour
{
    private List<DelayedAction> delayedActions = new List<DelayedAction>();

    internal bool countdownActive = true;

    void FixedUpdate()
    {
        if (countdownActive)
        {
            List<DelayedAction> actionsToRemove = new List<DelayedAction>();
            foreach (var da in delayedActions)
            {
                da.Time -= Time.fixedDeltaTime;
                if (da.Time <= 0)
                {
                    da.Action.Invoke();
                    actionsToRemove.Add(da);
                }
            }
            foreach (var a in actionsToRemove)
            {
                delayedActions.Remove(a);
            }
        }
    }

    public void InvokeLater(UnityAction action, float time)
    {
        delayedActions.Add(new DelayedAction(time, action));
    }
}
