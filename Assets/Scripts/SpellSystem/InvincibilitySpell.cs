using UnityEngine;

[CreateAssetMenu(fileName = "New Invincibility Spell", menuName = "Spells/Invincibility")]
public class InvincibilitySpell : Spell
{
    [Header("Invincibility Details")]
    public int invincibilityDuration = 7; // Duration in steps
    private int remainingSteps;

    public override void Cast(CharacterBase character)
    {
        if (remainingCasts <= 0)
        {
            Debug.LogWarning($"{spellName} has no casts remaining.");
            return;
        }

        remainingCasts--;
        remainingSteps = invincibilityDuration; // Reset steps on cast
        Debug.LogWarning($"{spellName} casted. Mystic is invincible for {invincibilityDuration} steps. Remaining casts: {remainingCasts}");

        // Activate invincibility for the character
        Mystic mystic = character as Mystic;
        if (mystic != null)
        {
            mystic.ActivateInvincibility(remainingSteps);
        }
    }

    // Call this method to decrement the remaining invincibility steps
    public void DecrementInvincibility()
    {
        if (remainingSteps > 0)
        {
            remainingSteps--;
        }
    }
}
