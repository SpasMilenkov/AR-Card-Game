using UnityEngine;

public class DarkWizard : MonsterUnit
{
    public float shadowBoltDamage = 20f;
    public float aoeSpellDamage = 12f; // Lower damage but hits all
    public float healAmount = 15f; // Amount to heal allies

    private int spellType = 0; // 0 = shadow bolt, 1 = AoE, 2 = heal

    private void Awake()
    {
        unitName = "Dark Wizard";
        maxHealth = 80;
        currentHealth = maxHealth;
        attackDamage = 8; // Low basic attack
        unitType = UnitType.Spellcaster;
        aggressiveness = 0.4f; // Balanced targeting
    }

    // Changed to match the base class return type (int)
    public override int Attack(Unit target)
    {
        // Choose a spell based on situation
        ChooseSpell();

        int damageDealt = 0;

        switch (spellType)
        {
            case 0:
                // Shadow Bolt (single target)
                damageDealt = CastShadowBolt(target);
                break;
            case 1:
                // AoE spell
                damageDealt = CastAoeSpell();
                break;
            case 2:
                // Heal allies
                CastHeal();
                damageDealt = 0; // Healing doesn't deal damage
                break;
        }

        return damageDealt;
    }

    private void ChooseSpell()
    {
        // Get references to all units
        PlayerUnit[] playerUnits = FindObjectOfType<GameManager>().playerUnits.ToArray();
        MonsterUnit[] monsterUnits = FindObjectOfType<GameManager>().monsterUnits.ToArray();

        // Count living allies
        int livingAllies = 0;
        bool needsHealing = false;

        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster.isAlive)
            {
                livingAllies++;

                // Check if any ally is below 50% health
                if (monster.currentHealth < (monster.maxHealth * 0.5f))
                    needsHealing = true;
            }
        }

        // Count living enemies
        int livingEnemies = 0;
        foreach (PlayerUnit player in playerUnits)
        {
            if (player.isAlive)
                livingEnemies++;
        }

        // Decision making
        if (needsHealing && Random.value < 0.7f)
        {
            // 70% chance to heal when allies need healing
            spellType = 2;
        }
        else if (livingEnemies >= 2 && Random.value < 0.6f)
        {
            // 60% chance to use AoE when multiple enemies
            spellType = 1;
        }
        else
        {
            // Otherwise use single target spell
            spellType = 0;
        }
    }

    // Changed to return int for damage dealt
    private int CastShadowBolt(Unit target)
    {
        if (target == null || !target.isAlive)
            return 0;

        Debug.Log(unitName + " casts Shadow Bolt at " + target.unitName + "!");
        int damage = Mathf.RoundToInt(shadowBoltDamage);
        target.TakeDamage(damage);

        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "casts Shadow Bolt on", damage);
        }

        return damage;
    }

    // Changed to return int for total damage dealt
    private int CastAoeSpell()
    {
        Debug.Log(unitName + " casts Dark Nova, hitting all players!");

        PlayerUnit[] playerUnits = FindObjectOfType<GameManager>().playerUnits.ToArray();
        int targetsHit = 0;
        int totalDamageDealt = 0;

        foreach (PlayerUnit player in playerUnits)
        {
            if (player.isAlive)
            {
                int damage = Mathf.RoundToInt(aoeSpellDamage);
                player.TakeDamage(damage);
                targetsHit++;
                totalDamageDealt += damage;

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.RegisterBattleAction(unitName, player.unitName, "hits with Dark Nova", damage);
                }
            }
        }

        Debug.Log("Dark Nova hit " + targetsHit + " targets for " + totalDamageDealt + " total damage!");

        return totalDamageDealt;
    }

    private void CastHeal()
    {
        Debug.Log(unitName + " casts Healing Shadows!");

        MonsterUnit[] monsterUnits = FindObjectOfType<GameManager>().monsterUnits.ToArray();
        int unitsHealed = 0;
        int totalHealing = 0;

        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster.isAlive && monster.currentHealth < monster.maxHealth)
            {
                int healing = Mathf.RoundToInt(healAmount);
                monster.currentHealth = Mathf.Min(monster.currentHealth + healing, monster.maxHealth);

                // Update health bar
                if (monster.healthBar != null)
                    monster.healthBar.UpdateHealthBar(monster.currentHealth, monster.maxHealth);

                unitsHealed++;
                totalHealing += healing;

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"{unitName} heals {monster.unitName} for {healing} HP");
                }
            }
        }

        Debug.Log("Healed " + unitsHealed + " allies for " + totalHealing + " total HP!");
    }

    // Wizard targets highest threat or lowest health
    public override PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        // If using shadow bolt, target lowest health
        if (spellType == 0)
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

        // For other spells, we don't need a specific target
        // but we'll return one anyway for the function to work
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive)
                return target;
        }

        return null;
    }
}