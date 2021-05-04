using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public Material innerCrust;
    public GameObject blockBreakParts;
    public GameObject gameManager;

    BlockManager blockMan;

    public Transform playerTransform;

    //Chunk Vars.
    public GameObject gridParent;
    public GameObject worldChunk;
    public Chunk[] savedChunks;
    public int chunks = 8;
    public int rendDist = 200;

    public Block blockToPlace;

    [System.NonSerialized]
    public Block[,] worldBlocks;

    [System.NonSerialized]
    public int[] topHeight;
    [System.NonSerialized]
    public int[] xPosBiomes;

    public BiomeData[] biomes;

    //Mesh Vars
    int seed = 0;
    int biomeSeed = 0;
    int currentBiome = 0;
    int lastBiome = 0;
    int meshMaxHeight = 100;

    //Cave vars
    public float caveNoisePickingDegree = .3f;
    public float caveNoiseStep = .5f;

    //Ore Vars
    int numOreDeposits = 500;
    Block ore;


    //Tree Vars
    int distToLastTree = 0;
    int distBetweenTrees = 0;

    public float meshBiomePickingDegree = .01f;
    public int meshWidth = 10;
    public int meshHeight = 10;
    public int lowerBy = 0;

    public float extremity = 8;
    public float pickingDegree = .3f;

    //Lighting vars.
    public int defaultLightingDist = 5;
    public int defaultLightingIncrements = 10;

    //Other
    public bool canInteractWithWorld;

    void Start()
    {
        blockMan = gameManager.GetComponent<BlockManager>();
        MeshSetup();
    }

    private void Update()
    {
        updateChunks();
    }

    void MeshSetup()
    {
        biomeSeed = UnityEngine.Random.Range(0, 1000000);

        generateChunks();

        //worldBlocks = new Block[meshHeight, meshWidth];
        //worldBlocks[0, 0] = new Block(0, 0, 1, 1, 0);

        GenerateBlocks();
        GenerateCaves();

        //Reset the top heights so that the trees and ores will generate better.
        remapTopheights();

        GenerateOres();
        generateTrees();

        updateTilemap();
        calcLightingAll();
        refreshChunkTilemaps();
    }

    public void GenerateBlocks()
    {
        seed = UnityEngine.Random.Range(0, 1000000);

        int yOffset = 0;

        worldBlocks = new Block[meshHeight, meshWidth];
        topHeight = new int[meshWidth];
        xPosBiomes = new int[meshWidth];

        for (int z = 0; z < meshWidth; z++)
        {
            //Look for what biome it is based on some perlin noise temperature.
            findCurrentBiome(z);
            //Now set the global biome log.
            xPosBiomes[z] = currentBiome;

            //Now set up the vars from the current biome.
            meshMaxHeight = biomes[currentBiome].maxHeight;
            extremity = biomes[currentBiome].extremity;
            pickingDegree = biomes[currentBiome].pickingDegree;

            //Make the offest your current block minus 1 (unless its 0);
            if (z != 0)
            {
                int testOffset = yOffset;
                yOffset = checkForlastBlock(z - 1);
                if (yOffset == 0) yOffset = testOffset;
            }

            int randStoneHeight = UnityEngine.Random.Range(0, 6);
            topHeight[z] = lowerBy + meshHeight - Mathf.RoundToInt(Mathf.PerlinNoise(0, (seed + z) * pickingDegree) * extremity) + yOffset;

            //Cap it at the max height.
            if (topHeight[z] > meshMaxHeight) topHeight[z] = meshMaxHeight;
            if (topHeight[z] > meshHeight) topHeight[z] = meshHeight;

            for (int y = 0; y < topHeight[z]; y++)
            {
                worldBlocks[y, z] = new Block(z, y, 0, 0, 0);

                //Check if it is the bottom of the map.
                if (y == 0)
                {
                    worldBlocks[y, z].blockID = 9;
                    worldBlocks[y, z].blockType = 4;

                    //Don't bother running through the other lists.
                    continue;
                }

                if (topHeight[z] - y <= randStoneHeight)
                {
                    worldBlocks[y, z].blockID = biomes[currentBiome].biomeTopLayerID;
                    worldBlocks[y, z].blockType = 0;
                }
                else
                {
                    worldBlocks[y, z].blockID = biomes[currentBiome].biomeBotLayerID;
                    worldBlocks[y, z].blockType = 1;
                }

            }

            lastBiome = currentBiome;
        }
    }

    int checkForlastBlock(int z)
    {
        int blockChange = 0;

        //The current biome is a global variable so we can just compare the 2.
        if (lastBiome != currentBiome)
        {
            //Get the difference.
            blockChange = topHeight[z] - (lowerBy + meshHeight - Mathf.RoundToInt(Mathf.PerlinNoise(0, (seed + z + 1) * pickingDegree) * extremity));
            OnNewBiome(z);
        }

        return blockChange;
    }

    void setNewOreChance()
    {
        ore = new Block(0, 0, biomes[currentBiome].biomeOreID[0], 3, 0);
    }

    public void generateChunks()
    {
        int increments = Mathf.RoundToInt(meshWidth / chunks);

        savedChunks = new Chunk[chunks];

        //Run through all of the chunks.
        for (int i = 0; i < chunks; i++)
        {
            //Initiate a new chunk.
            savedChunks[i] = new Chunk(i * increments, (i + 1) * increments, increments, Instantiate(worldChunk, new Vector3(0, 0, 0), Quaternion.identity), i);

            //Set it so that the parent is the world grid.
            savedChunks[i].savedChunk.transform.SetParent(gridParent.transform);
        }
    }

    public void updateChunks()
    {
        for (int i = 0; i < savedChunks.Length; i++)
        {
            savedChunks[i].savedChunk.GetComponent<TilemapRenderer>().enabled = savedChunks[i].showChunk(Mathf.RoundToInt(playerTransform.position.x), rendDist);
        }
    }

    public int getChunk(Vector2Int targetPos)
    {
        for (int i = 0; i < savedChunks.Length; i++)
        {
            if (targetPos.x < savedChunks[i].chunkXMax && targetPos.x >= savedChunks[i].chunkXMin)
            {
                return i;
            }
        }

        return 0;
    }

    public void updateTilemap()
    {
        List<int> updatedTilemapIDs = new List<int>();

        for (int i = 0; i < savedChunks.Length; i++)
        {
            savedChunks[i].chunkTilemap.ClearAllTiles();
        }

        for (int z = 0; z < meshWidth; z++)
        {
            findCurrentBiome(z);

            //Run through the board of blocks.
            for (int y = 0; y < meshHeight; y++)
            {
                if (worldBlocks[y, z] == null) continue;

                int currentChunk = getChunk(new Vector2Int(z, y));

                savedChunks[currentChunk].chunkTilemap.SetTile(new Vector3Int(z, y, 0), getBlock(worldBlocks[y, z]));
                updatedTilemapIDs.Add(currentChunk);
            }
        }

        /*
        for (int i = 0; i < updatedTilemapIDs.Count; i++)
        {
            savedChunks[updatedTilemapIDs[i]].chunkTilemap.RefreshAllTiles();
        }*/
    }

    public void refreshChunkTilemaps()
    {
        for (int i = 0; i < savedChunks.Length; i++)
        {
            savedChunks[i].chunkTilemap.RefreshAllTiles();
        }
    }

    public void deleteBlock(int z, int y, int handStrength)
    {
        if (!canInteractWithWorld) return;

        z = Mathf.Clamp(z, 0, meshWidth);
        y = Mathf.Clamp(y, 0, meshHeight);

        //Check is the hand isn't strong enought to break the block.
        if (!isInWorld(z, y)) return;
        if (worldBlocks[y, z] == null) return;
        if (handStrength < worldBlocks[y, z].blockStrength) return;

        GameObject parts = Instantiate(blockBreakParts, new Vector3(z, y, -.5f), Quaternion.identity);
        ParticleSystemRenderer renderer = parts.GetComponent<ParticleSystemRenderer>();
        renderer.material = blockMan.allItems[worldBlocks[y, z].blockID].breakMat;

        //Get and store the chunk of the selected block.
        int currentChunk = getChunk(new Vector2Int(z, 0));

        //Assess the block drop situation.
        if (worldBlocks[y, z].dropsItem)
        {
            GameObject droppedItem = Instantiate(FindObjectOfType<InventroyManager>().dropObject, new Vector3(z, y, 0), Quaternion.identity);

            ItemDropDataHolder data = droppedItem.GetComponent<ItemDropDataHolder>();

            //Set up the item data's vars.
            data.data = new InventorySlot(0, 0, worldBlocks[y, z].blockID, worldBlocks[y, z].blockType, 1, worldBlocks[y, z].maxItemsInStack, "BasicBlockUsageScript", FindObjectOfType<InventroyManager>().gameObject);
            data.data.itemDiplayImage = savedChunks[currentChunk].chunkTilemap.GetSprite(new Vector3Int(z, y, 0));
            droppedItem.GetComponent<SpriteRenderer>().sprite = data.data.itemDiplayImage;
            data.data.slotParameters = new float[8];
            data.data.slotParameters[6] = worldBlocks[y, z].blockID;
            data.data.slotParameters[7] = worldBlocks[y, z].blockType;
        }

        worldBlocks[y, z] = null;
        savedChunks[currentChunk].chunkTilemap.SetTile(new Vector3Int(z, y, 0), null);
        savedChunks[currentChunk].chunkTilemap.RefreshTile(new Vector3Int(z, y, 0));
    }

    public void placeBlock(int z, int y, int blockBiome)
    {
        if (!canInteractWithWorld) return;

        z = Mathf.Clamp(z, 0, meshWidth);
        y = Mathf.Clamp(y, 0, meshHeight);

        if (!isInWorld(z, y)) return;
        if (worldBlocks[y, z] != null) return;

        int currentChunk = getChunk(new Vector2Int(z, 0));

        worldBlocks[y, z] = new Block(z, y, blockToPlace.blockID, blockToPlace.blockType, 0);
        savedChunks[currentChunk].chunkTilemap.SetTile(new Vector3Int(z, y, 0), blockMan.allItems[blockToPlace.blockID].blockImage);
        savedChunks[currentChunk].chunkTilemap.RefreshTile(new Vector3Int(z, y, 0));
    }

    public Tile getBlock(Block block)
    {
        if (block.blockID < blockMan.allItems.Length)
        {
            return blockMan.allItems[block.blockID].blockImage;
        }
        return null;
    }

    void OnNewBiome(int curZ)
    {
        blendBiomes(curZ);
        resetTreeValues(currentBiome);
        setNewOreChance();
    }

    void blendBiomes(int z)
    {
        for (int j = 0; j < topHeight[z]; j++)
        {
            for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++)
            {
                //If out of bounds
                if (z - i < 0) continue;
                //If the target block or current block is nonexistent.
                if (worldBlocks[j, z - i] == null || worldBlocks[j, z] == null) continue;

                switch (worldBlocks[j, z - i].blockType)
                {
                    case 0:
                        worldBlocks[j, z - i].blockID = biomes[currentBiome].biomeTopLayerID;
                        break;

                    case 1:
                        worldBlocks[j, z - i].blockID = biomes[currentBiome].biomeBotLayerID;
                        break;

                    case 3:
                        worldBlocks[j, z - i].blockID = biomes[currentBiome].biomeOreID[0];
                        break;
                }
            }
        }
    }

    void generateTrees()
    {
        for (int z = 0; z < meshWidth; z++)
        {
            //First we need the biome.
            findCurrentBiome(z);

            //Generate trees.
            distToLastTree++;
            if (shouldMakeTree(z))
            {
                makeTree(z, topHeight[z], biomes[currentBiome].biomeTrees[0], biomes[currentBiome].biomeTrees[0].generationType);
                resetTreeValues(currentBiome);
                distToLastTree = 0;
            }
        }
    }

    bool shouldMakeTree(int z)
    {
        if (distToLastTree >= distBetweenTrees)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void resetTreeValues(int tBiome)
    {
        distBetweenTrees = UnityEngine.Random.Range(biomes[tBiome].biomeTrees[0].minTreeSpread, biomes[tBiome].biomeTrees[0].maxTreeSpread + 1);
    }

    public void makeTree(int z, int y, TreeData data, int treeType)
    {
        Vector2Int trunkTop;
        List<Vector2Int> bannedDirs = new List<Vector2Int>();

        int localTrunkHeight = UnityEngine.Random.Range(data.minTrunkHeight, data.maxTrunkHeight + 1);

        for (int i = 0; i < localTrunkHeight + 1; i++)
        {
            worldBlocks[y + i, z] = new Block(y + i, z, data.trunkID, 5, 0);
        }

        Vector2Int newPos = new Vector2Int();

        int leafWidth = localTrunkHeight + 1 - data.leafHeight;

        switch (treeType)
        {
            //Normal with braches and balls of leaves at the end of ach branch.
            case 0:
                //Set the starting place for each branch.
                trunkTop = new Vector2Int(y + localTrunkHeight, z);
                newPos = makeBranch(0, trunkTop, data);
                if (data.generateBranchesFromBranches) makeBranch(2, newPos, data);
                trunkTop.x -= 1;
                newPos = makeBranch(1, trunkTop, data);
                if (data.generateBranchesFromBranches) makeBranch(2, newPos, data);
                break;

            //More like "pine tree" trees
            case 1:
                if (!data.generateBranchesFromBranches) return;

                //For each layer.
                for (int i = data.leafHeight; i < localTrunkHeight + 1; i++)
                {
                    //Generate a certain amount of horiontal leaves to the left of the trunk.
                    for (int j = leafWidth * -1; j < 0; j++)
                    {
                        if (isInWorld(z + j, y + i)) if (worldBlocks[y + i, z + j] == null) worldBlocks[y + i, z + j] = new Block(z + j, y + i, data.leafID, 5, 0);
                    }

                    //Generate a certain amount of horiontal leaves to the right of the trunk.
                    for (int j = 1; j <= leafWidth; j++)
                    {
                        if (isInWorld(z + j, y + i)) if (worldBlocks[y + i, z + j] == null) worldBlocks[y + i, z + j] = new Block(z + j, y + i, data.leafID, 5, 0);
                    }

                    //Make the center leaves if there isn't a trunk.
                    if (isInWorld(z, y + i)) if (worldBlocks[y + i, z] == null) worldBlocks[y + i, z] = new Block(z, y + i, data.leafID, 5, 0);

                    //Make the amount of leaves slowly decrease.
                    leafWidth--;
                }

                if (isInWorld(z, y + localTrunkHeight + 1)) if (worldBlocks[y + localTrunkHeight + 1, z] == null) worldBlocks[y + localTrunkHeight + 1, z] = new Block(z, y + localTrunkHeight + 1, data.leafID, 5, 0);
                break;

            case 2:
                if (!data.generateBranchesFromBranches) return;

                //For each layer.
                for (int i = data.leafHeight; i < (localTrunkHeight + 1) * 2; i++)
                {
                    //Generate a certain amount of horiontal leaves to the left of the trunk.
                    for (int j = leafWidth * -1; j < 0; j++)
                    {
                        if (isInWorld(z + j, y + i)) if (worldBlocks[y + i, z + j] == null) worldBlocks[y + i, z + j] = new Block(z + j, y + i, data.leafID, 5, 0);
                    }

                    //Generate a certain amount of horiontal leaves to the right of the trunk.
                    for (int j = 1; j <= leafWidth; j++)
                    {
                        if (isInWorld(z + j, y + i)) if (worldBlocks[y + i, z + j] == null) worldBlocks[y + i, z + j] = new Block(z + j, y + i, data.leafID, 5, 0);
                    }

                    //Make the center leaves if there isn't a trunk.
                    if (isInWorld(z, y + i)) if (worldBlocks[y + i, z] == null) worldBlocks[y + i, z] = new Block(z, y + i, data.leafID, 5, 0);

                    //Make the amount of leaves slowly decrease.
                    if (i % 2 == 0) leafWidth--;
                }
                break;

            case 3:
                if (!data.generateBranchesFromBranches) return;

                //For each layer.
                for (int i = data.leafHeight; i < (localTrunkHeight + 1) * 2; i += 2)
                {
                    //Generate a certain amount of horiontal leaves to the left of the trunk.
                    for (int j = leafWidth * -1; j < 0; j++)
                    {
                        if (isInWorld(z + j, y + i)) if (worldBlocks[y + i, z + j] == null) worldBlocks[y + i, z + j] = new Block(z + j, y + i, data.leafID, 5, 0);
                    }

                    //Generate a certain amount of horiontal leaves to the right of the trunk.
                    for (int j = 1; j <= leafWidth; j++)
                    {
                        if (isInWorld(z + j, y + i)) if (worldBlocks[y + i, z + j] == null) worldBlocks[y + i, z + j] = new Block(z + j, y + i, data.leafID, 5, 0);
                    }

                    //Make the center leaves if there isn't a trunk.
                    if (isInWorld(z, y + i)) if (worldBlocks[y + i, z] == null) worldBlocks[y + i, z] = new Block(z, y + i, data.leafID, 5, 0);
                    if (isInWorld(z, y + i - 1)) if (worldBlocks[y + i - 1, z] == null) worldBlocks[y + i - 1, z] = new Block(z, y + i - 1, data.leafID, 5, 0);

                    //Make the amount of leaves slowly decrease.
                    leafWidth--;
                }
                break;
        }


    }

    Vector2Int makeBranch(int Dir, Vector2Int trunkTop, TreeData data)
    {
        Vector2Int curGenPos;
        Vector2Int newGenPos = new Vector2Int(0, 0);
        List<Vector2Int> bannedDirs = new List<Vector2Int>();
        int bLength = UnityEngine.Random.Range(data.minBranchLength, data.maxBranchLength);
        int newBranchSpot = UnityEngine.Random.Range(0, bLength);

        curGenPos = trunkTop;
        bannedDirs.Clear();

        switch (Dir)
        {
            case 0:
                bannedDirs.Add(new Vector2Int(0, -1));
                bannedDirs.Add(new Vector2Int(-1, 0));
                break;

            case 1:
                bannedDirs.Add(new Vector2Int(0, 1));
                bannedDirs.Add(new Vector2Int(-1, 0));
                break;

            case 2:
                bannedDirs.Add(new Vector2Int(0, 1));
                bannedDirs.Add(new Vector2Int(0, -1));
                bannedDirs.Add(new Vector2Int(-1, 0));

                bLength = UnityEngine.Random.Range(Mathf.Clamp(data.minBranchLength - 2, 0, 100), data.maxBranchLength - 2);
                break;

            case 3:
                //bannedDirs.Add(new Vector2Int(-1, 0));
                break;
        }

        for (int i = 0; i < bLength; i++)
        {
            BranchGenerator branch = new BranchGenerator(bannedDirs);
            curGenPos += branch.findNewDir(curGenPos, GetComponent<WorldGenerator>());
            if (i == newBranchSpot) newGenPos = curGenPos;
            worldBlocks[curGenPos.x, curGenPos.y] = new Block(curGenPos.x, curGenPos.y, data.trunkID, 6, 0);
        }
        makeLeaves(UnityEngine.Random.Range(data.minLeafSpawnRadius, data.maxLeafSpawnRadius + 1), data, curGenPos);

        return newGenPos;
    }

    void makeLeaves(int radius, TreeData data, Vector2Int pos)
    {
        LeafGenerator leafGen = new LeafGenerator(pos, GetComponent<WorldGenerator>(), data);
        leafGen.generateLeaves(radius);
    }

    void GenerateOres()
    {
        for (int i = 0; i < numOreDeposits; i++)
        {
            int x = UnityEngine.Random.Range(0, meshWidth);
            int y = UnityEngine.Random.Range(0, topHeight[x]);

            ore.x = x;
            ore.y = y;

            ore = new Block(x, y, biomes[xPosBiomes[x]].biomeOreID[0], 2, 0);

            OreGenerator oreGen = new OreGenerator(ore, 3, 0, 8, GetComponent<WorldGenerator>());
            oreGen.makeOre(20);
        }
    }

    void findCurrentBiome(int z)
    {
        //Find biome.
        for (int i = 0; i < biomes.Length; i++)
        {
            float curBiomeTemp = Mathf.PerlinNoise((z + biomeSeed) * meshBiomePickingDegree, 0);
            if (biomes[i].maxTemp >= curBiomeTemp && biomes[i].minTemp <= curBiomeTemp)
            {
                currentBiome = i;
                break;
            }
        }
    }

    public bool isInWorld(int z, int y)
    {
        return ((z >= 0 && z < meshWidth) && (y >= 0 && y < meshHeight));
    }

    public void GenerateVisibleWindPath()
    {
        WindPathCalculator windCalc = new WindPathCalculator();

        foreach (WindPath curWindPos in windCalc.getWorldWindPath(GetComponent<WorldGenerator>(), -1, 1))
        {
            worldBlocks[curWindPos.y + 4, curWindPos.x] = new Block(curWindPos.x, curWindPos.y + 4, 5, 0, 0);
        }
    }

    public void GenerateCaves()
    {
        CaveGenerator caveGen = new CaveGenerator(this.gameObject.GetComponent<WorldGenerator>(), 0, caveNoiseStep, Mathf.RoundToInt(UnityEngine.Random.Range(0, 1000000)), caveNoisePickingDegree);
        caveGen.initGeneration();
    }

    public void remapTopheights()
    {
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = meshHeight - 1; y >= 0; y--)
            {
                if (worldBlocks[y, x] != null)
                {
                    topHeight[x] = y + 1;
                    break;
                }
            }
        }
    }

    public void calcLightingAll()
    {
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                calcLighting(x, y, defaultLightingDist, defaultLightingIncrements);
            }
        }
    }

    public void calcLighting(int x, int y, int lightingDist, int increments)
    {
        //Check if the block that we are checking is empty.
        if (!isInWorld(x, y)) return;
        if (worldBlocks[y, x] == null) return;

        Vector3Int thisPos = new Vector3Int(x, y, 0);

        //Then check all the blocks within a certain radius.
        for (int d = 0; d < 360; d += increments)
        {
            for (int dist = 0; dist < lightingDist; dist++)
            {
                int newX = Mathf.RoundToInt(Mathf.Cos(d) * dist) + x;
                int newY = Mathf.RoundToInt(Mathf.Sin(d) * dist) + y;

                if (!isInWorld(newX, newY)) continue;

                if (worldBlocks[newY, newX] == null)
                {
                    int newLightLev = 0;//1 - (dist / lightingDist);

                    Color lightingColor = new Color(newLightLev, newLightLev, newLightLev);

                    savedChunks[getChunk(new Vector2Int(x, 0))].chunkTilemap.SetTileFlags(thisPos, TileFlags.None);
                    //savedChunks[getChunk(new Vector2Int(x, 0))].chunkTilemap.RemoveTileFlags(new Vector3Int(x, y, 0), TileFlags.LockColor);

                    savedChunks[getChunk(new Vector2Int(x, 0))].chunkTilemap.SetColor(thisPos, lightingColor);
                    savedChunks[getChunk(new Vector2Int(x, 0))].chunkTilemap.RefreshTile(thisPos);

                    if (x == 0) Debug.Log("Column" + y + " light level: " + savedChunks[getChunk(new Vector2Int(x, 0))].chunkTilemap.GetColor(thisPos).r + " wanted: " + lightingColor.r);
                }
            }
        }
    }
}
