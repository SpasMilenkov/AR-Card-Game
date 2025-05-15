using UnityEngine;
using System.Collections;

public class SkeletonArcher : MonsterUnit
{
    public int volleyArrowCount = 3; // Number of arrows in volley
    public float arrowDamageMultiplier = 0.4f; // Each arrow does 40% damage
    private int turnCounter = 0;
    private int volleyInterval = 3; // Use volley every 3 turns
    private bool useVolleyThisTurn = false;

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

        // Set animation timing properties
        attackAnimationDelay = 0.5f; // Standard bow draw time
    }

    // Track turns for special attack
    public void StartTurn()
    {
        turnCounter++;
        useVolleyThisTurn = (turnCounter >= volleyInterval);

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

    // Override attack animation to handle volley
    protected override IEnumerator PlayMonsterAttackAnimation(Unit target)
    {
        // Check if it's time for volley attack
        if (useVolleyThisTurn)
        {
            yield return StartCoroutine(PlayVolleyAttackAnimation(target));

            // Reset counter and volley flag
            turnCounter = 0;
            useVolleyThisTurn = false;

            // Reset animation state
            if (animator != null)
            {
                animator.SetBool("PreparingVolley", false);
            }
        }
        else
        {
            // Play normal attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Wait for animation to reach the "shoot" point
            yield return new WaitForSeconds(attackAnimationDelay);

            // Apply damage if target is still valid
            if (target != null && target.isAlive)
            {
                target.TakeDamage(attackDamage);

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "shoots", attackDamage);
                }
            }
        }
    }

    // Volley attack animation with proper timing
    private IEnumerator PlayVolleyAttackAnimation(Unit target)
    {
        if (target == null || !target.isAlive)
            yield break;

        // Play volley animation
        if (animator != null)
        {
            animator.SetTrigger("Ability");
        }

        // Update info layer for volley start
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} fires a volley of {volleyArrowCount} arrows at {target.unitName}!");
        }

        Debug.Log(unitName + " fires a volley of " + volleyArrowCount + " arrows!");
        int arrowDamage = Mathf.RoundToInt(attackDamage * arrowDamageMultiplier);
        int totalDamage = 0;

        // Wait for initial animation
        yield return new WaitForSeconds(0.5f);

        // Fire multiple arrows with short delays between them
        for (int i = 0; i < volleyArrowCount; i++)
        {
            if (target.isAlive)
            {
                // Apply damage for each arrow
                target.TakeDamage(arrowDamage);
                totalDamage += arrowDamage;

                // Play a small effect for each hit
                if (CombatSystem.Instance != null)
                {
                    CombatSystem.Instance.PlayCombatEffectAt(
                        CombatSystem.Instance.attackEffectPrefab,
                        target.transform.position);
                }

                // Wait a short time between arrows
                yield return new WaitForSeconds(0.2f);
            }
        }

        Debug.Log("Volley dealt " + totalDamage + " total damage!");

        // Update info layer for total damage
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "hits with volley for", totalDamage);
        }
    }

    // Override to return expected attack damage
    public override int PerformAttack(Unit target)
    {
        if (useVolleyThisTurn)
        {
            // Calculate expected volley damage
            int arrowDamage = Mathf.RoundToInt(attackDamage * arrowDamageMultiplier);
            return arrowDamage * volleyArrowCount;
        }
        else
        {
            // Regular attack damage
            return attackDamage;
        }
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