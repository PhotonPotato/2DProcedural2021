using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDataHolder : MonoBehaviour
{
    public InventorySlot mouseDataHolder;

    public int mouseItemsInStack;

    void Start()
    {
        
    }

    void Update()
    {
        mouseDataHolder.itemsInStack = mouseItemsInStack;
    }
}
