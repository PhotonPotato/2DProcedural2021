using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventroyManager : MonoBehaviour
{
    public WorldGenerator worldGen;

    //Inventory display
    public int currentSlot;
    public bool showingInventory = true;
    
    //Mouse vars
    public RectTransform mouseImage;
    public RectTransform canvas;
    public Camera mainCam;
    public Vector2 mousePos;

    //Player vars.
    public Transform playerPos;

    //Inventory vars
    public int inventoryRows = 7;
    public int inventoryColumns = 10;
    [System.NonSerialized] public Inventory playerInventory;
    public GameObject slotObject;
    public GameObject dropObject;
    public Sprite emptyImage;

    //Position vars.
    public GameObject inventoryParent;
    public int xOffset = 0;
    public int yOffset = 0;
    public int spawnIncrements = 5;

    //Pickup vars
    public float timeBetweenPickups = .3f;
    float pickUpTimer = 0f;
    public Transform pickupFromPoint;
    public float pickupRange = 0f;
    public LayerMask pickupMask;
    public Collider2D[] pickUpsInRange;

    //Inventory movement vars.
    public bool holdingItem;
    MouseDataHolder mouseData;

    public float timeBetweenDrops = .3f;
    float dropTimer = 0f;

    void Start()
    {
        playerInventory = new Inventory(inventoryRows, inventoryColumns, slotObject, dropObject, inventoryParent, playerPos, xOffset, yOffset, spawnIncrements, emptyImage);

        mouseData = mouseImage.gameObject.GetComponent<MouseDataHolder>();

        //Refresh the inventory display.
        displaySlots(false, 1);
        displaySlots(true, 1);
    }

    private void Update()
    {
        playerInventory.useSlot(0, 0);
        checkForPickups();
        checkForDrop();
        checkForInventoryOpen();
        calcPosOnCanvas();
        playerInventory.setInventorySelection(findClosestSlot());
        checkForItemMovement();
    }

    public void checkForPickups()
    {
        if (pickUpTimer > 0)
        {
            pickUpTimer -= Time.deltaTime;
            if(!Input.GetButtonDown("Pick Up")) return;
        }

        pickUpsInRange = Physics2D.OverlapCircleAll(new Vector2(pickupFromPoint.position.x, pickupFromPoint.position.y), pickupRange, pickupMask);
        //If there is nothing around to pick up.
        if (pickUpsInRange.Length == 0) return;

        if (Input.GetButton("Pick Up"))
        {
            GameObject currentPickUp = pickUpsInRange[0].gameObject;
            playerInventory.pickUpItem(currentPickUp, true);
            pickUpTimer = timeBetweenPickups;
        }
    }

    public void checkForDrop()
    {
        if (dropTimer > 0)
        {
            dropTimer -= Time.deltaTime;
            if (!Input.GetButtonDown("Drop")) return;
        }

        if (Input.GetButton("Drop"))
        {
            if (!playerInventory.isEmpty(0, 0))
            {
                playerInventory.dropItem(0, 0);
                playerInventory.updateSlot(0, 0, playerInventory.inventorySlots[0, 0], playerInventory.inventoryGameObjects[0, 0]);
            }
        }
    }

    public void checkForInventoryOpen()
    {
        mouseImage.GetComponent<Image>().enabled = showingInventory && holdingItem;

        //Check for keyboard input not manual override
        if (showingInventory && (Input.GetButtonDown("Inventory") || Input.GetButtonDown("Cancel")))
        {
            displaySlots(false, 1);
            playerInventory.resetInventoryAlphas();
            worldGen.canInteractWithWorld = true;
            return;
        }

        if (Input.GetButtonDown("Inventory") && !showingInventory)
        {
            displaySlots(true, 1);
            playerInventory.resetInventoryAlphas();
            worldGen.canInteractWithWorld = false;
            return;
        }
    }

    public void displaySlots(bool show, int start)
    {
        showingInventory = show;
        playerInventory.showingInventory = showingInventory;

        for (int row = start; row < inventoryRows; row++)
        {
            for (int column = 0; column < inventoryColumns; column++)
            {
                displaySlot(row, column, show);
            }
        }
    }

    public void displaySlot(int row, int column, bool show)
    {
        Image[] images = playerInventory.inventoryGameObjects[row, column].GetComponentsInChildren<Image>();

        for (int i = 0; i < images.Length; i++)
        {
            images[i].enabled = show;
        }

        Text slotText = playerInventory.inventoryGameObjects[row, column].GetComponentInChildren<Text>();

        if (playerInventory.inventorySlots[row, column].itemsInStack > 1) slotText.enabled = show;
    }

    public void calcPosOnCanvas()
    {
        mouseImage.transform.position = Input.mousePosition;
        mousePos = mouseImage.transform.localPosition;
    }

    public Vector2Int findClosestSlot()
    {
        
        Vector3 closest = new Vector3(0, 0, 10000);

        for (int row = 0; row  < inventoryRows; row++)
        {
            for (int column = 0; column < inventoryColumns; column++)
            {
                float dist = Mathf.Sqrt(Mathf.Pow(playerInventory.inventoryGameObjects[row, column].transform.localPosition.x - mousePos.x, 2) + ((Mathf.Pow(playerInventory.inventoryGameObjects[row, column].transform.localPosition.y - mousePos.y, 2))));
                
                if (dist < closest.z)
                {
                    closest.x = row;
                    closest.y = column;
                    closest.z = dist;
                }
            }
        }
        return new Vector2Int(Mathf.RoundToInt(closest.x), Mathf.RoundToInt(closest.y));
    }

    public void checkForItemMovement()
    {
        if (!showingInventory) return;

        //Keep in mind throughout this function that closestSlot.x is the row, and closestSlot.y is the column.
        Vector2Int closestSlot = findClosestSlot();

        //Left click
        if (Input.GetMouseButtonDown(0))
        {
            if (holdingItem)
            {
                //Check if the slot is empty.
                if (playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemID == -1)
                {
                    Debug.Log("Placing into empty inventory slot");

                    playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y].AddComponent(Type.GetType(mouseData.mouseDataHolder.itemClassName));
                    playerInventory.updateSlot(closestSlot.x, closestSlot.y, mouseData.mouseDataHolder, playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);
                    playerInventory.inventorySlots[closestSlot.x, closestSlot.y].initVars(playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);
                    holdingItem = false;
                    //mouseData.data.itemID = -1;
                }
                else
                {
                    //Flip around the items.
                    
                    //Check if they are the same item (and are not maxed out stacks) to just add stacks.
                    if (mouseData.mouseDataHolder.itemID == playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemID && playerInventory.inventorySlots[closestSlot.x, closestSlot.y].maxItemStack != playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack)
                    {
                        //If adding will overflow the stack.
                        if (mouseData.mouseDataHolder.itemsInStack + playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack > playerInventory.inventorySlots[closestSlot.x, closestSlot.y].maxItemStack)
                        {
                            //Now just move the amount to get the max stack to the inventory slot.
                            int diffToMax = playerInventory.inventorySlots[closestSlot.x, closestSlot.y].maxItemStack - playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack;

                            playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack = playerInventory.inventorySlots[closestSlot.x, closestSlot.y].maxItemStack;
                            playerInventory.updateSlot(closestSlot.x, closestSlot.y, playerInventory.inventorySlots[closestSlot.x, closestSlot.y], playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);

                            mouseData.mouseDataHolder.itemsInStack -= diffToMax;
                        }
                        else
                        {
                            //Esle then there won't be any overflow.
                            playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack += mouseData.mouseDataHolder.itemsInStack;
                            playerInventory.updateSlot(closestSlot.x, closestSlot.y, playerInventory.inventorySlots[closestSlot.x, closestSlot.y], playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);
                            holdingItem = false;
                            //mouseData.data.itemID = -1;
                        }
                    }
                    else
                    {
                        //First save the slots initial data
                        InventorySlot tempData;
                        tempData = playerInventory.inventorySlots[closestSlot.x, closestSlot.y];

                        //Then reset the slot to nothing (and destroy the component)
                        playerInventory.wipeSlot(closestSlot.x, closestSlot.y);

                        //Now set the slot using the update slot function to the mouses slot data.
                        playerInventory.updateSlot(closestSlot.x, closestSlot.y, mouseData.mouseDataHolder, playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);

                        //Then set up the component for the new slot.
                        playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y].AddComponent(Type.GetType(playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemClassName));

                        playerInventory.inventorySlots[closestSlot.x, closestSlot.y].initVars(playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);

                        //Finally set the mouses data to what the slot originally was.
                        mouseData.mouseDataHolder = tempData;
                    }
                }
            }
            else
            {
                //If there is something in that slot.
                if (playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemID != -1)
                {
                    holdingItem = true;
                    mouseData.mouseDataHolder = playerInventory.inventorySlots[closestSlot.x, closestSlot.y];
                    playerInventory.wipeSlot(closestSlot.x, closestSlot.y);
                    playerInventory.updateSlot(closestSlot.x, closestSlot.y, playerInventory.inventorySlots[closestSlot.x, closestSlot.y], playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);
                }
            }

            displaySlot(closestSlot.x, closestSlot.y, showingInventory);
            mouseImage.GetComponent<Image>().sprite = mouseData.mouseDataHolder.itemDiplayImage;

            mouseData.mouseItemsInStack = mouseData.mouseDataHolder.itemsInStack;
        }

        //Now check for right clicks.
        if (Input.GetMouseButtonDown(1))
        {
            if (playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemID == -1) return;
            Debug.Log(playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack);

            InventorySlot tempSlot = playerInventory.inventorySlots[closestSlot.x, closestSlot.y];

            int mouseItemsInStack = mouseData.mouseDataHolder.itemsInStack;
            int closestSlotItemsInStack = playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack;

            if (holdingItem && mouseData.mouseDataHolder.itemID == playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemID)
            {
                mouseData.mouseDataHolder = tempSlot;
                mouseItemsInStack++;
                closestSlotItemsInStack--;

                holdingItem = true;
            } 
            else if (!holdingItem && tempSlot.itemsInStack != -1)
            {
                mouseItemsInStack = 1;

                mouseData.mouseDataHolder = tempSlot;
                closestSlotItemsInStack--;

                holdingItem = true;
            }

            tempSlot.itemsInStack = closestSlotItemsInStack;
            playerInventory.updateSlot(closestSlot.x, closestSlot.y, tempSlot, playerInventory.inventoryGameObjects[closestSlot.x, closestSlot.y]);
            displaySlot(closestSlot.x, closestSlot.y, showingInventory);

            mouseData.mouseItemsInStack = mouseItemsInStack;

            Debug.Log(playerInventory.inventorySlots[closestSlot.x, closestSlot.y].itemsInStack);
            Debug.Log(mouseData.mouseDataHolder.itemsInStack);

            mouseImage.GetComponent<Image>().sprite = mouseData.mouseDataHolder.itemDiplayImage;
        }
    }
}
