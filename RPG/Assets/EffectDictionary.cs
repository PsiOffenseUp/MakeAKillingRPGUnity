using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class EffectMetadata
{
    [SerializeField] public string name;
    [SerializeField] public string resourcePath;
}

[System.Serializable]
public class EffectDictionary
{
    Dictionary<string, ParticleSystem> effectDictionary;
    [SerializeField] EffectMetadata[] effects;

    static readonly string effectsRoot = "prefabs/effects/";

    public void Initialize()
    {
        //Add all of the animations to the animation dictionary from their resources
        effectDictionary = new Dictionary<string, ParticleSystem>();
        for (int i = 0; i < effects.Length; i++)
        {
            effectDictionary[effects[i].name] = Resources.Load<ParticleSystem>(effectsRoot + effects[i].resourcePath);
        }
    }

    public ParticleSystem GetEffect(string key) { return effectDictionary[key]; }

    ///Changes the animation to the animation with the given key. If the animation is not found,
    ///does not transition
    public void MakeEffect(string key, Vector3 position, Quaternion rotation)
    {
        try
        {
            GameObject.Instantiate<ParticleSystem>(effectDictionary[key], position, rotation);
        }
        catch { Debug.Log("Could not find effect: " + key); }
    }

    #region Playing/pausing animation
    #endregion

    #region Sprite Manipulation
    #endregion
}
