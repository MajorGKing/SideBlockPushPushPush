using UnityEngine;

public class LineTouchController : BaseController
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
