using UnityEngine;
using System.Collections;

public class Archer : PlayerUnit
{
    public float aimedShotDamageMultiplier = 2.0f; // Double damage

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();
        unitName = "Archer";
        maxHealth = 80;
        currentHealth = maxHealth;
        attackDamage = 15;
        unitType = UnitType.Ranged;
        abilityCooldown = 2;

        // Set animation timing properties
        attackAnimationDelay = 0.5f; // Standard bow draw time
        abilityAnimationDelay = 0.7f; // Aimed shot takes a bit longer
    }

    // Override regular attack to play a ranged attack animation
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play archer attack sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayArcherAttackSound();
            }

            StartCoroutine(PlayRangedAttackAnimation(target));
        }
    }

    // Coroutine for ranged attack animation timing
    private IEnumerator PlayRangedAttackAnimation(Unit target)
    {
        // Play bow attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for animation to reach the "shoot" point
        yield return new WaitForSeconds(attackAnimationDelay);

        // Now apply damage if target is still valid
        if (target != null && target.isAlive)
        {
            Debug.Log(unitName + " shoots an arrow at " + target.unitName + " for " + attackDamage + " damage!");
            target.TakeDamage(attackDamage);

            // Update game info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "shoots", attackDamage);
            }
        }
    }

    // Aimed Shot ability: high damage to a single target
    protected override void ApplyAbilityEffects(Unit[] targets)
    {
        if (targets.Length > 0)
        {
            // Play archer ability sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayArcherAttackSound();
            }

            // Target first unit in the array (should be the selected target)
            Unit target = targets[0];

            if (target.isAlive)
            {
                int abilityDamage = Mathf.RoundToInt(attackDamage * aimedShotDamageMultiplier);
                target.TakeDamage(abilityDamage);

                // Visual feedback
                Debug.Log(unitName + " uses Aimed Shot on " + target.unitName +
                          " for " + abilityDamage + " damage!");

                // Update game info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "hits with Aimed Shot", abilityDamage);
                }
            }
        }
    }
}