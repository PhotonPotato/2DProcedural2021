using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlockUsageScript : MonoBehaviour
{
    WorldGenerator worldGen;
    BlockManager blockMan;

    float[] objParams;

    public void startItem(float[] parameters)
    {
        objParams = parameters;

        worldGen = FindObjectOfType<WorldGenerator>();
        blockMan = FindObjectOfType<BlockManager>();

        worldGen.blockToPlace = new Block(0, 0, Mathf.RoundToInt(objParams[6]), Mathf.RoundToInt(objParams[7]), 0);
    }

    public void updateItem(float[] parameters)
    {

    }

    public void endItem(float[] parameters)
    {

    }
}
