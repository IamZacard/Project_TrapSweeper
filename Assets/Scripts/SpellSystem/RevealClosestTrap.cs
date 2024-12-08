using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spells/RevealClosestTrap")]
public class RevealClosestTrap : Spell
{
    [Header("Spell Settings")]
    public int searchRange = 10; // Range to search for traps

    public override void Cast(CharacterBase character)
    {
        base.Cast(character);

        // Ensure the character is of type Gale
        if (character is not Gale gale)
        {
            Debug.LogWarning("Character is not Gale! Spell aborted.");
            return;
        }

        // Check shard requirement
        if (gale.shardCount < gale.requiredShardsForTrapReveal)
        {
            Debug.LogWarning("Not enough shards to cast the reveal spell!");
            return;
        }

        // Reveal the closest trap
        RevealClosestTrapInRange(gale);
    }

    private void RevealClosestTrapInRange(Gale gale)
    {
        GamePlay gamePlay = GameObject.FindObjectOfType<GamePlay>();
        if (gamePlay == null)
        {
            Debug.LogWarning("GamePlay instance not found.");
            return;
        }

        Vector3Int characterPosition = Vector3Int.FloorToInt(gale.transform.position);
        List<Cell> nearbyTrapCells = new List<Cell>();

        for (int x = -searchRange; x <= searchRange; x++)
        {
            for (int y = -searchRange; y <= searchRange; y++)
            {
                Vector3Int cellPosition = characterPosition + new Vector3Int(x, y, 0);

                if (IsWithinGridBounds(cellPosition, gamePlay) &&
                    gamePlay.TryGetCellAtPosition(new Vector2Int(cellPosition.x, cellPosition.y), out Cell cell) &&
                    cell.type == Cell.Type.Trap && !cell.revealed)
                {
                    nearbyTrapCells.Add(cell);
                }
            }
        }

        if (nearbyTrapCells.Count > 0)
        {
            // Reveal a random trap from the nearby traps
            Cell selectedTrapCell = nearbyTrapCells[Random.Range(0, nearbyTrapCells.Count)];
            gamePlay.RevealCellTrap(selectedTrapCell);
            Debug.Log($"Revealed trap at position: {selectedTrapCell.position}");
        }
        else
        {
            Debug.Log("No traps found within the specified range.");
        }
    }

    private bool IsWithinGridBounds(Vector3Int position, GamePlay gamePlay)
    {
        return position.x >= 0 && position.x < gamePlay.width &&
               position.y >= 0 && position.y < gamePlay.height;
    }
}
