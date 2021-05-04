using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlot
{
    public int itemID;
    public int itemType;
    public int maxItemStack;
    public int itemsInStack;
    public int xInInventory;
    public int yInInventory;
    public Sprite itemDiplayImage;
    public string itemClassName;
    public Type itemClass;
    public Component itemComponent;
    public GameObject creationObj;
    public float[] slotParameters;

    public int extraParams = 2;

    public InventorySlot(int x, int y, int itemID, int itemType, int itemsInStack, int maxItemsInStack, string newType, GameObject creationObj)
    {
        this.creationObj = creationObj;

        xInInventory = x;
        yInInventory = y;

        this.itemID = itemID;
        this.itemType = itemType;
        this.itemsInStack = itemsInStack;
        maxItemStack = maxItemsInStack;

        itemClassName = newType;
        itemClass = Type.GetType(itemClassName);
    }

    public void initVars(GameObject creationObj)
    {
        this.creationObj = creationObj;
        itemComponent = creationObj.GetComponent(Type.GetType(itemClassName));
    }
}
