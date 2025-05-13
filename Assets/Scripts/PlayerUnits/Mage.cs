using UnityEngine;
using System.Collections.Generic;

public class Mage : PlayerUnit
{
    public float fireballDamage = 25f; // Base fireball damage
    public float splashDamageMultiplier = 0.5f; // 50% damage to adjacent targets

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

        unitName = "Mage";
        maxHealth = 70; // Low HP
        currentHealth = maxHealth;
        attackDamage = 10; // Low basic attack
        unitType = UnitType.Spellcaster;
        abilityCooldown = 2;
    }

    // Fireball ability: AoE damage (primary target + adjacent units)
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility() && targets.Length > 0)
        {
            // Play fireball casting animation
            if (animator != null)
            {
                animator.SetTrigger("Ability");
            }

            // Primary target is first in array
            Unit primaryTarget = targets[0];

            if (primaryTarget.isAlive)
            {
                // Deal full damage to primary target
                int primaryDamage = Mathf.RoundToInt(fireballDamage);
                primaryTarget.TakeDamage(primaryDamage);

                // Find adjacent targets (handled by combat system)
                foreach (Unit splashTarget in targets)
                {
                    if (splashTarget != primaryTarget && splashTarget.isAlive)
                    {
                        int splashDamage = Mathf.RoundToInt(fireballDamage * splashDamageMultiplier);
                        splashTarget.TakeDamage(splashDamage);
                    }
                }

                // Visual feedback
                Debug.Log(unitName + " launches Fireball! " + primaryDamage +
                          " damage to primary target, splash damage to " +
                          (targets.Length - 1) + " additional targets!");
            }

            // Set cooldown
            currentCooldown = abilityCooldown;
        }
    }

    // Override regular attack to play a spell casting animation instead of melee
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play spell casting animation for basic attack
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            Debug.Log(unitName + " casts a bolt at " + target.unitName + " for " + attackDamage + " damage!");
            target.TakeDamage(attackDamage);

            // Update game info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "casts a bolt at", attackDamage);
            }
        }
    }
}