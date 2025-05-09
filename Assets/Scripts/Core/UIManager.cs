using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Panel references
    public GameObject actionPanel;
    public GameObject targetSelectionPanel;
    public GameObject gameOverPanel;
    
    // Button references
    public Button attackButton;
    public Button abilityButton;
    public TextMeshProUGUI abilityButtonText;
    
    // Unit info display
    public TextMeshProUGUI currentUnitNameText;
    public TextMeshProUGUI currentUnitHealthText;
    
    // Game over screen
    public TextMeshProUGUI gameResultText;
    public Button restartButton;
    
    private void Start()
    {
        // Set up button listeners
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        abilityButton.onClick.AddListener(OnAbilityButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        
        // Hide all panels initially
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }
    
    public void UpdateUnitUI(PlayerUnit unit)
    {
        // Show action panel
        actionPanel.SetActive(true);
        targetSelectionPanel.SetActive(false);
        
        // Update unit info
        currentUnitNameText.text = unit.unitName;
        currentUnitHealthText.text = "HP: " + unit.currentHealth + "/" + unit.maxHealth;
        
        // Update ability button
        abilityButtonText.text = GetAbilityName(unit);
        abilityButton.interactable = unit.CanUseAbility();
    }
    
    private string GetAbilityName(PlayerUnit unit)
    {
        // Return ability name based on unit type
        if (unit is Warrior) return "Whirlwind";
        if (unit is Knight) return "Shield Block";
        if (unit is Archer) return "Aimed Shot";
        if (unit is Mage) return "Fireball";
        if (unit is Rogue) return "Backstab";
        
        return "Ability";
    }
    
    private void OnAttackButtonClicked()
    {
        // Show target selection UI
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(true);
        
        // Enable target selection mode
        CombatSystem.Instance.StartTargetSelection(false);
    }
    
    private void OnAbilityButtonClicked()
    {
        // Show target selection UI (if needed)
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(true);
        
        // Enable ability target selection mode
        CombatSystem.Instance.StartTargetSelection(true);
    }
    
    public void ShowGameOverScreen(bool victory)
    {
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        
        gameResultText.text = victory ? "Victory!" : "Defeat!";
    }
    
    private void OnRestartButtonClicked()
    {
        // Reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}