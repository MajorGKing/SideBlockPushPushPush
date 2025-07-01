using System.Collections;
using UnityEngine;
using static Define;

public class SkillEffect : BaseObject
{
    private float _lifeTime;

    protected override void Init()
    {
        GameObjectType = EGameObjectType.SkillEffect;
    }

    public void SetInfo(float lifeTime)
    {
        _lifeTime = lifeTime;

        StartCoroutine(CoDestroyEffect());
    }

    private IEnumerator CoDestroyEffect()
    {
        yield return new WaitForSeconds(_lifeTime);

        StopAllCoroutines();
        Managers.Object.RemoveSkillEffect(this);
    }

}
