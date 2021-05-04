using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroyer : MonoBehaviour
{
    public float lifeTime = 3;

    void Start()
    {
        Destroy(this.gameObject, lifeTime);
    }
}
