using Cinemachine;
using System.Collections;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    // Singleton instance
    public static ScreenShakeManager Instance { get; private set; }

    // Default duration and magnitude of the shake
    public float defaultShakeDuration = 0.5f;
    public float defaultShakeMagnitude = 0.5f;

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private CinemachineBasicMultiChannelPerlin cinemachinePerlin;
    private Transform mainCameraTransform;
    private float initialAmplitude;
    private float initialFrequency;
    private Quaternion initialRotation;

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persistent between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetVirtualCameraReference();
    }

    private void SetVirtualCameraReference()
    {
        // Find the Cinemachine Virtual Camera in the scene
        cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (cinemachineVirtualCamera != null)
        {
            cinemachinePerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            mainCameraTransform = Camera.main.transform; // Cache the Main Camera's transform
            initialRotation = mainCameraTransform.localRotation; // Save the initial rotation of the Main Camera

            if (cinemachinePerlin != null)
            {
                initialAmplitude = cinemachinePerlin.m_AmplitudeGain;
                initialFrequency = cinemachinePerlin.m_FrequencyGain;
                Debug.Log("CinemachineBasicMultiChannelPerlin found and set up correctly.");
            }
            else
            {
                Debug.LogWarning("CinemachineBasicMultiChannelPerlin is not set up correctly on the CinemachineVirtualCamera.");
            }
        }
        else
        {
            Debug.LogWarning("No CinemachineVirtualCamera found in the scene.");
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        if (cinemachineVirtualCamera == null || cinemachinePerlin == null)
        {
            SetVirtualCameraReference();
        }

        if (cinemachinePerlin != null)
        {
            StartCoroutine(Shake(duration, magnitude));
        }
        else
        {
            Debug.LogWarning("Cinemachine Perlin Noise is not set up correctly.");
        }
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;

        // Set initial shake values
        cinemachinePerlin.m_AmplitudeGain = magnitude;
        cinemachinePerlin.m_FrequencyGain = magnitude;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to initial noise values after shaking
        cinemachinePerlin.m_AmplitudeGain = initialAmplitude;
        cinemachinePerlin.m_FrequencyGain = initialFrequency;

        // Explicitly reset camera rotation to the original rotation
        if (mainCameraTransform != null)
        {
            mainCameraTransform.localRotation = initialRotation;
        }
    }
}
