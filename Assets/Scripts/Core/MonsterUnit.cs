using UnityEngine;

public class MonsterUnit : Unit
{
    public float aggressiveness = 0.5f; // Affects target selection
    
    // Method for AI to choose a target
    public virtual PlayerUnit SelectTarget(PlayerUnit[] possibleTargets)
    {
        // Simple target selection - can be improved later
        foreach (PlayerUnit target in possibleTargets)
        {
            if (target.isAlive)
                return target;
        }
        return null;
    }
}