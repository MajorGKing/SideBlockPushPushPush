using UnityEngine;

public class LineTouchController : BaseObject
{
    public int _lineNumber;

    protected override void Init()
    {

    }

    public int LineTouched()
    {
        return _lineNumber;
    }
}
