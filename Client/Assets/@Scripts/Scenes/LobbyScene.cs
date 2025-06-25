using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.TitleScene;

        Managers.UI.ShowSceneUI<UI_LobbyScene>();
    }

    public override void Clear()
    {

    }
}
