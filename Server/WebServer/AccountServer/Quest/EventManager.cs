using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AccountServer;
using AccountServer.Services;

namespace Server.Quest
{
    public static class EventManager
    {
        private static readonly Dictionary<Define.EEventType, Action> _events = new();

        // 비동기 이벤트
        public static event Func<string, Define.EBroadcastEventType, int, bool, Task> OnBroadcastMissionEvent;

        // 정적 서비스 제공자
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// 이벤트 리스너 등록
        /// </summary>
        public static void AddEvent(Define.EEventType eventType, Action listener)
        {
            if (!_events.ContainsKey(eventType))
                _events[eventType] = () => { };

            _events[eventType] += listener;
        }

        /// <summary>
        /// 이벤트 리스너 제거
        /// </summary>
        public static void RemoveEvent(Define.EEventType eventType, Action listener)
        {
            if (_events.ContainsKey(eventType))
                _events[eventType] -= listener;
        }

        /// <summary>
        /// 이벤트 트리거 (동기 이벤트)
        /// </summary>
        public static void TriggerEvent(Define.EEventType eventType)
        {
            if (_events.ContainsKey(eventType))
                _events[eventType]?.Invoke();
        }

        /// <summary>
        /// 서비스 프로바이더 초기화 및 기본 이벤트 핸들러 등록
        /// </summary>
        public static void Init(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // 중복 등록 방지
            OnBroadcastMissionEvent -= OnHandleBroadcastMissionEvent;
            OnBroadcastMissionEvent += OnHandleBroadcastMissionEvent;
        }

        /// <summary>
        /// 이벤트 및 핸들러 초기화
        /// </summary>
        public static void Clear()
        {
            _events.Clear();
            OnBroadcastMissionEvent -= OnHandleBroadcastMissionEvent;
        }

        /// <summary>
        /// 비동기 브로드캐스트 이벤트 트리거
        /// </summary>
        public static async Task BroadcastMissionEvent(string jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
        {
            if (OnBroadcastMissionEvent == null)
                return;

            var handlers = OnBroadcastMissionEvent.GetInvocationList();
            var tasks = handlers
                .Cast<Func<string, Define.EBroadcastEventType, int, bool, Task>>()
                .Select(async handler =>
                {
                    try
                    {
                        await handler.Invoke(jwt, eventType, value, commitChanges);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[EventManager] Broadcast handler error: {ex.Message}");
                        // 필요 시 로깅 시스템에 전달
                    }
                });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 기본 브로드캐스트 미션 이벤트 핸들러
        /// </summary>
        private static async Task OnHandleBroadcastMissionEvent(string jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
        {
            if (_serviceProvider == null)
            {
                Console.Error.WriteLine("[EventManager] ServiceProvider is not initialized.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<QuestService>();

            await questService.OnHandleBroadcastMissionEvent(jwt, eventType, value, commitChanges);
        }
    }
}
