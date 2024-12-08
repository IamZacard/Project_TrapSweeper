using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "TeleportSpell", menuName = "Spells/TeleportSpell")]
public class TeleportSpell : Spell
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

        // Get the clicked cell
        if (!gamePlay.TryGetCellAtMousePosition(out Cell targetCell))
        {
            Debug.LogWarning("No valid cell found under mouse position.");
            return;
        }

        if (!character.isActive)
        {
            Debug.LogWarning("Cannot teleport due to being inactive.");
            return;
        }

        // Teleport the character
        character.transform.position = targetCell.position + new Vector3(.5f,.5f,0f);
        //Inform game that player teleported (same function as moving)
        gamePlay.PlayerMoved(character.transform.position);
        TeleportGameFeel(targetCell);
        Debug.Log($"{spellName}: Teleported to cell at {targetCell.position}.");
    }

    private void TeleportGameFeel(Cell cell)
    {
        // Configure and trigger game feel
        GameFeel teleportGameFeel = new GameFeel(
            AudioManager.SoundType.TeleportSound, // Sound type
            1f,                                   // Sound volume
            0.1f,                                 // Shake duration
            0.5f,                                 // Shake magnitude
            EffectManager.EffectType.TeleportEffect,  // Effect type
            (cell.position + new Vector3(.5f, .5f, 0f))                   // Source transform
        );

        teleportGameFeel.Trigger();
    }
}
