using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TreeData", menuName = "ScriptableObjects/TreeInformation")]
public class TreeData : ScriptableObject
{
    public int nativeBiome;
    public int generationType;

    public int maxTreeSpread;
    public int minTreeSpread;

    public int maxTrunkHeight;
    public int minTrunkHeight;

    public int maxBranchLength;
    public int minBranchLength;

    public int leafHeight;
    public int maxLeafSpawnRadius;
    public int minLeafSpawnRadius;

    public bool generateLeaves;
    public bool generateBranchesFromBranches;

    //Sprites:
    public int leafID;
    public int trunkID;
}
