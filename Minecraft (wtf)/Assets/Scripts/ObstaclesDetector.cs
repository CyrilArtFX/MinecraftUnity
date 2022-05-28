using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesDetector : MonoBehaviour
{
    [SerializeField] private LayerMask obstaclesLayer;

    public bool IsObstacles()
    {
        Collider[] obstacles = Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, obstaclesLayer);
        if (obstacles.Length > 0) return true;
        else return false;
    }
}
