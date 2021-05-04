using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTrailScript : MonoBehaviour
{
    public GameObject trailObj;
    public float timeBetween = 1;

    float timer = 0;

    List<GameObject> spawnedTrails;

    private void Awake()
    {
        trailObj = Resources.Load<GameObject>("Trails/BasicTrail");

        spawnedTrails = new List<GameObject>();
    }

    public void startItem(float[] parameters)
    {

    }

    public void updateItem(float[] parameters)
    {
        Vector3 pos = new Vector3(parameters[0], parameters[1], parameters[2]);
        Quaternion rot = new Quaternion(parameters[3], parameters[4], parameters[5], 0);

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            spawnedTrails.Add(Instantiate(trailObj, pos, rot));
            timer = timeBetween;
        }

        //Make a max amount of trails.
        if(spawnedTrails.Count > 5)
        {
            Destroy(spawnedTrails[0]);
            spawnedTrails.RemoveAt(0);
        }
    }

    public void endItem()
    {
        for (int i = 0; i < spawnedTrails.Count; i++)
        {
            Destroy(spawnedTrails[i]);
        }

        spawnedTrails = new List<GameObject>();
    }
}
