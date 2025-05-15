using UnityEngine;
using System.Collections;

public class Warrior : PlayerUnit
{
    public float whirlwindDamageMultiplier = 0.5f; // 50% damage to all enemies

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

        unitName = "Warrior";
        maxHealth = 120;
        currentHealth = maxHealth;
        attackDamage = 20;
        unitType = UnitType.Melee;
        abilityCooldown = 3;
    }

    // Whirlwind ability: hits all enemies for reduced damage
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility())
        {
            // Play sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGenericAbilitySound();
            }

            // Play whirlwind animation
            if (animator != null)
            {
                animator.SetTrigger("Ability");
            }

            int damageDealt = 0;
            int targetsHit = 0;

            foreach (Unit target in targets)
            {
                if (target.isAlive && target is MonsterUnit)
                {
                    int abilityDamage = Mathf.RoundToInt(attackDamage * whirlwindDamageMultiplier);
                    target.TakeDamage(abilityDamage);
                    damageDealt += abilityDamage;
                    targetsHit++;
                }
            }

            // Visual feedback
            Debug.Log(unitName + " uses Whirlwind! Hit " + targetsHit +
                      " targets for " + damageDealt + " total damage!");

            // Set cooldown
            currentCooldown = abilityCooldown;
        }
    }

    // Override Attack to add sound effect
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play attack sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWarriorAttackSound();
            }

            // Use the base implementation with proper timing
            base.Attack(target);
        }
    }
}