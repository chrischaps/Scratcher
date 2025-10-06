using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "FishDatabase", menuName = "Fishing Game/Fish Database")]
public class FishDatabase : ScriptableObject
{
    [Header("Fish Collection")] [SerializeField]
    private List<FishData> allFish = new();

    public List<FishData> GetAllFish()
    {
        return allFish;
    }

    public FishData GetFishByName(string fishName)
    {
        return allFish.FirstOrDefault(fish => fish.fishName.Equals(fishName, StringComparison.OrdinalIgnoreCase));
    }

    public List<FishData> GetFishByRarity(float minRarity, float maxRarity)
    {
        return allFish.Where(fish => fish.rarity >= minRarity && fish.rarity <= maxRarity).ToList();
    }

    public List<FishData> GetFishByWaterType(WaterType waterType)
    {
        return allFish.Where(fish => fish.waterTypes.Contains(waterType)).ToList();
    }

    public List<FishData> GetFishByTimeOfDay(TimeOfDay timeOfDay)
    {
        return allFish.Where(fish => fish.availableTimes.Contains(timeOfDay)).ToList();
    }

    public List<FishData> GetFishByWeather(WeatherCondition weather)
    {
        return allFish.Where(fish => fish.favoredWeather.Contains(weather)).ToList();
    }

    public int GetTotalFishCount()
    {
        return allFish.Count;
    }

    public FishData GetRandomFish()
    {
        if (allFish.Count == 0) return null;
        return allFish[Random.Range(0, allFish.Count)];
    }

#if UNITY_EDITOR
    [ContextMenu("Create Sample Fish")]
    private void CreateSampleFish()
    {
        // Clear existing
        allFish.Clear();

        // Common fish
        var bluegill = CreateFishData("Bluegill", "A common pond fish", 0.1f, 2, 0.3f, 1.2f, 5,
            new[] { TimeOfDay.Morning, TimeOfDay.Afternoon },
            new[] { WeatherCondition.Sunny, WeatherCondition.Cloudy },
            new[] { WaterType.Pond, WaterType.Lake });

        var bass = CreateFishData("Largemouth Bass", "A popular sport fish", 0.3f, 4, 1f, 3f, 15,
            new[] { TimeOfDay.Dawn, TimeOfDay.Evening },
            new[] { WeatherCondition.Cloudy },
            new[] { WaterType.Lake, WaterType.River });

        var trout = CreateFishData("Rainbow Trout", "Beautiful cold-water fish", 0.4f, 5, 0.8f, 2.5f, 20,
            new[] { TimeOfDay.Dawn, TimeOfDay.Morning },
            new[] { WeatherCondition.Cloudy, WeatherCondition.Rainy },
            new[] { WaterType.River });

        var carp = CreateFishData("Common Carp", "Large bottom feeder", 0.2f, 3, 2f, 8f, 12,
            new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Evening },
            new[] { WeatherCondition.Sunny, WeatherCondition.Cloudy },
            new[] { WaterType.Lake, WaterType.River });

        // Rare fish
        var salmon = CreateFishData("Atlantic Salmon", "Prized migratory fish", 0.7f, 7, 3f, 12f, 50,
            new[] { TimeOfDay.Dawn },
            new[] { WeatherCondition.Rainy },
            new[] { WaterType.River, WaterType.Ocean });

        // Legendary fish
        var goldenTrout = CreateFishData("Golden Trout", "Legendary mountain fish", 0.95f, 9, 1.5f, 4f, 200,
            new[] { TimeOfDay.Dawn },
            new[] { WeatherCondition.Sunny },
            new[] { WaterType.River });

        allFish.AddRange(new[] { bluegill, bass, trout, carp, salmon, goldenTrout });

        EditorUtility.SetDirty(this);
    }

    private FishData CreateFishData(string name, string desc, float rarity, int difficulty,
        float minWeight, float maxWeight, int value, TimeOfDay[] times,
        WeatherCondition[] weather, WaterType[] waters)
    {
        var fish = new FishData
        {
            fishName = name,
            description = desc,
            rarity = rarity,
            difficulty = difficulty,
            minWeight = minWeight,
            maxWeight = maxWeight,
            baseValue = value,
            availableTimes = times,
            favoredWeather = weather,
            waterTypes = waters
        };
        return fish;
    }
#endif
}