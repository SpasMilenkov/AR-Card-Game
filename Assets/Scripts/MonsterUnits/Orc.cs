using UnityEngine;

public class Orc : MonsterUnit
{
    private bool isEnraged = false;
    private float enrageThreshold = 0.5f; // Enrages at 50% HP
    private float enrageDamageBonus = 1.5f; // 50% more damage when enraged
    
    private void Awake()
    {
        unitName = "Orc";
        maxHealth = 100;
        currentHealth = maxHealth;
        attackDamage = 15;
        unitType = UnitType.Melee;
        aggressiveness = 0.5f; // Balanced targeting
    }
    
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        
        // Check if we should enrage
        if (!isEnraged && currentHealth <= (maxHealth * enrageThreshold))
        {
            isEnraged = true;
            attackDamage = Mathf.RoundToInt(attackDamage * enrageDamageBonus);
            Debug.Log(unitName + " becomes enraged! Attack increased to " + attackDamage);
            
            // Visual effect for enrage
            // (Add particle effect or color change here)
        }
    }
    
    // Orc targets randomly
    public override PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        // Filter for only alive targets
        System.Collections.Generic.List<PlayerUnit> aliveTargets = 
            new System.Collections.Generic.List<PlayerUnit>();
        
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive)
                aliveTargets.Add(target);
        }
        
        if (aliveTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, aliveTargets.Count);
            return aliveTargets[randomIndex];
        }
        
        return null;
    }
}