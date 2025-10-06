using System;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    [Header("Time Settings")] [SerializeField]
    private float dayLengthInMinutes = 24f; // Real minutes for a full day

    [SerializeField] private bool autoAdvanceTime = true;

    [Header("Current Time")] [SerializeField] [Range(0f, 24f)]
    private float currentTime = 8f; // 8 AM start

    private int currentDay = 1;

    private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    private float dayLengthInSeconds;
    public Action<int> OnDayChanged;

    public Action<TimeOfDay> OnTimeOfDayChanged;

    private void Start()
    {
        dayLengthInSeconds = dayLengthInMinutes * 60f;
        UpdateTimeOfDay();
    }

    private void Update()
    {
        if (autoAdvanceTime) AdvanceTime();
    }

    private void AdvanceTime()
    {
        currentTime += 24f / dayLengthInSeconds * Time.deltaTime;

        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
        }

        UpdateTimeOfDay();
    }

    private void UpdateTimeOfDay()
    {
        var newTimeOfDay = GetTimeOfDayFromHour(currentTime);

        if (newTimeOfDay != currentTimeOfDay)
        {
            currentTimeOfDay = newTimeOfDay;
            OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
        }
    }

    private TimeOfDay GetTimeOfDayFromHour(float hour)
    {
        if (hour >= 5f && hour < 8f)
            return TimeOfDay.Dawn;
        if (hour >= 8f && hour < 12f)
            return TimeOfDay.Morning;
        if (hour >= 12f && hour < 18f)
            return TimeOfDay.Afternoon;
        if (hour >= 18f && hour < 22f)
            return TimeOfDay.Evening;
        return TimeOfDay.Night;
    }

    public TimeOfDay GetCurrentTimeOfDay()
    {
        return currentTimeOfDay;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public string GetTimeString()
    {
        var hours = Mathf.FloorToInt(currentTime);
        var minutes = Mathf.FloorToInt((currentTime - hours) * 60);

        var period = hours >= 12 ? "PM" : "AM";
        var displayHours = hours;
        if (displayHours == 0) displayHours = 12;
        else if (displayHours > 12) displayHours -= 12;

        return $"{displayHours:00}:{minutes:00} {period}";
    }

    public void SetTime(float newTime)
    {
        currentTime = Mathf.Clamp(newTime, 0f, 24f);
        UpdateTimeOfDay();
    }

    public void SkipToTime(float targetTime)
    {
        currentTime = targetTime;
        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
        }

        UpdateTimeOfDay();
    }
}