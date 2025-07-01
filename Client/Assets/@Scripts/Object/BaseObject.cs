using UnityEngine;
using static Define;

public abstract class BaseObject : MonoBehaviour
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
