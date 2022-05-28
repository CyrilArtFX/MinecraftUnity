using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject obstaclesDetectorPrefab;
    [SerializeField] private GameObject updateTimer;

    [HideInInspector] public int[,,] blocsMatrix = new int[16,256,16];
    [HideInInspector] public GameObject[,,] cubesMatrix = new GameObject[16,256,16];
    private int[,] maxLandBloc = new int[16, 16];
    [HideInInspector] public float[,] treesInfluences = new float[24,24];

    int chunkPositionX, chunkPositionZ;

    [HideInInspector] public bool alreadyInitialized = false;

    public void CreateBlocMatrix(Vector3 position, int xPositionInMatrix, int yPositionInMatrix, int[,,] preGenMatrix)
    {
        transform.position = position;
        chunkPositionX = xPositionInMatrix;
        chunkPositionZ = yPositionInMatrix;

        //Génération du Chunk
        if (WorldChunks.instance.generationMode == WorldChunks.GenerationMode.superflat)
        {
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (y <= 56) blocsMatrix[x, y, z] = 1; //Stone
                        else if (y <= 59) blocsMatrix[x, y, z] = 2; //Dirt
                        else if (y == 60) blocsMatrix[x, y, z] = 3; //Grass
                        else blocsMatrix[x, y, z] = 0; //Air
                    }
                }
            }
        }
        else if (WorldChunks.instance.generationMode == WorldChunks.GenerationMode.procedural)
        {
            float[,] heightMap = MapGenerator.instance.GenerateNoiseMap(new Vector2Int(chunkPositionX * 16, -chunkPositionZ * 16), WorldChunks.instance.seed);
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        int maxHeightForThisBloc = (int)(heightMap[x, z] * 50) + 40;
                        if (y == maxHeightForThisBloc) maxLandBloc[x, z] = y;
                        if (y <= maxHeightForThisBloc)
                        {
                            if (maxHeightForThisBloc <= 62)
                            {
                                if (y <= maxHeightForThisBloc - 4) blocsMatrix[x, y, z] = 1; //Stone
                                else
                                {
                                    int rdm = Random.Range(0, 10);
                                    if (rdm < 9) blocsMatrix[x, y, z] = 7; //Sand
                                    else blocsMatrix[x, y, z] = 8; //Gravel
                                }
                            }
                            else if (maxHeightForThisBloc <= 87)
                            {
                                if (y <= maxHeightForThisBloc - 5) blocsMatrix[x, y, z] = 1; //Stone
                                else if (y <= maxHeightForThisBloc - 1) blocsMatrix[x, y, z] = 2; //Dirt
                                else blocsMatrix[x, y, z] = 3; //Grass
                            }
                            else if (maxHeightForThisBloc <= 89)
                            {
                                if (y <= maxHeightForThisBloc - 5) blocsMatrix[x, y, z] = 1; //Stone
                                else if (y <= maxHeightForThisBloc - 1) blocsMatrix[x, y, z] = 2; //Dirt
                                else blocsMatrix[x, y, z] = 6; //Snow
                            }
                            else
                            {
                                if (y <= maxHeightForThisBloc - 5) blocsMatrix[x, y, z] = 1; //Stone
                                else if (y <= maxHeightForThisBloc - 2) blocsMatrix[x, y, z] = 2; //Dirt
                                else blocsMatrix[x, y, z] = 6; //Snow
                            }
                        }
                        else if (maxHeightForThisBloc <= 60 && y <= 60) blocsMatrix[x, y, z] = 9; //Water Source
                        else
                        {
                            if (preGenMatrix[x, y, z] != 0) blocsMatrix[x, y, z] = preGenMatrix[x, y, z]; //For leaves if tree planted near this chunk
                            else blocsMatrix[x, y, z] = 0; //Air
                            //blocsMatrix[x, y, z] = 0;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < 24; x++)
            for (int z = 0; z < 24; z++)
                treesInfluences[x, z] = 1f;
    }

    public void PlaceTrees()
    {
        float[,] noiseProbaModifier = MapGenerator.instance.GenerateNoiseMap(new Vector2Int(chunkPositionX * 16, -chunkPositionZ * 16), WorldChunks.instance.seed * 2);
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                float probaOfTreeHere = 0f;
                int heigt = maxLandBloc[x, z] + 1;

                if(blocsMatrix[x, heigt - 1, z] == 3)
                {
                    //Probas phase 1 : has air around
                    if (x > 0)
                    {
                        if (blocsMatrix[x - 1, heigt, z] == 0) probaOfTreeHere += 0.25f;
                    }
                    else
                    {
                        if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
                        {
                            if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>().blocsMatrix[15, heigt, z] == 0) probaOfTreeHere += 0.25f;
                        }
                    }
                    if (x < 15)
                    {
                        if (blocsMatrix[x + 1, heigt, z] == 0) probaOfTreeHere += 0.25f;
                    }
                    else
                    {
                        if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
                        {
                            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().blocsMatrix[0, heigt, z] == 0) probaOfTreeHere += 0.25f;
                        }
                    }
                    if (z > 0)
                    {
                        if (blocsMatrix[x, heigt, z - 1] == 0) probaOfTreeHere += 0.25f;
                    }
                    else
                    {
                        if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
                        {
                            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().blocsMatrix[x, heigt, 15] == 0) probaOfTreeHere += 0.25f;
                        }
                    }
                    if (z < 15)
                    {
                        if (blocsMatrix[x, heigt, z + 1] == 0) probaOfTreeHere += 0.25f;
                    }
                    else
                    {
                        if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
                        {
                            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().blocsMatrix[x, heigt, 0] == 0) probaOfTreeHere += 0.25f;
                        }
                    }

                    if(probaOfTreeHere > 0)
                    {
                        //Probas phase 2 : other trees

                        probaOfTreeHere *= treesInfluences[x + 4, z + 4];
                        if (x >= 12 && WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
                        {
                            probaOfTreeHere *= WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().treesInfluences[x - 12, z + 4];
                        }
                        if (x <= 3 && WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
                        {
                            probaOfTreeHere *= WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>().treesInfluences[x + 20, z + 4];
                        }
                        if (z >= 12 && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
                        {
                            probaOfTreeHere *= WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().treesInfluences[x + 4, z - 12];
                        }
                        if (z <= 3 && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
                        {
                            probaOfTreeHere *= WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().treesInfluences[x + 4, z + 20];
                        }

                        if(probaOfTreeHere > 0)
                        {
                            probaOfTreeHere *= noiseProbaModifier[x, z];
                            float rdm = Random.Range(0f, 1f);
                            if (rdm <= (probaOfTreeHere / 30))
                            {
                                blocsMatrix[x, heigt - 1, z] = 2;
                                SetTreeInfluence(x, z);
                                int treeToPlace = (Random.Range(0, 10)) + 1;
                                PlaceStructure(x, heigt, z, ListOfAllStructures.instance.structures[treeToPlace]);
                            }
                        }
                    }
                }
            }
        }
    }

    public void CreateCubeMatrix()
    {
        //Génération des cubes
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    if (blocsMatrix[x, y, z] != 0)
                    {
                        if(HasAirAround(x, y, z, true))
                        {
                            cubesMatrix[x, y, z] = GameObject.Instantiate(ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z]].prefab);
                            cubesMatrix[x, y, z].transform.parent = gameObject.transform;
                            cubesMatrix[x, y, z].transform.localPosition = new Vector3(x, y, z);
                            bool isBlocLiquid = cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater;
                            if (isBlocLiquid) cubesMatrix[x, y, z].GetComponent<Water>().CreateWaterSourceForGeneration(this, x, y, z, y != 60);
                            cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().UpdateFaces(HasAirYPlus(x, y, z, isBlocLiquid), HasAirYMinus(x, y, z, isBlocLiquid), HasAirXPlus(x, y, z, isBlocLiquid), HasAirXMinus(x, y, z, isBlocLiquid), HasAirZPlus(x, y, z, isBlocLiquid), HasAirZMinus(x, y, z, isBlocLiquid));
                        }
                    }
                }
            }
        }
        alreadyInitialized = true;
    }

    void Start()
    {
    }

    public void DestroyBloc(int x, int y, int z)
    {
        blocsMatrix[x, y, z] = 0;

        if (x != 0 && blocsMatrix[x - 1, y, z] != 0)
        {
            if (cubesMatrix[x - 1, y, z] != null)
            {
                cubesMatrix[x - 1, y, z].SetActive(true);
                cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().UpdateXPlus(HasAirXPlus(x - 1, y, z, cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().isWater));
            }
            else PlaceBloc(x - 1, y, z, ListOfAllBlocs.instance.blocs[blocsMatrix[x - 1, y, z]]);
        }
        else if (x == 0)
        {
            if(WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[15, y, z] != 0)
                {
                    if (neighbourChunk.cubesMatrix[15, y, z] != null)
                    {
                        neighbourChunk.cubesMatrix[15, y, z].SetActive(true);
                        neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().UpdateXPlus(neighbourChunk.HasAirXPlus(15, y, z, neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().isWater));
                    }
                    else neighbourChunk.PlaceBloc(15, y, z, ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[15, y, z]]);
                }
            }
        }

        if (x != 15 && blocsMatrix[x + 1, y, z] != 0)
        {
            if (cubesMatrix[x + 1, y, z] != null)
            {
                cubesMatrix[x + 1, y, z].SetActive(true);
                cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().UpdateXMinus(HasAirXMinus(x + 1, y, z, cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().isWater));
            }
            else PlaceBloc(x + 1, y, z, ListOfAllBlocs.instance.blocs[blocsMatrix[x + 1, y, z]]);
        }
        else if (x == 15)
        {
            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[0, y, z] != 0)
                {
                    if (neighbourChunk.cubesMatrix[0, y, z] != null)
                    {
                        neighbourChunk.cubesMatrix[0, y, z].SetActive(true);
                        neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().UpdateXMinus(WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().HasAirXMinus(0, y, z, WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().isWater));
                    }
                    else neighbourChunk.PlaceBloc(0, y, z, ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[0, y, z]]);
                }
            }
        }

        if (z != 0 && blocsMatrix[x, y, z - 1] != 0)
        {
            if (cubesMatrix[x, y, z - 1] != null)
            {
                cubesMatrix[x, y, z - 1].SetActive(true);
                cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().UpdateZPlus(HasAirZPlus(x, y, z - 1, cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().isWater));
            }
            else PlaceBloc(x, y, z - 1, ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z - 1]]);
        }
        else if (z == 0)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 15] != 0)
                {
                    if (neighbourChunk.cubesMatrix[x, y, 15] != null)
                    {
                        neighbourChunk.cubesMatrix[x, y, 15].SetActive(true);
                        neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().UpdateZPlus(WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().HasAirZPlus(x, y, 15, WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().isWater));
                    }
                    else neighbourChunk.PlaceBloc(x, y, 15, ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 15]]);
                }
            }
        }

        if (z != 15 && blocsMatrix[x, y, z + 1] != 0)
        {
            if (cubesMatrix[x, y, z + 1] != null)
            {
                cubesMatrix[x, y, z + 1].SetActive(true);
                cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().UpdateZMinus(HasAirZMinus(x, y, z + 1, cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().isWater));
            }
            else PlaceBloc(x, y, z + 1, ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z + 1]]);
        }
        else if (z == 15)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 0] != 0)
                {
                    if (neighbourChunk.cubesMatrix[x, y, 0] != null)
                    {
                        neighbourChunk.cubesMatrix[x, y, 0].SetActive(true);
                        neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().UpdateZMinus(WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().HasAirZMinus(x, y, 0, WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().isWater));
                    }
                    else neighbourChunk.PlaceBloc(x, y, 0, ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 0]]);
                }
            }
        }

        if (y != 0 && blocsMatrix[x, y - 1, z] != 0)
        {
            if (cubesMatrix[x, y - 1, z] != null)
            {
                cubesMatrix[x, y - 1, z].SetActive(true);
                cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().UpdateYPlus(HasAirYPlus(x, y - 1, z, cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater));
            }
            else PlaceBloc(x, y - 1, z, ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z]]);
        }

        if (y != 255 && blocsMatrix[x, y + 1, z] != 0)
        {
            if (cubesMatrix[x, y + 1, z] != null)
            {
                cubesMatrix[x, y + 1, z].SetActive(true);
                cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().UpdateYMinus(HasAirYMinus(x, y + 1, z, cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().isWater));
            }
            else PlaceBloc(x, y + 1, z, ListOfAllBlocs.instance.blocs[blocsMatrix[x, y + 1, z]]);
        }

        Destroy(cubesMatrix[x, y, z]);
        CreateUpdateTimer(x, y, z);
    }

    public void PlaceBloc(int x, int y, int z, Bloc blocToPlace)
    {
        if (x == -1)
        {
            if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();

                GameObject liquid = default;
                if (neighbourChunk.cubesMatrix[15, y, z] != null)
                {
                    liquid = neighbourChunk.cubesMatrix[15, y, z];
                }

                if (!blocToPlace.prefab.GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[15, y, z] = GameObject.Instantiate(obstaclesDetectorPrefab);
                    neighbourChunk.cubesMatrix[15, y, z].transform.parent = neighbourChunk.gameObject.transform;
                    neighbourChunk.cubesMatrix[15, y, z].transform.localPosition = new Vector3(15, y, z);
                    bool isObstacles = neighbourChunk.cubesMatrix[15, y, z].GetComponent<ObstaclesDetector>().IsObstacles();
                    Destroy(neighbourChunk.cubesMatrix[15, y, z]);
                    neighbourChunk.cubesMatrix[15, y, z] = liquid;
                    if (isObstacles) return;
                }

                Destroy(liquid);
                neighbourChunk.blocsMatrix[15, y, z] = blocToPlace.index;
                neighbourChunk.cubesMatrix[15, y, z] = GameObject.Instantiate(blocToPlace.prefab);
                neighbourChunk.cubesMatrix[15, y, z].transform.parent = neighbourChunk.gameObject.transform;
                neighbourChunk.cubesMatrix[15, y, z].transform.localPosition = new Vector3(15, y, z);

                bool isBlocLiquid = neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().isWater;
                neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().UpdateFaces(neighbourChunk.HasAirYPlus(15, y, z, isBlocLiquid), neighbourChunk.HasAirYMinus(15, y, z, isBlocLiquid), neighbourChunk.HasAirXPlus(15, y, z, isBlocLiquid), neighbourChunk.HasAirXMinus(15, y, z, isBlocLiquid), neighbourChunk.HasAirZPlus(15, y, z, isBlocLiquid), neighbourChunk.HasAirZMinus(15, y, z, isBlocLiquid));
                neighbourChunk.UpdateFallingBloc(15, y, z);
                neighbourChunk.CheckForAroundBloc(15, y, z);
                neighbourChunk.CreateUpdateTimer(15, y, z);
                if(isBlocLiquid) neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().CreateWaterFlow(neighbourChunk, 15, y, z);
            }
            else return;
        }
        else if (x == 16)
        {
            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();

                GameObject liquid = default;
                if (neighbourChunk.cubesMatrix[0, y, z] != null)
                {
                    liquid = neighbourChunk.cubesMatrix[0, y, z];
                }

                if (!blocToPlace.prefab.GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[0, y, z] = GameObject.Instantiate(obstaclesDetectorPrefab);
                    neighbourChunk.cubesMatrix[0, y, z].transform.parent = neighbourChunk.gameObject.transform;
                    neighbourChunk.cubesMatrix[0, y, z].transform.localPosition = new Vector3(0, y, z);
                    bool isObstacles = neighbourChunk.cubesMatrix[0, y, z].GetComponent<ObstaclesDetector>().IsObstacles();
                    Destroy(neighbourChunk.cubesMatrix[0, y, z]);
                    neighbourChunk.cubesMatrix[0, y, z] = liquid;
                    if (isObstacles) return;
                }

                Destroy(liquid);
                neighbourChunk.blocsMatrix[0, y, z] = blocToPlace.index;
                neighbourChunk.cubesMatrix[0, y, z] = GameObject.Instantiate(blocToPlace.prefab);
                neighbourChunk.cubesMatrix[0, y, z].transform.parent = neighbourChunk.gameObject.transform;
                neighbourChunk.cubesMatrix[0, y, z].transform.localPosition = new Vector3(0, y, z);

                bool isBlocLiquid = neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().isWater;
                neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().UpdateFaces(neighbourChunk.HasAirYPlus(0, y, z, isBlocLiquid), neighbourChunk.HasAirYMinus(0, y, z, isBlocLiquid), neighbourChunk.HasAirXPlus(0, y, z, isBlocLiquid), neighbourChunk.HasAirXMinus(0, y, z, isBlocLiquid), neighbourChunk.HasAirZPlus(0, y, z, isBlocLiquid), neighbourChunk.HasAirZMinus(0, y, z, isBlocLiquid));
                neighbourChunk.UpdateFallingBloc(0, y, z);
                neighbourChunk.CheckForAroundBloc(0, y, z);
                neighbourChunk.CreateUpdateTimer(0, y, z);
                if (isBlocLiquid) neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().CreateWaterFlow(neighbourChunk, 0, y, z);
            }
            else return;
        }
        else if (z == -1)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();

                GameObject liquid = default;
                if (neighbourChunk.cubesMatrix[x, y, 15] != null)
                {
                    liquid = neighbourChunk.cubesMatrix[x, y, 15];
                }

                if (!blocToPlace.prefab.GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[x, y, 15] = GameObject.Instantiate(obstaclesDetectorPrefab);
                    neighbourChunk.cubesMatrix[x, y, 15].transform.parent = neighbourChunk.gameObject.transform;
                    neighbourChunk.cubesMatrix[x, y, 15].transform.localPosition = new Vector3(x, y, 15);
                    bool isObstacles = neighbourChunk.cubesMatrix[x, y, 15].GetComponent<ObstaclesDetector>().IsObstacles();
                    Destroy(neighbourChunk.cubesMatrix[x, y, 15]);
                    neighbourChunk.cubesMatrix[x, y, 15] = liquid;
                    if (isObstacles) return;
                }

                Destroy(liquid);
                neighbourChunk.blocsMatrix[x, y, 15] = blocToPlace.index;
                neighbourChunk.cubesMatrix[x, y, 15] = GameObject.Instantiate(blocToPlace.prefab);
                neighbourChunk.cubesMatrix[x, y, 15].transform.parent = neighbourChunk.gameObject.transform;
                neighbourChunk.cubesMatrix[x, y, 15].transform.localPosition = new Vector3(x, y, 15);

                bool isBlocLiquid = neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().isWater;
                neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().UpdateFaces(neighbourChunk.HasAirYPlus(x, y, 15, isBlocLiquid), neighbourChunk.HasAirYMinus(x, y, 15, isBlocLiquid), neighbourChunk.HasAirXPlus(x, y, 15, isBlocLiquid), neighbourChunk.HasAirXMinus(x, y, 15, isBlocLiquid), neighbourChunk.HasAirZPlus(x, y, 15, isBlocLiquid), neighbourChunk.HasAirZMinus(x, y, 15, isBlocLiquid));
                neighbourChunk.UpdateFallingBloc(x, y, 15);
                neighbourChunk.CheckForAroundBloc(x, y, 15);
                neighbourChunk.CreateUpdateTimer(x, y, 15);
                if (isBlocLiquid) neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().CreateWaterFlow(neighbourChunk, x, y, 15);
            }
            else return;
        }
        else if (z == 16)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();

                GameObject liquid = default;
                if (neighbourChunk.cubesMatrix[x, y, 0] != null)
                {
                    liquid = neighbourChunk.cubesMatrix[x, y, 0];
                }

                if (!blocToPlace.prefab.GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[x, y, 0] = GameObject.Instantiate(obstaclesDetectorPrefab);
                    neighbourChunk.cubesMatrix[x, y, 0].transform.parent = neighbourChunk.gameObject.transform;
                    neighbourChunk.cubesMatrix[x, y, 0].transform.localPosition = new Vector3(x, y, 0);
                    bool isObstacles = neighbourChunk.cubesMatrix[x, y, 0].GetComponent<ObstaclesDetector>().IsObstacles();
                    Destroy(neighbourChunk.cubesMatrix[x, y, 0]);
                    neighbourChunk.cubesMatrix[x, y, 0] = liquid;
                    if (isObstacles) return;
                }

                Destroy(liquid);
                neighbourChunk.blocsMatrix[x, y, 0] = blocToPlace.index;
                neighbourChunk.cubesMatrix[x, y, 0] = GameObject.Instantiate(blocToPlace.prefab);
                neighbourChunk.cubesMatrix[x, y, 0].transform.parent = neighbourChunk.gameObject.transform;
                neighbourChunk.cubesMatrix[x, y, 0].transform.localPosition = new Vector3(x, y, 0);

                bool isBlocLiquid = neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().isWater;
                neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().UpdateFaces(neighbourChunk.HasAirYPlus(x, y, 0, isBlocLiquid), neighbourChunk.HasAirYMinus(x, y, 0, isBlocLiquid), neighbourChunk.HasAirXPlus(x, y, 0, isBlocLiquid), neighbourChunk.HasAirXMinus(x, y, 0, isBlocLiquid), neighbourChunk.HasAirZPlus(x, y, 0, isBlocLiquid), neighbourChunk.HasAirZMinus(x, y, 0, isBlocLiquid));
                neighbourChunk.UpdateFallingBloc(x, y, 0);
                neighbourChunk.CheckForAroundBloc(x, y, 0);
                neighbourChunk.CreateUpdateTimer(x, y, 0);
                if (isBlocLiquid) neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().CreateWaterFlow(neighbourChunk, x, y, 0);
            }
            else return;
        }
        else if (y == 256) return;
        else if (y == -1) return;
        else
        {
            GameObject liquid = default;
            if (cubesMatrix[x, y, z] != null)
            {
                liquid = cubesMatrix[x, y, z];
            }

            if (!blocToPlace.prefab.GetComponent<FaceOptimiser>().isWater)
            {
                cubesMatrix[x, y, z] = GameObject.Instantiate(obstaclesDetectorPrefab);
                cubesMatrix[x, y, z].transform.parent = gameObject.transform;
                cubesMatrix[x, y, z].transform.localPosition = new Vector3(x, y, z);
                bool isObstacles = cubesMatrix[x, y, z].GetComponent<ObstaclesDetector>().IsObstacles();
                Destroy(cubesMatrix[x, y, z]);
                cubesMatrix[x, y, z] = liquid;
                if (isObstacles) return;
            }

            Destroy(liquid);
            blocsMatrix[x, y, z] = blocToPlace.index;
            cubesMatrix[x, y, z] = GameObject.Instantiate(blocToPlace.prefab);
            cubesMatrix[x, y, z].transform.parent = gameObject.transform;
            cubesMatrix[x, y, z].transform.localPosition = new Vector3(x, y, z);

            bool isBlocLiquid = cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater;
            cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().UpdateFaces(HasAirYPlus(x, y, z, isBlocLiquid), HasAirYMinus(x, y, z, isBlocLiquid), HasAirXPlus(x, y, z, isBlocLiquid), HasAirXMinus(x, y, z, isBlocLiquid), HasAirZPlus(x, y, z, isBlocLiquid), HasAirZMinus(x, y, z, isBlocLiquid));
            UpdateFallingBloc(x, y, z); 
            CheckForAroundBloc(x, y, z);
            CreateUpdateTimer(x, y, z);
            if (isBlocLiquid) cubesMatrix[x, y, z].GetComponent<Water>().CreateWaterFlow(this, x, y, z);
        }
    }

    void CheckForAroundBloc(int x, int y, int z)
    {
        if (x != 0 && blocsMatrix[x - 1, y, z] != 0 && cubesMatrix[x - 1, y, z] != null)
        {
            bool mustBeDestroy = !HasAirAround(x - 1, y, z, false);
            if (mustBeDestroy) Destroy(cubesMatrix[x - 1, y, z]);
            else cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().UpdateXPlus(HasAirXPlus(x - 1, y, z, cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().isWater));
        }
        else if (x == 0)
        {
            if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
                if(neighbourChunk.blocsMatrix[15, y, z] != 0 && neighbourChunk.cubesMatrix[15, y, z] != null)
                {
                    bool mustBeDestroy = !neighbourChunk.HasAirAround(15, y, z, false);
                    if (mustBeDestroy) Destroy(neighbourChunk.cubesMatrix[15, y, z]);
                    else neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().UpdateXPlus(neighbourChunk.HasAirXPlus(15, y, z, neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }

        if (x != 15 && blocsMatrix[x + 1, y, z] != 0 && cubesMatrix[x + 1, y, z] != null)
        {
            bool mustBeDestroy = !HasAirAround(x + 1, y, z, false);
            if (mustBeDestroy) Destroy(cubesMatrix[x + 1, y, z]);
            else cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().UpdateXMinus(HasAirXMinus(x + 1, y, z, cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().isWater));
        }
        else if (x == 15)
        {
            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[0, y, z] != 0 && neighbourChunk.cubesMatrix[0, y, z] != null)
                {
                    bool mustBeDestroy = !neighbourChunk.HasAirAround(0, y, z, false);
                    if (mustBeDestroy) Destroy(neighbourChunk.cubesMatrix[0, y, z]);
                    else neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().UpdateXMinus(neighbourChunk.HasAirXMinus(0, y, z, neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }

        if (z != 0 && blocsMatrix[x, y, z - 1] != 0 && cubesMatrix[x, y, z - 1] != null)
        {
            bool mustBeDestroy = !HasAirAround(x, y, z - 1, false);
            if (mustBeDestroy) Destroy(cubesMatrix[x, y, z - 1]);
            else cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().UpdateZPlus(HasAirZPlus(x, y, z - 1, cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().isWater));
        }
        else if (z == 0)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 15] != 0 && neighbourChunk.cubesMatrix[x, y, 15] != null)
                {
                    bool mustBeDestroy = !neighbourChunk.HasAirAround(x, y, 15, false);
                    if (mustBeDestroy) Destroy(neighbourChunk.cubesMatrix[x, y, 15]);
                    else neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().UpdateZPlus(neighbourChunk.HasAirZPlus(x, y, 15, neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }

        if (z != 15 && blocsMatrix[x, y, z + 1] != 0 && cubesMatrix[x, y, z + 1] != null)
        {
            bool mustBeDestroy = !HasAirAround(x, y, z + 1, false);
            if (mustBeDestroy) Destroy(cubesMatrix[x, y, z + 1]);
            else cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().UpdateZMinus(HasAirZMinus(x, y, z + 1, cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().isWater));
        }
        else if (z == 15)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 0] != 0 && neighbourChunk.cubesMatrix[x, y, 0] != null)
                {
                    bool mustBeDestroy = !neighbourChunk.HasAirAround(x, y, 0, false);
                    if (mustBeDestroy) Destroy(neighbourChunk.cubesMatrix[x, y, 0]);
                    else neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().UpdateZMinus(neighbourChunk.HasAirZMinus(x, y, 0, neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }

        if (y != 0 && blocsMatrix[x, y - 1, z] != 0 && cubesMatrix[x, y - 1, z] != null)
        {
            bool mustBeDestroy = !HasAirAround(x, y - 1, z, false);
            if (mustBeDestroy) Destroy(cubesMatrix[x, y - 1, z]);
            else cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().UpdateYPlus(HasAirYPlus(x, y - 1, z, cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater));
        }

        if (y != 255 && blocsMatrix[x, y + 1, z] != 0 && cubesMatrix[x, y + 1, z] != null)
        {
            bool mustBeDestroy = !HasAirAround(x, y + 1, z, false);
            if (mustBeDestroy) Destroy(cubesMatrix[x, y + 1, z]);
            else cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().UpdateYMinus(HasAirYMinus(x, y + 1, z, cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().isWater));
        }
    }

    public bool HasAirAround(int x, int y, int z, bool isCreatingChunk)
    {
        if (ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) return true;

        if (x != 0 && blocsMatrix[x - 1, y, z] == 0)
        {
            return true;
        }
        else if (x != 0 && ((cubesMatrix[x - 1, y, z] != null || isCreatingChunk) && ListOfAllBlocs.instance.blocs[blocsMatrix[x - 1, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        }
        else if (x == 0 && WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null && WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>().blocsMatrix[15, y, z] == 0)
        {
            return true;
        }
        else if (x == 0 && WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null
            && ((WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>().cubesMatrix[15, y, z] != null || isCreatingChunk)
            && ListOfAllBlocs.instance.blocs[WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>().blocsMatrix[15, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        } //SUIVANT
        else if (x != 15 && blocsMatrix[x + 1, y, z] == 0)
        {
            return true;
        }
        else if (x != 15 && ((cubesMatrix[x + 1, y, z] != null || isCreatingChunk) && ListOfAllBlocs.instance.blocs[blocsMatrix[x + 1, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        }
        else if (x == 15 && WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null && WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().blocsMatrix[0, y, z] == 0)
        {
            return true;
        }
        else if (x == 15 && WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null
            && ((WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().cubesMatrix[0, y, z] != null || isCreatingChunk)
            && ListOfAllBlocs.instance.blocs[WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>().blocsMatrix[0, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        } //SUIVANT
        else if (z != 0 && blocsMatrix[x, y, z - 1] == 0)
        {
            return true;
        }
        else if (z != 0 && ((cubesMatrix[x, y, z - 1] != null || isCreatingChunk) && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z - 1]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        }
        else if (z == 0 && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().blocsMatrix[x, y, 15] == 0)
        {
            return true;
        }
        else if (z == 0 && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null
            && ((WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().cubesMatrix[x, y, 15] != null || isCreatingChunk)
            && ListOfAllBlocs.instance.blocs[WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>().blocsMatrix[x, y, 15]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        } //SUIVANT
        else if (z != 15 && blocsMatrix[x, y, z + 1] == 0)
        {
            return true;
        }
        else if (z != 15 && ((cubesMatrix[x, y, z + 1] != null || isCreatingChunk) && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z + 1]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        }
        else if (z == 15 && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().blocsMatrix[x, y, 0] == 0)
        {
            return true;
        }
        else if (z == 15 && WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null
            && ((WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().cubesMatrix[x, y, 0] != null || isCreatingChunk)
            && ListOfAllBlocs.instance.blocs[WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>().blocsMatrix[x, y, 0]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        } //SUIVANT
        else if (y != 0 && blocsMatrix[x, y - 1, z] == 0)
        {
            return true;
        }
        else if (y != 0 && ((cubesMatrix[x, y - 1, z] != null || isCreatingChunk) && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        } //SUIVANT
        else if (y != 255 && blocsMatrix[x, y + 1, z] == 0)
        {
            return true;
        }
        else if (y != 0 && ((cubesMatrix[x, y + 1, z] != null || isCreatingChunk) && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y + 1, z]].prefab.GetComponent<FaceOptimiser>().isTransparent))
        {
            return true;
        } //FIN
        else
        {
            return false;
        }
    }

    public void CreateUpdateTimer(int x, int y, int z)
    {
        GameObject newUpdateTimer = GameObject.Instantiate(updateTimer);
        newUpdateTimer.transform.parent = this.gameObject.transform;
        newUpdateTimer.transform.localPosition = Vector3.zero;
        newUpdateTimer.GetComponent<UpdateTimer>().CreateUpdateTimer(this, x, y, z);
    }

    void UpdateFallingBloc(int x, int y, int z)
    {
        if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
        {
            if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().fallingBloc != null)
            {
                if (y != 0)
                {
                    if (blocsMatrix[x, y - 1, z] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                    {
                        Vector3 cubePos = cubesMatrix[x, y, z].transform.position;
                        GameObject fallingBloc = cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().fallingBloc;
                        Transform cubeParent = cubesMatrix[x, y, z].transform.parent;
                        DestroyBloc(x, y, z);
                        GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                        fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                    }
                }
            }
        }
    }

    public void UpdateAroundsFallingBlocs(int x, int y, int z)
    {
        //Debug.Log("Update chunk " + chunkPositionX.ToString() + " - " + chunkPositionZ.ToString() + "; bloc " + x.ToString() + ", " + y.ToString() + ", " + z.ToString());

        //Update X+
        if (x != 15)
        {
            if(blocsMatrix[x + 1, y, z] != 0 && cubesMatrix[x + 1, y, z] != null)
            {
                if(cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (blocsMatrix[x + 1, y - 1, z] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x + 1, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = cubesMatrix[x + 1, y, z].transform.position;
                            GameObject fallingBloc = cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = cubesMatrix[x + 1, y, z].transform.parent;
                            DestroyBloc(x + 1, y, z);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                            CreateUpdateTimer(x + 1, y, z);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[0, y, z] != 0 && neighbourChunk.cubesMatrix[0, y, z] != null)
            {
                if (neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (neighbourChunk.blocsMatrix[0, y - 1, z] == 0 || ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[0, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = neighbourChunk.cubesMatrix[0, y, z].transform.position;
                            GameObject fallingBloc = neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = neighbourChunk.cubesMatrix[0, y, z].transform.parent;
                            neighbourChunk.DestroyBloc(0, y, z);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(neighbourChunk);
                            neighbourChunk.CreateUpdateTimer(0, y, z);
                        }
                    }
                }
            }
        }

        //Update X-
        if (x != 0)
        {
            if (blocsMatrix[x - 1, y, z] != 0 && cubesMatrix[x - 1, y, z] != null)
            {
                if (cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (blocsMatrix[x - 1, y - 1, z] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x - 1, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = cubesMatrix[x - 1, y, z].transform.position;
                            GameObject fallingBloc = cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = cubesMatrix[x - 1, y, z].transform.parent;
                            DestroyBloc(x - 1, y, z);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                            CreateUpdateTimer(x - 1, y, z);
                        }
                    }
                }
            }
        }
        else if(WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[15, y, z] != 0 && neighbourChunk.cubesMatrix[15, y, z] != null)
            {
                if (neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (neighbourChunk.blocsMatrix[15, y - 1, z] == 0 || ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[15, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = neighbourChunk.cubesMatrix[15, y, z].transform.position;
                            GameObject fallingBloc = neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = neighbourChunk.cubesMatrix[15, y, z].transform.parent;
                            neighbourChunk.DestroyBloc(15, y, z);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(neighbourChunk);
                            neighbourChunk.CreateUpdateTimer(15, y, z);
                        }
                    }
                }
            }
        }

        //Update Z+
        if (z != 15)
        {
            if (blocsMatrix[x, y, z + 1] != 0 && cubesMatrix[x, y, z + 1] != null)
            {
                if (cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (blocsMatrix[x, y - 1, z + 1] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z + 1]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = cubesMatrix[x, y, z + 1].transform.position;
                            GameObject fallingBloc = cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = cubesMatrix[x, y, z + 1].transform.parent;
                            DestroyBloc(x, y, z + 1);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                            CreateUpdateTimer(x, y, z + 1);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[x, y, 0] != 0 && neighbourChunk.cubesMatrix[x, y, 0] != null)
            {
                if (neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (neighbourChunk.blocsMatrix[x, y - 1, 0] == 0 || ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y - 1, 0]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = neighbourChunk.cubesMatrix[x, y, 0].transform.position;
                            GameObject fallingBloc = neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = neighbourChunk.cubesMatrix[x, y, 0].transform.parent;
                            neighbourChunk.DestroyBloc(x, y, 0);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(neighbourChunk);
                            neighbourChunk.CreateUpdateTimer(x, y, 0);
                        }
                    }
                }
            }
        }

        //Update Z-
        if (z != 0)
        {
            if (blocsMatrix[x, y, z - 1] != 0 && cubesMatrix[x, y, z - 1] != null)
            {
                if (cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (blocsMatrix[x, y - 1, z - 1] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z - 1]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = cubesMatrix[x, y, z - 1].transform.position;
                            GameObject fallingBloc = cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = cubesMatrix[x, y, z - 1].transform.parent;
                            DestroyBloc(x, y, z - 1);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                            CreateUpdateTimer(x, y, z - 1);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[x, y, 15] != 0 && neighbourChunk.cubesMatrix[x, y, 15] != null)
            {
                if (neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y != 0)
                    {
                        if (neighbourChunk.blocsMatrix[x, y - 1, 15] == 0 || ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y - 1, 15]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = neighbourChunk.cubesMatrix[x, y, 15].transform.position;
                            GameObject fallingBloc = neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = neighbourChunk.cubesMatrix[x, y, 15].transform.parent;
                            neighbourChunk.DestroyBloc(x, y, 15);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(neighbourChunk);
                            neighbourChunk.CreateUpdateTimer(x, y, 15);
                        }
                    }
                }
            }
        }

        //Update Y+
        if (y != 255)
        {
            if (blocsMatrix[x, y + 1, z] != 0 && cubesMatrix[x, y + 1, z] != null)
            {
                if (cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (blocsMatrix[x, y, z] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                    {
                        Vector3 cubePos = cubesMatrix[x, y + 1, z].transform.position;
                        GameObject fallingBloc = cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().fallingBloc;
                        Transform cubeParent = cubesMatrix[x, y + 1, z].transform.parent;
                        DestroyBloc(x, y + 1, z);
                        GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                        fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                        CreateUpdateTimer(x, y + 1, z);
                    }
                }
            }
        }

        //Update Y-
        if (y != 0)
        {
            if (blocsMatrix[x, y - 1, z] != 0 && cubesMatrix[x, y - 1, z] != null)
            {
                if (cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().fallingBloc != null)
                {
                    if (y > 1)
                    {
                        if (blocsMatrix[x, y - 2, z] == 0 || ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 2, z]].prefab.GetComponent<FaceOptimiser>().isWater)
                        {
                            Vector3 cubePos = cubesMatrix[x, y - 1, z].transform.position;
                            GameObject fallingBloc = cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().fallingBloc;
                            Transform cubeParent = cubesMatrix[x, y - 1, z].transform.parent;
                            DestroyBloc(x, y - 1, z);
                            GameObject fallBloc = GameObject.Instantiate(fallingBloc, cubePos, Quaternion.identity, cubeParent);
                            fallBloc.GetComponent<FallingBloc>().StartFalling(this);
                            CreateUpdateTimer(x, y - 1, z);
                        }
                    }
                }
            }
        }
    }

    public void UpdateAroundsWaterBlocs(int x, int y, int z)
    {
        bool mustRecreateWaterFlowHere = false;
        bool hasWaterSourceAround = false;

        //Debug.Log("Update chunk " + chunkPositionX.ToString() + " - " + chunkPositionZ.ToString() + "; bloc " + x.ToString() + ", " + y.ToString() + ", " + z.ToString());

        //Update X+
        if (x != 15)
        {
            if (blocsMatrix[x + 1, y, z] != 0 && cubesMatrix[x + 1, y, z] != null)
            {
                if (cubesMatrix[x + 1, y, z].GetComponent<FaceOptimiser>().isWater)
                {
                    cubesMatrix[x + 1, y, z].GetComponent<Water>().RecalculateFlow();
                    if (cubesMatrix[x + 1, y, z].GetComponent<Water>().hasChanged) CreateUpdateTimer(x + 1, y, z);
                    if (cubesMatrix[x + 1, y, z].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (cubesMatrix[x + 1, y, z].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (blocsMatrix[x + 1, y, z] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            PlaceBloc(x + 1, y, z, ListOfAllBlocs.instance.blocs[10]);
                            cubesMatrix[x + 1, y, z].GetComponent<Water>().CreateWaterFlow(this, x + 1, y, z);
                            CreateUpdateTimer(x + 1, y, z);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[0, y, z] != 0 && neighbourChunk.cubesMatrix[0, y, z] != null)
            {
                if (neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().RecalculateFlow();
                    if (neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().hasChanged) neighbourChunk.CreateUpdateTimer(0, y, z);
                    if (neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (neighbourChunk.blocsMatrix[0, y, z] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            neighbourChunk.PlaceBloc(0, y, z, ListOfAllBlocs.instance.blocs[10]);
                            neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().CreateWaterFlow(neighbourChunk, 0, y, z);
                            neighbourChunk.CreateUpdateTimer(0, y, z);
                        }
                    }
                }
            }
        }

        //Update X-
        if (x != 0)
        {
            if (blocsMatrix[x - 1, y, z] != 0 && cubesMatrix[x - 1, y, z] != null)
            {
                if (cubesMatrix[x - 1, y, z].GetComponent<FaceOptimiser>().isWater)
                {
                    cubesMatrix[x - 1, y, z].GetComponent<Water>().RecalculateFlow();
                    if (cubesMatrix[x - 1, y, z].GetComponent<Water>().hasChanged) CreateUpdateTimer(x - 1, y, z);
                    if (cubesMatrix[x - 1, y, z].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (cubesMatrix[x - 1, y, z].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (blocsMatrix[x - 1, y, z] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            PlaceBloc(x - 1, y, z, ListOfAllBlocs.instance.blocs[10]);
                            cubesMatrix[x - 1, y, z].GetComponent<Water>().CreateWaterFlow(this, x - 1, y, z);
                            CreateUpdateTimer(x - 1, y, z);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[15, y, z] != 0 && neighbourChunk.cubesMatrix[15, y, z] != null)
            {
                if (neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().RecalculateFlow();
                    if (neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().hasChanged) neighbourChunk.CreateUpdateTimer(15, y, z);
                    if (neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (neighbourChunk.blocsMatrix[15, y, z] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            neighbourChunk.PlaceBloc(15, y, z, ListOfAllBlocs.instance.blocs[10]);
                            neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().CreateWaterFlow(neighbourChunk, 15, y, z);
                            neighbourChunk.CreateUpdateTimer(15, y, z);
                        }
                    }
                }
            }
        }

        //Update Z+
        if (z != 15)
        {
            if (blocsMatrix[x, y, z + 1] != 0 && cubesMatrix[x, y, z + 1] != null)
            {
                if (cubesMatrix[x, y, z + 1].GetComponent<FaceOptimiser>().isWater)
                {
                    cubesMatrix[x, y, z + 1].GetComponent<Water>().RecalculateFlow();
                    if (cubesMatrix[x, y, z + 1].GetComponent<Water>().hasChanged) CreateUpdateTimer(x, y, z + 1);
                    if (cubesMatrix[x, y, z + 1].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (cubesMatrix[x, y, z + 1].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (blocsMatrix[x, y, z + 1] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            PlaceBloc(x, y, z + 1, ListOfAllBlocs.instance.blocs[10]);
                            cubesMatrix[x, y, z + 1].GetComponent<Water>().CreateWaterFlow(this, x, y, z + 1);
                            CreateUpdateTimer(x, y, z + 1);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[x, y, 0] != 0 && neighbourChunk.cubesMatrix[x, y, 0] != null)
            {
                if (neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().RecalculateFlow();
                    if (neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().hasChanged) neighbourChunk.CreateUpdateTimer(x, y, 0);
                    if (neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (neighbourChunk.blocsMatrix[x, y, 0] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            neighbourChunk.PlaceBloc(x, y, 0, ListOfAllBlocs.instance.blocs[10]);
                            neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().CreateWaterFlow(neighbourChunk, x, y, 0);
                            neighbourChunk.CreateUpdateTimer(x, y, 0);
                        }
                    }
                }
            }
        }

        //Update Z-
        if (z != 0)
        {
            if (blocsMatrix[x, y, z - 1] != 0 && cubesMatrix[x, y, z - 1] != null)
            {
                if (cubesMatrix[x, y, z - 1].GetComponent<FaceOptimiser>().isWater)
                {
                    cubesMatrix[x, y, z - 1].GetComponent<Water>().RecalculateFlow();
                    if (cubesMatrix[x, y, z - 1].GetComponent<Water>().hasChanged) CreateUpdateTimer(x, y, z - 1);
                    if (cubesMatrix[x, y, z - 1].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (cubesMatrix[x, y, z - 1].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (blocsMatrix[x, y, z - 1] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            PlaceBloc(x, y, z - 1, ListOfAllBlocs.instance.blocs[10]);
                            cubesMatrix[x, y, z - 1].GetComponent<Water>().CreateWaterFlow(this, x, y, z - 1);
                            CreateUpdateTimer(x, y, z - 1);
                        }
                    }
                }
            }
        }
        else if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
        {
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[x, y, 15] != 0 && neighbourChunk.cubesMatrix[x, y, 15] != null)
            {
                if (neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().isWater)
                {
                    neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().RecalculateFlow();
                    if (neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().hasChanged) neighbourChunk.CreateUpdateTimer(x, y, 15);
                    if (neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
            else if (neighbourChunk.blocsMatrix[x, y, 15] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        if (cubesMatrix[x, y, z].GetComponent<Water>().flowValue == 9 || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
                        {
                            neighbourChunk.PlaceBloc(x, y, 15, ListOfAllBlocs.instance.blocs[10]);
                            neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().CreateWaterFlow(neighbourChunk, x, y, 15);
                            neighbourChunk.CreateUpdateTimer(x, y, 15);
                        }
                    }
                }
            }
        }

        //Update Y+
        if (y != 255)
        {
            if (blocsMatrix[x, y + 1, z] != 0 && cubesMatrix[x, y + 1, z] != null)
            {
                if (cubesMatrix[x, y + 1, z].GetComponent<FaceOptimiser>().isWater)
                {
                    if (cubesMatrix[x, y + 1, z].GetComponent<Water>().flowValue > 1) mustRecreateWaterFlowHere = true;
                    if (cubesMatrix[x, y + 1, z].GetComponent<Water>().flowValue == 9) hasWaterSourceAround = true;
                }
            }
        }

        //Update Y-
        if (y != 0)
        {
            if (blocsMatrix[x, y - 1, z] != 0 && cubesMatrix[x, y - 1, z] != null)
            {
                if (cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater)
                {
                    cubesMatrix[x, y - 1, z].GetComponent<Water>().RecalculateFlow();
                    if (cubesMatrix[x, y - 1, z].GetComponent<Water>().hasChanged) CreateUpdateTimer(x, y - 1, z);
                }
            }
            else if (blocsMatrix[x, y - 1, z] == 0)
            {
                if (blocsMatrix[x, y, z] != 0 && cubesMatrix[x, y, z] != null)
                {
                    if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        PlaceBloc(x, y - 1, z, ListOfAllBlocs.instance.blocs[10]);
                        cubesMatrix[x, y - 1, z].GetComponent<Water>().CreateWaterFlow(this, x, y - 1, z);
                        CreateUpdateTimer(x, y - 1, z);
                    }
                }
            }
        }

        if (blocsMatrix[x, y, z] != 0) mustRecreateWaterFlowHere = false;

        if (mustRecreateWaterFlowHere)
        {
            if (hasWaterSourceAround || (cubesMatrix[x, y - 1, z] != null && !cubesMatrix[x, y - 1, z].GetComponent<FaceOptimiser>().isWater))
            {
                PlaceBloc(x, y, z, ListOfAllBlocs.instance.blocs[10]);
                cubesMatrix[x, y, z].GetComponent<Water>().CreateWaterFlow(this, x, y, z);
                CreateUpdateTimer(x, y, z);
            }
        }
    }

    void FixedUpdate()
    {
        if(chunkPositionX < WorldChunks.instance.playerXPos - WorldChunks.instance.renderDistance || chunkPositionX > WorldChunks.instance.playerXPos + WorldChunks.instance.renderDistance || chunkPositionZ < WorldChunks.instance.playerYPos - WorldChunks.instance.renderDistance || chunkPositionZ > WorldChunks.instance.playerYPos + WorldChunks.instance.renderDistance)
        {
            this.gameObject.SetActive(false);
        }
    }





    private bool HasAirYPlus(int x, int y, int z, bool isLiquid)
    {
        if (y != 255)
        {
            if (blocsMatrix[x, y + 1, z] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y + 1, z]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[blocsMatrix[x, y + 1, z]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
        else return false;
    }

    private bool HasAirYMinus(int x, int y, int z, bool isLiquid)
    {
        if (y != 0)
        {
            if (blocsMatrix[x, y - 1, z] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[blocsMatrix[x, y - 1, z]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
        else return false;
    }

    private bool HasAirXPlus(int x, int y, int z, bool isLiquid)
    {
        if(x != 15)
        {
            if (blocsMatrix[x + 1, y, z] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[blocsMatrix[x + 1, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[blocsMatrix[x + 1, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] == null) return false;
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[0, y, z] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[0, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[0, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
    }

    private bool HasAirXMinus(int x, int y, int z, bool isLiquid)
    {
        if (x != 0)
        {
            if (blocsMatrix[x - 1, y, z] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[blocsMatrix[x - 1, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[blocsMatrix[x - 1, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] == null) return false;
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[15, y, z] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[15, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[15, y, z]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
    }

    private bool HasAirZPlus(int x, int y, int z, bool isLiquid)
    {
        if (z != 15)
        {
            if (blocsMatrix[x, y, z + 1] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z + 1]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z + 1]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] == null) return false;
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[x, y, 0] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 0]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 0]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
    }

    private bool HasAirZMinus(int x, int y, int z, bool isLiquid)
    {
        if (z != 0)
        {
            if (blocsMatrix[x, y, z - 1] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z - 1]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z - 1]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] == null) return false;
            ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
            if (neighbourChunk.blocsMatrix[x, y, 15] == 0) return true;
            if (isLiquid && ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 15]].prefab.GetComponent<FaceOptimiser>().isWater) return false;
            if (ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 15]].prefab.GetComponent<FaceOptimiser>().isTransparent) return true;
            return false;
        }
    }

    public int[] ReturnAdjacentFlowValues(int x, int y, int z)
    {
        int[] flowValues = { -1, -1, -1, -1, -1 } ;

        if (x != 15) // First Value -> X+
        {
            if (blocsMatrix[x + 1, y, z] == 0) flowValues[0] = -1;
            else if (!ListOfAllBlocs.instance.blocs[blocsMatrix[x + 1, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[0] = -1;
            else flowValues[0] = cubesMatrix[x + 1, y, z].GetComponent<Water>().flowValue;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] == null) flowValues[0] = 8;
            else
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[0, y, z] == 0) flowValues[0] = -1;
                else if (!ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[0, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[0] = -1;
                else flowValues[0] = neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().flowValue;
            }
        }

        if (x != 0) // Second Value -> X-
        {
            if (blocsMatrix[x - 1, y, z] == 0) flowValues[1] = -1;
            else if (!ListOfAllBlocs.instance.blocs[blocsMatrix[x - 1, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[1] = -1;
            else flowValues[1] = cubesMatrix[x - 1, y, z].GetComponent<Water>().flowValue;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] == null) flowValues[1] = 8;
            else
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[15, y, z] == 0) flowValues[1] = -1;
                else if (!ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[15, y, z]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[1] = -1;
                else flowValues[1] = neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().flowValue;
            }
        }

        if (z != 15) // Third Value -> Z+
        {
            if (blocsMatrix[x, y, z + 1] == 0) flowValues[2] = -1;
            else if (!ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z + 1]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[2] = -1;
            else flowValues[2] = cubesMatrix[x, y, z + 1].GetComponent<Water>().flowValue;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] == null) flowValues[2] = 8;
            else
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 0] == 0) flowValues[2] = -1;
                else if (!ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 0]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[2] = -1;
                else flowValues[2] = neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().flowValue;
            }
        }

        if (z != 0) // Fourth Value -> Z-
        {
            if (blocsMatrix[x, y, z - 1] == 0) flowValues[3] = -1;
            else if (!ListOfAllBlocs.instance.blocs[blocsMatrix[x, y, z - 1]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[3] = -1;
            else flowValues[3] = cubesMatrix[x, y, z - 1].GetComponent<Water>().flowValue;
        }
        else
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] == null) flowValues[3] = 8;
            else
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 15] == 0) flowValues[3] = -1;
                else if (!ListOfAllBlocs.instance.blocs[neighbourChunk.blocsMatrix[x, y, 15]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[3] = -1;
                else flowValues[3] = neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().flowValue;
            }
        }

        if (y != 255) // Fifth Value -> Y+
        {
            if (blocsMatrix[x, y + 1, z] == 0) flowValues[4] = -1;
            else if (!ListOfAllBlocs.instance.blocs[blocsMatrix[x, y + 1, z]].prefab.GetComponent<FaceOptimiser>().isWater) flowValues[4] = -1;
            else flowValues[4] = cubesMatrix[x, y + 1, z].GetComponent<Water>().flowValue;
        }

        return flowValues;
    }

    public float[] GetYValues(int x, int y, int z)
    {
        float[] yValues = { 1f, 1f, 1f, 1f };

        if(x == -1)
        {
            if(WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX - 1, chunkPositionZ].GetComponent<ChunkManager>();
                if(neighbourChunk.blocsMatrix[15, y, z] != 0)
                {
                    if(neighbourChunk.cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        yValues = neighbourChunk.cubesMatrix[15, y, z].GetComponent<Water>().yValues;
                    }
                }
            }
        }
        else if (x == 16)
        {
            if (WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX + 1, chunkPositionZ].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[0, y, z] != 0)
                {
                    if (neighbourChunk.cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().isWater)
                    {
                        yValues = neighbourChunk.cubesMatrix[0, y, z].GetComponent<Water>().yValues;
                    }
                }
            }
        }
        else if (z == -1)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ - 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 15] != 0)
                {
                    if (neighbourChunk.cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().isWater)
                    {
                        yValues = neighbourChunk.cubesMatrix[x, y, 15].GetComponent<Water>().yValues;
                    }
                }
            }
        }
        else if (z == 16)
        {
            if (WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1] != null)
            {
                ChunkManager neighbourChunk = WorldChunks.instance.chunks[chunkPositionX, chunkPositionZ + 1].GetComponent<ChunkManager>();
                if (neighbourChunk.blocsMatrix[x, y, 0] != 0)
                {
                    if (neighbourChunk.cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().isWater)
                    {
                        yValues = neighbourChunk.cubesMatrix[x, y, 0].GetComponent<Water>().yValues;
                    }
                }
            }
        }
        else
        {
            if (blocsMatrix[x, y, z] != 0)
            {
                if (cubesMatrix[x, y, z].GetComponent<FaceOptimiser>().isWater)
                {
                    yValues = cubesMatrix[x, y, z].GetComponent<Water>().yValues;
                }
            }
        }

        return yValues;
    }

    void SetTreeInfluence(int x, int z)
    {
        x += 4;
        z += 4;

        treesInfluences[x - 2, z - 1] *= 0f;
        treesInfluences[x - 2, z] *= 0f;
        treesInfluences[x - 2, z + 1] *= 0f;
        treesInfluences[x - 1, z - 2] *= 0f;
        treesInfluences[x - 1, z - 1] *= 0f;
        treesInfluences[x - 1, z] *= 0f;
        treesInfluences[x - 1, z + 1] *= 0f;
        treesInfluences[x - 1, z + 2] *= 0f;
        treesInfluences[x, z - 2] *= 0f;
        treesInfluences[x, z - 1] *= 0f;
        treesInfluences[x, z] = 0f;
        treesInfluences[x, z + 1] *= 0f;
        treesInfluences[x, z + 2] *= 0f;
        treesInfluences[x + 1, z - 2] *= 0f;
        treesInfluences[x + 1, z - 1] *= 0f;
        treesInfluences[x + 1, z] *= 0f;
        treesInfluences[x + 1, z + 1] *= 0f;
        treesInfluences[x + 1, z + 2] *= 0f;
        treesInfluences[x + 2, z - 1] *= 0f;
        treesInfluences[x + 2, z] *= 0f;
        treesInfluences[x + 2, z + 1] *= 0f;

        treesInfluences[x - 3, z - 1] *= 0.5f;
        treesInfluences[x - 3, z] *= 0.5f;
        treesInfluences[x - 3, z + 1] *= 0.5f;
        treesInfluences[x - 2, z - 2] *= 0.5f;
        treesInfluences[x - 2, z + 2] *= 0.5f;
        treesInfluences[x - 1, z - 3] *= 0.5f;
        treesInfluences[x - 1, z + 3] *= 0.5f;
        treesInfluences[x, z - 3] *= 0.5f;
        treesInfluences[x, z + 3] *= 0.5f;
        treesInfluences[x + 1, z - 3] *= 0.5f;
        treesInfluences[x + 1, z + 3] *= 0.5f;
        treesInfluences[x + 2, z - 2] *= 0.5f;
        treesInfluences[x + 2, z + 2] *= 0.5f;
        treesInfluences[x + 3, z - 1] *= 0.5f;
        treesInfluences[x + 3, z] *= 0.5f;
        treesInfluences[x + 3, z + 1] *= 0.5f;

        treesInfluences[x - 4, z - 1] *= 0.8f;
        treesInfluences[x - 4, z] *= 0.8f;
        treesInfluences[x - 4, z + 1] *= 0.8f;
        treesInfluences[x - 3, z - 3] *= 0.8f;
        treesInfluences[x - 3, z - 2] *= 0.8f;
        treesInfluences[x - 3, z + 2] *= 0.8f;
        treesInfluences[x - 3, z + 3] *= 0.8f;
        treesInfluences[x - 2, z - 3] *= 0.8f;
        treesInfluences[x - 2, z + 3] *= 0.8f;
        treesInfluences[x - 1, z - 4] *= 0.8f;
        treesInfluences[x - 1, z + 4] *= 0.8f;
        treesInfluences[x, z - 4] *= 0.8f;
        treesInfluences[x, z + 4] *= 0.8f;
        treesInfluences[x + 1, z - 4] *= 0.8f;
        treesInfluences[x + 1, z + 4] *= 0.8f;
        treesInfluences[x + 2, z - 3] *= 0.8f;
        treesInfluences[x + 2, z + 3] *= 0.8f;
        treesInfluences[x + 3, z - 3] *= 0.8f;
        treesInfluences[x + 3, z - 2] *= 0.8f;
        treesInfluences[x + 3, z + 2] *= 0.8f;
        treesInfluences[x + 3, z + 3] *= 0.8f;
        treesInfluences[x + 4, z - 1] *= 0.8f;
        treesInfluences[x + 4, z] *= 0.8f;
        treesInfluences[x + 4, z + 1] *= 0.8f;
    }

    void PlaceStructure(int x, int y, int z, Structure structureToPlace)
    {
        for(int i = 0; i < structureToPlace.prefab.transform.childCount; i++)
        {
            int posX = x + (int)(structureToPlace.prefab.transform.GetChild(i).localPosition.x);
            int posY = y + (int)(structureToPlace.prefab.transform.GetChild(i).localPosition.y);
            int posZ = z + (int)(structureToPlace.prefab.transform.GetChild(i).localPosition.z);
            int posChunkX = chunkPositionX;
            int posChunkZ = chunkPositionZ;

            if(posX > 15)
            {
                posChunkX += 1;
                posX -= 16;
            }
            else if (posX < 0)
            {
                posChunkX -= 1;
                posX += 16;
            }
            if (posZ > 15)
            {
                posChunkZ += 1;
                posZ -= 16;
            }
            else if (posZ < 0)
            {
                posChunkZ -= 1;
                posZ += 16;
            }

            if(WorldChunks.instance.chunks[posChunkX, posChunkZ] != null)
            {
                ChunkManager chunkToPlaceBloc = WorldChunks.instance.chunks[posChunkX, posChunkZ].GetComponent<ChunkManager>();
                if(chunkToPlaceBloc.blocsMatrix[posX, posY, posZ] == 0)
                {
                    if (chunkToPlaceBloc.alreadyInitialized) chunkToPlaceBloc.PlaceBloc(posX, posY, posZ, ListOfAllBlocs.instance.blocs[structureToPlace.prefab.transform.GetChild(i).gameObject.GetComponent<FaceOptimiser>().blocIndex]);
                    else chunkToPlaceBloc.blocsMatrix[posX, posY, posZ] = structureToPlace.prefab.transform.GetChild(i).gameObject.GetComponent<FaceOptimiser>().blocIndex;
                }
            }
            else
            {
                if (!WorldChunks.instance.preGenMatrices.ContainsKey(new Vector2Int(posChunkX, posChunkZ))) WorldChunks.instance.preGenMatrices.Add(new Vector2Int(posChunkX, posChunkZ), new int[16, 256, 16]);
                WorldChunks.instance.preGenMatrices[new Vector2Int(posChunkX, posChunkZ)][posX, posY, posZ] = structureToPlace.prefab.transform.GetChild(i).gameObject.GetComponent<FaceOptimiser>().blocIndex;
            }
        }
    }

    public void UpdateXPlusEdge()
    {
        for(int y = 0; y < 256; y++)
        {
            for(int z = 0; z < 16; z++)
            {
                if(cubesMatrix[15, y, z] != null)
                {
                    cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().UpdateXPlus(HasAirXPlus(15, y, z, cubesMatrix[15, y, z].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }
    }

    public void UpdateXMinusEdge()
    {
        for (int y = 0; y < 256; y++)
        {
            for (int z = 0; z < 16; z++)
            {
                if (cubesMatrix[0, y, z] != null)
                {
                    cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().UpdateXMinus(HasAirXMinus(0, y, z, cubesMatrix[0, y, z].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }
    }

    public void UpdateZPlusEdge()
    {
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                if (cubesMatrix[x, y, 15] != null)
                {
                    cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().UpdateZPlus(HasAirZPlus(x, y, 15, cubesMatrix[x, y, 15].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }
    }

    public void UpdateZMinusEdge()
    {
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                if (cubesMatrix[x, y, 0] != null)
                {
                    cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().UpdateZMinus(HasAirZMinus(x, y, 0, cubesMatrix[x, y, 0].GetComponent<FaceOptimiser>().isWater));
                }
            }
        }
    }

    public bool IsThisBlocAir(int x, int y, int z)
    {
        int posChunkX = chunkPositionX;
        int posChunkZ = chunkPositionZ;
        if (x > 15)
        {
            posChunkX += 1;
            x -= 16;
        }
        else if (x < 0)
        {
            posChunkX -= 1;
            x += 16;
        }
        if (z > 15)
        {
            posChunkZ += 1;
            z -= 16;
        }
        else if (z < 0)
        {
            posChunkZ -= 1;
            z += 16;
        }

        if (WorldChunks.instance.chunks[posChunkX, posChunkZ] == null) return true;
        else return WorldChunks.instance.chunks[posChunkX, posChunkZ].GetComponent<ChunkManager>().blocsMatrix[x, y, z] == 0;
    }
}
