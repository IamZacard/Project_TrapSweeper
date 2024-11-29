using UnityEngine;

public class OrbMovement : MonoBehaviour
{
    public float amplitude = 0.1f; // Max height of the bobbing movement
    public float speed = 3f;      // Speed of the movement (up and down)

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        startPosition = transform.position; // Save initial position
    }

    void Update()
    {
        // Update the timer based on speed
        timer += Time.deltaTime * speed;

        // Calculate new position using a sine wave for smooth movement
        float yOffset = Mathf.Sin(timer) * amplitude;

        // Apply the new position while keeping X and Z constant
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }

    // Optional: Public methods to dynamically change amplitude or speed
    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
