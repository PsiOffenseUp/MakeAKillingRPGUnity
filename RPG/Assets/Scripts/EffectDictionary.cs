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

    static readonly string effectsRoot = "effects/";

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

    /// <summary> Makes an effect at the given position with the given rotations </summary>
    public void MakeEffect(string key, Vector3 position, Quaternion rotation)
    {
        try
        {
            GameObject.Instantiate<ParticleSystem>(effectDictionary[key], position, rotation);
        }
        catch { Debug.Log("Could not find effect: " + key); }
    }

    /// <summary> Makes an effect at the given position </summary>
    public void MakeEffect(string key, Vector3 position)
    {
        try
        {
            ParticleSystem ps = effectDictionary[key];
            GameObject.Instantiate<ParticleSystem>(ps, position, ps.transform.rotation);
        }
        catch { Debug.Log("Could not find effect: " + key); }
    }

    /// <summary> Makes an effect at the given position </summary>
    public void MakeEffect(string key, Vector3 offset, Transform parent_trans)
    {
        try
        {
            ParticleSystem ps = effectDictionary[key];
            GameObject.Instantiate<ParticleSystem>(ps, parent_trans.position + offset, ps.transform.rotation, parent_trans);
        }
        catch { Debug.Log("Could not find effect: " + key); }
    }

    #region Playing/pausing animation
    #endregion

    #region Sprite Manipulation
    #endregion
}
