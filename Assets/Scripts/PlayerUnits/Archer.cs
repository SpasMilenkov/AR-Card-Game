using UnityEngine;

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
    }

    // Override regular attack to play a ranged attack animation
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play bow attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

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
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility() && targets.Length > 0)
        {
            // Play aimed shot animation
            if (animator != null)
            {
                animator.SetTrigger("Ability");
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
            }

            // Set cooldown
            currentCooldown = abilityCooldown;
        }
    }
}