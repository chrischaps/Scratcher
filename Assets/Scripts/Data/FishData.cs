using UnityEngine;

[System.Serializable]
public class FishData
{
    [Header("Basic Info")]
    public string fishName;
    public Sprite fishSprite;
    [TextArea(2, 4)]
    public string description;

    [Header("Fishing Properties")]
    [Range(0f, 1f)]
    public float rarity = 0.5f; // 0 = common, 1 = legendary

    [Range(1, 10)]
    public int difficulty = 5; // How hard to catch

    public float minWeight = 0.5f;
    public float maxWeight = 2f;

    [Header("Value")]
    public int baseValue = 10;

    [Header("Conditions")]
    public TimeOfDay[] availableTimes;
    public WeatherCondition[] favoredWeather;
    public WaterType[] waterTypes;
}

[System.Serializable]
public enum TimeOfDay
{
    Dawn,
    Morning,
    Afternoon,
    Evening,
    Night
}

[System.Serializable]
public enum WeatherCondition
{
    Sunny,
    Cloudy,
    Rainy,
    Stormy
}

[System.Serializable]
public enum WaterType
{
    Pond,
    River,
    Lake,
    Ocean
}