using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CaughtFish
{
    public FishData fishData;
    public float weight;
    public int value;
    public DateTime dateCaught;

    public CaughtFish(FishData fish, float fishWeight)
    {
        fishData = fish;
        weight = fishWeight;
        value = Mathf.RoundToInt(fish.baseValue * (weight / ((fish.minWeight + fish.maxWeight) * 0.5f)));
        dateCaught = DateTime.Now;
    }
}

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")] [SerializeField]
    private int maxInventorySize = 50;

    [Header("Current Inventory")] [SerializeField]
    private List<CaughtFish> caughtFish = new();

    [SerializeField] private int totalValue;

    public Action<CaughtFish> OnFishAdded;
    public Action<CaughtFish> OnFishSold;
    public Action<int> OnValueChanged;

    private void Start()
    {
        // Subscribe to fishing events
        var fishingController = FindObjectOfType<FishingController>();
        if (fishingController != null) fishingController.OnFishCaught += OnFishCaughtHandler;
    }

    private void OnFishCaughtHandler(FishData fishData, float weight)
    {
        AddFish(fishData, weight);
    }

    public bool AddFish(FishData fishData, float weight)
    {
        if (caughtFish.Count >= maxInventorySize)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        var newCatch = new CaughtFish(fishData, weight);
        caughtFish.Add(newCatch);
        totalValue += newCatch.value;

        OnFishAdded?.Invoke(newCatch);
        OnValueChanged?.Invoke(totalValue);

        Debug.Log($"Caught {fishData.fishName} ({weight:F1}kg) worth {newCatch.value} coins!");
        return true;
    }

    public bool SellFish(CaughtFish fish)
    {
        if (caughtFish.Contains(fish))
        {
            caughtFish.Remove(fish);
            totalValue -= fish.value;

            OnFishSold?.Invoke(fish);
            OnValueChanged?.Invoke(totalValue);

            Debug.Log($"Sold {fish.fishData.fishName} for {fish.value} coins!");
            return true;
        }

        return false;
    }

    public void SellAllFish()
    {
        var soldValue = 0;
        var soldCount = caughtFish.Count;

        foreach (var fish in caughtFish.ToList())
        {
            soldValue += fish.value;
            OnFishSold?.Invoke(fish);
        }

        caughtFish.Clear();
        totalValue = 0;

        OnValueChanged?.Invoke(totalValue);

        Debug.Log($"Sold {soldCount} fish for {soldValue} coins total!");
    }

    public List<CaughtFish> GetCaughtFish()
    {
        return new List<CaughtFish>(caughtFish);
    }

    public int GetInventoryCount()
    {
        return caughtFish.Count;
    }

    public int GetTotalValue()
    {
        return totalValue;
    }

    public bool IsInventoryFull()
    {
        return caughtFish.Count >= maxInventorySize;
    }

    public CaughtFish GetHeaviestFish()
    {
        return caughtFish.OrderByDescending(f => f.weight).FirstOrDefault();
    }

    public CaughtFish GetMostValuableFish()
    {
        return caughtFish.OrderByDescending(f => f.value).FirstOrDefault();
    }

    public List<CaughtFish> GetFishByType(string fishName)
    {
        return caughtFish.Where(f => f.fishData.fishName == fishName).ToList();
    }

    public Dictionary<string, int> GetFishCounts()
    {
        var counts = new Dictionary<string, int>();

        foreach (var fish in caughtFish)
            if (counts.ContainsKey(fish.fishData.fishName))
                counts[fish.fishData.fishName]++;
            else
                counts[fish.fishData.fishName] = 1;

        return counts;
    }

    public InventoryStats GetStats()
    {
        return new InventoryStats
        {
            totalFishCaught = caughtFish.Count,
            totalValue = totalValue,
            uniqueSpecies = GetFishCounts().Count,
            heaviestFish = GetHeaviestFish(),
            mostValuableFish = GetMostValuableFish()
        };
    }
}

[Serializable]
public class InventoryStats
{
    public int totalFishCaught;
    public int totalValue;
    public int uniqueSpecies;
    public CaughtFish heaviestFish;
    public CaughtFish mostValuableFish;
}