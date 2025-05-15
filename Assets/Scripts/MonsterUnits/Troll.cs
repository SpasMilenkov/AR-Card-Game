using UnityEngine;
using System.Collections;

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

        // Set animation timing properties
        attackAnimationDelay = 0.7f; // Trolls have slower, heavier attacks
    }

    // Regenerate HP at the start of turn
    public void StartTurn()
    {
        // Toggle attack availability
        canAttackThisTurn = !canAttackThisTurn;

        // Regenerate HP
        if (isAlive)
        {
            StartCoroutine(RegenerateWithAnimation());
        }
    }

    // Add animation timing to regeneration
    private IEnumerator RegenerateWithAnimation()
    {
        // Play regeneration animation/effect if available
        if (animator != null && !canAttackThisTurn)
        {
            // If you have a specific regeneration animation, use it
            // Otherwise, you could use a generic "Idle" animation
            animator.SetTrigger("Idle");
        }

        // Visual indicator for regeneration
        GameObject regenEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        regenEffect.transform.position = transform.position;
        regenEffect.transform.localScale = Vector3.one * 0.1f;

        // Set material for healing effect
        Renderer renderer = regenEffect.GetComponent<Renderer>();
        renderer.material.color = Color.green;

        // Remove collider
        Destroy(regenEffect.GetComponent<Collider>());

        // Wait a moment for visual effect
        yield return new WaitForSeconds(0.5f);

        // Apply regeneration
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

        // Clean up the visual effect
        Destroy(regenEffect, 0.5f);
    }

    // Override attack method with proper timing
    protected override IEnumerator PlayMonsterAttackAnimation(Unit target)
    {
        if (!canAttackThisTurn)
        {
            // Can't attack this turn - just play an idle animation
            if (animator != null)
            {
                animator.SetTrigger("Idle");
            }

            Debug.Log(unitName + " is recovering and cannot attack this turn.");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} is recovering and cannot attack this turn");
            }

            yield break;
        }

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for animation to reach the "hit" point
        yield return new WaitForSeconds(attackAnimationDelay);

        // Now apply damage if target is still valid
        if (target != null && target.isAlive)
        {
            // Apply damage
            target.TakeDamage(attackDamage);
            Debug.Log($"{unitName} smashes {target.unitName} for {attackDamage} damage!");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "smashes", attackDamage);
            }
        }
    }

    // Override to return expected attack damage
    public override int PerformAttack(Unit target)
    {
        if (canAttackThisTurn)
        {
            return attackDamage;
        }
        else
        {
            return 0; // No damage when resting
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
}