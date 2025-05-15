using UnityEngine;
using System.Collections;

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

        // Set animation timing properties
        attackAnimationDelay = 0.6f; // Spell casting takes a bit longer
        abilityAnimationDelay = 0.8f; // Fireball takes longer to cast
    }

    // Override for applying ability effects
    protected override void ApplyAbilityEffects(Unit[] targets)
    {
        if (targets.Length > 0)
        {
            // Play fireball sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMageAbilitySound();
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
        }
    }

    // Override basic attack to play a spell casting animation
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play spell attack sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.fireballCast);
            }

            // Start spell attack animation with proper timing
            StartCoroutine(PlaySpellAttackAnimation(target));
        }
    }

    // Coroutine for spell attack animation timing
    private IEnumerator PlaySpellAttackAnimation(Unit target)
    {
        // Play spell casting animation for basic attack
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for animation to reach the "hit" point
        yield return new WaitForSeconds(attackAnimationDelay);

        // Now apply damage if target is still valid
        if (target != null && target.isAlive)
        {
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