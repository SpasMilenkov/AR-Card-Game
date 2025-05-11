using UnityEngine;

public class Goblin : MonsterUnit
{
    private int attacksPerTurn = 2; // Goblins attack twice
    private float secondAttackDamageMultiplier = 0.5f; // Second attack does half damage

    private void Awake()
    {
        unitName = "Goblin";
        maxHealth = 60; // Low HP
        currentHealth = maxHealth;
        attackDamage = 10; // Low damage
        unitType = UnitType.Melee;
        aggressiveness = 0.7f; // Prefers weaker targets
    }

    // Override to attack twice - changed to return int
    public override int Attack(Unit target)
    {
        int totalDamage = 0;

        // First attack
        if (target != null && target.isAlive)
        {
            target.TakeDamage(attackDamage);
            totalDamage += attackDamage;

            // Update info layer for first attack
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "attacks", attackDamage);
            }
        }

        // Check if target is still alive and we can do second attack
        if (target != null && target.isAlive)
        {
            // Second attack with reduced damage
            int secondDamage = Mathf.RoundToInt(attackDamage * secondAttackDamageMultiplier);
            Debug.Log(unitName + " attacks again for " + secondDamage + " damage!");
            target.TakeDamage(secondDamage);
            totalDamage += secondDamage;

            // Update info layer for second attack
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "attacks again", secondDamage);
            }
        }

        return totalDamage;
    }

    // Goblin selects the weakest (lowest HP) target
    public override PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        PlayerUnit weakestTarget = null;
        int lowestHP = int.MaxValue;

        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive && target.currentHealth < lowestHP)
            {
                weakestTarget = target;
                lowestHP = target.currentHealth;
            }
        }

        return weakestTarget;
    }
}