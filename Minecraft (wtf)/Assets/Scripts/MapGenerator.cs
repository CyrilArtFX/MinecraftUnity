using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 16;
    public float noiseScale;

    [Min(0)] public int octaves;
    [Range(0,1)] public float persistance;
    [Min(1)] public float lacunarity;

    private void Awake()
    {
        instance = this;
    }

    public float[,] GenerateNoiseMap(Vector2Int offset, int seed)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, normalizeMode);
        return noiseMap;
    }
}