using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game References")] [SerializeField]
    private FishDatabase fishDatabase;

    [SerializeField] private GameTimeManager timeManager;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private FishingGameUI gameUI;

    [Header("Player")] [SerializeField] private IsometricPlayerController player;

    [SerializeField] private Transform playerSpawnPoint;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        PauseGame(pauseStatus);
    }

    private void InitializeGame()
    {
        // Find components if not assigned
        if (fishDatabase == null)
            fishDatabase = Resources.Load<FishDatabase>("FishDatabase");

        if (timeManager == null)
            timeManager = FindObjectOfType<GameTimeManager>();

        if (inventorySystem == null)
            inventorySystem = FindObjectOfType<InventorySystem>();

        if (gameUI == null)
            gameUI = FindObjectOfType<FishingGameUI>();

        if (player == null)
            player = FindObjectOfType<IsometricPlayerController>();

        // Spawn player if needed
        if (player != null && playerSpawnPoint != null) player.transform.position = playerSpawnPoint.position;

        Debug.Log("Fishing Game Initialized!");
        Debug.Log("Controls:");
        Debug.Log("- WASD/Arrow Keys: Move");
        Debug.Log("- Left Shift: Sprint");
        Debug.Log("- Left Click/Enter: Cast fishing line");
        Debug.Log("- E: Interact/Cancel fishing");
        Debug.Log("- Mouse Scroll: Zoom camera");
    }

    public FishDatabase GetFishDatabase()
    {
        return fishDatabase;
    }

    public void PauseGame(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}