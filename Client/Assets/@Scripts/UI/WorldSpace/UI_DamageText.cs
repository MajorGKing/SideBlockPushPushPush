using UnityEngine;

public class UI_DamageText : UI_WorldSpace
{
    private enum Texts
    {
        Text_Amount,
    }

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindTexts(typeof(Texts));
    }

    public void SetInfo(int amount)
    {
        transform.position += Vector3.up * 3.0f;
        GetText((int)Texts.Text_Amount).text = $"{amount:N0}";
    }

    public void OnDespawned()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
