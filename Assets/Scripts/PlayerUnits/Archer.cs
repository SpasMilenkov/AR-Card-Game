using UnityEngine;

public class Archer : PlayerUnit
{
    public float aimedShotDamageMultiplier = 2.0f; // Double damage
    
    private void Awake()
    {
        unitName = "Archer";
        maxHealth = 80;
        currentHealth = maxHealth;
        attackDamage = 15;
        unitType = UnitType.Ranged;
        abilityCooldown = 2;
    }
    
    // Aimed Shot ability: high damage to a single target
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility() && targets.Length > 0)
        {
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