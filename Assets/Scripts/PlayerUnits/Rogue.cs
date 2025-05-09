using UnityEngine;

public class Rogue : PlayerUnit
{
    public float criticalChance = 0.8f; // 80% chance for backstab to crit
    public float criticalMultiplier = 2.5f; // 250% damage on crit
    
    private void Awake()
    {
        unitName = "Rogue";
        maxHealth = 90;
        currentHealth = maxHealth;
        attackDamage = 18;
        unitType = UnitType.Assassin;
        abilityCooldown = 2;
    }
    
    // Backstab ability: high chance for critical hit
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility() && targets.Length > 0)
        {
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
                }
                else
                {
                    abilityDamage = attackDamage;
                    Debug.Log(unitName + " uses Backstab on " + target.unitName + 
                              " for " + abilityDamage + " damage.");
                }
                
                target.TakeDamage(abilityDamage);
            }
            
            // Set cooldown
            currentCooldown = abilityCooldown;
        }
    }
}