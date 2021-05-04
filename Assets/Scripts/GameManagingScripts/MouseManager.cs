using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public float miningRange = 10;

    public Vector3 mousePos;
    public GameObject worldGenObj;
    WorldGenerator worldGenScript;
    public GameObject player;

    public int handStrength = 0;

    private void Start()
    {
        worldGenScript = worldGenObj.GetComponent<WorldGenerator>();
    }

    private void FixedUpdate()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Break?
        if (Input.GetMouseButtonDown(0))
        {
            //Check if it is within range.
            if (Mathf.Sqrt(Mathf.Pow(mousePos.x - player.transform.position.x, 2) + Mathf.Pow(mousePos.y - player.transform.position.y, 2)) <= miningRange)
            {
                if (worldGenScript.canInteractWithWorld) worldGenScript.deleteBlock(Mathf.RoundToInt(mousePos.x + .3f) - 1, Mathf.RoundToInt(mousePos.y) - 1, handStrength);
            }
        }

        //Place?
        if (Input.GetMouseButton(1))
        {
            if (worldGenScript.canInteractWithWorld)
            {
                //Check if it is within range.
                if (Mathf.Sqrt(Mathf.Pow(mousePos.x - player.transform.position.x, 2) + Mathf.Pow(mousePos.y - player.transform.position.y, 2)) <= miningRange)
                {
                    worldGenScript.placeBlock(Mathf.RoundToInt(mousePos.x + .3f) - 1, Mathf.RoundToInt(mousePos.y) - 1, 0);
                }
            }
        }
    }
}
