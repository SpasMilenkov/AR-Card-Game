using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all player units
/// </summary>
public class PlayerUnit : Unit
{
    public int abilityCooldown;
    public int currentCooldown = 0;

    /// <summary>
    /// Reset ability cooldown
    /// </summary>
    public void ResetCooldown()
    {
        currentCooldown = 0;
    }

    /// <summary>
    /// Decrease cooldown by one turn
    /// </summary>
    public void DecreaseCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }

    /// <summary>
    /// Check if ability can be used
    /// </summary>
    public bool CanUseAbility()
    {
        return isAlive && currentCooldown <= 0;
    }

    /// <summary>
    /// Use a special ability on target(s) - now uses coroutines for timing
    /// </summary>
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility())
        {
            // Start the ability sequence with proper timing
            StartCoroutine(PlayAbilityAnimationWithTiming(targets));

            // Provide feedback immediately through info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} begins using a special ability!");
            }
        }
        else
        {
            Debug.LogWarning($"{unitName} cannot use ability: cooldown={currentCooldown}");

            // Play error sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound(); // Use button click as a "can't do" sound
            }

            // Inform player why ability can't be used
            if (GameInfoLayer.Instance != null)
            {
                if (currentCooldown > 0)
                {
                    GameInfoLayer.Instance.AddLogEntry($"{unitName}'s ability is on cooldown for {currentCooldown} more turns!");
                }
                else
                {
                    GameInfoLayer.Instance.AddLogEntry($"{unitName} cannot use ability!");
                }
            }
        }
    }

    /// <summary>
    /// Coroutine to handle ability animation timing
    /// </summary>
    protected virtual IEnumerator PlayAbilityAnimationWithTiming(Unit[] targets)
    {
        // Play ability animation
        if (animator != null)
        {
            animator.SetTrigger("Ability");
        }

        // Wait for animation to reach the effect point
        yield return new WaitForSeconds(abilityAnimationDelay);

        // Now apply ability effects
        ApplyAbilityEffects(targets);

        // Set cooldown after effects are applied
        currentCooldown = abilityCooldown;

        // Update info layer about cooldown
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName}'s ability is now on cooldown for {currentCooldown} turns");
        }
    }

    /// <summary>
    /// Apply ability effects - override in child classes
    /// </summary>
    protected virtual void ApplyAbilityEffects(Unit[] targets)
    {
        // Base implementation does nothing
        Debug.Log($"{unitName} ability has no effect in base class");
    }

    /// <summary>
    /// Can be called by animation events when ability effect frame is reached
    /// </summary>
    public void OnAbilityEffectFrame()
    {
        // This can be used with animation events instead of delay timers
        // Would need to store targets temporarily and apply effects here
    }
}