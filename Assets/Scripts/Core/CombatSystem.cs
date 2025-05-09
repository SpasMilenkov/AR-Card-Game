using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance;
    
    private bool isSelectingTarget = false;
    private bool isUsingAbility = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void StartTargetSelection(bool forAbility)
    {
        isSelectingTarget = true;
        isUsingAbility = forAbility;
    }
    
    private void Update()
    {
        // Handle target selection
        if (isSelectingTarget && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Check if we hit a monster
                MonsterUnit monster = hit.collider.GetComponent<MonsterUnit>();
                
                if (monster != null && monster.isAlive)
                {
                    // Target selected
                    if (isUsingAbility)
                    {
                        UseAbilityOnTarget(monster);
                    }
                    else
                    {
                        AttackTarget(monster);
                    }
                    
                    // End selection mode
                    isSelectingTarget = false;
                    
                    // Move to next unit
                    GameManager.Instance.SetNextActivePlayerUnit();
                }
            }
        }
    }
    
    private void AttackTarget(Unit target)
    {
        PlayerUnit activeUnit = GameManager.Instance.currentActiveUnit;
        if (activeUnit != null)
        {
            activeUnit.Attack(target);
        }
    }
    
    private void UseAbilityOnTarget(Unit target)
    {
        PlayerUnit activeUnit = GameManager.Instance.currentActiveUnit;
        if (activeUnit != null)
        {
            // Handle different abilities based on unit type
            if (activeUnit is Warrior)
            {
                // Whirlwind hits all monsters
                activeUnit.UseAbility(GameManager.Instance.monsterUnits.ToArray());
            }
            else if (activeUnit is Mage)
            {
                // Fireball hits targeted monster and adjacent monsters
                List<Unit> targets = new List<Unit>();
                targets.Add(target);
                
                // Add adjacent monsters if needed
                
                activeUnit.UseAbility(targets.ToArray());
            }
            else
            {
                // Single target abilities
                activeUnit.UseAbility(new Unit[] { target });
            }
        }
    }
}