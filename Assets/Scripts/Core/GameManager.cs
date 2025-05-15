using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameManager handles the core game logic, unit spawning, and game state management
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    // Singleton instance
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    #endregion

    #region Game State
    // Game states
    public enum GameState
    {
        Setup,          // Initial card placement
        PlayerTurn,     // Player is selecting actions
        MonsterTurn,    // Monsters are taking actions
        Victory,        // Player has won
        Defeat          // Player has lost
    }

    // Current game state
    [Header("Game State")]
    public GameState currentState = GameState.Setup;
    private List<PlayerUnit> _unitsActedThisTurn = new List<PlayerUnit>();
    #endregion

    #region References
    [Header("Core References")]
    public UIManager uiManager;
    public CombatSystem combatSystem;
    private AudioManager audioManager;
    #endregion

    #region Unit Management
    [Header("Units")]
    public List<PlayerUnit> playerUnits = new List<PlayerUnit>();
    public List<MonsterUnit> monsterUnits = new List<MonsterUnit>();
    public PlayerUnit currentActiveUnit;

    [Header("Prefabs")]
    public GameObject[] playerUnitPrefabs;
    public GameObject[] monsterUnitPrefabs;
    #endregion

    #region Spawn Settings
    [Header("Spawn Settings")]
    // How high above the card to spawn player units (in meters)
    [Tooltip("Height above the card to spawn units (in meters)")]
    [Range(0.01f, 0.5f)]
    public float cardSpawnHeight = 0.005f;

    // How far in front of players to spawn monsters (in meters)
    [Tooltip("Distance in front of players to spawn monsters (in meters)")]
    [Range(0.3f, 3.0f)]
    public float monsterDistance = 0.3f;

    // Spacing between monster units (in meters)
    [Tooltip("Space between each monster unit (in meters)")]
    [Range(0.1f, 1.0f)]
    public float monsterSpacing = 0.10f;
    #endregion

    #region Debug
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool drawDebugLines = true;

    private Vector3[] debugSpawnPositions = new Vector3[3];
    #endregion

    // Unit type names for better information
    private static readonly string[] UnitTypeNames =
    {
        "Archer", "Knight", "Mage", "Warrior", "Rogue"
    };

    private void Start()
    {
        audioManager = AudioManager.Instance;

        SetGameState(GameState.Setup);

        if (uiManager != null)
        {
            uiManager.HideAllPanels();
        }

        if (showDebugInfo)
        {
            Debug.Log("GameManager initialized in Setup state. Ready for card detection.");
        }

        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.ClearAllInfo();
            GameInfoLayer.Instance.UpdateGameStateInfo(currentState);
            GameInfoLayer.Instance.AddLogEntry("Game initialized. Place your cards to begin.");
        }

        if (audioManager != null)
        {
            audioManager.PlayMusic(audioManager.battleTheme);
        }
    }

    private void OnDrawGizmos()
    {
        if (drawDebugLines && Application.isPlaying)
        {
            // Draw player positions
            Gizmos.color = Color.green;
            foreach (var unit in playerUnits)
            {
                if (unit != null)
                {
                    Gizmos.DrawWireSphere(unit.transform.position, 0.1f);
                }
            }

            // Draw monster positions
            Gizmos.color = Color.red;
            foreach (var monster in monsterUnits)
            {
                if (monster != null)
                {
                    Gizmos.DrawWireSphere(monster.transform.position, 0.1f);
                }
            }

            // Draw debug spawn positions
            Gizmos.color = Color.yellow;
            foreach (var pos in debugSpawnPositions)
            {
                if (pos != Vector3.zero)
                {
                    Gizmos.DrawWireCube(pos, new Vector3(0.1f, 0.02f, 0.1f));
                }
            }
        }
    }

    #region Player Unit Spawning

    /// <summary>
    /// Spawns a player unit at the position of a detected card
    /// Let Vuforia handle the positioning completely
    /// </summary>
    public void SpawnPlayerUnit(int unitType, Transform cardTransform)
    {
        if (currentState != GameState.Setup || playerUnits.Count >= 3)
            return;

        // Check for valid prefab index
        if (unitType < 0 || unitType >= playerUnitPrefabs.Length)
        {
            Debug.LogError($"Invalid unit type: {unitType}. Must be between 0 and {playerUnitPrefabs.Length - 1}");
            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"ERROR: Invalid unit type: {unitType}");
            }
            return;
        }

        GameObject unitObject = Instantiate(playerUnitPrefabs[unitType], cardTransform);

        if (audioManager != null)
        {
            audioManager.PlayCardPlacedSound();
        }

        PlayerUnit unit = unitObject.GetComponent<PlayerUnit>();
        if (unit == null)
        {
            Debug.LogError($"Spawned object does not have a PlayerUnit component: {unitObject.name}");
            Destroy(unitObject);

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"ERROR: Failed to spawn player unit");
            }

            return;
        }

        playerUnits.Add(unit);


        if (GameInfoLayer.Instance != null)
        {
            string unitName = (unitType >= 0 && unitType < UnitTypeNames.Length)
                ? UnitTypeNames[unitType]
                : $"Unknown ({unitType})";

            GameInfoLayer.Instance.RegisterUnitSpawn(unit.unitName, true);
            GameInfoLayer.Instance.UpdateSpawnInfo();

            int remaining = 3 - playerUnits.Count;
            if (remaining > 0)
            {
                GameInfoLayer.Instance.AddLogEntry($"Need {remaining} more card{(remaining > 1 ? "s" : "")} to start battle");
            }
            else
            {
                GameInfoLayer.Instance.AddLogEntry("All player units ready! Starting battle soon...");
            }
        }

        if (playerUnits.Count == 1)
        {
            Invoke("SpawnMonsters", 0.5f);
        }

        // If we have reached 3 units, start the battle
        if (playerUnits.Count == 3)
        {
            Invoke("StartBattle", 1.0f);
        }
    }
    #endregion

    #region Monster Spawning
    /// <summary>
    /// Spawns monster units facing the player units
    /// </summary>
    public void SpawnMonsters()
    {
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry("Spawning monster units...");
        }

        ClearMonsterUnits();

        // Randomly select monster types (unique selections)
        List<int> selectedTypes = new List<int>();
        while (selectedTypes.Count < 3 && selectedTypes.Count < monsterUnitPrefabs.Length)
        {
            int randomType = Random.Range(0, monsterUnitPrefabs.Length);
            if (!selectedTypes.Contains(randomType))
            {
                selectedTypes.Add(randomType);
            }
        }

        // Calculate spawn formation
        Vector3 playerCenter, forwardDir, rightDir;
        CalculateMonsterFormation(out playerCenter, out forwardDir, out rightDir);

        // Base position for monsters (in front of players)
        Vector3 basePosition = playerCenter + (forwardDir * monsterDistance);


        // Spawn three monsters in a line
        for (int i = 0; i < 3; i++)
        {
            // Position: center monster directly in front, others to sides
            float offset = (i - 1) * monsterSpacing;
            Vector3 spawnPos = basePosition + (rightDir * offset);

            // Keep Y position appropriate (same as player center)
            spawnPos.y = playerCenter.y;

            // Add a small random offset to each monster to prevent perfect overlap
            spawnPos += new Vector3(
                Random.Range(-0.05f, 0.05f),
                0,
                Random.Range(-0.05f, 0.05f)
            );

            // Rotation to face players
            Quaternion rotation = Quaternion.LookRotation(-forwardDir);

            // Instantiate the monster
            if (i < selectedTypes.Count && i < monsterUnitPrefabs.Length)
            {
                GameObject monsterObj = Instantiate(monsterUnitPrefabs[selectedTypes[i]], spawnPos, rotation);

                // Play monster spawn sound
                if (audioManager != null && i == 0)
                {
                    audioManager.PlayMonsterAttackSound();
                }

                MonsterUnit monster = monsterObj.GetComponent<MonsterUnit>();

                if (monster != null)
                {
                    monsterUnits.Add(monster);


                    // Update the info layer
                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.RegisterUnitSpawn(monster.unitName, false);
                    }
                }
            }
        }

        // Final update to spawn info
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.UpdateSpawnInfo();
            GameInfoLayer.Instance.AddLogEntry($"Spawned {monsterUnits.Count} monster units");
        }
    }

    /// <summary>
    /// Calculates the formation for monster spawning based on player positions
    /// </summary>
    private void CalculateMonsterFormation(out Vector3 center, out Vector3 forward, out Vector3 right)
    {
        // Default values
        center = Vector3.zero;
        forward = Vector3.forward;
        right = Vector3.right;

        // If no player units, use defaults
        if (playerUnits.Count == 0)
            return;

        // Calculate center of all player units
        Vector3 sum = Vector3.zero;
        int validUnits = 0;

        foreach (var unit in playerUnits)
        {
            if (unit != null)
            {
                sum += unit.transform.position;
                validUnits++;
            }
        }

        // Update center if we have valid units
        if (validUnits > 0)
        {
            center = sum / validUnits;
        }

        // --- AR SPECIFIC ---

        if (Camera.main != null)
        {
            Vector3 cameraToPlayerDir = (center - Camera.main.transform.position).normalized;

            cameraToPlayerDir.y = 0;
            cameraToPlayerDir = cameraToPlayerDir.normalized;

            forward = cameraToPlayerDir;

            right = Vector3.Cross(Vector3.up, forward).normalized;

        }
        else
        {
            forward = Vector3.forward;
            right = Vector3.right;
        }
    }

    /// <summary>
    /// Clears all monster units
    /// </summary>
    private void ClearMonsterUnits()
    {
        foreach (var monster in monsterUnits)
        {
            if (monster != null && monster.gameObject != null)
            {
                Destroy(monster.gameObject);
            }
        }

        monsterUnits.Clear();
    }
    #endregion

    #region Game Flow
    /// <summary>
    /// Starts the battle phase
    /// </summary>
    public void StartBattle()
    {
        if (showDebugInfo)
        {
            Debug.Log("Starting battle");
        }

        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry("Battle starting! Player turn first.");
        }

        if (audioManager != null)
        {
            audioManager.PlayGameStateMusic(GameState.PlayerTurn);
        }

        SetGameState(GameState.PlayerTurn);
        StartPlayerTurn();
    }

    /// <summary>
    /// Sets up and begins the player's turn
    /// </summary>
    private void StartPlayerTurn()
    {
        Debug.Log("Starting player turn");

        _unitsActedThisTurn.Clear();

        int unitsReadied = 0;
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.isAlive)
            {
                unit.DecreaseCooldown();
                unitsReadied++;

                Debug.Log($"Decreased cooldown for {unit.unitName}. Current cooldown: {unit.currentCooldown}");

                // Log ability status
                if (unit.CanUseAbility())
                {
                    Debug.Log($"{unit.unitName}'s ability is ready");
                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.AddLogEntry($"{unit.unitName}'s ability is ready");
                    }
                }
            }
        }

        Debug.Log($"Readied {unitsReadied} player units for the turn");

        foreach (var unit in playerUnits)
        {
            if (unit != null && unit is Knight knight && unit.isAlive)
            {
                knight.StartTurn();
            }
        }

        SetNextActivePlayerUnit();
    }

    /// <summary>
    /// Sets the next available player unit as active
    /// </summary>
    public void SetNextActivePlayerUnit()
    {
        Debug.Log("Setting next active player unit...");

        // If no current active unit, start from beginning
        int startIndex = 0;

        // If we have a current active unit, find its index to start search from next unit
        // and add current unit to acted list if it's not already there
        if (currentActiveUnit != null)
        {
            startIndex = playerUnits.IndexOf(currentActiveUnit) + 1;
            Debug.Log($"Current active unit: {currentActiveUnit.name} at index {startIndex - 1}");

            // Mark current unit as having acted this turn
            if (!_unitsActedThisTurn.Contains(currentActiveUnit))
            {
                _unitsActedThisTurn.Add(currentActiveUnit);
                Debug.Log($"Marked {currentActiveUnit.name} as having acted this turn");
            }
        }

        // Clear current active unit
        currentActiveUnit = null;

        // Look for next living unit that hasn't acted yet
        for (int i = 0; i < playerUnits.Count; i++)
        {
            // Get the next unit in circular order
            int indexToCheck = (startIndex + i) % playerUnits.Count;
            PlayerUnit unit = playerUnits[indexToCheck];

            // Check if unit is alive AND hasn't acted this turn
            if (unit != null && unit.isAlive && !_unitsActedThisTurn.Contains(unit))
            {
                currentActiveUnit = unit;
                Debug.Log($"Found next active unit: {unit.name} at index {indexToCheck}");
                break;
            }
        }

        if (currentActiveUnit != null)
        {

            if (uiManager != null)
            {
                uiManager.UpdateUnitUI(currentActiveUnit);
            }

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"Active unit: {currentActiveUnit.unitName}");
                GameInfoLayer.Instance.UpdateBattleInfo();
            }

            if (audioManager != null)
            {
                audioManager.PlayButtonClickSound();
            }
        }
        else
        {
            // No living units found that haven't acted yet, end player turn
            Debug.Log("No more units that haven't acted, ending player turn");
            EndPlayerTurn();
        }
    }

    /// <summary>
    /// Ends the player's turn and starts the monster turn
    /// </summary>
    public void EndPlayerTurn()
    {
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.isAlive)
            {
                // Knights have a special EndTurn method to handle shield duration
                if (unit is Knight knight)
                {
                    knight.EndTurn();
                }
            }
        }

        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry("==== PLAYER TURN ENDING ====");
            GameInfoLayer.Instance.AddLogEntry("Transitioning to monster turn...");
        }

        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        SetGameState(GameState.MonsterTurn);
        StartMonsterTurn();
    }

    /// <summary>
    /// Executes the monster turn
    /// </summary>
    private void StartMonsterTurn()
    {
        int aliveMonsters = 0;

        foreach (var monster in monsterUnits)
        {
            if (monster != null && monster.isAlive)
            {
                aliveMonsters++;
            }
        }

        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"==== MONSTER TURN STARTING ====");
            GameInfoLayer.Instance.AddLogEntry($"Active monsters: {aliveMonsters}");
        }

        foreach (var monster in monsterUnits)
        {
            if (monster != null && monster.isAlive)
            {
                if (monster is Troll)
                {
                    GameInfoLayer.Instance?.AddLogEntry($"Troll {monster.unitName} starting turn");
                    ((Troll)monster).StartTurn();
                }
                else if (monster is SkeletonArcher)
                {
                    GameInfoLayer.Instance?.AddLogEntry($"Archer {monster.unitName} starting turn");
                    ((SkeletonArcher)monster).StartTurn();
                }
            }
        }

        // Process monster actions with a slight delay between each
        StartCoroutine(ProcessMonsterActions());
    }

    /// <summary>
    /// Coroutine to process monster actions with slight delays
    /// </summary>
    private IEnumerator ProcessMonsterActions()
    {
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry("Monsters choosing targets...");
        }

        yield return new WaitForSeconds(0.5f);

        int attackingMonsters = 0;
        int successfulAttacks = 0;

        // Process each monster action
        foreach (var monster in monsterUnits)
        {
            if (monster != null && monster.isAlive)
            {
                attackingMonsters++;

                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"-- {monster.unitName}'s turn --");
                }

                // First, handle special monster-specific "StartTurn" behavior
                if (monster is Troll trollUnit)
                {
                    trollUnit.StartTurn();
                    // Add delay for regeneration animation to complete
                    yield return new WaitForSeconds(1.0f);
                }
                else if (monster is SkeletonArcher archerUnit)
                {
                    archerUnit.StartTurn();
                    yield return new WaitForSeconds(0.3f);
                }

                // Visual indicator that this monster is acting
                StartCoroutine(HighlightActiveMonster(monster.transform, 0.5f));

                // Small delay before action
                yield return new WaitForSeconds(0.3f);

                int alivePlayerUnits = 0;
                foreach (var player in playerUnits)
                {
                    if (player != null && player.isAlive)
                        alivePlayerUnits++;
                }

                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"Available targets: {alivePlayerUnits}");
                }

                // Select target using the monster's targeting logic
                PlayerUnit target = monster.SelectTarget(playerUnits.ToArray());

                if (target != null)
                {
                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.AddLogEntry($"{monster.unitName} targeting {target.unitName}...");
                        GameInfoLayer.Instance.AddLogEntry($"Target health before: {target.currentHealth}/{target.maxHealth}");
                    }

                    // Show line connecting attacker to target
                    if (CombatSystem.Instance != null)
                    {
                        CombatSystem.Instance.ShowAttackLine(monster.transform.position, target.transform.position, false);
                    }
                    else
                    {
                        ShowAttackLine(monster.transform.position, target.transform.position);
                    }

                    // Small delay before attack
                    yield return new WaitForSeconds(0.3f);

                    // Get expected damage for monsters that track it
                    int expectedDamage = 0;
                    if (monster is SkeletonArcher skeletonArcher)
                    {
                        // Check if using volley this turn
                        expectedDamage = skeletonArcher.PerformAttack(target);
                    }
                    else if (monster is Troll trollAttacker)
                    {
                        // Check if troll can attack this turn
                        expectedDamage = trollAttacker.PerformAttack(target);
                    }
                    else if (monster is DarkWizard wizard)
                    {
                        // Different spell types
                        expectedDamage = wizard.PerformAttack(target);
                    }
                    else
                    {
                        expectedDamage = monster.PerformAttack(target);
                    }

                    // Attack - starts the animation sequence
                    // Damage is applied after animation plays through the coroutine
                    monster.Attack(target);

                    // For Dark Wizard's AoE spell and Skeleton Archer's volley, we wait longer
                    if (monster is DarkWizard || monster is SkeletonArcher)
                    {
                        yield return new WaitForSeconds(2.0f);
                    }
                    else
                    {
                        // Wait for attack animation and effects to complete
                        yield return new WaitForSeconds(1.0f);
                    }

                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.AddLogEntry($"Attack complete - Expected damage: {expectedDamage}");
                        GameInfoLayer.Instance.AddLogEntry($"Target health after: {target.currentHealth}/{target.maxHealth}");
                    }

                    if (expectedDamage > 0)
                    {
                        successfulAttacks++;
                    }

                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.AddLogEntry($"ERROR: {monster.unitName} couldn't find a target!");
                    }

                    // Small delay even if no target found
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"==== MONSTER TURN SUMMARY ====");
            GameInfoLayer.Instance.AddLogEntry($"Monsters that acted: {attackingMonsters}");
            GameInfoLayer.Instance.AddLogEntry($"Successful attacks: {successfulAttacks}");
            GameInfoLayer.Instance.AddLogEntry("Monster turn complete");
        }

        EndMonsterTurn();
    }

    // Visual indicator for active monster
    private IEnumerator HighlightActiveMonster(Transform monsterTransform, float duration)
    {
        // Create a highlight ring
        GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        highlight.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        highlight.transform.position = monsterTransform.position - new Vector3(0, 0.05f, 0);

        // Set material to a glowing color
        Renderer renderer = highlight.GetComponent<Renderer>();
        renderer.material.color = new Color(1f, 0.5f, 0.2f, 0.7f);
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.2f, 0.7f));

        // Remove collider
        Destroy(highlight.GetComponent<Collider>());

        // Keep visible for specified duration
        yield return new WaitForSeconds(duration);

        // Clean up
        Destroy(highlight);
    }

    // Visual line showing who is attacking whom
    private void ShowAttackLine(Vector3 from, Vector3 to)
    {
        GameObject lineObj = new GameObject("AttackLine");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        // Configure line
        line.startWidth = 0.03f;
        line.endWidth = 0.01f;
        line.positionCount = 2;

        // Elevate line slightly to ensure visibility
        Vector3 fromPos = from + Vector3.up * 0.05f;
        Vector3 toPos = to + Vector3.up * 0.05f;

        // Set positions
        line.SetPosition(0, fromPos);
        line.SetPosition(1, toPos);

        // Red color for attack
        line.startColor = Color.red;
        line.endColor = Color.red;

        // Create material
        line.material = new Material(Shader.Find("Sprites/Default"));

        // Destroy after short delay
        Destroy(lineObj, 0.5f);
    }

    // Create hit effect at target
    private void PlayHitEffect(Vector3 position)
    {
        // Only create effect if we have the combat system with an effect
        if (CombatSystem.Instance != null && CombatSystem.Instance.attackEffectPrefab != null)
        {
            CombatSystem.Instance.PlayCombatEffectAt(CombatSystem.Instance.attackEffectPrefab, position);
        }
        else
        {
            // Simple fallback effect
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * 0.2f;

            Renderer renderer = effect.GetComponent<Renderer>();
            renderer.material.color = Color.red;

            // Remove collider
            Destroy(effect.GetComponent<Collider>());

            // Destroy after animation
            StartCoroutine(AnimateHitEffect(effect.transform));
        }
    }

    private IEnumerator AnimateHitEffect(Transform effectTransform)
    {
        Vector3 originalScale = effectTransform.localScale;
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Scale up and then fade out
            effectTransform.localScale = originalScale * (1f + t);

            // Update renderer color for fade out
            Renderer renderer = effectTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1f - t;
                renderer.material.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy the object
        Destroy(effectTransform.gameObject);
    }

    /// <summary>
    /// Ends the monster turn and checks game status
    /// </summary>
    private void EndMonsterTurn()
    {
        CheckBattleStatus();

        if (currentState != GameState.Victory && currentState != GameState.Defeat)
        {
            SetGameState(GameState.PlayerTurn);
            StartPlayerTurn();
        }
    }

    /// <summary>
    /// Checks if either side has been defeated
    /// </summary>
    public void CheckBattleStatus()
    {
        // Check for player defeat
        bool anyPlayerAlive = false;
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.isAlive)
            {
                anyPlayerAlive = true;
                break;
            }
        }

        if (!anyPlayerAlive)
        {
            SetGameState(GameState.Defeat);

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry("DEFEAT! All player units have been defeated.");
            }

            // Play defeat sound
            if (audioManager != null)
            {
                audioManager.PlayDefeatSound();
            }

            if (uiManager != null)
            {
                uiManager.ShowGameOverScreen(false);
            }
            return;
        }

        // Check for player victory
        bool anyMonsterAlive = false;
        foreach (var monster in monsterUnits)
        {
            if (monster != null && monster.isAlive)
            {
                anyMonsterAlive = true;
                break;
            }
        }

        if (!anyMonsterAlive)
        {
            SetGameState(GameState.Victory);

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry("VICTORY! All monsters have been defeated.");
            }

            // Play victory sound
            if (audioManager != null)
            {
                audioManager.PlayVictorySound();
            }

            if (uiManager != null)
            {
                uiManager.ShowGameOverScreen(true);
            }
        }
    }

    /// <summary>
    /// Changes the game state and logs it
    /// </summary>
    private void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            GameState oldState = currentState;

            currentState = newState;

            if (showDebugInfo)
            {
                Debug.Log($"Game state changed from {oldState} to {newState}");
            }

            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.UpdateGameStateInfo(newState);
            }

            if (audioManager != null)
            {
                audioManager.PlayGameStateMusic(newState);
            }
        }
    }
    #endregion

    /// <summary>
    /// Resets the game to setup state
    /// </summary>
    public void RestartGame()
    {
        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry("Game restarting...");
        }

        // Clear all units
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.gameObject != null)
            {
                Destroy(unit.gameObject);
            }
        }
        playerUnits.Clear();

        foreach (var monster in monsterUnits)
        {
            if (monster != null && monster.gameObject != null)
            {
                Destroy(monster.gameObject);
            }
        }
        monsterUnits.Clear();

        // Clear references
        currentActiveUnit = null;

        // Reset debug positions
        for (int i = 0; i < debugSpawnPositions.Length; i++)
        {
            debugSpawnPositions[i] = Vector3.zero;
        }

        // Return to setup state
        SetGameState(GameState.Setup);

        // Hide UI
        if (uiManager != null)
        {
            uiManager.HideAllPanels();
        }

        // Reset info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.ClearAllInfo();
            GameInfoLayer.Instance.AddLogEntry("Game restarted. Place your cards to begin.");
        }

        // Play menu music
        if (audioManager != null)
        {
            audioManager.PlayMusic(audioManager.menuTheme);
        }

        if (showDebugInfo)
        {
            Debug.Log("Game restarted");
        }
    }
}