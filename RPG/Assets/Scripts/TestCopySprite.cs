using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCopySprite : MonoBehaviour
{
    [SerializeField] SpriteRenderer sourceRenderer, targetRenderer;

    // Update is called once per frame
    void Update()
    {
        targetRenderer.sprite = sourceRenderer.sprite;
        //targetRenderer.flipX = !sourceRenderer.flipX;
    }
}
