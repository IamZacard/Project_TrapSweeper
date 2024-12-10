using UnityEngine;

[CreateAssetMenu(fileName = "RevealSurroundingsSpell", menuName = "Spells/RevealSurroundingsSpell")]
public class RevealSurroundingsSpell : Spell
{
    public override void Cast(CharacterBase character)
    {
        if (remainingCasts <= 0)
        {
            Debug.LogWarning($"{spellName} has no casts remaining.");
            return;
        }

        GamePlay gamePlay = GameObject.FindObjectOfType<GamePlay>();
        if (gamePlay == null)
        {
            Debug.LogWarning("GamePlay instance not found.");
            return;
        }

        if (!gamePlay.IsLevelGenerated || gamePlay.IsGameOver || gamePlay.IsLevelComplete)
        {
            Debug.LogWarning("Cannot cast spell under current game conditions.");
            return;
        }

        Vector2Int characterPosition = gamePlay.GetCellPosition(character.transform.position);
        if (characterPosition == Vector2Int.zero)
        {
            Debug.LogWarning("Invalid character position.");
            return;
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int targetPosition = characterPosition + new Vector2Int(x, y);

                if (gamePlay.TryGetCellAtPosition(targetPosition, out Cell cell))
                {
                    if (!cell.revealed)
                    {
                        cell.revealed = true;
                        gamePlay.UpdateBoard();
                    }
                }
            }
        }

        if (gamePlay.TryGetCellAtPosition(characterPosition, out Cell characterCell))
        {
            RevealSurCellsGameFeel(characterCell);
        }

        Debug.Log($"{spellName}: Revealed all surrounding cells.");

        // Decrement cast count only after successful execution
        base.SpellDecrement();
    }

    private void RevealSurCellsGameFeel(Cell cell)
    {
        // Configure and trigger game feel
        GameFeel revealSurCellsGameFeel = new GameFeel(
            AudioManager.SoundType.SageReveal, // Sound type
            1f,                                   // Sound volume
            0.5f,                                 // Shake duration
            1.5f,                                 // Shake magnitude
            EffectManager.EffectType.RevealCellsSpellEffect,  // Effect type
            (cell.position + new Vector3(.5f, .5f, 0f))                   // Source transform
        );

        revealSurCellsGameFeel.Trigger();
    }
}