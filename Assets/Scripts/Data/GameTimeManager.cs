using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float dayLengthInMinutes = 24f; // Real minutes for a full day
    [SerializeField] private bool autoAdvanceTime = true;

    [Header("Current Time")]
    [SerializeField, Range(0f, 24f)] private float currentTime = 8f; // 8 AM start

    public System.Action<TimeOfDay> OnTimeOfDayChanged;
    public System.Action<int> OnDayChanged;

    private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    private int currentDay = 1;
    private float dayLengthInSeconds;

    private void Start()
    {
        dayLengthInSeconds = dayLengthInMinutes * 60f;
        UpdateTimeOfDay();
    }

    private void Update()
    {
        if (autoAdvanceTime)
        {
            AdvanceTime();
        }
    }

    private void AdvanceTime()
    {
        currentTime += (24f / dayLengthInSeconds) * Time.deltaTime;

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
        TimeOfDay newTimeOfDay = GetTimeOfDayFromHour(currentTime);

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
        else if (hour >= 8f && hour < 12f)
            return TimeOfDay.Morning;
        else if (hour >= 12f && hour < 18f)
            return TimeOfDay.Afternoon;
        else if (hour >= 18f && hour < 22f)
            return TimeOfDay.Evening;
        else
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
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60);

        string period = hours >= 12 ? "PM" : "AM";
        int displayHours = hours;
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