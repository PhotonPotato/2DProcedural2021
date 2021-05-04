using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPath
{
    public Vector2Int nextDir;
    public int x;
    public int y;

    public WindPath(int x, int y, Vector2Int nextDir)
    {
        this.x = x;
        this.y = y;
        this.nextDir = nextDir;
    }
}
