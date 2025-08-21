using System;
using System.Collections.Generic;

public class EventManager
{
	private Dictionary<Define.EEventType, Action> _events = new Dictionary<Define.EEventType, Action>();
    public event Action<Define.EBroadcastEventType, int> OnBroadcastMissionEvent;

    public void AddEvent(Define.EEventType eventType, Action listener)
	{
		if (_events.ContainsKey(eventType) == false)
			_events.Add(eventType, new Action(() => { }));

		_events[eventType] += listener;
	}

	public void RemoveEvent(Define.EEventType eventType, Action listener)
	{
        if (_events.ContainsKey(eventType))
			_events[eventType] -= listener;
	}

	public void TriggerEvent(Define.EEventType eventType)
	{
		if (_events.ContainsKey(eventType))
			_events[eventType].Invoke();
	}

	public void Init()
	{
		OnBroadcastMissionEvent -= OnHandleBroadcastMissionEvent;
		OnBroadcastMissionEvent += OnHandleBroadcastMissionEvent;
    }

	public void Clear()
	{
		_events.Clear();
        OnBroadcastMissionEvent -= OnHandleBroadcastMissionEvent;
    }

	public void BroadcastMissionEvent(Define.EBroadcastEventType eventType, int value)
	{
		UnityEngine.Debug.Log($"{eventType} : {value}");
        OnBroadcastMissionEvent?.Invoke(eventType, value);
    }

    void OnHandleBroadcastMissionEvent(Define.EBroadcastEventType eventType, int value)
    {
        foreach (var mission in Managers.Game.MissionSaveDatas)
        {
            if (mission.MissionState == Define.EMissionState.Progress)
                mission.OnHandleBroadcastMissionEvent(eventType, value);
        }
    }

}
