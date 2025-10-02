using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using WebPacket;
using Object = UnityEngine.Object;

public class UI_TitleScene : UI_Scene
{
	private enum GameObjects
	{
        Background,
	}

	private enum Texts
	{
        StartText,
        StatusText,
    }

	private enum TitleSceneState
	{
		None,
		AssetLoading,
		AssetLoaded,
		ConnectingToServer,
		ConnectedToServer,
		FailedToConnectToServer,
	}

	TitleSceneState _state = TitleSceneState.None;
	TitleSceneState State
	{
		get { return _state; }
		set
		{
			_state = value;
			switch (value)
			{
				case TitleSceneState.None:
					break;
				case TitleSceneState.AssetLoading:
					GetText((int)Texts.StatusText).text = $"TODO 로딩중";
					break;
				case TitleSceneState.AssetLoaded:
					GetText((int)Texts.StatusText).text = "TODO 로딩 완료";
					break;
				case TitleSceneState.ConnectingToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속중";
					break;
				case TitleSceneState.ConnectedToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속 성공";
					break;
				case TitleSceneState.FailedToConnectToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속 실패";
					break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();

		BindGameObjects(typeof(GameObjects));
		BindTexts(typeof(Texts));

        GetText(((int)Texts.StartText)).gameObject.BindEvent(OnClickNextButton);
        GetText(((int)Texts.StartText)).gameObject.SetActive(false);

        UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        UICanvas.worldCamera = Camera.main;
    }

	protected override void Start()
	{
		base.Start();

		// Load 시작
		State = TitleSceneState.AssetLoading;

		Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
		{
			GetText((int)Texts.StatusText).text = $"TODO 로딩중 : {key} {count}/{totalCount}";

			Debug.Log($"TODO 로딩중 : {key} {count}/{totalCount}");

			if (count == totalCount)
			{
				OnAssetLoaded().Forget();
			}
		});
	}

	private async UniTask OnAssetLoaded()
	{
		State = TitleSceneState.AssetLoaded;
		Managers.Data.Init();
		Managers.Game.Init();
        //GetText(((int)Texts.StartText)).gameObject.SetActive(true);


		// TODO ILHAK
		Debug.Log("Connecting To Server");
		State = TitleSceneState.ConnectingToServer;

        

		// Guest로그인 수행
		{
            // 1. 디바이스의 고유 ID를 가져옵니다.
            string uniqueId = SystemInfo.deviceUniqueIdentifier;
            Debug.Log($"Device Unique ID: {uniqueId}");

            // 2. 요청 패킷을 만듭니다.
            var req = new LoginAccountPacketReq
            {
                userId = uniqueId,
                token = "" // 게스트 로그인이므로 토큰은 비워둡니다.
            };

            LoginAccountPacketRes res = await Managers.Web.SendPostRequestAsync<LoginAccountPacketRes>("api/account/login/guest", req);

            if (res.success)
            {
                //Debug.Log($"Guest Login Success! AccountDbId: {res.accountDbId}");
                //Debug.Log($"Guest Login Success! JWT: {res.jwt}");

                Managers.Web.jwt = res.jwt;

                OnConnectionSuccess();
            }
            else
            {
                Debug.LogError("Guest Login Failed.");
            }
        }



        //IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
        //IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
        //Managers.Network.GameServer.Connect(endPoint, OnConnectionSuccess, OnConnectionFailed);
    }

	private void OnConnectionSuccess()
	{
        GetText(((int)Texts.StartText)).gameObject.SetActive(true);

        //Debug.Log("Connected To Server");
        //State = TitleSceneState.ConnectedToServer;

        //GetObject((int)GameObjects.StartButton).gameObject.SetActive(true);

        //StartCoroutine(CoSendTestPackets());
    }

	//private void OnConnectionFailed()
	//{
	//	Debug.Log("Failed To Connect To Server");
	//	State = TitleSceneState.FailedToConnectToServer;
	//}

	//IEnumerator CoSendTestPackets()
	//{
	//	while (true)
	//	{
	//		yield return new WaitForSeconds(1);

	//		C_Test pkt = new C_Test();
	//		pkt.Temp = 1;
	//		Managers.Network.Send(pkt);
	//	}
	//}

	private void OnClickNextButton(PointerEventData evt)
	{
        Managers.Time.Init();
        Managers.Scene.LoadScene(Define.EScene.LobbyScene);
    }
}
