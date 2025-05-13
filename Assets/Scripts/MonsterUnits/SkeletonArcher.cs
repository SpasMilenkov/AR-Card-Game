using UnityEngine;

public class SkeletonArcher : MonsterUnit
{
    public int volleyArrowCount = 3; // Number of arrows in volley
    public float arrowDamageMultiplier = 0.4f; // Each arrow does 40% damage
    private int turnCounter = 0;
    private int volleyInterval = 3; // Use volley every 3 turns

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

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

        // Update animation state or parameters if needed
        if (animator != null && turnCounter >= volleyInterval - 1)
        {
            // Optional: Show a "preparing" animation if volley is coming next turn
            animator.SetBool("PreparingVolley", turnCounter >= volleyInterval - 1);
        }

        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            if (turnCounter >= volleyInterval)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} prepares a volley attack");
            }
            else if (turnCounter == volleyInterval - 1)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} is nocking multiple arrows");
            }
        }
    }

    // Changed return type to int
    public override int Attack(Unit target)
    {
        // Check if it's time for volley attack
        if (turnCounter >= volleyInterval)
        {
            // Play volley animation
            if (animator != null)
            {
                // You can use "Ability" for the volley animation if you don't have a specific one
                animator.SetTrigger("Ability");
            }

            // Reset counter
            turnCounter = 0;

            // Reset animation state
            if (animator != null)
            {
                animator.SetBool("PreparingVolley", false);
            }

            // Perform volley attack
            return VolleyAttack(target);
        }
        else
        {
            // Play normal attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Normal attack
            if (target != null && target.isAlive)
            {
                target.TakeDamage(attackDamage);

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "shoots", attackDamage);
                }
                return attackDamage;
            }
            return 0;
        }
    }

    // Changed to return int for total damage
    private int VolleyAttack(Unit target)
    {
        if (target == null || !target.isAlive)
            return 0;

        Debug.Log(unitName + " fires a volley of " + volleyArrowCount + " arrows!");
        int arrowDamage = Mathf.RoundToInt(attackDamage * arrowDamageMultiplier);
        int totalDamage = 0;

        // Update info layer for volley start
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} fires a volley of {volleyArrowCount} arrows at {target.unitName}!");
        }

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

        // Update info layer for total damage
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "hits with volley for", totalDamage);
        }

        return totalDamage;
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