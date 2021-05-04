using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WindData", menuName = "ScriptableObjects/WindInformation")]
public class WindData : ScriptableObject
{
    public float maxWindSpawnTemp;
    public float minWindSpawnTemp;

    public float maxWindSpeed;
    public float minWindSpeed;

    public float maxWindSize;
    public float minWindSize;
}
