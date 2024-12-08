using System.Collections.Generic;
using UnityEngine;

public class Violet : CharacterBase
{
    [Header("Character Spells")]
    public List<Spell> spells = new List<Spell>();
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // Assign spells dynamically
        Spell flagSpell = Resources.Load<Spell>("FlagSpell");

        if (flagSpell != null) spells.Add(flagSpell);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CastSpell(0); // Cast the first spell (flag)
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CastSpell(1); // Cast the second spell
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
}
