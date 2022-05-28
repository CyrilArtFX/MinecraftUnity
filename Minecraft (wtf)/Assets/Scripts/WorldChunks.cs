using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunks : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField, Range(0, 32)] public int renderDistance = 6;

    [SerializeField] GameObject chunk;

    [HideInInspector] public GameObject[,] chunks = new GameObject[1000, 1000];

    [HideInInspector] public int playerXPos, playerYPos;
    [HideInInspector] public int seed;

    [HideInInspector] public IDictionary<Vector2Int, int[,,]> preGenMatrices = new Dictionary<Vector2Int, int[,,]>();

    public GenerationMode generationMode;

    public static WorldChunks instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        playerXPos = (int)(player.position.x / 16) + 500;
        playerYPos = (int)(player.position.z / 16) + 500;

        seed = Random.Range(0, 99999999);


        List<Vector2Int> chunksToCreate = new List<Vector2Int>();
        for (int x = playerXPos - renderDistance; x <= playerXPos + renderDistance; x++)
        {
            for (int y = playerYPos - renderDistance; y <= playerYPos + renderDistance; y++)
            {
                if (chunks[x, y] == null) chunksToCreate.Add(new Vector2Int(x, y));
            }
        }
        if (chunksToCreate.Count > 0) CreateChunks(chunksToCreate);

        player.gameObject.SetActive(true);
    }

    void Update()
    {

    }
        

    private void FixedUpdate()
    {
        playerXPos = (int)(player.position.x / 16) + 500;
        playerYPos = (int)(player.position.z / 16) + 500;


        List<Vector2Int> chunksToCreate = new List<Vector2Int>();
        List<Vector2Int> chunksThatHasBecomeVisible = new List<Vector2Int>();
        for(int x = playerXPos - renderDistance; x <= playerXPos + renderDistance; x++)
        {
            for(int y = playerYPos - renderDistance; y <= playerYPos + renderDistance; y++)
            {
                if (chunks[x, y] == null)
                {
                    chunksToCreate.Add(new Vector2Int(x, y));
                    chunksThatHasBecomeVisible.Add(new Vector2Int(x, y));
                }
                else if (!chunks[x, y].activeInHierarchy)
                {
                    chunks[x, y].SetActive(true);
                    chunksThatHasBecomeVisible.Add(new Vector2Int(x, y));
                }
            }
        }
        if (chunksToCreate.Count > 0) CreateChunks(chunksToCreate);
        if(chunksThatHasBecomeVisible.Count > 0) UpdateAdjacentChunks(chunksThatHasBecomeVisible);
    }

    void CreateChunks(List<Vector2Int> chunksPos)
    {
        foreach(Vector2Int chunkPos in chunksPos) //To be sure we don't create a chunk that has already been created
        {
            if (chunks[chunkPos.x, chunkPos.y] != null) chunksPos.Remove(chunkPos);
        }

        foreach (Vector2Int chunkPos in chunksPos) //First pass : create chunks and their bloc matrix
        {
            chunks[chunkPos.x, chunkPos.y] = Instantiate(chunk, this.transform);
            chunks[chunkPos.x, chunkPos.y].name = "Chunk - " + chunkPos.x + " - " + chunkPos.y;
            int[,,] preGenMatrix = new int[16, 256, 16];
            if (preGenMatrices.ContainsKey(chunkPos)) preGenMatrix = preGenMatrices[chunkPos];
            chunks[chunkPos.x, chunkPos.y].GetComponent<ChunkManager>().CreateBlocMatrix(new Vector3((chunkPos.x * 16) - 8000, 0, (chunkPos.y * 16) - 8000), chunkPos.x, chunkPos.y, preGenMatrix);
        }

        if(generationMode == GenerationMode.procedural) //Only if we are in procedural mode
        {
            foreach (Vector2Int chunkPos in chunksPos) //Second pass : create trees
            {
                chunks[chunkPos.x, chunkPos.y].GetComponent<ChunkManager>().PlaceTrees();
            }
        }

        foreach (Vector2Int chunkPos in chunksPos) //Third pass : create cube matrix
        {
            chunks[chunkPos.x, chunkPos.y].GetComponent<ChunkManager>().CreateCubeMatrix();
            chunks[chunkPos.x, chunkPos.y].SetActive(true);
        }
    }

    void UpdateAdjacentChunks(List<Vector2Int> chunksPos)
    {
        List<ChunkManager> chunksToUpdateXPlusEdge = new List<ChunkManager>();
        List<ChunkManager> chunksToUpdateXMinusEdge = new List<ChunkManager>();
        List<ChunkManager> chunksToUpdateZPlusEdge = new List<ChunkManager>();
        List<ChunkManager> chunksToUpdateZMinusEdge = new List<ChunkManager>();

        foreach(Vector2Int chunkPos in chunksPos)
        {
            if (chunks[chunkPos.x - 1, chunkPos.y] != null && chunks[chunkPos.x - 1, chunkPos.y].activeInHierarchy) chunksToUpdateXPlusEdge.Add(chunks[chunkPos.x - 1, chunkPos.y].GetComponent<ChunkManager>());
            if (chunks[chunkPos.x + 1, chunkPos.y] != null && chunks[chunkPos.x + 1, chunkPos.y].activeInHierarchy) chunksToUpdateXMinusEdge.Add(chunks[chunkPos.x + 1, chunkPos.y].GetComponent<ChunkManager>());
            if (chunks[chunkPos.x, chunkPos.y - 1] != null && chunks[chunkPos.x, chunkPos.y - 1].activeInHierarchy) chunksToUpdateZPlusEdge.Add(chunks[chunkPos.x, chunkPos.y - 1].GetComponent<ChunkManager>());
            if (chunks[chunkPos.x, chunkPos.y + 1] != null && chunks[chunkPos.x, chunkPos.y + 1].activeInHierarchy) chunksToUpdateZMinusEdge.Add(chunks[chunkPos.x, chunkPos.y + 1].GetComponent<ChunkManager>());
        }

        foreach (ChunkManager chunk in chunksToUpdateXPlusEdge) chunk.UpdateXPlusEdge();
        foreach (ChunkManager chunk in chunksToUpdateXMinusEdge) chunk.UpdateXMinusEdge();
        foreach (ChunkManager chunk in chunksToUpdateZPlusEdge) chunk.UpdateZPlusEdge();
        foreach (ChunkManager chunk in chunksToUpdateZMinusEdge) chunk.UpdateZMinusEdge();
    }

    public enum GenerationMode
    {
        superflat,
        procedural
    }
}
