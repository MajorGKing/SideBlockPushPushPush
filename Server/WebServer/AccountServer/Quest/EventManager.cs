using AccountServer;
using AccountServer.Services;

namespace Server.Quest
{
	public class EventManager
	{
		private static Dictionary<Define.EEventType, Action> _events = new Dictionary<Define.EEventType, Action>();
		public static event Action<string, Define.EBroadcastEventType, int, bool> OnBroadcastMissionEvent;

        private static IServiceProvider _serviceProvider;

        public static void AddEvent(Define.EEventType eventType, Action listener)
		{
			if (_events.ContainsKey(eventType) == false)
				_events.Add(eventType, new Action(() => { }));

			_events[eventType] += listener;
		}

		public static void RemoveEvent(Define.EEventType eventType, Action listener)
		{
			if (_events.ContainsKey(eventType))
				_events[eventType] -= listener;
		}

		public static void TriggerEvent(Define.EEventType eventType)
		{
			if (_events.ContainsKey(eventType))
				_events[eventType].Invoke();
		}

		public static void Init(IServiceProvider serviceProvider)
		{
            _serviceProvider = serviceProvider;

            OnBroadcastMissionEvent -= OnHandleBroadcastMissionEvent;
			OnBroadcastMissionEvent += OnHandleBroadcastMissionEvent;
		}

		public static void Clear()
		{
			_events.Clear();
			OnBroadcastMissionEvent -= OnHandleBroadcastMissionEvent;
		}

		public static void BroadcastMissionEvent(string Jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
		{
			OnBroadcastMissionEvent?.Invoke(Jwt, eventType, value, commitChanges);
		}

		private static void OnHandleBroadcastMissionEvent(string Jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
		{
            // Check Achievement
            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<QuestService>();

            questService.OnHandleBroadcastMissionEvent(Jwt, eventType, value, commitChanges);

            //Managers.Game.OnHandleBroadcastEventValue(eventType, value);
        }

    } 
}
