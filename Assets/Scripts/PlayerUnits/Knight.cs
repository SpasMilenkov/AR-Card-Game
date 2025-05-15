using UnityEngine;
using System.Collections;

public class Knight : PlayerUnit
{
    public float damageReductionAmount = 0.5f; // 50% damage reduction
    public int shieldDuration = 1; // Lasts 1 turn
    private int shieldRemainingTurns = 0;

    private void Awake()
    {
        // Get animator component
        animator = GetComponent<Animator>();

        unitName = "Knight";
        maxHealth = 150; // Higher HP
        currentHealth = maxHealth;
        attackDamage = 15; // Lower damage
        unitType = UnitType.Tank;
        abilityCooldown = 3; // Cooldown in turns

        // Set animation timing properties
        attackAnimationDelay = 0.5f; // Standard attack time
        abilityAnimationDelay = 0.6f; // Shield raise is relatively quick
    }

    // Shield Block ability: reduces damage taken for 1 turn
    protected override void ApplyAbilityEffects(Unit[] targets)
    {
        // Shield block affects the knight itself, not targets
        shieldRemainingTurns = shieldDuration;

        // Play ability sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGenericAbilitySound();
        }

        // Visual feedback
        Debug.Log(unitName + " uses Shield Block! Damage reduced for " + shieldDuration + " turn.");

        // Update game info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} raises shield! Damage reduced for {shieldDuration} turn.");
        }

        // Activate visual effect if available
        Transform shield = transform.Find("ShieldEffect");
        if (shield != null)
            shield.gameObject.SetActive(true);
    }

    public override void TakeDamage(int damage)
    {
        // Play take damage animation
        if (animator != null)
        {
            // If shield is active, play a block animation if available
            if (shieldRemainingTurns > 0)
            {
                animator.SetTrigger("Ability");

                // Play shield block sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHitMetal);
                }
            }
            else
            {
                animator.SetTrigger("TakeDamage");

                // Play generic hit sound (if available in AudioManager)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHitArmor);
                }
            }
        }

        // Apply damage reduction if shield is active
        if (shieldRemainingTurns > 0)
        {
            int reducedDamage = Mathf.RoundToInt(damage * (1 - damageReductionAmount));
            int damagePrevented = damage - reducedDamage;

            // Update game info layer about damage reduction
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName}'s shield absorbs {damagePrevented} damage!");
            }

            // Apply reduced damage using base method
            base.TakeDamage(reducedDamage);
            Debug.Log(unitName + "'s shield absorbs " +
                      damagePrevented + " damage!");
        }
        else
        {
            base.TakeDamage(damage);
        }
    }

    public void StartTurn()
    {
        // This is called at the start of each player turn
        // Update shield status indicator if needed
        if (shieldRemainingTurns > 0)
        {
            Debug.Log($"{unitName}'s shield is active for {shieldRemainingTurns} more turns");

            // Make sure shield visual is active
            Transform shield = transform.Find("ShieldEffect");
            if (shield != null)
                shield.gameObject.SetActive(true);

            // Update game info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.AddLogEntry($"{unitName}'s shield is active for {shieldRemainingTurns} more turns");
            }
        }
    }

    // Call this at the end of each turn
    public void EndTurn()
    {
        if (shieldRemainingTurns > 0)
        {
            shieldRemainingTurns--;

            if (shieldRemainingTurns <= 0)
            {

                // Deactivate shield visual
                Transform shield = transform.Find("ShieldEffect");
                if (shield != null)
                    shield.gameObject.SetActive(false);

                Debug.Log(unitName + "'s shield fades away.");

                // Update game info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"{unitName}'s shield fades away");
                }
            }
        }
    }

    // Override Attack to add sound effect
    public override void Attack(Unit target)
    {
        if (target != null && target.isAlive)
        {
            // Play attack sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWarriorAttackSound();
            }

            // Use the base implementation 
            base.Attack(target);
        }
    }
}