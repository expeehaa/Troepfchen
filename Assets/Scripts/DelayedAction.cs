using UnityEngine.Events;

public class DelayedAction
{
    public float Time;
    public UnityAction Action;

    public DelayedAction(float time, UnityAction action)
    {
        Time = time;
        Action = action;
    }
}