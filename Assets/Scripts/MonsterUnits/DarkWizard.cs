using UnityEngine;
using System.Collections;

public class DarkWizard : MonsterUnit
{
    public float shadowBoltDamage = 20f;
    public float aoeSpellDamage = 12f; // Lower damage but hits all
    public float healAmount = 15f; // Amount to heal allies

    private int spellType = 0; // 0 = shadow bolt, 1 = AoE, 2 = heal

    private void Awake()
    {
        // Get the animator component
        animator = GetComponent<Animator>();

        unitName = "Dark Wizard";
        maxHealth = 80;
        currentHealth = maxHealth;
        attackDamage = 8; // Low basic attack
        unitType = UnitType.Spellcaster;
        aggressiveness = 0.4f; // Balanced targeting

        // Set animation timing properties
        attackAnimationDelay = 0.6f; // Spell casting takes longer
    }

    // Override attack method with proper timing based on spell type
    protected override IEnumerator PlayMonsterAttackAnimation(Unit target)
    {
        // Choose a spell based on situation
        ChooseSpell();

        // Different animations and timing based on spell type
        switch (spellType)
        {
            case 0:
                yield return StartCoroutine(CastShadowBoltWithAnimation(target));
                break;
            case 1:
                yield return StartCoroutine(CastAoeSpellWithAnimation());
                break;
            case 2:
                yield return StartCoroutine(CastHealWithAnimation());
                break;
        }
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

    // Shadow Bolt spell with animation timing
    private IEnumerator CastShadowBoltWithAnimation(Unit target)
    {
        if (target == null || !target.isAlive)
            yield break;

        // Play Shadow Bolt animation
        if (animator != null)
        {
            animator.SetTrigger("Attack"); // Use standard attack for shadow bolt
        }

        // Wait for animation to reach the "cast" point
        yield return new WaitForSeconds(attackAnimationDelay);

        // Cast announcement
        Debug.Log(unitName + " casts Shadow Bolt at " + target.unitName + "!");

        // Update info layer before damage
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} casts Shadow Bolt at {target.unitName}!");
        }

        // Apply damage
        int damage = Mathf.RoundToInt(shadowBoltDamage);
        target.TakeDamage(damage);

        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.RegisterBattleAction(unitName, target.unitName, "casts Shadow Bolt on", damage);
        }
    }

    // AoE Dark Nova spell with animation timing
    private IEnumerator CastAoeSpellWithAnimation()
    {
        // Play AoE spell animation
        if (animator != null)
        {
            animator.SetTrigger("Ability"); // Use ability animation for AoE
        }

        // Cast announcement
        Debug.Log(unitName + " casts Dark Nova, hitting all players!");

        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} begins casting Dark Nova!");
        }

        // Wait for animation to reach the "cast" point - AoE needs more time
        yield return new WaitForSeconds(0.8f);

        // Get all player units
        PlayerUnit[] playerUnits = FindObjectOfType<GameManager>().playerUnits.ToArray();
        int targetsHit = 0;
        int totalDamageDealt = 0;

        // Create a central effect for the spell
        GameObject centralEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centralEffect.transform.position = transform.position + Vector3.up * 0.5f;
        centralEffect.transform.localScale = Vector3.one * 0.2f;

        // Set material properties for dark energy effect
        Renderer renderer = centralEffect.GetComponent<Renderer>();
        renderer.material.color = new Color(0.5f, 0, 0.5f, 0.8f);
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", new Color(0.5f, 0, 0.5f) * 0.8f);

        // Remove collider
        Destroy(centralEffect.GetComponent<Collider>());

        // Wait for effect to expand
        float duration = 0.5f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            centralEffect.transform.localScale = Vector3.one * 0.2f * (1 + t * 3);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy central effect
        Destroy(centralEffect);

        // Hit all player units with a small delay between hits
        foreach (PlayerUnit player in playerUnits)
        {
            if (player.isAlive)
            {
                int damage = Mathf.RoundToInt(aoeSpellDamage);
                player.TakeDamage(damage);
                targetsHit++;
                totalDamageDealt += damage;

                // Create individual hit effects
                if (CombatSystem.Instance != null)
                {
                    CombatSystem.Instance.PlayCombatEffectAt(
                        CombatSystem.Instance.attackEffectPrefab,
                        player.transform.position);
                }

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.RegisterBattleAction(unitName, player.unitName, "hits with Dark Nova", damage);
                }

                // Small delay between hits for better visual
                yield return new WaitForSeconds(0.1f);
            }
        }

        Debug.Log("Dark Nova hit " + targetsHit + " targets for " + totalDamageDealt + " total damage!");

        // Final summary
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"Dark Nova hit {targetsHit} targets for {totalDamageDealt} total damage!");
        }
    }

    // Healing spell with animation timing
    private IEnumerator CastHealWithAnimation()
    {
        // Play Heal animation
        if (animator != null)
        {
            // You could use a special "Heal" trigger if you have one, or reuse "Ability"
            animator.SetTrigger("Ability");
        }

        // Cast announcement
        Debug.Log(unitName + " casts Healing Shadows!");

        // Update info layer
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"{unitName} begins casting Healing Shadows!");
        }

        // Wait for animation to reach the "cast" point
        yield return new WaitForSeconds(0.7f);

        // Apply healing
        MonsterUnit[] monsterUnits = FindObjectOfType<GameManager>().monsterUnits.ToArray();
        int unitsHealed = 0;
        int totalHealing = 0;

        foreach (MonsterUnit monster in monsterUnits)
        {
            if (monster.isAlive && monster.currentHealth < monster.maxHealth)
            {
                int healing = Mathf.RoundToInt(healAmount);
                monster.currentHealth = Mathf.Min(monster.currentHealth + healing, monster.maxHealth);

                unitsHealed++;
                totalHealing += healing;

                // Create healing visual effect
                GameObject healEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                healEffect.transform.position = monster.transform.position;
                healEffect.transform.localScale = Vector3.one * 0.2f;

                // Green healing effect
                Renderer renderer = healEffect.GetComponent<Renderer>();
                renderer.material.color = Color.green;
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", Color.green * 0.8f);

                // Remove collider
                Destroy(healEffect.GetComponent<Collider>());

                // Animate and destroy the effect
                StartCoroutine(AnimateAndDestroyEffect(healEffect.transform));

                // Update info layer
                if (GameInfoLayer.Instance != null)
                {
                    GameInfoLayer.Instance.AddLogEntry($"{unitName} heals {monster.unitName} for {healing} HP");
                }

                // Small delay between heals for better visuals
                yield return new WaitForSeconds(0.2f);
            }
        }

        Debug.Log("Healed " + unitsHealed + " allies for " + totalHealing + " total HP!");

        // Final summary
        if (GameInfoLayer.Instance != null)
        {
            GameInfoLayer.Instance.AddLogEntry($"Healing Shadows restored {totalHealing} HP to {unitsHealed} allies");
        }
    }

    // Helper for animating visual effects
    private IEnumerator AnimateAndDestroyEffect(Transform effectTransform)
    {
        Vector3 startScale = effectTransform.localScale;
        Vector3 endScale = startScale * 2f;
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Fade out
            Renderer renderer = effectTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1f - t;
                renderer.material.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(effectTransform.gameObject);
    }

    // Override to return expected damage based on spell type
    public override int PerformAttack(Unit target)
    {
        switch (spellType)
        {
            case 0: // Shadow Bolt
                return Mathf.RoundToInt(shadowBoltDamage);
            case 1: // AoE
                // Return damage per target - total damage would be this * number of targets
                return Mathf.RoundToInt(aoeSpellDamage);
            case 2: // Heal
                return 0; // No damage for healing spell
            default:
                return attackDamage;
        }
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