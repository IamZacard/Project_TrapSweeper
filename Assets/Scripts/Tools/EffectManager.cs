using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public enum EffectType
    {
        StepEffect,
        CellRevealEffect,
        TrapExplosionEffect
    }

    [System.Serializable]
    public class EffectPrefab
    {
        public EffectType type;
        public GameObject prefab;
    }

    public static EffectManager Instance { get; private set; }

    [Header("Effect Prefabs")]
    public List<EffectPrefab> effectPrefabs;

    private Dictionary<EffectType, GameObject> effectDictionary;

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persistent between scenes
            InitializeEffectDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEffectDictionary()
    {
        effectDictionary = new Dictionary<EffectType, GameObject>();
        foreach (var effectPrefab in effectPrefabs)
        {
            if (!effectDictionary.ContainsKey(effectPrefab.type))
            {
                effectDictionary.Add(effectPrefab.type, effectPrefab.prefab);
            }
        }
    }

    public void TriggerEffect(EffectType effectType, Vector3 position, Quaternion rotation)
    {
        if (effectDictionary.TryGetValue(effectType, out GameObject prefab))
        {
            Instantiate(prefab, position, rotation);
        }
        else
        {
            Debug.LogWarning($"Effect of type {effectType} is not registered in EffectManager.");
        }
    }
}
