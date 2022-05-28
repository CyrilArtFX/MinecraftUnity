using System.Collections.Generic;
using UnityEngine;

public class ListOfAllStructures : MonoBehaviour
{
    public List<Structure> structures = new List<Structure>();

    public static ListOfAllStructures instance;

    private void Awake()
    {
        instance = this;
    }
}
