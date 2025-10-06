using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaterZone : MonoBehaviour
{
    [Header("Water Zone Properties")] [SerializeField]
    private WaterType waterType = WaterType.Pond;

    [SerializeField] private string zoneName = "Fishing Spot";

    [Header("Fish Population")] [SerializeField]
    private List<FishSpawnData> availableFish = new();

    [Header("Environmental Factors")] [SerializeField]
    private float baseSuccessRate = 0.7f;

    [SerializeField] private WeatherCondition currentWeather = WeatherCondition.Sunny;

    private GameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);

        var col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(transform.position, col.bounds.size);
        else
            Gizmos.DrawSphere(transform.position, 2f);

        // Draw water type indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify UI that player entered fishing zone
            var fishingController = other.GetComponent<FishingController>();
            if (fishingController != null)
            {
                // Could trigger zone info display
            }
        }
    }

    public FishData TryGetFish()
    {
        if (availableFish.Count == 0) return null;

        // Filter fish based on current conditions
        var eligibleFish = GetEligibleFish();

        if (eligibleFish.Count == 0) return null;

        // Calculate weighted selection
        var totalWeight = eligibleFish.Sum(f => f.spawnWeight * f.currentPopulation);
        var randomValue = Random.Range(0f, totalWeight);

        var currentWeight = 0f;
        foreach (var fishSpawn in eligibleFish)
        {
            currentWeight += fishSpawn.spawnWeight * fishSpawn.currentPopulation;
            if (randomValue <= currentWeight)
            {
                // Check if catch is successful based on fish difficulty and zone conditions
                if (IsCatchSuccessful(fishSpawn.fishData))
                {
                    // Slightly reduce population for this fish
                    fishSpawn.currentPopulation = Mathf.Max(0.1f, fishSpawn.currentPopulation - 0.05f);
                    return fishSpawn.fishData;
                }

                break;
            }
        }

        return null;
    }

    private List<FishSpawnData> GetEligibleFish()
    {
        var currentTime = GetCurrentTimeOfDay();

        return availableFish.Where(fishSpawn =>
        {
            var fish = fishSpawn.fishData;

            // Check time availability
            if (fish.availableTimes.Length > 0 && !fish.availableTimes.Contains(currentTime))
                return false;

            // Check weather conditions
            if (fish.favoredWeather.Length > 0 && !fish.favoredWeather.Contains(currentWeather))
                return false;

            // Check water type
            if (fish.waterTypes.Length > 0 && !fish.waterTypes.Contains(waterType))
                return false;

            // Check if population is too depleted
            if (fishSpawn.currentPopulation < 0.1f)
                return false;

            return true;
        }).ToList();
    }

    private bool IsCatchSuccessful(FishData fish)
    {
        var successRate = baseSuccessRate;

        // Adjust based on fish difficulty (higher difficulty = lower success rate)
        successRate -= (fish.difficulty - 1) * 0.05f;

        // Adjust based on rarity (rarer fish are harder to catch)
        successRate -= fish.rarity * 0.2f;

        // Weather bonus
        if (fish.favoredWeather.Length > 0 && fish.favoredWeather.Contains(currentWeather))
            successRate += 0.15f;

        // Time bonus
        var currentTime = GetCurrentTimeOfDay();
        if (fish.availableTimes.Length > 0 && fish.availableTimes.Contains(currentTime))
            successRate += 0.1f;

        successRate = Mathf.Clamp01(successRate);
        return Random.value < successRate;
    }

    private TimeOfDay GetCurrentTimeOfDay()
    {
        if (timeManager != null)
            return timeManager.GetCurrentTimeOfDay();

        // Default fallback
        return TimeOfDay.Morning;
    }

    public void RestockFish()
    {
        foreach (var fishSpawn in availableFish)
            fishSpawn.currentPopulation = Mathf.Min(1f, fishSpawn.currentPopulation + 0.1f);
    }

    public void SetWeather(WeatherCondition weather)
    {
        currentWeather = weather;
    }

    public string GetZoneInfo()
    {
        var availableSpecies = GetEligibleFish().Count;
        return $"{zoneName} ({waterType})\nAvailable Species: {availableSpecies}";
    }

    [Serializable]
    public class FishSpawnData
    {
        public FishData fishData;

        [Range(0f, 1f)] public float spawnWeight = 1f;

        [Range(0f, 1f)] public float currentPopulation = 1f; // Depletes with fishing
    }
}