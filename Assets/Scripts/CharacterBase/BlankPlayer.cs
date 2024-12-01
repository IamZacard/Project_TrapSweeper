using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankPlayer : CharacterBase
{
    [Header("Wizard Spells")]
    public Spell activeSpell;

    private GamePlay gameplay;

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
    }

    protected override void Update()
    {
        base.Update();
        if (CanUseSpell())
        {
            UseActiveSpell();
        }
    }

    private bool CanUseSpell()
    {
        return base.isActive && Input.GetMouseButtonDown(1) && gameplay != null &&
               !gameplay.IsGameOver && !gameplay.IsLevelComplete && gameplay.IsLevelGenerated;
    }

    public void UseActiveSpell()
    {
        if (activeSpell != null)
        {
            activeSpell.Activate(this);
        }
        else
        {
            Debug.LogWarning("No active spell assigned.");
        }
    }
}
