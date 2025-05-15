using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all monster units
/// </summary>
public class MonsterUnit : Unit
{
    public float aggressiveness = 0.5f; // Affects target selection

    /// <summary>
    /// Select a target from available player units
    /// </summary>
    public virtual PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        // Filter for alive targets
        List<PlayerUnit> aliveTargets = new List<PlayerUnit>();

        foreach (PlayerUnit target in possibleTargets)
        {
            if (target != null && target.isAlive)
                aliveTargets.Add(target);
        }

        // Return a random target if there are any alive targets
        if (aliveTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, aliveTargets.Count);
            return aliveTargets[randomIndex];
        }

        return null;
    }

    /// <summary>
    /// Attack a target unit with proper animation timing
    /// Return type changed to void to match Unit.Attack
    /// </summary>
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play monster attack sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMonsterAttackSound();
            }

            // Start attack coroutine to handle animation timing
            StartCoroutine(PlayMonsterAttackAnimation(target));
        }
    }

    /// <summary>
    /// Perform the actual monster attack and return damage for tracking
    /// </summary>
    public virtual int PerformAttack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            int damage = attackDamage;
            target.TakeDamage(damage);

            // Debug message
            Debug.Log($"{unitName} attacks {target.unitName} for {damage} damage!");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "attacks", damage);
            }

            return damage;
        }

        return 0;
    }

    /// <summary>
    /// Coroutine to handle attack animation timing
    /// </summary>
    protected virtual IEnumerator PlayMonsterAttackAnimation(Unit target)
    {
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
            PerformAttack(target);
        }
    }

    // Override the Die method to add visual feedback for monster deaths
    protected override void Die()
    {
        isAlive = false;

        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Play monster death sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMonsterAttackSound(); // Reuse the monster sound for death
        }

        Debug.Log(unitName + " has been defeated!");

        // Play death animation with timing
        StartCoroutine(PlayDeathAnimation());

        // Update game state check
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckBattleStatus();
        }
    }

    private IEnumerator PlayDeathAnimation()
    {
        // First, play a death effect
        if (CombatSystem.Instance != null && CombatSystem.Instance.attackEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(CombatSystem.Instance.attackEffectPrefab, transform.position, Quaternion.identity);
            deathEffect.transform.localScale = Vector3.one * 2.0f; // Make it bigger for death
        }

        // Change material color to indicate death
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                // Fade to grey and transparent
                Color originalColor = renderer.material.color;
                renderer.material.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

                // Enable transparency
                renderer.material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.DisableKeyword("_ALPHATEST_ON");
                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                renderer.material.renderQueue = 3000;
            }
        }

        // Add a sinking effect
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition - new Vector3(0, 0.3f, 0); // Sink into the ground

        // Sink over 1 second
        float duration = 1.0f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Move downward with easing
            transform.position = Vector3.Lerp(startPosition, endPosition, t * t);

            // Rotate slightly for dramatic effect
            transform.rotation = Quaternion.Euler(
                Mathf.Lerp(0, 15, t),
                transform.rotation.eulerAngles.y,
                Mathf.Lerp(0, 10, t)
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we reach the final position
        transform.position = endPosition;
    }
}