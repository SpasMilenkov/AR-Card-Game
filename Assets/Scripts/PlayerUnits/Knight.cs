using UnityEngine;

public class Knight : PlayerUnit
{
    public float damageReductionAmount = 0.5f; // 50% damage reduction
    public int shieldDuration = 1; // Lasts 1 turn
    private int shieldRemainingTurns = 0;

    private void Awake()
    {
        unitName = "Knight";
        maxHealth = 150; // Higher HP
        currentHealth = maxHealth;
        attackDamage = 15; // Lower damage
        unitType = UnitType.Tank;
        abilityCooldown = 3; // Cooldown in turns
    }

    // Shield Block ability: reduces damage taken for 1 turn
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility())
        {
            shieldRemainingTurns = shieldDuration;

            // Visual feedback
            Debug.Log(unitName + " uses Shield Block! Damage reduced for " + shieldDuration + " turn.");

            // Activate visual effect if available
            Transform shield = transform.Find("ShieldEffect");
            if (shield != null)
                shield.gameObject.SetActive(true);

            // Set cooldown
            currentCooldown = abilityCooldown;
        }
    }

    public override void TakeDamage(int damage)
    {
        // Apply damage reduction if shield is active
        if (shieldRemainingTurns > 0)
        {
            int reducedDamage = Mathf.RoundToInt(damage * (1 - damageReductionAmount));
            base.TakeDamage(reducedDamage);

            Debug.Log(unitName + "'s shield absorbs " +
                      (damage - reducedDamage) + " damage!");
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
            }
        }
    }
}