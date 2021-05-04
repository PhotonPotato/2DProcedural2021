using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreGenerator
{
    public Block ore;
    public int maxRadius;
    public int minRadius;
    public int oresInVein;
    public WorldGenerator worldGen;

    public OreGenerator(Block newBlock, int newMax, int newMin, int oresInVein, WorldGenerator newWorldGenerator)
    {
        ore = newBlock;
        maxRadius = newMax;
        minRadius = newMin;
        this.oresInVein = oresInVein;
        worldGen = newWorldGenerator;
    }

    public void makeOre(int degrees)
    {
        for (int i = 0; i < 360; i += 360 / degrees)
        {
            for (int j = UnityEngine.Random.Range(minRadius, maxRadius + 1); j >= 0; j--)
            {
                int x = ore.x + Mathf.RoundToInt(Mathf.Sin(i) * j);
                int y = ore.y + Mathf.RoundToInt(Mathf.Cos(i) * j);

                if(worldGen.isInWorld(x, y))
                {
                    if(worldGen.worldBlocks[y, x] != null && worldGen.worldBlocks[y, x].blockType != 4)
                    {
                        worldGen.worldBlocks[y, x] = ore;
                    }
                }
            }
        }
    }
}
