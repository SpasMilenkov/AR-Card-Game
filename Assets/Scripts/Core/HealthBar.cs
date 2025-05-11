// Updated HealthBar.cs to allow disabling camera facing

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HealthBar component for displaying unit health
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;            // Image component for the fill bar
    public TextMeshProUGUI healthText; // Optional text to show health values
    
    [Header("Settings")]
    public bool showNumbers = true;    // Whether to show numerical health
    public bool hideAtFullHealth = false; // Hide the bar when health is full
    public bool alwaysFaceCamera = false; // Set to false to maintain orientation with unit
    
    [Header("Colors")]
    public Color healthyColor = new Color(0.0f, 0.75f, 0.0f);
    public Color middleColor = new Color(0.9f, 0.9f, 0.0f);  
    public Color lowColor = new Color(0.9f, 0.0f, 0.0f);
    
    // Health thresholds
    private const float MIDDLE_HEALTH_THRESHOLD = 0.65f;
    private const float LOW_HEALTH_THRESHOLD = 0.35f;
    
    private void Start()
    {
        // Initially hide if needed
        if (hideAtFullHealth)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void LateUpdate()
    {
        // Only face camera if enabled
        if (alwaysFaceCamera && Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
    
    /// <summary>
    /// Updates the health bar fill and text
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // Calculate health ratio
        float healthRatio = Mathf.Clamp01(currentHealth / maxHealth);
        
        // Update fill amount
        if (fillImage != null)
        {
            fillImage.fillAmount = healthRatio;
            
            // Update color based on health ratio
            if (healthRatio <= LOW_HEALTH_THRESHOLD)
            {
                fillImage.color = lowColor;
            }
            else if (healthRatio <= MIDDLE_HEALTH_THRESHOLD)
            {
                fillImage.color = middleColor;
            }
            else
            {
                fillImage.color = healthyColor;
            }
        }
        
        // Update health text if needed
        if (showNumbers && healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
        
        // Show/hide based on settings
        if (hideAtFullHealth)
        {
            gameObject.SetActive(healthRatio < 1.0f);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}