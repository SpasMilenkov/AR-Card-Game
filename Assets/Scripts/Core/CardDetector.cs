using UnityEngine;

public class CardDetector : MonoBehaviour
{
    public int cardType; // 0 = Archer, 1 = Knight, 2 = Mage, 3 = Warrior, 4 = Rogue
    private bool wasDetected = false;

    // You'll need to connect this to the DefaultObserverEventHandler's OnTargetFound event
    // in the Inspector by adding this method to the Unity Event
    public void OnCardFound()
    {
        if (!wasDetected && GameManager.Instance.currentState == GameManager.GameState.Setup)
        {
            // Spawn unit at this position
            GameManager.Instance.SpawnPlayerUnit(cardType, transform);
            wasDetected = true;
            Debug.Log("Card Type " + cardType + " detected and unit spawned!");
        }
    }

    // For testing or resetting
    public void ResetDetection()
    {
        wasDetected = false;
    }
}