using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public Transform target;

    public float speed = 10;

    Vector3 targetPos;
    Vector3 smoothedPos;

    private void FixedUpdate()
    {
        targetPos = target.position;

        smoothedPos.x += (target.position.x - transform.position.x) / speed;
        smoothedPos.y += (target.position.y - transform.position.y) / speed;
        smoothedPos.z = transform.position.z;

        transform.position = smoothedPos;
    }
}
