using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    // Basic stats
    public string unitName;
    public int maxHealth;
    public int currentHealth;
    public int attackDamage;
    public UnitType unitType;
    public bool isAlive = true;
    
    // References
    public GameObject unitModel;
    public Transform attackPoint;
    public HealthBar healthBar;
    
    // Enum for unit categories
    public enum UnitType { Melee, Ranged, Tank, Spellcaster, Assassin }
    
    // Virtual methods for abilities
    public virtual void Attack(Unit target)
    {
        target.TakeDamage(attackDamage);
    }
    
    public virtual void UseAbility(Unit[] targets)
    {
        // Override in child classes
    }
    
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Update health bar
        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        
        // Check if dead
        if (currentHealth <= 0)
        {
            isAlive = false;
            Die();
        }
    }
    
    protected virtual void Die()
    {
        // Common death behavior
        gameObject.SetActive(false);
        GameManager.Instance.CheckBattleStatus();
    }
}