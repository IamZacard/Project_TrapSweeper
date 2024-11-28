using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public enum SoundType
    {
        EnterWordSound,
        CharacterPick,
        ButtonClick,
        ErrorSound,
        LevelStartSound,
        FootStepSound,
        LevelComplete,
        LoseStepOnTrap,
        BoardRevealSound,
        PlateRevealSound,
        Success,
        MagicBlockReleaseSound,
        PortalSound,
        FlagSpell,

        GaleBlast,
        GalePickUp,
        VioletTeleport,
        MysticInvincible,
        SageReveal,
        ShuffProc,
        ShuffExplotion,
        CatMeowSound,

        TalkSound,               
        DialogStart, 
    }

    [System.Serializable]
    public class SoundEntry
    {
        public SoundType soundType;
        public AudioClip audioClip;
    }

    public List<SoundEntry> soundEntries;

    private static AudioManager instance;
    private AudioSource audioSource;

    public Slider musicVolumeSlider;
    public Slider effectsVolumeSlider;

    // Ensure these are assigned in the inspector.
    public AudioSource musicPlayer;
    public AudioSource effectsPlayer;

    public TextMeshProUGUI musicPlayerNumber;
    public TextMeshProUGUI effectsPlayerNumber;

    private static float volumeVar = 100f;

    public GameObject OptionsPanel;

    [Range(0f, 1f)]
    public float volume = 1f; // Default volume value is 1 (maximum)

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        effectsVolumeSlider.onValueChanged.AddListener(SetEffectsVolume);

        // Initialize the sliders with the actual volume values
        musicVolumeSlider.value = musicPlayer.volume * 100f; // Convert to 0-100 range
        effectsVolumeSlider.value = effectsPlayer.volume * 100f;

        musicPlayer.volume = 0.5f; // Default to 50%
        effectsPlayer.volume = 0.5f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OptionsPanel.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            OptionsPanel.SetActive(!OptionsPanel.activeSelf);

            // Sync sliders and UI text with current AudioSource volume when panel is opened
            if (OptionsPanel.activeSelf)
            {
                musicVolumeSlider.value = musicPlayer.volume * 100f;
                effectsVolumeSlider.value = effectsPlayer.volume * 100f;

                musicPlayerNumber.text = (musicPlayer.volume * 100f).ToString("F0");
                effectsPlayerNumber.text = (effectsPlayer.volume * 100f).ToString("F0");
            }
        }
    }

    private void SetMusicVolume(float volume)
    {
        musicPlayer.volume = volume / 100f; // Convert 0-100 range to 0-1 for AudioSource
        musicPlayerNumber.text = volume.ToString("F0"); // Update slider text
    }

    private void SetEffectsVolume(float volume)
    {
        effectsPlayer.volume = volume / 100f; // Convert 0-100 range to 0-1 for AudioSource
        effectsPlayerNumber.text = volume.ToString("F0"); // Update slider text
    }

    public void ApplyMusicSettings()
    {
        musicPlayer.volume = volumeVar / 100f; // Adjust based on volumeVar if needed
    }

    public void ApplyEffectsSettings()
    {
        effectsPlayer.volume = volumeVar / 100f; // Adjust based on volumeVar if needed
    }

    public void PlaySound(SoundType soundType, float pitch)
    {
        AudioClip clip = GetAudioClip(soundType);
        if (clip != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlaySound(AudioClip clip, float pitch)
    {
        if (clip != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetAudioClip(SoundType soundType)
    {
        foreach (var entry in soundEntries)
        {
            if (entry.soundType == soundType)
                return entry.audioClip;
        }
        Debug.LogError("Sound entry not found for " + soundType);
        return null;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume; 
    }

    public void ToggleOptionsPanel()
    {
        OptionsPanel.SetActive(!OptionsPanel.activeSelf);
    }
}
