using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base class for all units in the game (players and monsters)
/// </summary>
public class Unit : MonoBehaviour
{
    // Unit properties
    public string unitName;
    public int maxHealth;
    public int currentHealth;
    public int attackDamage;
    public bool isAlive = true;

    // Animation timing properties
    [Header("Animation Timing")]
    public float attackAnimationDelay = 0.5f; // Time to wait for attack animation to "hit"
    public float abilityAnimationDelay = 0.7f; // Time to wait for ability animation to take effect
    public float deathAnimationDuration = 1.0f; // How long death animation plays

    // Unit type for targeting purposes
    public enum UnitType
    {
        Melee,
        Ranged,
        Tank,
        Spellcaster,
        Assassin
    }

    // Reference to the animator component
    protected Animator animator;

    // For animation hit coordination
    protected Unit _currentTarget;
    protected int _pendingDamage;

    void Awake()
    {
        // Get the animator component
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        if (animator != null)
        {
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"[{unitName}] No runtime animator controller assigned!");
            }
            else
            {
                Debug.Log($"[{unitName}] Using animator controller: {animator.runtimeAnimatorController.name}");
            }
        }
        else
        {
            Debug.LogError($"[{unitName}] No animator component found!");
        }
    }

    public UnitType unitType;

    /// <summary>
    /// Deal damage to a target unit - now uses coroutine for timing
    /// </summary>
    public virtual void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            Debug.Log(unitName + " starts attack animation against " + target.unitName);
            StartCoroutine(PlayAttackAnimationWithTiming(target, attackDamage));
        }
    }

    /// <summary>
    /// Coroutine to handle attack animation timing
    /// </summary>
    protected virtual IEnumerator PlayAttackAnimationWithTiming(Unit target, int damage)
    {
        // Store pending damage info
        _currentTarget = target;
        _pendingDamage = damage;

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for animation to reach the "hit" point
        yield return new WaitForSeconds(attackAnimationDelay);

        // Now apply damage if target is still valid
        if (target != null && target.isAlive)
        {
            ApplyDamage(target, damage);
        }

        // Clear pending attack
        _currentTarget = null;
    }

    /// <summary>
    /// Can be called by animation events when hit frame is reached
    /// </summary>
    public void OnAnimationHitFrame()
    {
        if (_currentTarget != null && _currentTarget.isAlive)
        {
            ApplyDamage(_currentTarget, _pendingDamage);
            _currentTarget = null; // Clear after applying
        }
    }

    /// <summary>
    /// Helper method to apply damage separately from playing animation
    /// </summary>
    protected virtual void ApplyDamage(Unit target, int damage)
    {
        Debug.Log(unitName + " deals " + damage + " damage to " + target.unitName);
        target.TakeDamage(damage);

        // Update game info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "attacks", damage);
        }
    }

    /// <summary>
    /// Take damage and update health
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive)
            return;

        // Play take damage animation
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Play sound effect for taking damage
        if (AudioManager.Instance != null && damage > 0)
        {
            // Use human growl for player units, monster growl for monster units
            if (this is PlayerUnit)
                AudioManager.Instance.PlayHumanSound();
            else if (this is MonsterUnit)
                AudioManager.Instance.PlayMonsterAttackSound();
        }

        // Log before damage is applied
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"DAMAGE: {unitName} taking {damage} damage (HP: {currentHealth}/{maxHealth})");
        }

        currentHealth -= damage;

        // Log after damage is applied
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"RESULT: {unitName} new HP: {currentHealth}/{maxHealth}");
        }

        // Add visual damage effect
        ShowDamageEffect(damage);

        // Check if unit died
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    /// <summary>
    /// Handle unit death
    /// </summary>
    protected virtual void Die()
    {
        isAlive = false;

        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log(unitName + " has been defeated!");

        // Start death effects with timing
        StartCoroutine(PlayDeathEffects());

        // Update game state check
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckBattleStatus();
        }
    }

    /// <summary>
    /// Play death effects with appropriate timing
    /// </summary>
    protected virtual IEnumerator PlayDeathEffects()
    {
        // Play death sound based on unit type
        if (AudioManager.Instance != null)
        {
            if (this is PlayerUnit)
                AudioManager.Instance.PlayHumanSound();
            else if (this is MonsterUnit)
                AudioManager.Instance.PlayMonsterAttackSound();
        }

        // Visual death effect - fade out
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.material != null)
            {
                Color fadedColor = r.material.color;
                fadedColor.a = 0.5f;
                r.material.color = fadedColor;
            }
        }

        // Wait for death animation to finish
        yield return new WaitForSeconds(deathAnimationDuration);

        // Further visual changes can be applied here after animation completes
    }

    /// <summary>
    /// Use a special ability on target(s)
    /// </summary>
    public virtual void UseAbility(Unit[] targets)
    {
        // Play ability animation
        if (animator != null)
        {
            animator.SetTrigger("Ability");
        }

        // Override in derived classes
        Debug.Log(unitName + " uses an ability!");
    }

    // New method for damage visual feedback
    protected virtual void ShowDamageEffect(int damageAmount)
    {
        // Flash the model red
        StartCoroutine(FlashEffect());

        // Show floating damage text
        ShowFloatingDamageText(damageAmount);
    }

    // Flash the model briefly
    private IEnumerator FlashEffect()
    {
        // Get all renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Store original materials
        Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Store the first material (main material)
                originalMaterials[renderer] = new Material(renderer.material);
            }
        }

        // Create flash effect (red tint)
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Apply red flash color
                renderer.material.color = Color.red;

                // Add emission if possible
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", Color.red * 0.5f);
            }
        }

        // Wait for a short duration
        yield return new WaitForSeconds(0.1f);

        // Restore original materials
        foreach (var renderer in renderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer))
            {
                renderer.material.color = originalMaterials[renderer].color;

                // Reset emission
                if (originalMaterials[renderer].IsKeywordEnabled("_EMISSION"))
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", originalMaterials[renderer].GetColor("_EmissionColor"));
                }
                else
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    // Show floating damage number
    private void ShowFloatingDamageText(int damageAmount)
    {
        // Create a TextMesh for the damage number
        GameObject damageTextObj = new GameObject($"DamageText_{damageAmount}");
        damageTextObj.transform.position = transform.position + Vector3.up * 0.5f;

        // Add TextMesh component
        TextMesh textMesh = damageTextObj.AddComponent<TextMesh>();
        textMesh.text = damageAmount.ToString();
        textMesh.fontSize = 48;
        textMesh.color = Color.red;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        // Make it face the camera
        damageTextObj.AddComponent<Billboard>();

        // Add animation
        StartCoroutine(AnimateDamageText(damageTextObj.transform));

        // Destroy after animation finishes
        Destroy(damageTextObj, 1.0f);
    }

    // Animation for damage text
    private IEnumerator AnimateDamageText(Transform textTransform)
    {
        Vector3 startPos = textTransform.position;
        Vector3 endPos = startPos + Vector3.up * 0.5f;
        float duration = 1.0f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Move upward
            textTransform.position = Vector3.Lerp(startPos, endPos, t);

            // Scale up and then down
            float scale = 1.0f;
            if (t < 0.5f)
                scale = 1.0f + t; // Scale up to 1.5
            else
                scale = 2.0f - t; // Scale back down

            textTransform.localScale = Vector3.one * scale;

            // Fade out toward the end
            TextMesh textMesh = textTransform.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1.0f - (t * t); // Quadratic fade out
                textMesh.color = color;
            }

            // Update elapsed time
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}