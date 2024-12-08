using System.Collections.Generic;
using UnityEngine;

public class Mystic : CharacterBase
{
    [Header("Character Spells")]
    public List<Spell> spells = new List<Spell>();

    public bool isInvincible = false; // Track invincibility state
    private int remainingInvincibleSteps = 0;

    private CharacterAnimator characterAnimator;

    protected override void Awake()
    {
        base.Awake();
        characterAnimator = GetComponent<CharacterAnimator>();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CastSpell(0); // Cast the first spell (right click)
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CastSpell(1); // Cast the second spell (left click)
        }

        if(Input.GetKeyDown(KeyCode.R)) { ResetState(); }  
    }

    public void CastSpell(int index)
    {
        if (index >= 0 && index < spells.Count)
        {
            Spell spell = spells[index];
            spell.Cast(this);
        }
        else
        {
            Debug.LogWarning("Invalid spell index.");
        }
    }

    public override void Move(Vector2 direction)
    {
        base.Move(direction); // Call the base class logic first

        if (isInvincible)
        {
            DecrementInvincibilitySteps();
        }
    }

    public override void OnTrapStepped(Cell cell, System.Action defaultExplosionAction)
    {
        if (isInvincible)
        {
            cell.flagged = true;
            DecrementInvincibilitySteps();
        }
        else
        {
            defaultExplosionAction?.Invoke();
        }
    }

    public void ActivateInvincibility(int steps)
    {
        remainingInvincibleSteps = steps;
        SetInvincibilityState(true);
        Debug.Log($"Mystic activated invincibility for {steps} steps.");
    }

    private void SetInvincibilityState(bool state)
    {
        isInvincible = state;
        characterAnimator?.SetInvincibility(state);
    }

    public void ResetState()
    {
        isInvincible = false; // Reset invincibility state
        remainingInvincibleSteps = 0; // Reset invincibility steps

        foreach (Spell spell in spells)
        {
            spell.ResetCastCount(); // Reset cast count for each spell
        }

        Debug.Log("Mystic state has been reset.");
    }

    private void DecrementInvincibilitySteps()
    {
        if (remainingInvincibleSteps > 0)
        {
            remainingInvincibleSteps--;
            Debug.Log($"Invincible steps remaining: {remainingInvincibleSteps}");

            if (remainingInvincibleSteps <= 0)
            {
                SetInvincibilityState(false);
            }
        }
    }
}
