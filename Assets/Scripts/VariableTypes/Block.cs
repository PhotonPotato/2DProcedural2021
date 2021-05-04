using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    //Block Coords
    public int x;
    public int y;

    //Every block type will ahev a unique ID.
    public int blockID;
    public int blockType;

    public int blockStrength;
    public float blockBreakTime;

    public bool dropsItem = true;
    public int maxItemsInStack = 20;

    public Block(int newX, int newY, int newBlockID, int newBlockType, int newBlockStrength)
    {
        x = newX;
        y = newY;
        blockID = newBlockID;
        blockType = newBlockType;
    }
}
