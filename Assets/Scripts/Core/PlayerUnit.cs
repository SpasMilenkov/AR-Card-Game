using UnityEngine;

public class PlayerUnit : Unit
{
    public int abilityCooldown;
    public int currentCooldown = 0;
    
    public void ResetCooldown()
    {
        currentCooldown = 0;
    }
    
    public void DecreaseCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }
    
    public bool CanUseAbility()
    {
        return currentCooldown <= 0;
    }
    
    public override void UseAbility(Unit[] targets)
    {
        if (CanUseAbility())
        {
            // Ability logic in child classes
            currentCooldown = abilityCooldown;
        }
    }
}