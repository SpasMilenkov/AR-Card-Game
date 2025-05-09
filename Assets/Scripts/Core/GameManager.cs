using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameManager Instance;

    // Game state
    public enum GameState { Setup, PlayerTurn, MonsterTurn, Victory, Defeat }
    public GameState currentState;

    // Toggle between AR and non-AR testing
    [Header("Testing Options")]
    public bool useARMode = true; // Set to false for non-AR testing
    private int debugUnitCount = 0; // For non-AR testing

    // Units
    public List<PlayerUnit> playerUnits = new List<PlayerUnit>();
    public List<MonsterUnit> monsterUnits = new List<MonsterUnit>();
    public PlayerUnit currentActiveUnit;

    // References
    public UIManager uiManager;
    public CombatSystem combatSystem;
    public Transform playerUnitSpawnArea;
    public Transform monsterUnitSpawnArea;

    // Prefabs
    public GameObject[] playerUnitPrefabs;
    public GameObject[] monsterUnitPrefabs;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Start in setup state
        SetGameState(GameState.Setup);

        // Non-AR Testing: Spawn monsters immediately
        if (!useARMode)
        {
            SpawnMonsters();
        }
    }

    private void Update()
    {
        // Only add debugging controls in non-AR mode
        if (!useARMode && currentState == GameState.Setup)
        {
            // Press 1-5 to spawn player units for testing
            if (Input.GetKeyDown(KeyCode.Alpha1)) DebugSpawnPlayerUnit(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) DebugSpawnPlayerUnit(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) DebugSpawnPlayerUnit(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) DebugSpawnPlayerUnit(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) DebugSpawnPlayerUnit(4);
        }
    }

    // AR Version: Called by CardDetector when Vuforia detects a card
    public void SpawnPlayerUnit(int unitType, Transform cardTransform)
    {
        // Don't spawn more than 3 units
        if (playerUnits.Count >= 3)
            return;

        // Position slightly above the card (adjust Y value as needed)
        Vector3 spawnPos = cardTransform.position + new Vector3(0, 0.1f, 0);

        SpawnPlayerUnitAtPosition(unitType, spawnPos);
    }

    // Non-AR Version: Called from Update for keyboard testing
    private void DebugSpawnPlayerUnit(int unitType)
    {
        // Don't spawn more than 3 units
        if (playerUnits.Count >= 3)
            return;

        // Calculate position in front of camera for testing
        Vector3 spawnPos;

        if (playerUnitSpawnArea != null)
        {
            // Use the spawn area with offset for multiple units
            spawnPos = playerUnitSpawnArea.position + new Vector3(debugUnitCount * 0.3f - 0.3f, 0, 0);
        }
        else
        {
            // Fallback to camera-based positioning
            spawnPos = Camera.main.transform.position +
                       Camera.main.transform.forward * 1.0f +
                       Camera.main.transform.right * (debugUnitCount - 1) * 0.3f;
        }

        debugUnitCount++;
        SpawnPlayerUnitAtPosition(unitType, spawnPos);
    }

    // Common method used by both AR and non-AR versions
    private void SpawnPlayerUnitAtPosition(int unitType, Vector3 position)
    {
        if (unitType >= 0 && unitType < playerUnitPrefabs.Length)
        {
            // Instantiate the unit
            GameObject unitObj = Instantiate(playerUnitPrefabs[unitType], position, Quaternion.identity);
            PlayerUnit unit = unitObj.GetComponent<PlayerUnit>();
            playerUnits.Add(unit);

            Debug.Log("Player unit spawned: " + unit.unitName);

            // If we have 3 player units, start the game
            if (playerUnits.Count == 3)
            {
                StartBattle();
            }
            // If we're in AR mode, spawn monsters after the first player unit
            else if (useARMode && playerUnits.Count == 1)
            {
                SpawnMonsters();
            }
        }
    }

    private void SpawnMonsters()
    {
        // Clear any existing monsters (in case of reset)
        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster != null && monster.gameObject != null)
                Destroy(monster.gameObject);
        }
        monsterUnits.Clear();

        // Randomly select 3 monster types
        List<int> selectedTypes = new List<int>();
        while (selectedTypes.Count < 3)
        {
            int randomType = Random.Range(0, monsterUnitPrefabs.Length);
            if (!selectedTypes.Contains(randomType))
                selectedTypes.Add(randomType);
        }

        // Position calculation
        Vector3 basePos;
        Vector3 facingDir;

        if (useARMode && playerUnits.Count > 0)
        {
            // AR Mode: Position relative to player units
            Vector3 playerCenter = Vector3.zero;
            foreach (PlayerUnit unit in playerUnits)
            {
                playerCenter += unit.transform.position;
            }
            playerCenter /= playerUnits.Count;

            // Use monster spawn area offset from player center
            basePos = playerCenter + new Vector3(0, 0, -0.5f); // 0.5m away from players
            facingDir = playerCenter;
        }
        else
        {
            // Non-AR Testing: Use the predefined spawn area
            basePos = monsterUnitSpawnArea.position;

            // Face toward player spawn area or camera
            if (playerUnitSpawnArea != null)
                facingDir = playerUnitSpawnArea.position;
            else
                facingDir = Camera.main.transform.position;
        }

        // Spawn the monsters
        float spacing = 0.3f; // Distance between monsters
        for (int i = 0; i < 3; i++)
        {
            // Calculate position in a line formation
            Vector3 spawnPos = basePos + new Vector3((i - 1) * spacing, 0, 0);

            // Look toward players
            Quaternion lookRotation = Quaternion.LookRotation(facingDir - spawnPos);

            // Instantiate monster
            GameObject monsterObj = Instantiate(monsterUnitPrefabs[selectedTypes[i]], spawnPos, lookRotation);
            MonsterUnit monster = monsterObj.GetComponent<MonsterUnit>();
            monsterUnits.Add(monster);

            Debug.Log("Monster spawned: " + monster.unitName);
        }
    }

    private void StartBattle()
    {
        SetGameState(GameState.PlayerTurn);
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        // Reset all player units for new turn
        foreach (PlayerUnit unit in playerUnits)
        {
            if (unit.isAlive)
            {
                unit.DecreaseCooldown();
            }
        }

        // Set first living unit as active
        SetNextActivePlayerUnit();
    }

    public void SetNextActivePlayerUnit()
    {
        // Find next living player unit
        currentActiveUnit = null;

        foreach (PlayerUnit unit in playerUnits)
        {
            if (unit.isAlive)
            {
                currentActiveUnit = unit;
                break;
            }
        }

        if (currentActiveUnit != null)
        {
            // Update UI for current unit
            uiManager.UpdateUnitUI(currentActiveUnit);
        }
        else
        {
            // No living units, end turn
            EndPlayerTurn();
        }
    }

    public void EndPlayerTurn()
    {
        SetGameState(GameState.MonsterTurn);
        StartMonsterTurn();
    }

    private void StartMonsterTurn()
    {
        // Reset monsters for new turn
        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster.isAlive)
            {
                // Call special monster turn start methods
                if (monster is Troll)
                    ((Troll)monster).StartTurn();

                if (monster is SkeletonArcher)
                    ((SkeletonArcher)monster).StartTurn();
            }
        }

        // Monster AI takes actions
        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster.isAlive)
            {
                // Choose target
                PlayerUnit target = monster.SelectTarget(playerUnits.ToArray());

                if (target != null)
                {
                    // Attack target
                    monster.Attack(target);
                }
            }
        }

        // End monster turn
        EndMonsterTurn();
    }

    private void EndMonsterTurn()
    {
        CheckBattleStatus();

        if (currentState != GameState.Victory && currentState != GameState.Defeat)
        {
            SetGameState(GameState.PlayerTurn);
            StartPlayerTurn();
        }
    }

    public void CheckBattleStatus()
    {
        // Check if all player units are dead
        bool anyPlayerAlive = false;
        foreach (PlayerUnit unit in playerUnits)
        {
            if (unit.isAlive)
            {
                anyPlayerAlive = true;
                break;
            }
        }

        if (!anyPlayerAlive)
        {
            SetGameState(GameState.Defeat);
            uiManager.ShowGameOverScreen(false);
            return;
        }

        // Check if all monster units are dead
        bool anyMonsterAlive = false;
        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster.isAlive)
            {
                anyMonsterAlive = true;
                break;
            }
        }

        if (!anyMonsterAlive)
        {
            SetGameState(GameState.Victory);
            uiManager.ShowGameOverScreen(true);
        }

        // Update knight shields
        foreach (PlayerUnit unit in playerUnits)
        {
            if (unit.isAlive && unit is Knight knight)
            {
                knight.EndTurn();
            }
        }
    }

    private void SetGameState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Game State: " + newState);
    }

    // Add this method for resetting the game (called from restart button)
    public void RestartGame()
    {
        // Clear existing units
        foreach (PlayerUnit unit in playerUnits)
        {
            if (unit != null && unit.gameObject != null)
                Destroy(unit.gameObject);
        }
        playerUnits.Clear();

        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster != null && monster.gameObject != null)
                Destroy(monster.gameObject);
        }
        monsterUnits.Clear();

        // Reset variables
        currentActiveUnit = null;
        debugUnitCount = 0;

        // Return to setup state
        SetGameState(GameState.Setup);

        // Hide UI panels
        // if (uiManager != null)
        // {
        //     uiManager.HideAllPanels();
        // }

        // Non-AR mode: spawn monsters again
        if (!useARMode)
        {
            SpawnMonsters();
        }
    }
}