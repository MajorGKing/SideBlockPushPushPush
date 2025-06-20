using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindFirstObjectByType<BaseScene>(); } }
    public Define.EScene NextSceneType;

    public void LoadScene(Define.EScene type, Transform parents = null)
    {
        NextSceneType = type;
        Managers.Clear();
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(GetSceneName(Define.EScene.LoadingScene), LoadSceneMode.Single, true);
    }

    public string GetSceneName(Define.EScene type)
    {
        string name = System.Enum.GetName(typeof(Define.EScene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
