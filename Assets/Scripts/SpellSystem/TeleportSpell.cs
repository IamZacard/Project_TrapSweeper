using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "TeleportSpell", menuName = "Spells/TeleportSpell")]
public class TeleportSpell : Spell
{
    public static event System.Action<Vector3> OnPlayerTPed;
    public override void Activate(CharacterBase character)
    {
        base.Activate(character);
        Debug.Log($"{spellName} activated for teleportation!");

        // Start teleportation logic
        TeleportOnClick(character);
    }

    private void TeleportOnClick(CharacterBase character)
    {
        GamePlay gamePlay = FindObjectOfType<GamePlay>();
        if (gamePlay == null)
        {
            Debug.LogError("GamePlay script not found in the scene!");
            return;
        }

        character.StartCoroutine(TeleportRoutine(character, gamePlay));
    }

    private IEnumerator TeleportRoutine(CharacterBase character, GamePlay gamePlay)
    {
        while (true)
        {
            // Handle teleportation on left-click
            if (Input.GetMouseButtonDown(0) && gamePlay.TryGetCellAtMousePosition(out Cell cell))
            {
                Vector3 newPosition = new Vector3(cell.position.x + 0.5f, cell.position.y + 0.5f, 0f);
                character.transform.position = newPosition;
                Debug.Log($"Teleported {character.name} to {cell.position}");


                // Notify GamePlay script via event
                OnPlayerTPed?.Invoke(character.transform.position);

                yield break;
            }

            yield return null;
        }
    }
}
