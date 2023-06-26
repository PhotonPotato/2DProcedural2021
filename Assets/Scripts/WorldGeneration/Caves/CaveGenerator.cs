using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaveGenerator
{
    WorldGenerator worldGen;
    int numCaves;
    float noiseStepUpperBound;
    float noiseStepLowerBound;
    int seed;
    float pickingDegree;

    bool[,] noiseStorage;

    public CaveGenerator(WorldGenerator worldGen, int numCaves, float noiseStepUpperBound, float noiseStepLowerBound, int seed, float pickingDegree)
    {
        this.worldGen = worldGen;
        this.numCaves = numCaves;

        this.noiseStepUpperBound = noiseStepUpperBound;
        this.noiseStepLowerBound = noiseStepLowerBound;

        this.seed = seed;
        this.pickingDegree = pickingDegree;

        noiseStorage = new bool[worldGen.meshWidth, worldGen.meshHeight];
    }

    public void initGeneration()
    {
        initSteppedNoise();
        setWorldVals();
    }

    void initSteppedNoise()
    {
        for (int x = 0; x < worldGen.meshWidth; x++)
        {
            for (int y = 0; y < worldGen.meshHeight; y++)
            {
                float val = Mathf.PerlinNoise((x + seed) * pickingDegree, (y + seed) * pickingDegree);

                //Cave = white, val == 1, true.
                bool steppedVal = noiseStepUpperBound > val && val > noiseStepLowerBound;

                noiseStorage[x, y] = steppedVal;
            }
        }
    }

    void setWorldVals()
    {
        for (int x = 0; x < worldGen.meshWidth; x++)
        {
            for (int y = 0; y < worldGen.meshHeight; y++)
            {
                //Consitions.
                if (worldGen.worldBlocks[y, x] == null) continue;
                if (worldGen.worldBlocks[y, x].blockType == 4) continue;

                if (noiseStorage[x, y])
                {
                    worldGen.worldBlocks[y, x] = null;
                }
            }
        }
    }

    /* Steps:
     * Generate perlin noise.
     * Step the noise to make distinguished black and white pockets (no in between).
     * *optional* count up the number of caves.
     * Set the white pockets in the noise that correspond to the world map to empty blocks (unless they are above the surface)
     * *optional* save the edges of the caves in a list for later use in an advanced ore generator.
     * *optional* merge some caves together.
     */
}
