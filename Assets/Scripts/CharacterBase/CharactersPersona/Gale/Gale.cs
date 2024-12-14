using System.Collections.Generic;
using UnityEngine;

public class Gale : CharacterBase
{
    [Header("Character Spells")]
    public List<Spell> spells = new List<Spell>();

    [Header("Magic Shards")]
    public int shardCount = 0; // Tracks collected shards
    public int requiredShardsForTrapReveal = 3; // Shards required for second spell

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            shardCount = 0;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other); // Ensure other interactions still work

        // Collect magic shards when colliding with them
        if (other.CompareTag("Shard"))
        {
            shardCount++;
            AudioManager.Instance.PlaySound(AudioManager.SoundType.GalePickUp, 1f);
            Destroy(other.gameObject); // Destroy the shard
            Debug.Log($"Magic Shard collected! Total: {shardCount}");
        }
    }

    public void CastSpell(int index)
    {
        if (!base.isActive)
        {
            // Play an error sound
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ErrorSound, 1f);
            return; // Prevent movement if the player is not active
        }

        if (index >= 0 && index < spells.Count)
        {
            Spell spell = spells[index];

            // Ensure second spell checks shard requirements
            if (index == 1 && shardCount < requiredShardsForTrapReveal)
            {
                AudioManager.Instance.PlaySound(AudioManager.SoundType.ErrorSound, 1f);
                Debug.LogWarning("Not enough shards to cast the spell!");
                return;
            }

            spell.Cast(this); // Cast the spell first

            if (index == 1)
            {
                shardCount -= requiredShardsForTrapReveal; // Deduct shards after successful casting
                Debug.Log($"Second spell cast! Remaining shards: {shardCount}");
            }
        }
        else
        {
            Debug.LogWarning("Invalid spell index.");
        }
    }
}