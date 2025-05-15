using UnityEngine;
using System.Collections;

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

        // Set animation timing properties
        attackAnimationDelay = 0.6f; // Orcs have a slightly slower, heavier attack
    }

    public override void TakeDamage(int damage)
    {
        // Use base implementation first
        base.TakeDamage(damage);

        // Check if we should enrage
        if (!isEnraged && currentHealth <= (maxHealth * enrageThreshold) && isAlive)
        {
            StartCoroutine(EnrageWithAnimation());
        }
    }

    // Add animation timing to the enrage effect
    private IEnumerator EnrageWithAnimation()
    {
        isEnraged = true;

        // Play enrage animation if available
        if (animator != null)
        {
            animator.SetTrigger("Ability");
        }

        // Wait for animation to complete
        yield return new WaitForSeconds(0.5f);

        // Apply enrage effect
        attackDamage = Mathf.RoundToInt(attackDamage * enrageDamageBonus);

        Debug.Log(unitName + " becomes enraged! Attack increased to " + attackDamage);

        // Update info layer about enrage
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} becomes enraged! Attack increased to {attackDamage}");
        }

        // Visual effect for enrage - add particle effect or color change
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Add reddish tint
                renderer.material.color = Color.Lerp(renderer.material.color, Color.red, 0.3f);

                // Add emission for glowing effect
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", Color.red * 0.3f);
            }
        }
    }

    // Override attack animation to handle enraged state
    protected override IEnumerator PlayMonsterAttackAnimation(Unit target)
    {
        // Play attack animation with different parameters based on enrage state
        if (animator != null)
        {
            if (isEnraged)
            {
                animator.SetBool("IsEnraged", true);
            }
            animator.SetTrigger("Attack");
        }

        // Wait for animation to reach hit point (shorter delay when enraged for faster attacks)
        float delay = isEnraged ? attackAnimationDelay * 0.8f : attackAnimationDelay;
        yield return new WaitForSeconds(delay);

        // Apply damage if target is still valid
        if (target != null && target.isAlive)
        {
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
        }
    }

    // Return current damage accounting for enrage state
    public override int PerformAttack(Unit target)
    {
        return attackDamage; // Current attack damage includes enrage bonus if active
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