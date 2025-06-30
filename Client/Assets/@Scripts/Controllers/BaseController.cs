using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;
using Event = Spine.Event;

public abstract class BaseController : MonoBehaviour
{
    public EGameObjectType GameObjectType { get; protected set; }

    protected abstract void Init();

    private void Awake()
    {
        Init();
    }

    protected virtual void DestroyObject()
    {
        Managers.Resource.Destroy(gameObject);
    }
}

