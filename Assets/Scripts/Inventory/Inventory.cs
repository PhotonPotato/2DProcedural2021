using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public InventorySlot[,] inventorySlots;

    public GameObject[,] inventoryGameObjects;
    public int increments = 5;
    public int xOffset = 0;
    public int yOffset = 0;

    int rows;
    int columns;
    public GameObject creationObject;
    GameObject slotObject;
    GameObject dropObject;
    Transform pos;

    Sprite emptySlotImage;

    public bool showingInventory;

    public Inventory(int rows, int columns, GameObject slotObj, GameObject dropObj, GameObject baseObj, Transform basePos, int xOffset, int yOffset, int increments, Sprite emptyImage)
    {
        this.rows = rows;
        this.columns = columns;
        slotObject = slotObj;
        dropObject = dropObj;
        creationObject = baseObj;
        pos = basePos;

        inventorySlots = new InventorySlot[rows, columns];
        inventoryGameObjects = new GameObject[rows, columns];

        //Spawn mods
        this.xOffset = xOffset;
        this.yOffset = yOffset;
        this.increments = increments;

        emptySlotImage = emptyImage;

        initSlots();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                inventorySlots[i, j] = new InventorySlot(i * increments + xOffset, j * increments + yOffset, -1, 0, 0, 0, "", inventoryGameObjects[i, j]);

                //Wipe the image.
                inventorySlots[i, j].itemDiplayImage = emptySlotImage;
            }
        }

        updateSlots();
    }

    public void initSlots()
    {
        int x = 0;
        int y = increments * (columns - 1);

        for (int row = 0; row < rows; row++)
        {
            x = 0;
            for (int column = 0; column < columns; column++)
            {
                inventoryGameObjects[row, column] = Instantiate(slotObject, new Vector3(x * increments + xOffset, y * increments + yOffset, 0), Quaternion.identity);
                inventoryGameObjects[row, column].transform.SetParent(creationObject.transform);

                x += increments;
            }

            y -= increments;
        }
    }

    public void updateSlots()
    {
        for(int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                updateSlot(i, j, inventorySlots[i, j], inventoryGameObjects[i, j]);
            }
        }
    }

    public void updateSlot(int row, int column, InventorySlot slotData, GameObject slotObj)
    {
        inventorySlots[row, column] = slotData;

        if (slotObj.GetComponent<Image>() == null) slotObj.AddComponent<Image>();
        Image slotImage = slotObj.GetComponentsInChildren<Image>()[1];
        slotImage.sprite = slotData.itemDiplayImage;

        Text slotText = slotObj.GetComponentInChildren<Text>();

        //If there is more than one in the stack then put an indicator of ow many are in the stack.
        if (slotData.itemsInStack > 1)
        {
            slotText.text = slotData.itemsInStack.ToString();
            slotText.enabled = true;
        }
        else
        {
            slotText.enabled = false;
        }

        //Have the inventory override the enabled variable.
        if (!showingInventory && row != 0) slotText.enabled = false;
    }

    public void dropItem(int row, int column)
    {
        //Send the end item command to stop any other processes.
        setParams(row, column);
        inventorySlots[row, column].itemComponent.SendMessage("endItem", inventorySlots[row, column].slotParameters);

        GameObject droppedItem = Instantiate(dropObject, pos.position, pos.rotation);

        droppedItem.GetComponent<ItemDropDataHolder>().data = inventorySlots[row, column];
        droppedItem.GetComponent<SpriteRenderer>().sprite = inventorySlots[row, column].itemDiplayImage;
        wipeSlot(row, column);
    }

    public void pickUpItem(GameObject pickUpObj, bool destroyObj)
    {
        Debug.Log("Picking Up");

        //Check for open slots, set 1 slot to certain item's data.
        InventorySlot data = pickUpObj.GetComponent<ItemDropDataHolder>().data;
        Vector2Int slotPos = isInventoyFull(data);

        //If the returned value is in the inventory.
        if(slotPos.x != -1 || slotPos.y == -2)
        {
            //This value will be returned if the item is just being stacked.
            if (slotPos.y != -2)
            {
                updateSlot(slotPos.x, slotPos.y, inventorySlots[slotPos.x, slotPos.y], inventoryGameObjects[slotPos.x, slotPos.y]);

                //Set up the class that it will run off of.
                inventoryGameObjects[slotPos.x, slotPos.y].AddComponent(Type.GetType(data.itemClassName));

                //Set the slot.
                data.initVars(inventoryGameObjects[slotPos.x, slotPos.y]);
                inventorySlots[slotPos.x, slotPos.y] = data;

                updateSlot(slotPos.x, slotPos.y, inventorySlots[slotPos.x, slotPos.y], inventoryGameObjects[slotPos.x, slotPos.y]);

                setParams(slotPos.x, slotPos.y);
                inventorySlots[slotPos.x, slotPos.y].itemComponent.SendMessage("startItem", inventorySlots[slotPos.x, slotPos.y].slotParameters);
            }

            if (destroyObj) Destroy(pickUpObj);
        }
    }

    public void wipeSlot(int row, int column)
    {
        //Destroy the component.
        Destroy(inventorySlots[row, column].itemComponent);

        //reset an inventory slot so that it is recognized as empty.
        inventorySlots[row, column] = new InventorySlot(inventorySlots[row, column].xInInventory, inventorySlots[row, column].yInInventory, -1, 0, 0, 0, "", creationObject);
        inventorySlots[row, column].itemDiplayImage = emptySlotImage;
    }

    public void useSlot(int row, int column)
    {
        //If the slot is empty.
        if (inventorySlots[row, column].itemID == -1) return;

        //Call the special item update function.
        setParams(row, column);
        inventorySlots[row, column].itemComponent.SendMessage("updateItem", inventorySlots[row, column].slotParameters);
    }

    //Conditionary functions.
    public Vector2Int isInventoyFull(InventorySlot addedItem)
    {
        //Look for stacks first.
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                //If the slot is stackable (and the same item)
                if (inventorySlots[row, column].itemID != -1 && inventorySlots[row, column].itemID == addedItem.itemID)
                {
                    //If the inventory slot that we are looking at is full.
                    if (inventorySlots[row, column].itemsInStack >= inventorySlots[row, column].maxItemStack) continue;

                    //Save a temporary int of the total of the items in the stack.
                    int totalInStack = inventorySlots[row, column].itemsInStack + addedItem.itemsInStack;

                    //If the added items are greater than the stack max.
                    if (totalInStack > inventorySlots[row, column].maxItemStack)
                    {
                        //Then make the stack full and move on to the next open slot.
                        //The total is now the left over of copleting the current inventory stack.
                        totalInStack -= addedItem.maxItemStack - inventorySlots[row, column].itemsInStack;
                        inventorySlots[row, column].itemsInStack = inventorySlots[row, column].maxItemStack;
                        addedItem.itemsInStack = totalInStack;
                    }
                    else
                    {
                        //Then adding the items to the stack won't overflow the stack.
                        inventorySlots[row, column].itemsInStack += addedItem.itemsInStack;
                           
                        //Make sure to udate the slot.
                        updateSlot(row, column, inventorySlots[row, column], inventoryGameObjects[row, column]);

                        return new Vector2Int(-1, -2);
                    }
                }
            }
        }

        //Now just look for the first empty spot.
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                //If the slot is empty.
                if (isEmpty(row, column))
                {
                    //Return the first empty slot.
                    return new Vector2Int(row, column);
                }
            }
        }

        //Return an impossible slot so that the caller will know that there are no empty slots.
        return new Vector2Int(-1, -1);
    }

    public bool isEmpty(int row, int column)
    {
        if(inventorySlots[row, column].itemID == -1) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Inventory visuals.
    public void resetInventoryAlphas()
    {
        Color slotColor;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                slotColor = inventoryGameObjects[row, column].GetComponent<Image>().color;
                slotColor.a = 1;
                inventoryGameObjects[row, column].GetComponent<Image>().color = slotColor;
            }
        }
    }

    public void setInventorySelection(Vector2Int closestPos)
    {
        Color slotColor;
        if (!showingInventory) return;

        resetInventoryAlphas();

        slotColor = inventoryGameObjects[closestPos.x, closestPos.y].GetComponent<Image>().color;
        slotColor.a = .5f;
        inventoryGameObjects[closestPos.x, closestPos.y].GetComponent<Image>().color = slotColor;
    }

    void setParams(int row, int column)
    {
        //Make the params the position values.
        inventorySlots[row, column].slotParameters[0] = pos.position.x;
        inventorySlots[row, column].slotParameters[1] = pos.position.y;
        inventorySlots[row, column].slotParameters[2] = pos.position.z;

        //Make the params the rotation values.
        inventorySlots[row, column].slotParameters[3] = pos.rotation.x;
        inventorySlots[row, column].slotParameters[4] = pos.rotation.y;
        inventorySlots[row, column].slotParameters[5] = pos.rotation.z;
    }
}
