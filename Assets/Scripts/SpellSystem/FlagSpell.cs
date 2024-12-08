using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "FlagSpell", menuName = "Spells/FlagSpell")]
public class FlagSpell : Spell
{
    public override void Cast(CharacterBase character)
    {
        base.Cast(character);
        GamePlay gamePlay = GameObject.FindObjectOfType<GamePlay>();
        if (gamePlay == null)
        {
            Debug.LogWarning("GamePlay instance not found.");
            return;
        }

        // Spell checks
        if (!gamePlay.IsLevelGenerated || gamePlay.IsGameOver || gamePlay.IsLevelComplete)
        {
            Debug.LogWarning("Cannot cast spell under current game conditions.");
            return;
        }

        if (gamePlay.TryGetCellAtMousePosition(out Cell cell))
        {
            // Block flagging for revealed non-trap cells
            if (cell.revealed && cell.type != Cell.Type.Trap)
            {
                Debug.Log("Cannot flag a revealed non-trap cell.");
                return;
            }

            // Toggle flag on traps (hidden or revealed)
            if (cell.flagged)
            {
                cell.flagged = false; // Unflag the cell
                gamePlay.flagCount += 1;
                FlagGameFeel(cell);
            }
            else if (gamePlay.flagCount > 0)
            {
                cell.flagged = true; // Flag the cell
                gamePlay.flagCount -= 1;
                FlagGameFeel(cell);
            }

            gamePlay.UpdateBoard();
            Debug.Log($"{spellName}: Cell flag toggled successfully.");
        }
        else
        {
            Debug.Log("No valid cell found under mouse position.");
        }

    }

    private void FlagGameFeel(Cell cell)
    {
        // Configure and trigger game feel
        GameFeel flagGameFeel = new GameFeel(
            AudioManager.SoundType.FlagSpell, // Sound type
            1f,                                   // Sound volume
            0.1f,                                 // Shake duration
            0.5f,                                 // Shake magnitude
            EffectManager.EffectType.FlagPlacementEffect,  // Effect type
            (cell.position + new Vector3(.5f, .5f, 0f))                   // Source transform
        );

        flagGameFeel.Trigger();
    }
}