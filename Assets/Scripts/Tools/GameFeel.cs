using UnityEngine;
using UnityEngine.TextCore.Text;
public class GameFeel
{
    private AudioManager.SoundType soundType;
    private float soundVolume;
    private float shakeDuration;
    private float shakeMagnitude;
    private EffectManager.EffectType effectType;
    private Vector3 position; // Change from Transform to Vector3

    // Constructor to initialize game feel elements
    public GameFeel(
        AudioManager.SoundType soundType,
        float soundVolume,
        float shakeDuration,
        float shakeMagnitude,
        EffectManager.EffectType effectType,
        Vector3 position) // Accept Vector3 instead of Transform
    {
        this.soundType = soundType;
        this.soundVolume = soundVolume;
        this.shakeDuration = shakeDuration;
        this.shakeMagnitude = shakeMagnitude;
        this.effectType = effectType;
        this.position = position;
    }

    // Trigger all game feel elements
    public void Trigger()
    {
        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(soundType, soundVolume);
        }

        // Trigger screen shake
        if (ScreenShakeManager.Instance != null)
        {
            ScreenShakeManager.Instance.TriggerShake(shakeDuration, shakeMagnitude);
        }

        // Trigger visual effect
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.TriggerEffect(effectType, position, Quaternion.identity);
        }
    }
}
