using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance { get; private set; }

    [Header("Selected Character")]
    [SerializeField] private string selectedCharacter;

    [Header("Character Prefabs")]
    [SerializeField] private List<CharacterPrefab> characterPrefabs; // ScriptableObject or manually assigned in Inspector

    private Dictionary<string, GameObject> characterPrefabDict;

    public string SelectedCharacter
    {
        get => selectedCharacter;
        private set => selectedCharacter = value;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize the prefab dictionary
            characterPrefabDict = new Dictionary<string, GameObject>();
            foreach (var prefab in characterPrefabs)
            {
                characterPrefabDict[prefab.characterName] = prefab.prefab;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectedCharacter(string characterName)
    {
        SelectedCharacter = characterName;
        Debug.Log($"Selected character stored: {SelectedCharacter}");
    }

    public GameObject GetSelectedCharacterPrefab()
    {
        if (characterPrefabDict.TryGetValue(selectedCharacter, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogError($"No prefab found for character: {selectedCharacter}");
        return null;
    }
}

[System.Serializable]
public class CharacterPrefab
{
    public string characterName;
    public GameObject prefab;
}
