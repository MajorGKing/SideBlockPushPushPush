using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : BaseScene
{
    private UI_LoadingPopup _ui;
    public Define.EScene _nextSceneType;


    protected override void Awake()
    {
        base.Awake();
        SceneType = Define.EScene.LoadingScene;
        _ui = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
    }

    protected override void Start()
    {
        base.Start();

        _nextSceneType = Managers.Scene.NextSceneType;
        StartCoroutine(LoadNextScene());
    }

    public override void Clear()
    {
    }

    IEnumerator LoadNextScene()
    {
        // TODO fake loading
        yield return new WaitForSeconds(1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(Managers.Scene.GetSceneName(_nextSceneType));
        operation.allowSceneActivation = false; // 씬의 자동 전환 false

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (operation.progress >= 0.9f)
            {
                // progressBar.value = 1f;
                // progressText.text = "100%";
                operation.allowSceneActivation = true;
                Managers.Clear();
            }

            yield return null;
        }
    }
}