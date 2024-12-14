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
}