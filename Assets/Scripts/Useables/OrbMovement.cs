using UnityEngine;
using DG.Tweening;

public class OrbMovement : MonoBehaviour
{
    public float amplitude = 0.1f; // Base height of the bobbing movement
    public float speed = 3f;      // Base speed of the movement (up and down)
    public float randomAmplitudeRange = 0.05f; // Random variation for amplitude
    public float randomSpeedRange = 0.5f;      // Random variation for speed
    public float randomStartDelay = 0.5f;      // Max random delay before starting

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Save initial position

        // Generate random values
        float randomAmplitude = amplitude + Random.Range(-randomAmplitudeRange, randomAmplitudeRange);
        float randomSpeed = speed + Random.Range(-randomSpeedRange, randomSpeedRange);
        float randomDelay = Random.Range(0, randomStartDelay);

        // Create the bobbing motion with randomness
        DOVirtual.DelayedCall(randomDelay, () =>
        {
            transform.DOMoveY(startPosition.y + randomAmplitude, 1f / randomSpeed)
                     .SetLoops(-1, LoopType.Yoyo) // Loop infinitely, Yoyo for back-and-forth
                     .SetEase(Ease.InOutSine);   // Smooth easing for natural movement
        });
    }
}