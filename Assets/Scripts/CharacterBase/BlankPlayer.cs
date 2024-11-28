using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankPlayer : CharacterBase
{
    [Header("Wizard Spells")]
    public Spell activeSpell;

    private GamePlay gameplay;


    private void Awake()
    {
        controls = new PlayerMovement();
    }

    private void Start()
    {
        // Get the GamePlay instance in the scene
        gameplay = FindObjectOfType<GamePlay>();

        if (gameplay == null)
        {
            Debug.LogError("GamePlay instance not found in the scene.");
        }

        controls.Main.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && gameplay != null && !gameplay.IsGameOver && !gameplay.IsLevelComplete)
        {
            UseActiveSpell();
        }
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
