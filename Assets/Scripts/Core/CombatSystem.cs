using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Add this import

/// <summary>
/// CombatSystem handles player targeting and combat actions
/// </summary>
public class CombatSystem : MonoBehaviour
{
    #region Singleton
    public static CombatSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [Header("Targeting")]
    public Color normalTargetColor = Color.yellow;
    public Color abilityTargetColor = Color.cyan;
    public float targetSelectionMaxDistance = 100f; // Max distance for raycasting

    [Header("Visual Feedback")]
    public GameObject attackEffectPrefab;
    public GameObject abilityEffectPrefab;
    public GameObject targetSelectionIndicatorPrefab; // Assign a circular arrow prefab in the editor
    private GameObject currentSelectionIndicator;

    // Private state
    private bool isSelectingTarget = false;
    private bool isUsingAbility = false;
    private List<Renderer> highlightedRenderers = new List<Renderer>();

    private void Start()
    {
        // Get the layers that monsters are on
        int monsterLayer = LayerMask.NameToLayer("Default");

        // Set a debug message
        Debug.Log($"Monster layer: {monsterLayer}. Ensure this layer is enabled in Physics settings.");
    }

    private void Update()
    {
        // Handle target selection with both mouse and touch support
        if (isSelectingTarget)
        {
            bool inputDetected = false;
            Vector2 inputPosition = Vector2.zero;

            // Check for mouse input (for testing in editor)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                inputDetected = true;
                inputPosition = Mouse.current.position.ReadValue();
            }

            // Check for touch input (for mobile)
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                inputDetected = true;
                inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            if (inputDetected)
            {
                // Process targeting with the input position
                HandleTargetSelection(inputPosition);
            }
        }
    }

    private void HandleTargetSelection(Vector2 screenPosition)
    {
        DebugRaycastVisualization(screenPosition);

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        RaycastHit[] hits = Physics.RaycastAll(ray, targetSelectionMaxDistance);

        Debug.Log($"Raycast detected {hits.Length} hits");

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool foundTarget = false;

        foreach (var hit in hits)
        {
            Debug.Log($"Evaluating hit on: {hit.collider.gameObject.name} at position {hit.point}");

            // Try to find monster directly or in parent
            MonsterUnit monster = hit.collider.GetComponent<MonsterUnit>();
            if (monster == null)
            {
                monster = hit.collider.GetComponentInParent<MonsterUnit>();
            }

            if (monster != null && monster.isAlive)
            {

                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"Selected target: {monster.unitName}");
                }

                ClearHighlightedTargets();

                if (isUsingAbility)
                {
                    UseAbilityOnTarget(monster);
                }
                else
                {
                    AttackTarget(monster);
                }

                // End selection mode
                isSelectingTarget = false;

                // Hide target panel
                if (GameManager.Instance?.uiManager != null)
                    GameManager.Instance.uiManager.targetSelectionPanel.SetActive(false);

                // Wait for attack/ability animation to complete before moving to next unit
                float actionDelay = isUsingAbility ? 2.5f : 1.5f;
                StartCoroutine(DelayedNextUnit(actionDelay));

                foundTarget = true;
                break;
            }
        }

        if (!foundTarget)
        {
            // Provide feedback to player
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry("Invalid target - tap on a glowing enemy");
            }
        }
    }

    /// <summary>
    /// Debug visualization for raycasts to help troubleshoot targeting
    /// </summary>
    public void DebugRaycastVisualization(Vector2 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Debug.DrawRay(ray.origin, ray.direction * targetSelectionMaxDistance, Color.green, 3f);

        // Create a temporary visual indicator
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.transform.localScale = Vector3.one * 0.05f;
        debugSphere.GetComponent<Renderer>().material.color = Color.green;

        // Position it at ray origin
        debugSphere.transform.position = ray.origin;

        // Create another for ray end
        GameObject debugSphereEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphereEnd.transform.localScale = Vector3.one * 0.05f;
        debugSphereEnd.GetComponent<Renderer>().material.color = Color.red;

        // Position it at ray end
        debugSphereEnd.transform.position = ray.origin + ray.direction * targetSelectionMaxDistance;

        // Destroy after 3 seconds
        Destroy(debugSphere, 3f);
        Destroy(debugSphereEnd, 3f);

        // Log raycast points for debugging
        Debug.Log($"Raycast from {ray.origin} in direction {ray.direction}");

        // Also log hits
        RaycastHit[] hits = Physics.RaycastAll(ray, targetSelectionMaxDistance);
        foreach (var hit in hits)
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name} at distance {hit.distance}");

            // Create hit point visualization
            GameObject hitSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitSphere.transform.localScale = Vector3.one * 0.02f;
            hitSphere.GetComponent<Renderer>().material.color = Color.yellow;
            hitSphere.transform.position = hit.point;
            Destroy(hitSphere, 3f);
        }
    }

    private IEnumerator DelayedNextUnit(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayerUnit activeUnit = GameManager.Instance.currentActiveUnit;

        if (activeUnit != null)
        {
            GameManager.Instance.SetNextActivePlayerUnit();
        }
        else
        {
            // Failsafe in case of error
            GameManager.Instance.EndPlayerTurn();
        }
    }

    public class RotateObject : MonoBehaviour
    {
        public float rotationSpeed = 90f; // degrees per second

        private void Update()
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    // Method to hide the indicator:
    private void HideSelectionIndicator()
    {
        if (currentSelectionIndicator != null)
        {
            Destroy(currentSelectionIndicator);
            currentSelectionIndicator = null;
        }
    }

    private void OnDisable()
    {
        HideSelectionIndicator();
    }

    public void CancelTargetSelection()
    {
        isSelectingTarget = false;
        HideSelectionIndicator();
        ClearHighlightedTargets();

        // Show action panel again
        if (GameManager.Instance?.uiManager != null)
        {
            GameManager.Instance.uiManager.targetSelectionPanel.SetActive(false);
            GameManager.Instance.uiManager.actionPanel.SetActive(true);
        }

        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry("Target selection canceled");
        }
    }

    /// <summary>
    /// Shows a selection indicator above a potential target
    /// </summary>
    private void ShowSelectionIndicator(Transform targetTransform)
    {
        // Remove any existing indicator
        HideSelectionIndicator();

        // If no prefab, try to create a simple one
        if (targetSelectionIndicatorPrefab == null)
        {
            targetSelectionIndicatorPrefab = new GameObject("DefaultSelectionIndicator");
            var line = targetSelectionIndicatorPrefab.AddComponent<LineRenderer>();
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
            line.positionCount = 20;
            line.useWorldSpace = false;

            // Create a circle
            for (int i = 0; i < 20; i++)
            {
                float angle = i * Mathf.PI * 2 / 20;
                float x = Mathf.Sin(angle) * 0.3f;
                float z = Mathf.Cos(angle) * 0.3f;
                line.SetPosition(i, new Vector3(x, 0.5f, z));
            }

            // Set color
            line.material = new Material(Shader.Find("Standard"));
            line.material.color = Color.red;
        }

        // Create the indicator
        currentSelectionIndicator = Instantiate(targetSelectionIndicatorPrefab);
        currentSelectionIndicator.transform.SetParent(targetTransform);
        currentSelectionIndicator.transform.localPosition = Vector3.up * 0.5f; // Position above target

        // Add rotation animation
        var rotator = currentSelectionIndicator.AddComponent<RotateObject>();
        rotator.rotationSpeed = 90f; // 90 degrees per second
    }

    /// <summary>
    /// Start target selection mode for attack or ability use
    /// </summary>
    public void StartTargetSelection(bool forAbility)
    {
        isSelectingTarget = true;
        isUsingAbility = forAbility;

        HighlightValidTargets(forAbility);

        if (GameInfoLayer.Instance != null)
        {
            if (forAbility)
            {
                GameInfoLayer.Instance.AddLogEntry("Select target for ability");
            }
            else
            {
                GameInfoLayer.Instance.AddLogEntry("Select target for attack");
            }
        }
    }

    /// <summary>
    /// Highlights valid targets for selection
    /// </summary>
    private void HighlightValidTargets(bool forAbility)
    {
        ClearHighlightedTargets();

        // Get all monster units
        MonsterUnit[] monsters = FindObjectsByType<MonsterUnit>(FindObjectsSortMode.None);
        Debug.Log($"Found {monsters.Length} monsters to highlight");

        foreach (MonsterUnit monster in monsters)
        {

            if (monster != null && monster.isAlive)
            {
                // Highlight the monster itself
                Renderer[] monsterRenderers = monster.GetComponentsInChildren<Renderer>();
                foreach (Renderer monsterRenderer in monsterRenderers)
                {
                    if (monsterRenderer != null)
                    {
                        // Store original material color
                        Color originalColor = monsterRenderer.material.color;

                        // Enable emission for glow effect
                        monsterRenderer.material.EnableKeyword("_EMISSION");
                        Color glowColor = forAbility ? abilityTargetColor : normalTargetColor;
                        monsterRenderer.material.SetColor("_EmissionColor", glowColor * 0.5f);

                        // Add to list for cleanup
                        highlightedRenderers.Add(monsterRenderer);
                    }
                }

                // Create highlight sphere above monster
                GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                highlight.name = $"TargetHighlight_{monster.name}";
                highlight.tag = "TargetHighlight";
                // Make it a child of the monster but floating above it
                highlight.transform.SetParent(monster.transform);
                highlight.transform.localPosition = new Vector3(0, 0.5f, 0); // Above monster
                highlight.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Large for visibility

                // Set material properties
                Renderer renderer = highlight.GetComponent<Renderer>();

                // Set bright color with high emission
                Color highlightColor = forAbility ? abilityTargetColor : normalTargetColor;
                renderer.material.color = new Color(
                    highlightColor.r * 2.0f,
                    highlightColor.g * 2.0f,
                    highlightColor.b * 2.0f,
                    0.7f);

                // Make it glow
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", highlightColor * 3.0f); // Very bright emission

                // Make it transparent
                renderer.material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.DisableKeyword("_ALPHATEST_ON");
                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                renderer.material.renderQueue = 3000;

                // IMPORTANT: Remove collider to avoid interferring with raycasts
                Collider highlightCollider = highlight.GetComponent<Collider>();
                if (highlightCollider != null)
                {
                    Destroy(highlightCollider);
                }

                // Add pulsating effect
                StartCoroutine(PulseHighlight(renderer));

                // Store for cleanup
                highlightedRenderers.Add(renderer);

                Debug.Log($"Created highlight for monster: {monster.name}");
            }
        }

        // Also inform player via UI
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry(forAbility ?
                "Select target for ability (tap on a glowing enemy)" :
                "Select target for attack (tap on a glowing enemy)");
        }
    }

    /// <summary>
    /// Clear all highlighted targets
    /// </summary>
    private void ClearHighlightedTargets()
    {
        // Find all highlight objects and destroy them
        GameObject[] highlights = GameObject.FindGameObjectsWithTag("TargetHighlight");
        foreach (GameObject highlight in highlights)
        {
            Destroy(highlight);
        }

        // Reset any renderer effects
        foreach (Renderer renderer in highlightedRenderers)
        {
            if (renderer != null)
            {
                // Reset material color
                renderer.material.color = Color.white;

                // Disable emission
                renderer.material.DisableKeyword("_EMISSION");
            }
        }

        highlightedRenderers.Clear();
    }
    private IEnumerator PulseHighlight(Renderer highlightRenderer)
    {
        float time = 0;
        Color originalColor = highlightRenderer.material.color;

        while (highlightRenderer != null)
        {
            time += Time.deltaTime;
            float pulse = 0.7f + 0.3f * Mathf.Sin(time * 5f);

            if (highlightRenderer != null && highlightRenderer.material != null)
            {
                Color newColor = new Color(
                    originalColor.r * pulse,
                    originalColor.g * pulse,
                    originalColor.b * pulse,
                    originalColor.a);

                highlightRenderer.material.color = newColor;

                // Also pulse emission
                highlightRenderer.material.SetColor("_EmissionColor", newColor * 0.8f);
            }

            yield return null;
        }
    }

    /// <summary>
    /// Attack a target - updated to coordinate with animations
    /// </summary>
    private void AttackTarget(Unit target)
    {
        PlayerUnit activeUnit = GameManager.Instance.currentActiveUnit;
        if (activeUnit != null && target != null)
        {
            Debug.Log($"{activeUnit.unitName} is attacking {target.unitName}");

            // Show attack line between unit and target
            ShowAttackLine(activeUnit.transform.position, target.transform.position, false);

            activeUnit.Attack(target);

        }
        else
        {
            Debug.LogError("Attack failed: activeUnit or target is null");
            if (activeUnit == null) Debug.LogError("currentActiveUnit is null");
            if (target == null) Debug.LogError("target is null");
        }
    }

    /// <summary>
    /// Use ability on target - updated to coordinate with animations
    /// </summary>
    private void UseAbilityOnTarget(Unit target)
    {
        PlayerUnit activeUnit = GameManager.Instance.currentActiveUnit;
        if (activeUnit != null && target != null)
        {
            Debug.Log($"{activeUnit.unitName} is using ability on {target.unitName}");

            if (!activeUnit.CanUseAbility())
            {
                Debug.LogWarning($"{activeUnit.unitName} cannot use ability: cooldown={activeUnit.currentCooldown}");

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"{activeUnit.unitName} cannot use ability: cooldown={activeUnit.currentCooldown}");
                }
                return;
            }

            // Show ability effect line/arc between unit and target(s)
            ShowAttackLine(activeUnit.transform.position, target.transform.position, true);

            // Handle different abilities based on unit type
            if (activeUnit is Warrior)
            {
                // Whirlwind hits all monsters
                activeUnit.UseAbility(GameManager.Instance.monsterUnits.ToArray());
            }
            else if (activeUnit is Mage)
            {
                // Fireball hits targeted monster and adjacent monsters
                List<Unit> targets = new List<Unit>();
                targets.Add(target);

                // Find adjacent monsters
                List<MonsterUnit> adjacentMonsters = GetAdjacentMonsters(target as MonsterUnit);
                foreach (MonsterUnit adjacent in adjacentMonsters)
                {
                    targets.Add(adjacent);

                    // Show splash effect lines
                    ShowAttackLine(target.transform.position, adjacent.transform.position, true, 0.5f);
                }

                activeUnit.UseAbility(targets.ToArray());
            }
            else
            {
                // Single target abilities (Knight, Archer, Rogue)
                activeUnit.UseAbility(new Unit[] { target });

            }

        }
        else
        {
            Debug.LogError("UseAbility failed: activeUnit or target is null");
        }
    }

    /// <summary>
    /// Find adjacent monsters to a target monster
    /// </summary>
    private List<MonsterUnit> GetAdjacentMonsters(MonsterUnit target)
    {
        List<MonsterUnit> adjacentMonsters = new List<MonsterUnit>();

        if (target == null)
            return adjacentMonsters;

        // Get all monster units
        MonsterUnit[] allMonsters = FindObjectsByType<MonsterUnit>(FindObjectsSortMode.None);

        // Find monsters that are nearby
        float adjacencyDistance = 0.4f;

        foreach (MonsterUnit monster in allMonsters)
        {
            if (monster != target && monster.isAlive)
            {
                float distance = Vector3.Distance(monster.transform.position, target.transform.position);

                if (distance <= adjacencyDistance)
                {
                    adjacentMonsters.Add(monster);
                }
            }
        }

        return adjacentMonsters;
    }

    /// <summary>
    /// Show a line between attacker and target
    /// </summary>
    public void ShowAttackLine(Vector3 from, Vector3 to, bool isAbility, float duration = 0.7f)
    {
        GameObject lineObj = new GameObject("AttackLine");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        // line config
        line.startWidth = isAbility ? 0.05f : 0.03f;
        line.endWidth = 0.01f;
        line.positionCount = 2;

        // Elevate line slightly to ensure visibility
        Vector3 fromPos = from + Vector3.up * 0.05f;
        Vector3 toPos = to + Vector3.up * 0.05f;

        line.SetPosition(0, fromPos);
        line.SetPosition(1, toPos);

        Color lineColor = isAbility ? abilityTargetColor : Color.red;
        line.startColor = lineColor;
        line.endColor = lineColor;

        line.material = new Material(Shader.Find("Sprites/Default"));

        StartCoroutine(AnimateAttackLine(line, duration));
    }

    // Animate attack line to fade out
    private IEnumerator AnimateAttackLine(LineRenderer line, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration && line != null)
        {
            float t = elapsedTime / duration;

            // Fade transparency
            Color startColor = line.startColor;
            Color endColor = line.endColor;

            startColor.a = 1f - t;
            endColor.a = 1f - t;

            line.startColor = startColor;
            line.endColor = endColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the line object when done
        if (line != null)
            Destroy(line.gameObject);
    }

    /// <summary>
    /// Play a combat effect at a position
    /// </summary>
    public void PlayCombatEffectAt(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

            // Auto-destroy after a set time
            Destroy(effect, 2.0f);
        }
        else
        {
            // Create a simple effect if no prefab is available
            GameObject simpleEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            simpleEffect.transform.position = position;
            simpleEffect.transform.localScale = Vector3.one * 0.2f;

            // Set material for effect
            Renderer renderer = simpleEffect.GetComponent<Renderer>();
            renderer.material.color = Color.yellow;

            // Remove collider
            Destroy(simpleEffect.GetComponent<Collider>());

            // Add animation
            StartCoroutine(AnimateEffect(simpleEffect.transform));
        }
    }

    private IEnumerator AnimateEffect(Transform effectTransform)
    {
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Scale up and fade out
            effectTransform.localScale = Vector3.one * 0.2f * (1 + t);

            // Fade out color
            Renderer renderer = effectTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1 - t;
                renderer.material.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy when animation is complete
        Destroy(effectTransform.gameObject);
    }
}