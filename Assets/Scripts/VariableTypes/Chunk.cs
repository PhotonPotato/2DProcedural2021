using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk
{
    public int chunkXMax;
    public int chunkXMin;
    public int chunkID;
    public int chunkSize;

    public GameObject savedChunk;
    public Tilemap chunkTilemap;


    public Chunk(int minX, int maxX, int size, GameObject chunk, int chunkID)
    {
        chunkXMin = minX;
        chunkXMax = maxX;

        chunkSize = size;
        this.chunkID = chunkID;

        savedChunk = chunk;
        chunkTilemap = savedChunk.GetComponent<Tilemap>();
    }

    public bool showChunk(int targetX, int maxDist)
    {
        //If you are close enough to the chunk borders.
        if (Mathf.Abs(targetX - chunkXMin) <= maxDist || Mathf.Abs(targetX - chunkXMax) <= maxDist)
        {
            return true;
        }

        return false;
    }
}
