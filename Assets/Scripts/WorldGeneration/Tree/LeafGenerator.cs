using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafGenerator
{
    Vector2Int leafCenterPos;
    WorldGenerator worldGen;
    TreeData data;

    public LeafGenerator(Vector2Int pos, WorldGenerator newWorldGen, TreeData newData)
    {
        leafCenterPos = pos;
        worldGen = newWorldGen;
        data = newData;
    }

    public void generateLeaves(int radius)
    {
        if (!data.generateLeaves) return;

        //Run through all of the degrees and generate a circle.
        for(int i = 0; i < 360; i++)
        {
            for(int j = radius; j >= 0; j--)
            {
                int x = leafCenterPos.y + Mathf.RoundToInt(Mathf.Cos(i) * j);
                int y = leafCenterPos.x + Mathf.RoundToInt(Mathf.Sin(i) * j) + 2;

                if (worldGen.isInWorld(x, y))
                {
                    if(worldGen.worldBlocks[y, x] == null) worldGen.worldBlocks[y, x] = new Block(x, y, data.leafID, 5, 0);
                }
            }
        }
    }
}
