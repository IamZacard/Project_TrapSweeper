using UnityEngine;

public abstract class Spell : ScriptableObject, ISpell
{
    [Header("Spell Details")]
    public string spellName;
    public string description;
    public Sprite icon;

    [Header("Spell Mechanics")]
    public int maxCasts;
    public int remainingCasts;

    private void OnEnable()
    {
        remainingCasts = maxCasts;
    }

    public virtual void Cast(CharacterBase character)
    {
        if (remainingCasts <= 0)
        {
            Debug.LogWarning($"{spellName} has no casts remaining.");
            return;
        }

        Debug.Log($"{spellName} casted.");
        // The decrement now happens after derived logic handles the spell effects.
    }

    public void SpellDecrement()
    {
        remainingCasts--;
        Debug.Log($"{spellName} casted. Remaining casts: {remainingCasts}");
    }

    public void ResetCastCount()
    {
        remainingCasts = maxCasts;
        Debug.Log($"Spell '{name}' cast count reset to {maxCasts}");
    }
}

