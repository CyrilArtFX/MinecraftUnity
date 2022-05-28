using UnityEngine;

public class FallingBloc : MonoBehaviour
{
    [SerializeField] private Bloc bloc;
    [SerializeField] private LayerMask blocLayer;
    [SerializeField] private LayerMask fallingBlocLayer;

    private int stepsSinceStartFalling;

    private ChunkManager chunk;

    public void StartFalling(ChunkManager _chunk)
    {
        stepsSinceStartFalling = 0;

        chunk = _chunk;
    }

    private void FixedUpdate()
    {
        stepsSinceStartFalling++;
        if(stepsSinceStartFalling > 2)
        {
            Physics.Raycast(transform.position + new Vector3(0f, -0.5f, 0f), Vector3.down, out RaycastHit hit, 0.2f, blocLayer);
            if(hit.distance < 0.2f && hit.distance > 0f)
            {
                transform.position += new Vector3(0f, -hit.distance, 0f);
                if(hit.collider.gameObject.layer != fallingBlocLayer)
                {
                    Vector3 position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
                    int x = Mathf.RoundToInt(position.x - chunk.gameObject.transform.position.x);
                    int z = Mathf.RoundToInt(position.z - chunk.gameObject.transform.position.z);
                    int y = Mathf.RoundToInt(position.y);
                    gameObject.layer = 0;
                    chunk.PlaceBloc(x, y, z, bloc);
                    Destroy(gameObject);
                }
            }
            else
            {
                transform.position += new Vector3(0f, -0.12f, 0f);
            }
        }
        if (transform.position.y < -1) Destroy(gameObject);
    }
}
