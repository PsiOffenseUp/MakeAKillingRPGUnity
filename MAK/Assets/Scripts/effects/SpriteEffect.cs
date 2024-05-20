using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteEffect : MonoBehaviour
{
    [SerializeField] AnimationHandler animationHandler;

    protected virtual void Awake()
    {
        animationHandler.Initialize();
    }

    protected virtual void Update()
    {
        animationHandler.OnUpdate();
    }
}
