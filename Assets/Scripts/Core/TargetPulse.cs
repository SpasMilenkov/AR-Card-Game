using UnityEngine;

public class TargetPulse : MonoBehaviour
{
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.2f;

    private Vector3 originalScale;
    private float pulseTime;

    private void Start()
    {
        originalScale = transform.localScale;
        pulseTime = Random.Range(0f, 2f); // Randomize starting phase
    }

    private void Update()
    {
        pulseTime += Time.deltaTime * pulseSpeed;
        float pulse = 1f + Mathf.Sin(pulseTime) * pulseAmount;

        transform.localScale = originalScale * pulse;
    }
}