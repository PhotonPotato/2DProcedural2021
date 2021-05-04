using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeData", menuName = "ScriptableObjects/BiomeInformation")]
public class BiomeData : ScriptableObject
{
    public int maxHeight;
    public int oreGenerationDegree;
    public int maxOreLineLength;
    public int minOreLineLength;

    public int biomeTopLayerID = 0;
    public int biomeBotLayerID = 0;
    public int[] biomeOreID;

    public float maxTemp;
    public float minTemp;
    public float extremity;
    public float pickingDegree;

    public Material topLayerMat;
    public Material stoneLayerMat;
    public Material oreLayerMat;

    public TreeData[] biomeTrees;
}
