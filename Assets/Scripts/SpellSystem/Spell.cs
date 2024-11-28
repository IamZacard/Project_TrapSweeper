using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/Spell")]
public class Spell : ScriptableObject
{
    [Header("Spell Details")]
    public string spellName;
    [TextArea] public string description;
    public bool isPassive;
    public Sprite icon;

    public virtual void Activate(CharacterBase character)
    {
        // Define default activation behavior if needed
        Debug.Log($"{spellName} activated for {character.name}");
    }
}
