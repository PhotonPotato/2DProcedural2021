using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPathCalculator
{
    List<WindPath> newWindPath;
    Block[,] blocks;
    int[] topHeights;

    Vector2Int currentPos;

    public WindPathCalculator()
    {

    }

    public List<WindPath> getWorldWindPath(WorldGenerator worldGen, int dir, int yAboveGround)
    {
        newWindPath = new List<WindPath>();

        //Get some data and set some variables.
        blocks = worldGen.worldBlocks;
        topHeights = worldGen.topHeight;

        //Set up where the wind starts. (The edge of the map at the top of the ground to the left or to th right)
        if(dir == 1) currentPos = new Vector2Int(0, topHeights[0] + yAboveGround);
        else currentPos = new Vector2Int(worldGen.meshWidth - 1, topHeights[worldGen.meshWidth - 1] + yAboveGround);

        while (true)
        {
            int lastLength = newWindPath.Count;

            for (int i = -1; i <= 1; i++)
            {
                if (!worldGen.isInWorld(currentPos.x + dir, currentPos.y + i)) continue;

                if (blocks[currentPos.y + i, currentPos.x + dir] == null)
                {
                    currentPos = new Vector2Int(currentPos.x + dir, currentPos.y + i);
                    newWindPath.Add(new WindPath(currentPos.x, currentPos.y, new Vector2Int(i, dir)));
                    break;
                }
            }

            if (newWindPath.Count == lastLength) break;
        }

        return newWindPath;
    }
}
