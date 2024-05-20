using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;


public class Shop : InteractableObject
{
    //TODO: Add ability to get random deals!

    [SerializeField] int itemCount = 5; //Total number of items to sell in the shop
    int[] itemStock;
    float[] salePercents;
    uint totalWeight = 0;
    int usableWeight = 0;
    int currentItem = 0;
    bool shopOpen = false;
    //Chances of a sale happening, first value is weight and second is discount if chosen
    (int, float)[] saleChances = {(80, 0.0f), (10, 10.0f), (5, 15.0f), (3, 25.0f), (2, 50.0f) };

    enum ACTION { IDLE, LOCKED, } //Actions the shop can take
    StateMachine<ACTION> action;

    #region UI Variables
    [SerializeField] ShopItem[] items;
    [SerializeField] Color[] colors;
    [SerializeField] GameObject shopUI;
    [SerializeField] GameObject itemCameraAnchor;
    [SerializeField] GameObject itemOffsetAnchor;
    [SerializeField] GameObject talkEffect;
    [SerializeField] TMPro.TextMeshProUGUI coinsText, nameText, descriptionText, priceText;
    [SerializeField] Color textBuyColor, textCouldNotBuyColor;
    Color defaultTextColor;
    LookingCamera cameraRef;
    readonly Vector3 itemCameraOffset = new Vector3(4.0f, -0.5f, 0.0f);
    
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        defaultTextColor = coinsText.color;
        Canvas shopCanvas = shopUI.GetComponent<Canvas>();
        shopCanvas.worldCamera = GameplayManager.mainCamera;
        shopCanvas.planeDistance = 2;
        action = new StateMachine<ACTION>(ACTION.IDLE);
    }

    protected override void Update()
    {
        base.Update();

        action.Update();

        //If the shop is open, allow the player to interact with it
        if(shopOpen && action.current != ACTION.LOCKED)
        {
            //If the player presses right, move the UI right
            if (ControlManager.RightDown())
            {
                items[currentItem].SetItemDeselected(); //Mark item as deselected
                currentItem++;
                currentItem %= itemCount;
                GameplayManager.audioPlayer.PlaySFX("test/move_ui");
                UpdateItemUI();
                //Prevent the player with interacting for 4 frames
                action.Transition(ACTION.LOCKED);
                action.Transition(ACTION.IDLE, 9);
            }
            else if (ControlManager.LeftDown()) //Move UI left
            {
                items[currentItem].SetItemDeselected(); //Mark item as deselected
                currentItem--;
                GameplayManager.audioPlayer.PlaySFX("test/move_ui");
                if (currentItem < 0) currentItem = itemCount - 1;
                UpdateItemUI();
                //Prevent the player with interacting for 4 frames
                action.Transition(ACTION.LOCKED);
                action.Transition(ACTION.IDLE, 9);
            }
            else if (ControlManager.AttackPressed()) //Buy an item
            {
                //If the item is sold out
                if (itemStock[currentItem] == -1)
                {
                    //TODO: Display a message about the item being sold out and play SFX
                    coinsText.color = textCouldNotBuyColor;
                    StartCoroutine(PlayTextAnimation());
                }
                else if (GameplayManager.saveData.money >= items[currentItem].item.buyPrice) //Check if the player has enough money
                    OnBuy();
                else
                {
                    //TODO: Display some UI telling the player they don't have enough money
                }
            }
            else if (ControlManager.BackPressed()) //If back is pressed, exit the menu
                OnPlayerFinishInteract();
        }
    }

    #region Methods for Buying and Selling

    void OnBuy()
    {
        GameplayManager.saveData.money -= items[currentItem].item.buyPrice; //Take the players money

        //Play an effect
        coinsText.text = FormatMoneyString(GameplayManager.saveData.money); //Update the money displayed
        coinsText.color = textBuyColor;
        StartCoroutine(PlayTextAnimation());

        //If the item is usable or collectible, add it directly to player inventory
        //Otherwise, spawn an item near the shop

        //Mark the item as purchased
        itemStock[currentItem] = -1;
        items[currentItem].ChangeToBought();
        items[currentItem].SetSelectColor(Color.gray);
        UpdateItemUI();

        //Prevent the player with interacting for 16 frames
        action.Transition(ACTION.LOCKED);
        action.Transition(ACTION.IDLE, 16);
    }

    void OnSell()
    {

    }

    #endregion

    #region Methods For Refreshing Stock
    void FindWeights()
    {
        //Initialize normal item weights
        totalWeight = 0;
        foreach(KeyValuePair<int, ItemData> id in ItemTable.normalDict)
            totalWeight += id.Value.weight;
    }

    void FindUsableWeights()
    {
        //Initialize usable item weights for buyable items
        usableWeight = 0;
        foreach (KeyValuePair<int, UsableItemData> id in ItemTable.usableDict)
        {

        }
    }

    //Refreshes all of the stock in the shop to new items
    void RefreshStock()
    {
        int[] oldStock = itemStock; //Keep track of old stock to make new stock better
        FindWeights();
        FindUsableWeights();
        itemStock = new int[itemCount]; //Clear out the previous stock
        int newItem;
        int usableItemCount = usableWeight > 0 ? 1 : 0;
        bool isNew;

        //Pick random values and add them to the new item stock
        int i, j;
        for(i = 0; i < itemCount - usableItemCount;)
        {
            newItem = PickRandomNormalItem();
            isNew = true;

            //Check if the rolled item was picked already
            for(j = 0; j < i; j++)
            {
                //If the item has already been rolled before, exit the loop
                //Also prevent some oldStock from spawning just to keep things fresh
                if (newItem == itemStock[j] || (oldStock != null && newItem == oldStock[j]))
                {
                    isNew = false;
                    break;
                }
            }

            //If the rolled item is, in fact, new, use it
            if(isNew)
            {
                itemStock[i] = newItem; //Keep track of the item
                items[i].SetItemData(ItemTable.GetItemDataFromId(newItem)); //Set overworld item
                items[i].SetSelectColor(colors[i]); //Reset select color
                i++;
            }
        }

        //If a usable item should be added to the stock
        if(usableItemCount == 1)
        {

        }

        //TODO: Add small chance for a sale
    }

    //Picks a random value out of the normal item dictionary
    int PickRandomNormalItem()
    {
        //Get a random value
        ulong randVal = GameplayManager.GetRandomInt(totalWeight);
        uint tempWeight = 0;

        //Go through all of the items and their weights
        foreach (KeyValuePair<int, ItemData> id in ItemTable.normalDict)
        {
            tempWeight += id.Value.weight;
            if (randVal < tempWeight) //If the random value is within the range for the current item, return it
                return id.Key;
        }

        return 0; //Just to be safe, shouldn't ever trigger
    }

    //Picks a random buyable usable item
    int PickRandomUsableItem()
    {
        //Get a random value
        ulong randVal = GameplayManager.GetRandomInt(totalWeight);
        uint tempWeight = 0;

        //Go through all of the items and their weights
        foreach (KeyValuePair<int, ItemData> id in ItemTable.normalDict)
        {
            tempWeight += id.Value.weight;
            if (randVal < tempWeight) //If the random value is within the range for the current item, return it
                return id.Key;
        }

        return 0; //Just to be safe, shouldn't ever trigger
    }

    #endregion

    #region Methods For Displaying Stock and UI

    void OpenShopUI()
    {
        //Set player to talking state so they can't move
        GameplayManager.player.OnTalk();

        //Hide the UI so we can display shop UI
        GameplayManager.uiManager.HideUI();

        //Prevent the player with interacting for 3 frames
        action.Transition(ACTION.LOCKED);
        action.Transition(ACTION.IDLE, 3);

        //Set where the camera will look
        cameraRef = GameplayManager.mainCamera.GetComponent<LookingCamera>();
        cameraRef.SetMode(LookingCamera.MODE.STATIC);
        cameraRef.SetPosition(itemCameraAnchor.transform.position);
        UpdateItemUI();
        cameraRef.SetFocus(itemOffsetAnchor);
        cameraRef.LookAtFocus(); //Instantly pan to the item looked at

        //Set the text for the money
        coinsText.text = FormatMoneyString(GameplayManager.saveData.money);

        //Display the shop ui
        shopUI.SetActive(true);

        //Set the shop to be open
        shopOpen = true;
    }

    void CloseShopUI()
    {
        //Deselect selected item
        items[currentItem].SetItemDeselected(); //Mark item as deselected

        //Hide the shop UI
        shopUI.SetActive(false);

        //Reset the camera to look at the player
        cameraRef.SetFocus(GameplayManager.player.gameObject);
        cameraRef.SetMode(LookingCamera.MODE.FOLLOW);
        cameraRef.ImmediatelyGoToOffet();
        cameraRef.LookAtFocus(); //Instantly pan back to player

        //Show the main UI again
        GameplayManager.uiManager.ShowUI();

        //Tell the player they are out of the dialogue
        GameplayManager.player.EndTalk();

        //Set the shop to no longer be open
        shopOpen = false;
    }

    //Updates the item UI upon a new item being selected and moves the camera
    void UpdateItemUI()
    {
        itemOffsetAnchor.transform.position = items[currentItem].transform.position + itemCameraOffset;
        items[currentItem].SetItemSelected(); //Mark item as selected for effects

        //Update the text
        if(itemStock[currentItem] == -1) //If the item has been purchased, display as such
        {
            descriptionText.text = "";
            nameText.text = "Sold Out";
            priceText.text = "Bought!";
        }
        else
        {
            descriptionText.text = items[currentItem].item.description;
            nameText.text = items[currentItem].item.name;
            priceText.text = FormatMoneyString(items[currentItem].item.buyPrice);
        }
    }

    string FormatMoneyString(int money)
    {
        if (money == 0)
            return "0000";

        string moneyString = "";
        int tempInterp = money;
        //Find how many zeroes should be in the beginning of the string
        while (tempInterp < 1000)
        {
            moneyString += '0';
            tempInterp *= 10;
        }
        moneyString += money;

        return moneyString;
    }

    #endregion

    #region Effect Methods

    float animationTime = 0.6f;
    bool locked = false;
    IEnumerator PlayTextAnimation()
    {
        locked = false;
        yield return null;
        locked = true;
        while (coinsText.color != defaultTextColor && locked)
        {
            coinsText.color = Color.Lerp(coinsText.color, defaultTextColor, Time.deltaTime / animationTime);
            yield return null;
        }
        locked = false;
    }

    #endregion

    #region Time related overrides
    protected override void OnNewDay()
    {
        base.OnNewDay();
        RefreshStock();
    }

    #endregion

    #region Save-Related Methods

    protected override void OnFirstLoad()
    {
        base.OnFirstLoad();
        RefreshStock();
        CopyToSave(GameplayManager.saveData);
    }

    public override void CopyToSave(Save save)
    {
        base.CopyToSave(save);
        save.shopStock = itemStock;
    }

    public override void LoadFromSave(Save save)
    {
        base.LoadFromSave(save);
        itemStock = save.shopStock;

        //Load all of the items into the game from the save
        for (int i = 0; i < itemCount; i++)
        {
            if (itemStock[i] == -1) //If item was sold already, set the sprite
            {
                items[i].SetSpriteSoldOut();
                items[i].SetSelectColor(Color.gray);
            }
            else
            {
                items[i].SetItemData(ItemTable.GetItemDataFromId(itemStock[i])); //Set overworld item
                items[i].SetSelectColor(colors[i]);
            }
        }
    }

    #endregion

    #region Trigger related methods

    protected override void OnPlayerInteract()
    {
        base.OnPlayerInteract();
        OpenShopUI();
    }

    protected override void OnPlayerFinishInteract()
    {
        base.OnPlayerFinishInteract();
        CloseShopUI();
    }

    #endregion

}
