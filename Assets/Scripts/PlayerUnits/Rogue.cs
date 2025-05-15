using UnityEngine;
using System.Collections;

public class Rogue : PlayerUnit
{
    public float criticalChance = 0.8f; // 80% chance for backstab to crit
    public float criticalMultiplier = 2.5f; // 250% damage on crit

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

        unitName = "Rogue";
        maxHealth = 90;
        currentHealth = maxHealth;
        attackDamage = 18;
        unitType = UnitType.Assassin;
        abilityCooldown = 2;

        // Set animation timing properties
        attackAnimationDelay = 0.4f; // Rogues attack quickly
        abilityAnimationDelay = 0.5f; // Backstab is quick
    }

    // Backstab ability - updated to use proper timing
    protected override void ApplyAbilityEffects(Unit[] targets)
    {
        if (targets.Length > 0)
        {
            // Play ability sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGenericAbilitySound();
            }

            Unit target = targets[0];
            if (target.isAlive)
            {
                // Check for critical hit
                bool isCritical = Random.value <= criticalChance;
                int abilityDamage;

                if (isCritical)
                {
                    abilityDamage = Mathf.RoundToInt(attackDamage * criticalMultiplier);
                    Debug.Log(unitName + " lands a CRITICAL Backstab on " +
                              target.unitName + " for " + abilityDamage + " damage!");

                    // Inform the game info layer
                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.AddLogEntry($"{unitName} lands a CRITICAL Backstab!");
                        GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "critically backstabs", abilityDamage);
                    }
                }
                else
                {
                    abilityDamage = attackDamage;
                    Debug.Log(unitName + " uses Backstab on " + target.unitName +
                              " for " + abilityDamage + " damage.");

                    // Inform the game info layer
                    if (GameInfoLayer.Instance != null)
                    {
                        GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "backstabs", abilityDamage);
                    }
                }

                target.TakeDamage(abilityDamage);
            }
        }
    }

    // Override attack to add rogue-specific animations if needed
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play attack sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHitFlesh);
            }

            // Use the base class implementation which now has proper timing
            base.Attack(target);
        }
    }
}