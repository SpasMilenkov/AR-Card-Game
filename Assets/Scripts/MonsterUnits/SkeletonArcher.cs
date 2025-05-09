using UnityEngine;

public class SkeletonArcher : MonsterUnit
{
    public int volleyArrowCount = 3; // Number of arrows in volley
    public float arrowDamageMultiplier = 0.4f; // Each arrow does 40% damage
    private int turnCounter = 0;
    private int volleyInterval = 3; // Use volley every 3 turns
    
    private void Awake()
    {
        unitName = "Skeleton Archer";
        maxHealth = 70;
        currentHealth = maxHealth;
        attackDamage = 12;
        unitType = UnitType.Ranged;
        aggressiveness = 0.6f; // Targets squishier units
    }
    
    // Track turns for special attack
    public void StartTurn()
    {
        turnCounter++;
    }
    
    public override void Attack(Unit target)
    {
        // Check if it's time for volley attack
        if (turnCounter >= volleyInterval)
        {
            // Reset counter
            turnCounter = 0;
            
            // Perform volley attack
            VolleyAttack(target);
        }
        else
        {
            // Normal attack
            base.Attack(target);
        }
    }
    
    private void VolleyAttack(Unit target)
    {
        Debug.Log(unitName + " fires a volley of " + volleyArrowCount + " arrows!");
        
        int arrowDamage = Mathf.RoundToInt(attackDamage * arrowDamageMultiplier);
        int totalDamage = 0;
        
        // Fire multiple arrows
        for (int i = 0; i < volleyArrowCount; i++)
        {
            if (target.isAlive)
            {
                target.TakeDamage(arrowDamage);
                totalDamage += arrowDamage;
            }
        }
        
        Debug.Log("Volley dealt " + totalDamage + " total damage!");
    }
    
    // Prefer targeting spellcasters and ranged units
    public override PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        // First look for Spellcaster units
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive && target.unitType == UnitType.Spellcaster)
                return target;
        }
        
        // Then look for Ranged units
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive && target.unitType == UnitType.Ranged)
                return target;
        }
        
        // Finally, any living unit
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive)
                return target;
        }
        
        return null;
    }
}