using System.Collections.Generic;
using UnityEngine;

// Define the condition interface
public interface IGameStateCondition
{
    bool CanUseSpell(GamePlay gameplay, Spell spell);
}

// Implement the game state conditions
public class LevelGeneratedCondition : IGameStateCondition
{
    public bool CanUseSpell(GamePlay gameplay, Spell spell)
        => gameplay.IsLevelGenerated;
}

public class GameOverCondition : IGameStateCondition
{
    public bool CanUseSpell(GamePlay gameplay, Spell spell)
        => !gameplay.IsGameOver;
}

public class LevelCompleteCondition : IGameStateCondition
{
    public bool CanUseSpell(GamePlay gameplay, Spell spell)
        => !gameplay.IsLevelComplete;
}

// Refactored Player class
public class BlankPlayer : CharacterBase
{
    [Header("Wizard Spells")]
    public List<SpellKeyBinding> spellBindings; // List of spells with assigned keys
    private GamePlay gameplay;

    private List<IGameStateCondition> spellConditions;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        gameplay = FindObjectOfType<GamePlay>();

        if (gameplay == null)
        {
            Debug.LogError("GamePlay instance not found in the scene.");
        }

        // Initialize the spell conditions list with default conditions
        spellConditions = new List<IGameStateCondition>
        {
            new LevelGeneratedCondition(),
            new GameOverCondition(),
            new LevelCompleteCondition()
        };
    }

    protected override void Update()
    {
        base.Update();

        foreach (var binding in spellBindings)
        {
            if (Input.GetKeyDown(binding.activationKey))
            {
                UseSpell(binding.spell);
            }
        }
    }

    private void UseSpell(Spell spell)
    {
        if (isActive && CanUseSpell(spell))
        {
            ActivateSpell(spell);
        }
        else
        {
            Debug.LogWarning("Cannot use the spell under current conditions.");
        }
    }

    private bool CanUseSpell(Spell spell)
    {
        if (spell == null || gameplay == null) return false;

        if (spell is TeleportSpell) return true; // Allow teleport without constraints

        foreach (var condition in spellConditions)
        {
            if (!condition.CanUseSpell(gameplay, spell))
            {
                return false;
            }
        }

        return true;
    }

    private void ActivateSpell(Spell spell)
    {
        spell.Activate(this);
        Debug.Log($"Used spell: {spell.spellName}");
    }
}

// Spell key binding remains unchanged
[System.Serializable]
public class SpellKeyBinding
{
    public Spell spell;         // The spell to be used
    public KeyCode activationKey; // The key to activate the spell
}
