using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Light2DFlickerController : MonoBehaviour
{
    private Light2D light2D;

    // Flicker parameters
    public float minInnerRadius = 1.0f;
    public float maxInnerRadius = 2.0f;
    public float minOuterRadius = 2.5f;
    public float maxOuterRadius = 3.5f;
    public float flickerSpeed = 1.0f; // Speed multiplier for flicker

    void Start()
    {
        light2D = GetComponent<Light2D>();
        if (light2D == null)
        {
            Debug.LogError("Light2D component not found on the GameObject.");
            return;
        }

        StartCoroutine(FlickerCoroutine());
    }

    private IEnumerator FlickerCoroutine()
    {
        while (true)
        {
            // Generate new radii values based on flicker effect
            float newInnerRadius = Mathf.Lerp(minInnerRadius, maxInnerRadius, Mathf.PerlinNoise(Time.time * flickerSpeed, 0f));
            float newOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, Mathf.PerlinNoise(0f, Time.time * flickerSpeed));

            // Apply the new radii to the Light2D component
            light2D.pointLightInnerRadius = newInnerRadius;
            light2D.pointLightOuterRadius = newOuterRadius;

            yield return null;
        }
    }
}
