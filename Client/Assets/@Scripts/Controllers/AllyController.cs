using UnityEngine;

public abstract class AllyController : CreatureController
{
    protected bool _doWork;
    protected bool _auto = true;

    protected bool _isWaitingAttack;

    public void SetAuto(bool auto)
    {
        _auto = auto;
    }

    public abstract void SetStartAI(bool start);
}
