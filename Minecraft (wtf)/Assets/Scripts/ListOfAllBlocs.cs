using System.Collections.Generic;
using UnityEngine;

public class ListOfAllBlocs : MonoBehaviour
{
    public List<Bloc> blocs = new List<Bloc>();

    public static ListOfAllBlocs instance;

    private void Awake()
    {
        instance = this;
    }
}
