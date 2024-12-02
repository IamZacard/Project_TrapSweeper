using UnityEngine;

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

        if (gamePlay.TryGetCellAtMousePosition(out Cell cell))
        {
            if (cell.revealed)
            {
                Debug.Log("Cannot flag a revealed cell.");
                return;
            }

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

            gamePlay.UpdateBoard();
            Debug.Log($"{spellName}: Cell flagged successfully.");
        }
        else
        {
            Debug.Log("No valid cell found under mouse position.");
        }
    }
}
