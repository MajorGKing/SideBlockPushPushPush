using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TimeManager
{
    private float _minute = 60;
    private DateTime _lastMissionTime;
    public DateTime lastMissionTime
    {
        get { return _lastMissionTime; }
        set
        {
            Debug.Log("Tiee : " + value);
            //CheckDailyReset(lastMissionTime);
            //CheckWeeklyReset(lastMissionTime);

            bool isFIrst = false;

            if(lastMissionTime == default(DateTime))
            {
                isFIrst = true;
            }

            _lastMissionTime = value;
            Managers.Game.SaveMissionTime(lastMissionTime);

            if(isFIrst == true)
            {
                lastMissionTime = DateTime.Now;
            }
        }
    }

    public void Init()
    {
        Debug.Log(lastMissionTime);

        TimeStart();
    }

    private void TimeStart()
    {
        Managers.Instance.StartCoroutine(CoStartTimer());
    }

    IEnumerator CoStartTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            _minute--;

            if (_minute == 0)
            {
                Debug.Log("Min change");
                _minute = 60;
                lastMissionTime = DateTime.Now;
            }
        }
    }

    private void CheckDailyReset(DateTime lastTime)
    {
        if (lastTime == default(DateTime))
            return;

        DateTime now = DateTime.Now;

        // 날짜가 다르면 오늘 오전 9시를 넘겼는지 확인
        if (lastTime.Date != now.Date)
        {
            DateTime nineAM = now.Date.AddHours(9);

            // 오늘 오전 9시를 넘겼다면 출석 처리
            if (now >= nineAM)
            {
                Debug.Log("출석 처리: 오늘 오전 9시 넘김");
                // 출석 처리 수행
                DayMissionReset();

            }
        }
        else
        {
            // 같은 날짜일 때 lastTime이 9시 이전이면 오늘 9시를 넘겼는지 체크
            DateTime nineAM = now.Date.AddHours(9);

            if (lastTime < nineAM && now >= nineAM)
            {
                Debug.Log("출석 처리: 오늘 오전 9시 넘김");
                // 출석 처리 수행
                DayMissionReset();
            }
        }
    }

    private void CheckWeeklyReset(DateTime lastTime)
    {
        if (lastTime == default(DateTime))
            return;

        DateTime now = DateTime.Now;

        // 이번 주 월요일의 오전 9시 계산
        DateTime monday9AM = GetThisMondayAt9AM(now);

        // 현재 시간이 월요일 오전 9시를 넘겼다면 주간 리셋 처리
        if (now >= monday9AM && lastTime < monday9AM)
        {
            Debug.Log("주간 리셋: 월요일 오전 9시를 넘겼습니다.");
            // 주간 리셋 작업 수행
            WeekMissionReset();
        }
    }

    private DateTime GetThisMondayAt9AM(DateTime currentTime)
    {
        // 현재 날짜의 월요일 구하기
        int daysSinceMonday = (int)currentTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysSinceMonday < 0) daysSinceMonday += 7; // 월요일이 지난 경우 7일 더함

        // 이번 주 월요일 00:00을 구하고, 거기서 오전 9시로 설정
        DateTime thisMonday = currentTime.Date.AddDays(-daysSinceMonday); // 이번 주 월요일의 00:00
        return thisMonday.AddHours(9); // 월요일 오전 9시
    }


    private void DayMissionReset()
    {
        {
            var normalMissions = Managers.Data.MissionDataDic.Where(x => x.Value.MissionType == Define.EMissionType.Normal).Select(m => m.Value).ToList();

            foreach (var normalMission in normalMissions)
            {
                var missionSave = Managers.Game.GetMissionData(normalMission.TemplateId);
                missionSave.StackedPoint = 0;
                missionSave.MissionState = Define.EMissionState.Progress;
            }
        }

        {
            var dayMission = Managers.Data.MissionDataDic.Where(x => x.Value.MissionType == Define.EMissionType.Day).Select(m => m.Value).First();
            var dayMissionSave = Managers.Game.GetMissionData(dayMission.TemplateId);
            dayMissionSave.StackedPoint = 0;
            dayMissionSave.MissionState = Define.EMissionState.Progress;
            //for (int i = 0; i < dayMissionSave.PointStepMissionState.Count; i++)
            //{
            //    dayMissionSave.PointStepMissionState[i] = Define.EMissionState.Progress;
            //}
        }

        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
        Managers.Game.SaveGame();
    }

    private void WeekMissionReset()
    {
        {
            var normalMissions = Managers.Data.MissionDataDic.Where(x => x.Value.MissionType == Define.EMissionType.Normal).Select(m => m.Value).ToList();

            foreach (var normalMission in normalMissions)
            {
                var missionSave = Managers.Game.GetMissionData(normalMission.TemplateId);
                missionSave.StackedPoint = 0;
                missionSave.MissionState = Define.EMissionState.Progress;
            }
        }

        {
            var weekMission = Managers.Data.MissionDataDic.Where(x => x.Value.MissionType == Define.EMissionType.Week).Select(m => m.Value).First();
            var weekMissionSave = Managers.Game.GetMissionData(weekMission.TemplateId);
            weekMissionSave.StackedPoint = 0;
            weekMissionSave.MissionState = Define.EMissionState.Progress;
            //for (int i = 0; i < weekMissionSave.PointStepMissionState.Count; i++)
            //{
            //    weekMissionSave.PointStepMissionState[i] = Define.EMissionState.Progress;
            //}
        }

        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
        Managers.Game.SaveGame();
    }
}
