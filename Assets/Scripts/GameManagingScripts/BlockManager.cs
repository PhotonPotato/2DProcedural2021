using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockManager : MonoBehaviour
{
    [System.Serializable]
    public struct blockMetaDict{
        public int blockType;
        public Tile blockImage;
        public Material breakMat;
    }
    public blockMetaDict[] allItems;
}
