using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for pointer events

public class CharacterSelection : MonoBehaviour
{
    [Header("Buttons")]
    public Button galeButton;
    public Button violetButton;
    public Button mysticButton;
    public Button sageButton;
    public Button shufflegrinButton;
    public Button startGameButton;

    [Header("Selected Character")]
    private string selectedCharacter;

    private void Start()
    {
        // Disable the start button initially
        startGameButton.interactable = false;

        // Assign methods to each button
        galeButton.onClick.AddListener(() => SelectCharacter("Gale"));
        violetButton.onClick.AddListener(() => SelectCharacter("Violet"));
        mysticButton.onClick.AddListener(() => SelectCharacter("Mystic"));
        sageButton.onClick.AddListener(() => SelectCharacter("Sage"));
        shufflegrinButton.onClick.AddListener(() => SelectCharacter("Shufflegrin"));
        startGameButton.onClick.AddListener(StartGame);

        // Add hover scaling to each button
        AddHoverEffects(galeButton);
        AddHoverEffects(violetButton);
        AddHoverEffects(mysticButton);
        AddHoverEffects(sageButton);
        AddHoverEffects(shufflegrinButton);
    }

    private void SelectCharacter(string characterName)
    {
        selectedCharacter = characterName;
        AudioManager.Instance.PlaySound(AudioManager.SoundType.CharacterPick, 1f);

        Debug.Log($"Selected Character: {selectedCharacter}");

        // Enable start game button
        startGameButton.interactable = true;
    }

    private void StartGame()
    {
        if (string.IsNullOrEmpty(selectedCharacter)) return;

        AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick, 1f);

        // Store selected character in CharacterSelectionManager
        CharacterSelectionManager.Instance.SetSelectedCharacter(selectedCharacter);

        // Load the next scene
        SceneManager.LoadScene("Level1");
    }

    private void AddHoverEffects(Button button)
    {
        // Add EventTrigger component if not already present
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Add PointerEnter event for scaling up
        EventTrigger.Entry entryEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entryEnter.callback.AddListener((data) => OnHoverEnter(button));
        trigger.triggers.Add(entryEnter);

        // Add PointerExit event for scaling down
        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((data) => OnHoverExit(button));
        trigger.triggers.Add(entryExit);
    }

    private void OnHoverEnter(Button button)
    {
        button.transform.localScale = Vector3.one * 1.2f; // Scale to 1.2
        AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick, 1f);
    }

    private void OnHoverExit(Button button)
    {
        button.transform.localScale = Vector3.one; // Reset to default scale
    }
}
