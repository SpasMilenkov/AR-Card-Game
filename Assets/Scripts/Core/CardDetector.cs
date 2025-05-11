using UnityEngine;

/// <summary>
/// Detects card recognition events from Vuforia and spawns the appropriate unit
/// Simplified to let Vuforia handle positioning
/// </summary>
public class CardDetector : MonoBehaviour
{
    [Header("Card Settings")]
    [Tooltip("Type of unit to spawn (0=Archer, 1=Knight, 2=Mage, 3=Warrior, 4=Rogue)")]
    [Range(0, 4)]
    public int cardType = 0;

    [Header("Debug")]
    [Tooltip("Show visual indicator for card tracking")]
    public bool showVisualIndicator = false;

    // Track if this card has already been detected
    private bool wasDetected = false;

    // Visual indicator object
    private GameObject visualIndicator;

    // Card type names for better debugging
    private static readonly string[] UnitTypeNames =
    {
        "Archer", "Knight", "Mage", "Warrior", "Rogue"
    };

    // Used to track this card's index (for player info)
    private int cardIndex;
    private static int nextCardIndex = 0;

    private void Awake()
    {
        // Assign a unique index to this card
        cardIndex = nextCardIndex++;
    }

    /// <summary>
    /// Called by Vuforia's DefaultObserverEventHandler when a card is recognized
    /// </summary>
    public void OnCardFound()
    {
        // Only respond if this card hasn't been detected yet and game is in setup state
        if (!wasDetected && GameManager.Instance != null &&
            GameManager.Instance.currentState == GameManager.GameState.Setup)
        {
            string unitTypeName = (cardType >= 0 && cardType < UnitTypeNames.Length)
                ? UnitTypeNames[cardType]
                : $"Unknown ({cardType})";

            Debug.Log($"Card detected: {unitTypeName} at position {transform.position}");

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                GameInfoLayer.Instance.RegisterCardDetection(cardType, cardIndex);
                GameInfoLayer.Instance.AddLogEntry($"Card detected: {unitTypeName}");
            }

            // Mark as detected to prevent duplicate spawns
            wasDetected = true;

            // Request unit spawn from GameManager - pass this transform
            GameManager.Instance.SpawnPlayerUnit(cardType, transform);
        }
    }

    /// <summary>
    /// Called by Vuforia's DefaultObserverEventHandler when a card is lost from view
    /// </summary>
    public void OnCardLost()
    {
        // Only relevant during setup phase if card was previously detected
        if (wasDetected && GameManager.Instance != null &&
            GameManager.Instance.currentState == GameManager.GameState.Setup)
        {
            // Update visual indicator to show tracking loss
            if (visualIndicator != null)
            {
                visualIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            }

            // We don't remove the unit, just indicate tracking loss
            if (GameInfoLayer.Instance != null)
            {
                string unitTypeName = (cardType >= 0 && cardType < UnitTypeNames.Length)
                    ? UnitTypeNames[cardType]
                    : $"Unknown ({cardType})";

                GameInfoLayer.Instance.AddLogEntry($"Card tracking lost: {unitTypeName}. Unit remains spawned.");
            }
        }
    }

    /// <summary>
    /// Resets the card detection status (used when restarting game)
    /// </summary>
    public void ResetDetection()
    {
        if (wasDetected)
        {
            wasDetected = false;

            // Update info layer
            if (GameInfoLayer.Instance != null)
            {
                string unitTypeName = (cardType >= 0 && cardType < UnitTypeNames.Length)
                    ? UnitTypeNames[cardType]
                    : $"Unknown ({cardType})";

                GameInfoLayer.Instance.AddLogEntry($"Card reset: {unitTypeName}");
            }

            // Reset visual indicator
            if (visualIndicator != null)
            {
                // Reset color based on card type
                Renderer renderer = visualIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color;
                    switch (cardType)
                    {
                        case 0: color = new Color(0.0f, 0.8f, 0.0f); break; // Archer (green)
                        case 1: color = new Color(0.0f, 0.0f, 0.8f); break; // Knight (blue)
                        case 2: color = new Color(0.8f, 0.0f, 0.8f); break; // Mage (purple)
                        case 3: color = new Color(0.8f, 0.0f, 0.0f); break; // Warrior (red)
                        case 4: color = new Color(0.8f, 0.8f, 0.0f); break; // Rogue (yellow)
                        default: color = Color.white; break;
                    }
                    renderer.material.color = color;
                }
            }

            // Destroy any child units
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<PlayerUnit>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}