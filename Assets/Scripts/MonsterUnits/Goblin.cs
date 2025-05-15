using UnityEngine;
using System.Collections;

public class Goblin : MonsterUnit
{
    private int attacksPerTurn = 2; // Goblins attack twice
    private float secondAttackDamageMultiplier = 0.5f; // Second attack does half damage

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

        unitName = "Goblin";
        maxHealth = 60; // Low HP
        currentHealth = maxHealth;
        attackDamage = 10; // Low damage
        unitType = UnitType.Melee;
        aggressiveness = 0.7f; // Prefers weaker targets

        // Set animation timing properties
        attackAnimationDelay = 0.4f; // Goblins attack quickly
    }

    // Override the attack animation coroutine to handle double attack
    protected override IEnumerator PlayMonsterAttackAnimation(Unit target)
    {
        // First attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for first attack animation to reach hit point
        yield return new WaitForSeconds(attackAnimationDelay);

        // Apply first attack damage if target is still valid
        int totalDamage = 0;
        if (target != null && target.isAlive)
        {
            // First attack with full damage
            int firstDamage = attackDamage;
            target.TakeDamage(firstDamage);
            totalDamage += firstDamage;

            // Update info layer for first attack
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "attacks", firstDamage);
            }
        }

        // Check if target is still alive for second attack
        if (target != null && target.isAlive)
        {
            // Small delay between attacks
            yield return new WaitForSeconds(0.3f);

            // Second attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Wait for second attack animation to reach hit point
            yield return new WaitForSeconds(attackAnimationDelay);

            // Apply second attack with reduced damage
            if (target != null && target.isAlive)
            {
                int secondDamage = Mathf.RoundToInt(attackDamage * secondAttackDamageMultiplier);
                target.TakeDamage(secondDamage);
                totalDamage += secondDamage;

                Debug.Log(unitName + " attacks again for " + secondDamage + " damage!");

                // Update info layer for second attack
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "attacks again", secondDamage);
                }
            }
        }
    }

    // Override PerformAttack to return the expected total damage
    public override int PerformAttack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Calculate expected total damage (for planning)
            int firstDamage = attackDamage;
            int secondDamage = Mathf.RoundToInt(attackDamage * secondAttackDamageMultiplier);
            return firstDamage + secondDamage;
        }

        return 0;
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