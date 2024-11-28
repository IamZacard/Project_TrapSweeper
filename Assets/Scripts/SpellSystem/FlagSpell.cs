using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlagSpell", menuName = "Spells/FlagSpell")]
public class FlagSpell : Spell
{
    public override void Activate(CharacterBase character)
    {
        base.Activate(character);

        // Reference to GamePlay for accessing the flagging logic
        GamePlay gamePlay = GameObject.FindObjectOfType<GamePlay>();
        if (gamePlay == null)
        {
            Debug.LogWarning("GamePlay instance not found.");
            return;
        }

        // Attempt to flag a cell
        if (gamePlay.TryGetCellAtMousePosition(out Cell cell))
        {
            if (cell.revealed) return;

            if (cell.flagged)
            {
                cell.flagged = false;
                gamePlay.flagCount += 1;
            }
            else if (gamePlay.flagCount > 0)
            {
                cell.flagged = true;
                gamePlay.flagCount -= 1;
            }

            // Update board visuals
            gamePlay.UpdateBoard();
            Debug.Log($"{spellName}: Flagging action performed.");
        }
        else
        {
            Debug.Log("No valid cell found under mouse position.");
        }
    }
}

