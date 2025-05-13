using UnityEngine;

public class Troll : MonsterUnit
{
    public float regenerationAmount = 5f; // HP regenerated per turn
    private bool canAttackThisTurn = true; // Trolls attack every other turn

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

        unitName = "Troll";
        maxHealth = 180; // Very high HP
        currentHealth = maxHealth;
        attackDamage = 25; // High damage
        unitType = UnitType.Tank;
        aggressiveness = 0.3f; // Prefers tanks and melee units
    }

    // Regenerate HP at the start of turn
    public void StartTurn()
    {
        // Toggle attack availability
        canAttackThisTurn = !canAttackThisTurn;

        // Regenerate HP
        if (isAlive)
        {
            // Play regeneration animation/effect if available
            if (animator != null && !canAttackThisTurn)
            {
                // If you have a specific regeneration animation, use it
                // Otherwise, you could use a generic "Idle" animation
                animator.SetTrigger("Idle");
            }

            int regenAmount = Mathf.RoundToInt(regenerationAmount);
            currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            Debug.Log(unitName + " regenerates " + regenAmount + " HP!");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} regenerates {regenAmount} HP");
                if (!canAttackThisTurn)
                {
                    GameInfoLayer.Instance.AddLogEntry($"{unitName} is resting this turn");
                }
            }
        }
    }

    // Changed to return int
    public override int Attack(Unit target)
    {
        if (canAttackThisTurn && target != null && target.isAlive)
        {
            // Play attack animation only when able to attack
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Apply damage
            target.TakeDamage(attackDamage);
            Debug.Log($"{unitName} smashes {target.unitName} for {attackDamage} damage!");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "smashes", attackDamage);
            }
            return attackDamage;
        }
        else
        {
            Debug.Log(unitName + " is recovering and cannot attack this turn.");
            return 0;
        }
    }

    // Troll prefers to attack Tank units
    public override PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        // First look for Tank units
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive && target.unitType == UnitType.Tank)
                return target;
        }

        // Then look for melee units
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive && target.unitType == UnitType.Melee)
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

    // Troll has no ability, so we don't need to override UseAbility
    // It will never be called since CanUseAbility() isn't implemented
}