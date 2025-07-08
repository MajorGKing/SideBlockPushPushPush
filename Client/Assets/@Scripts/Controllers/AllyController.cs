using System.Collections.Generic;
using UnityEngine;

public abstract class AllyController : CreatureController
{
    protected bool _doWork;
    protected bool _auto = true;

    protected bool _isWaitingAttack;

    protected List<SpriteRenderer> _myBlocks;

    public void SetAuto(bool auto)
    {
        _auto = auto;
    }

    public abstract void SetStartAI(bool start);

    public void SetBlocks(List<SpriteRenderer> blockSet)
    {
        _myBlocks = blockSet;
    }
}
