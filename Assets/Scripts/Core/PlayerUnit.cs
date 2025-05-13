using UnityEngine;

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
    /// Use a special ability on target(s)
    /// </summary>
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility())
        {
            // Play ability animation
            if (animator != null)
            {
                animator.SetTrigger("Ability");
            }

            // Ability logic in child classes
            currentCooldown = abilityCooldown;

            // Provide feedback through info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} uses a special ability!");
            }
        }
    }
}