using UnityEngine;

public class Orc : MonsterUnit
{
    private bool isEnraged = false;
    private float enrageThreshold = 0.5f; // Enrages at 50% HP
    private float enrageDamageBonus = 1.5f; // 50% more damage when enraged

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

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

            // Play enrage animation if available
            if (animator != null)
            {
                // You can use "Ability" for this if you don't have a specific "Enrage" animation
                animator.SetTrigger("Ability");
            }

            Debug.Log(unitName + " becomes enraged! Attack increased to " + attackDamage);

            // Update info layer about enrage
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName} becomes enraged! Attack increased to {attackDamage}");
            }

            // Visual effect for enrage
            // (Add particle effect or color change here)
        }
    }

    // Changed return type to int
    public override int Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play attack animation with different parameters based on enrage state
            if (animator != null)
            {
                if (isEnraged)
                {
                    // You can set a parameter to modify the animation when enraged
                    animator.SetBool("IsEnraged", true);
                }
                animator.SetTrigger("Attack");
            }

            // Apply damage
            target.TakeDamage(attackDamage);

            // Log the attack
            Debug.Log($"{unitName} {(isEnraged ? "furiously" : "")} attacks {target.unitName} for {attackDamage} damage!");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterBattleAction(
                    unitName,
                    target.unitName,
                    isEnraged ? "furiously attacks" : "attacks",
                    attackDamage
                );
            }
            return attackDamage;
        }
        return 0;
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