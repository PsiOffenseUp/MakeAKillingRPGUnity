using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;

//Items that will be displayed in the shop
[System.Serializable]
public class ShopItem : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] EffectDictionary effects;
    [SerializeField] GameObject selectEffectObject;
    SpriteRenderer selectEffectRenderer;
    static Sprite soldOutSprite = null;
    static readonly Vector3 buyEffectOffset = new Vector3(0.2f, 0.0f, -1.0f);
    static readonly Vector3 selectEffectOffset = new Vector3(-0.2f, 0.1f, -0.4f);
    Vector3 initialSelectPosition;
    Quaternion initialSelectRotation;
    Vector3 initialSelectOffset;
    Vector3 finalSelectOffset;
    float distanceSelectEffect = 1.4f;

    public ItemData item { get; private set; }
    const string itemImagePath = "Sprites/shop/";
    bool selected = false;
    Color selectColor;

    // Start is called before the first frame update
    void Awake()
    {
        //Load the sprite for all items once
        if (soldOutSprite == null)
            soldOutSprite = Resources.Load<Sprite>(itemImagePath + "sold_out");

        effects.Initialize();

        initialSelectRotation = selectEffectObject.transform.rotation;

        selectEffectRenderer = selectEffectObject.GetComponent<SpriteRenderer>();
        selectEffectObject.SetActive(false);
    }

    #region Methods for UI and Effects
    public void SetItemData(ItemData item_data)
    {
        item = item_data;
        //Load the image for the sprite
        spriteRenderer.sprite = Resources.Load<Sprite>(itemImagePath + item_data.path);
        selectEffectRenderer.sprite = spriteRenderer.sprite;
    }

    public void SetSelectColor(Color color)
    {
        selectColor = color;
    }

    //Changes the item to bought and plays effects
    public void ChangeToBought()
    {
        //Set sprite to some purchased sprite and play a SFX
        effects.MakeEffect("buy", transform.position + buyEffectOffset);
        SetSpriteSoldOut();
        GameplayManager.audioPlayer.PlaySFX("test/select_ui");
    }

    //Changes the item to the bought sprite, but plays no effects
    public void SetSpriteSoldOut()
    {
        spriteRenderer.sprite = soldOutSprite;
        selectEffectRenderer.sprite = soldOutSprite;
    }

    public void SetItemSelected()
    {
        selected = true;
        selectEffectObject.SetActive(true);

        //Set up variables for the animations\
        initialSelectPosition = selectEffectObject.transform.position - GameplayManager.mainCamera.transform.position;
        initialSelectPosition.Normalize();
        initialSelectOffset = 0.05f * initialSelectPosition;
        finalSelectOffset = distanceSelectEffect * initialSelectPosition + selectEffectOffset;
        initialSelectPosition = transform.position + initialSelectOffset;
        selectEffectObject.transform.position = initialSelectPosition;
        StartCoroutine(PlaySelectionAnimation());
    }

    public void SetItemDeselected()
    {
        selected = false;
        StartCoroutine(PlayDeselectionAnimation());
    }

    float fraction;
    const float animationDuration = 0.36f;
    static readonly Vector3 rotateAxis = new Vector3(1.0f, 0.8f, 0.1f);
    static readonly float selectScaleGrowth = 0.6f;
    const float ROTATE_SPEED = 2.5f;
    const float ROTATE_AMP = 20.12f;
    IEnumerator PlaySelectionAnimation()
    {
        //Initialize variables and reset placement
        float timeElapsed = 0.0f;
        selectEffectObject.transform.position = initialSelectPosition;
        selectEffectObject.transform.rotation = initialSelectRotation;
        selectEffectObject.transform.localScale = Vector3.one;
        selectEffectRenderer.material.SetColor("_BGColor", selectColor);

        //Movement to final position
        while (selected && timeElapsed < animationDuration)
        {
            timeElapsed += Time.deltaTime;
            fraction = Mathf.Exp(timeElapsed / animationDuration - 1.0f) + 0.3679f;
            selectEffectObject.transform.position = initialSelectPosition + 
                Vector3.Lerp(initialSelectOffset, finalSelectOffset, fraction);
            selectEffectObject.transform.localScale = (1.0f + fraction * selectScaleGrowth) * Vector3.one;
            yield return null;
        }

        //Rotatey effect
        rotateAxis.Normalize();
        while(selected)
        {
            selectEffectObject.transform.Rotate(rotateAxis, ROTATE_AMP * Time.deltaTime * Mathf.Sin(ROTATE_SPEED * Time.time));
            yield return null;
        }

        yield return null;
    }

    IEnumerator PlayDeselectionAnimation()
    {
        float timeElapsed = 0.0f;
        Vector3 startPosition = selectEffectObject.transform.position;

        //Movement to final position
        while (!selected && selectEffectObject.transform.position != initialSelectPosition)
        {
            timeElapsed += Time.deltaTime;
            fraction = Mathf.Exp(timeElapsed / animationDuration - 1.0f) + 0.3679f;
            selectEffectObject.transform.position = Vector3.Lerp(startPosition, initialSelectPosition, fraction);
            selectEffectObject.transform.localScale = (1.0f + (1.0f - fraction) * selectScaleGrowth) * Vector3.one;
            yield return null;
        }

        selectEffectObject.SetActive(false); //Disable the effect
        yield return null;
    }

    #endregion
}
