using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchGenerator
{
    List<Vector2Int> bannnedDirs = new List<Vector2Int>();
    List<Vector2Int> possibleDirs = new List<Vector2Int>();
    List<Vector2Int> allDirs = new List<Vector2Int>();
    Vector2Int currentDir = new Vector2Int();

    public BranchGenerator(List<Vector2Int> newBannedDirs)
    {
        bannnedDirs = newBannedDirs;
    }

    public Vector2Int findNewDir(Vector2Int startPos, WorldGenerator worldGen)
    {
        //Set up the possibilities
        //If the dir is banned then don't add it.
        possibleDirs.Clear();
        allDirs.Clear();

        //Set up the allDirs so that we have a list of all of the possibilities.
        allDirs.Add(new Vector2Int(0, 1));
        allDirs.Add(new Vector2Int(0, -1));
        allDirs.Add(new Vector2Int(1, 0));
        allDirs.Add(new Vector2Int(-1, 0));

        currentDir = new Vector2Int(0, 0);
        for(int i = 0; i < allDirs.Count; i++)
        {
            if (!bannnedDirs.Contains(allDirs[i]))
            {
                if(worldGen.isInWorld(startPos.y + allDirs[i].y, startPos.x + allDirs[i].x)) if(worldGen.worldBlocks[startPos.x + allDirs[i].x, startPos.y + allDirs[i].y] == null)
                {
                   possibleDirs.Add(allDirs[i]);
                }
            }
        }

        if (possibleDirs.Count > 0) currentDir = possibleDirs[UnityEngine.Random.Range(0, possibleDirs.Count)];
        return currentDir;
    }
}
