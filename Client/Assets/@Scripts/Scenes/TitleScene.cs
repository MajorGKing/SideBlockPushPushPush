using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;

public class TitleScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.TitleScene;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;  
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        GraphicsSettings.transparencySortAxis = new Vector3(0.0f, 1.0f, 0.0f);
        Application.runInBackground = true;

        // 예외적으로 직접 등록한다 (UI_TitleScene은 애셋 로딩도 담당하기 때문)
        Managers.UI.SceneUI = GameObject.FindAnyObjectByType<UI_TitleScene>();

        SetResolution();
    }

	protected override void Start()
    {
        base.Start();

		//IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
		//IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
		//Managers.Network.GameServer.Connect(endPoint);
		//CoSendTestPackets();
	}

	public override void Clear()
	{

	}

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

    public void SetResolution()
    {
        int setWidth = 1920; // 사용자 설정 너비
        int setHeight = 1080; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }
}
