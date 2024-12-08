using System;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : CharacterBase
{
    [Header("Character Spells")]
    public List<Spell> spells = new List<Spell>();

    private GamePlay gameplay;
    protected override void Awake()
    {
        base.Awake();

        gameplay = FindObjectOfType<GamePlay>();
        if (gameplay == null)
            Debug.LogError("GamePlay script not found in the scene!");
    }
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CastSpell(0); // Cast the first spell (flag)
        }
    }

    public void CastSpell(int index)
    {
        if (index >= 0 && index < spells.Count)
        {
            Spell spell = spells[index];
            spell.Cast(this); // Pass this character as the caster
        }
        else
        {
            Debug.LogWarning("Invalid spell index.");
        }
    }

    public override void OnTrapStepped(Cell cell, Action defaultExplosionAction)
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f); // 50% chance
        if (randomValue > 0.5f)
        {
            Debug.Log("Goblin disabled the trap!");
            cell.flagged = true;
            gameplay.flagCount--;

            AudioManager.Instance.PlaySound(AudioManager.SoundType.ShuffProc, 1f);
            ScreenShakeManager.Instance.TriggerShake(.5f, .5f);

            gameplay.UpdateBoard(); // Or any method to reflect this change visually
        }
        else
        {
            defaultExplosionAction.Invoke(); // Perform the default explosion logic
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ShuffExplotion, 1f);
            ScreenShakeManager.Instance.TriggerShake(.5f, 1f);
        }
    }
}
