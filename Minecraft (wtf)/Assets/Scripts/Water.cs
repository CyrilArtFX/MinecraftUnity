using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [Header("Base values")]
    [SerializeField] bool source;
    [SerializeField] MeshFilter mainMeshFilter;
    [SerializeField] MeshCollider meshCollider;

    [Header("Quads mesh filters")]
    [SerializeField] MeshFilter yPlusQuad;
    [SerializeField] MeshFilter yPlusQuadReversed;
    [SerializeField] MeshFilter xPlusQuad;
    [SerializeField] MeshFilter xPlusQuadReversed;
    [SerializeField] MeshFilter xMinusQuad;
    [SerializeField] MeshFilter xMinusQuadReversed;
    [SerializeField] MeshFilter zPlusQuad;
    [SerializeField] MeshFilter zPlusQuadReversed;
    [SerializeField] MeshFilter zMinusQuad;
    [SerializeField] MeshFilter zMinusQuadReversed;

    [Header("Materials")]
    [SerializeField] MeshRenderer yPlusRenderer;
    [SerializeField] MeshRenderer yPlusReversedRenderer;
    [SerializeField] Material waterStillMaterial;
    [SerializeField] Material waterFlow16Material;
    [SerializeField] Material waterFlow32Material;


    [HideInInspector] public int flowValue;
    [HideInInspector] public float[] yValues = { 1f, 1f, 1f, 1f }; //for all yValues : [0] is X+ Z+  [1] is X+ Z-  [2] is X- Z+  [3] is X- Z-
    [HideInInspector] public bool hasChanged;
    [HideInInspector] public Vector3 flowForce = new Vector3(0f, 0f, 0f);

    Orientation orientation = Orientation.XPlus;

    ChunkManager chunkParent;
    int posX;
    int posY;
    int posZ;

    private void Awake()
    {
        if (source) flowValue = 9;
    }

    public void CreateWaterFlow(ChunkManager chunk, int x, int y, int z)
    {
        chunkParent = chunk;
        posX = x;
        posY = y;
        posZ = z;

        RecalculateFlow();
    }

    public void CreateWaterSourceForGeneration(ChunkManager chunk, int x, int y, int z, bool hasWaterAbove)
    {
        chunkParent = chunk;
        posX = x;
        posY = y;
        posZ = z;

        flowValue = 9;
        orientation = Orientation.Neutral;
        flowForce = Vector3.zero;
        float uniformYValue = hasWaterAbove ? 1f : 0.88f;
        for (int i = 0; i < 4; i++) yValues[i] = uniformYValue;
        UpdateMesh();
        ChangeMaterialOnYPlus();
    }

    public void RecalculateFlow()
    {
        hasChanged = false;

        int oldFlowValue = flowValue;
        float oldYValue0 = yValues[0];
        float oldYValue1 = yValues[1];
        float oldYValue2 = yValues[2];
        float oldYValue3 = yValues[3];

        int[] adjacentFlowValues = chunkParent.ReturnAdjacentFlowValues(posX, posY, posZ);
        if (source)
        {
            flowValue = 9;
            CalculateFlowDirection(adjacentFlowValues);
            CalculateYValues(adjacentFlowValues);
            UpdateMesh();
            ChangeMaterialOnYPlus();

            if (oldYValue0 != yValues[0] || oldYValue1 != yValues[1] || oldYValue2 != yValues[2] || oldYValue3 != yValues[3])
            {
                hasChanged = true;
            }
            return;
        }

        int maxAdjacentFlowValue = -1;
        int numberOfAdjacentSources = 0;
        for (int i = 0; i < 4; i++)
        {
            if (adjacentFlowValues[i] > maxAdjacentFlowValue) maxAdjacentFlowValue = adjacentFlowValues[i];
            if (adjacentFlowValues[i] == 9) numberOfAdjacentSources++;
        }

        if (numberOfAdjacentSources >= 2)
        {
            TransformToSource();
            CalculateFlowDirection(adjacentFlowValues);
            CalculateYValues(adjacentFlowValues);
            UpdateMesh();
            ChangeMaterialOnYPlus();
            return;
        }
        else if (adjacentFlowValues[4] != -1)
        {
            flowValue = 8;

            CalculateFlowDirection(adjacentFlowValues);
            CalculateYValues(adjacentFlowValues);
            UpdateMesh();
            ChangeMaterialOnYPlus();
        }
        else if (maxAdjacentFlowValue < 2)
        {
            DestroyWaterFlow();
        }
        else
        {
            if (maxAdjacentFlowValue == 9) maxAdjacentFlowValue = 8;
            flowValue = maxAdjacentFlowValue - 1;

            CalculateFlowDirection(adjacentFlowValues);
            CalculateYValues(adjacentFlowValues);
            UpdateMesh();
            ChangeMaterialOnYPlus();
        }

        if (oldFlowValue != flowValue || oldYValue0 != yValues[0] || oldYValue1 != yValues[1] || oldYValue2 != yValues[2] || oldYValue3 != yValues[3])
        {
            hasChanged = true;
        }
    }

    void UpdateMesh()
    {
        float y1 = yValues[0];
        float y2 = yValues[2];
        float y3 = yValues[1];
        float y4 = yValues[3];

        Mesh mainMesh = mainMeshFilter.mesh;

        Vector3[] vertices = new Vector3[24];

        Vector3 verticeD1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 verticeD2 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 verticeD3 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 verticeD4 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 verticeU1 = new Vector3(0.5f, y1 - 0.5f, 0.5f);
        Vector3 verticeU2 = new Vector3(-0.5f, y2 - 0.5f, 0.5f);
        Vector3 verticeU3 = new Vector3(0.5f, y3 - 0.5f, -0.5f);
        Vector3 verticeU4 = new Vector3(-0.5f, y4 - 0.5f, -0.5f);

        vertices.SetValue(verticeD1, 0);
        vertices.SetValue(verticeD2, 1);
        vertices.SetValue(verticeU1, 2);
        vertices.SetValue(verticeU2, 3);
        vertices.SetValue(verticeU3, 4);
        vertices.SetValue(verticeU4, 5);
        vertices.SetValue(verticeD3, 6);
        vertices.SetValue(verticeD4, 7);

        vertices.SetValue(verticeU1, 8);
        vertices.SetValue(verticeU2, 9);
        vertices.SetValue(verticeU3, 10);
        vertices.SetValue(verticeU4, 11);
        vertices.SetValue(verticeD3, 12);
        vertices.SetValue(verticeD1, 13);
        vertices.SetValue(verticeD2, 14);
        vertices.SetValue(verticeD4, 15);

        vertices.SetValue(verticeD2, 16);
        vertices.SetValue(verticeU2, 17);
        vertices.SetValue(verticeU4, 18);
        vertices.SetValue(verticeD4, 19);
        vertices.SetValue(verticeD3, 20);
        vertices.SetValue(verticeU3, 21);
        vertices.SetValue(verticeU1, 22);
        vertices.SetValue(verticeD1, 23);
        mainMesh.SetVertices(vertices);

        mainMeshFilter.mesh = mainMesh;
        meshCollider.sharedMesh = mainMesh;




        //Quad Parts

        //Quad XPlus
        Vector3[] xPlusVertices = new Vector3[4];
        Vector3 vrtxXPlus1 = new Vector3(0.5f, y1 - 0.5f, 0f);
        Vector3 vrtxXPlus2 = new Vector3(-0.5f, y3 - 0.5f, 0f);
        Vector3 vrtxXPlus3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxXPlus4 = new Vector3(-0.5f, -0.5f, 0f);
        xPlusVertices.SetValue(vrtxXPlus1, 3);
        xPlusVertices.SetValue(vrtxXPlus2, 2);
        xPlusVertices.SetValue(vrtxXPlus3, 1);
        xPlusVertices.SetValue(vrtxXPlus4, 0);
        xPlusQuad.mesh.SetVertices(xPlusVertices);
        Vector2[] xPlusUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y3), new Vector2(1f, y1) };
        xPlusQuad.mesh.uv = xPlusUVs;

        //Quad XPlus Reversed
        Vector3[] xPlusRVertices = new Vector3[4];
        Vector3 vrtxXPlusR1 = new Vector3(0.5f, y3 - 0.5f, 0f);
        Vector3 vrtxXPlusR2 = new Vector3(-0.5f, y1 - 0.5f, 0f);
        Vector3 vrtxXPlusR3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxXPlusR4 = new Vector3(-0.5f, -0.5f, 0f);
        xPlusRVertices.SetValue(vrtxXPlusR1, 3);
        xPlusRVertices.SetValue(vrtxXPlusR2, 2);
        xPlusRVertices.SetValue(vrtxXPlusR3, 1);
        xPlusRVertices.SetValue(vrtxXPlusR4, 0);
        xPlusQuadReversed.mesh.SetVertices(xPlusRVertices);
        Vector2[] xPlusRUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y1), new Vector2(1f, y3) };
        xPlusQuadReversed.mesh.uv = xPlusRUVs;

        //Quad XMinus
        Vector3[] xMinusVertices = new Vector3[4];
        Vector3 vrtxXMinus1 = new Vector3(0.5f, y4 - 0.5f, 0f);
        Vector3 vrtxXMinus2 = new Vector3(-0.5f, y2 - 0.5f, 0f);
        Vector3 vrtxXMinus3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxXMinus4 = new Vector3(-0.5f, -0.5f, 0f);
        xMinusVertices.SetValue(vrtxXMinus1, 3);
        xMinusVertices.SetValue(vrtxXMinus2, 2);
        xMinusVertices.SetValue(vrtxXMinus3, 1);
        xMinusVertices.SetValue(vrtxXMinus4, 0);
        xMinusQuad.mesh.SetVertices(xMinusVertices);
        Vector2[] xMinusUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y2), new Vector2(1f, y4) };
        xMinusQuad.mesh.uv = xMinusUVs;

        //Quad XMinus Reversed
        Vector3[] xMinusRVertices = new Vector3[4];
        Vector3 vrtxXMinusR1 = new Vector3(0.5f, y2 - 0.5f, 0f);
        Vector3 vrtxXMinusR2 = new Vector3(-0.5f, y4 - 0.5f, 0f);
        Vector3 vrtxXMinusR3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxXMinusR4 = new Vector3(-0.5f, -0.5f, 0f);
        xMinusRVertices.SetValue(vrtxXMinusR1, 3);
        xMinusRVertices.SetValue(vrtxXMinusR2, 2);
        xMinusRVertices.SetValue(vrtxXMinusR3, 1);
        xMinusRVertices.SetValue(vrtxXMinusR4, 0);
        xMinusQuadReversed.mesh.SetVertices(xMinusRVertices);
        Vector2[] xMinusRUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y4), new Vector2(1f, y2) };
        xMinusQuadReversed.mesh.uv = xMinusRUVs;

        //Quad ZPlus
        Vector3[] zPlusVertices = new Vector3[4];
        Vector3 vrtxZPlus1 = new Vector3(0.5f, y2 - 0.5f, 0f);
        Vector3 vrtxZPlus2 = new Vector3(-0.5f, y1 - 0.5f, 0f);
        Vector3 vrtxZPlus3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxZPlus4 = new Vector3(-0.5f, -0.5f, 0f);
        zPlusVertices.SetValue(vrtxZPlus1, 3);
        zPlusVertices.SetValue(vrtxZPlus2, 2);
        zPlusVertices.SetValue(vrtxZPlus3, 1);
        zPlusVertices.SetValue(vrtxZPlus4, 0);
        zPlusQuad.mesh.SetVertices(zPlusVertices);
        Vector2[] zPlusUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y1), new Vector2(1f, y2) };
        zPlusQuad.mesh.uv = zPlusUVs;

        //Quad ZPlus Reversed
        Vector3[] zPlusRVertices = new Vector3[4];
        Vector3 vrtxZPlusR1 = new Vector3(0.5f, y1 - 0.5f, 0f);
        Vector3 vrtxZPlusR2 = new Vector3(-0.5f, y2 - 0.5f, 0f);
        Vector3 vrtxZPlusR3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxZPlusR4 = new Vector3(-0.5f, -0.5f, 0f);
        zPlusRVertices.SetValue(vrtxZPlusR1, 3);
        zPlusRVertices.SetValue(vrtxZPlusR2, 2);
        zPlusRVertices.SetValue(vrtxZPlusR3, 1);
        zPlusRVertices.SetValue(vrtxZPlusR4, 0);
        zPlusQuadReversed.mesh.SetVertices(zPlusRVertices);
        Vector2[] zPlusRUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y2), new Vector2(1f, y1) };
        zPlusQuadReversed.mesh.uv = zPlusRUVs;

        //Quad ZMinus
        Vector3[] zMinusVertices = new Vector3[4];
        Vector3 vrtxZMinus1 = new Vector3(0.5f, y3 - 0.5f, 0f);
        Vector3 vrtxZMinus2 = new Vector3(-0.5f, y4 - 0.5f, 0f);
        Vector3 vrtxZMinus3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxZMinus4 = new Vector3(-0.5f, -0.5f, 0f);
        zMinusVertices.SetValue(vrtxZMinus1, 3);
        zMinusVertices.SetValue(vrtxZMinus2, 2);
        zMinusVertices.SetValue(vrtxZMinus3, 1);
        zMinusVertices.SetValue(vrtxZMinus4, 0);
        zMinusQuad.mesh.SetVertices(zMinusVertices);
        Vector2[] zMinusUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y4), new Vector2(1f, y3) };
        zMinusQuad.mesh.uv = zMinusUVs;

        //Quad ZMinus Reversed
        Vector3[] zMinusRVertices = new Vector3[4];
        Vector3 vrtxZMinusR1 = new Vector3(0.5f, y4 - 0.5f, 0f);
        Vector3 vrtxZMinusR2 = new Vector3(-0.5f, y3 - 0.5f, 0f);
        Vector3 vrtxZMinusR3 = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vrtxZMinusR4 = new Vector3(-0.5f, -0.5f, 0f);
        zMinusRVertices.SetValue(vrtxZMinusR1, 3);
        zMinusRVertices.SetValue(vrtxZMinusR2, 2);
        zMinusRVertices.SetValue(vrtxZMinusR3, 1);
        zMinusRVertices.SetValue(vrtxZMinusR4, 0);
        zMinusQuadReversed.mesh.SetVertices(zMinusRVertices);
        Vector2[] zMinusRUVs = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y3), new Vector2(1f, y4) };
        zMinusQuadReversed.mesh.uv = zMinusRUVs;




        //------------Quad YPlus Part (complicated)------------------


        //Quad YPlus
        Vector3[] yPlusVertices = new Vector3[4];
        Vector3 vrtxYPlus1 = new Vector3(0.5f, 0.5f, -y1 + 1f);
        Vector3 vrtxYPlus2 = new Vector3(-0.5f, 0.5f, -y2 + 1f);
        Vector3 vrtxYPlus3 = new Vector3(0.5f, -0.5f, -y3 + 1f);
        Vector3 vrtxYPlus4 = new Vector3(-0.5f, -0.5f, -y4 + 1f);
        yPlusVertices.SetValue(vrtxYPlus1, 3);
        yPlusVertices.SetValue(vrtxYPlus2, 2);
        yPlusVertices.SetValue(vrtxYPlus3, 1);
        yPlusVertices.SetValue(vrtxYPlus4, 0);
        yPlusQuad.mesh.SetVertices(yPlusVertices);

        Vector2[] yPlusUVs = new Vector2[4];
        switch (orientation)
        {
            case Orientation.Neutral:
                yPlusUVs[0] = new Vector2(1f, 0f);
                yPlusUVs[1] = new Vector2(1f, 1f);
                yPlusUVs[2] = new Vector2(0f, 0f);
                yPlusUVs[3] = new Vector2(0f, 1f);
                break;
            case Orientation.XPlus:
                yPlusUVs[0] = new Vector2(1f, 0f);
                yPlusUVs[1] = new Vector2(1f, 1f);
                yPlusUVs[2] = new Vector2(0f, 0f);
                yPlusUVs[3] = new Vector2(0f, 1f);
                break;
            case Orientation.XMinus:
                yPlusUVs[0] = new Vector2(0f, 1f);
                yPlusUVs[1] = new Vector2(0f, 0f);
                yPlusUVs[2] = new Vector2(1f, 1f);
                yPlusUVs[3] = new Vector2(1f, 0f);
                break;
            case Orientation.ZPlus:
                yPlusUVs[0] = new Vector2(0f, 0f);
                yPlusUVs[1] = new Vector2(1f, 0f);
                yPlusUVs[2] = new Vector2(0f, 1f);
                yPlusUVs[3] = new Vector2(1f, 1f);
                break;
            case Orientation.ZMinus:
                yPlusUVs[0] = new Vector2(1f, 1f);
                yPlusUVs[1] = new Vector2(0f, 1f);
                yPlusUVs[2] = new Vector2(1f, 0f);
                yPlusUVs[3] = new Vector2(0f, 0f);
                break;
            case Orientation.XPlusZPlus:
                yPlusUVs[0] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                yPlusUVs[1] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusUVs[2] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusUVs[3] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                break;
            case Orientation.XPlusZMinus:
                yPlusUVs[0] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusUVs[1] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                yPlusUVs[2] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                yPlusUVs[3] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                break;
            case Orientation.XMinusZPlus:
                yPlusUVs[0] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusUVs[1] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                yPlusUVs[2] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                yPlusUVs[3] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                break;
            case Orientation.XMinusZMinus:
                yPlusUVs[0] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                yPlusUVs[1] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusUVs[2] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusUVs[3] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                break;
        }
        yPlusQuad.mesh.uv = yPlusUVs;

        int vrtxMain1;
        int vrtxMain2;
        int vrtxOther1;
        int vrtxOther2;

        if ((y1 + y4) / 2 > (y2 + y3) / 2)
        {
            if (y1 > y4)
            {
                vrtxMain1 = 3;
                vrtxMain2 = 0;
            }
            else
            {
                vrtxMain1 = 0;
                vrtxMain2 = 3;
            }
            if (y2 > y3)
            {
                vrtxOther1 = 1;
                vrtxOther2 = 2;
            }
            else
            {
                vrtxOther1 = 2;
                vrtxOther2 = 1;
            }
        }
        else
        {
            if (y2 > y3)
            {
                vrtxMain1 = 2;
                vrtxMain2 = 1;
            }
            else
            {
                vrtxMain1 = 1;
                vrtxMain2 = 2;
            }
            if (y1 > y4)
            {
                vrtxOther1 = 3;
                vrtxOther2 = 0;
            }
            else
            {
                vrtxOther1 = 0;
                vrtxOther2 = 3;
            }
        }
        int[] yPlusTriangles = { vrtxMain2, vrtxMain1, vrtxOther1, vrtxMain1, vrtxMain2, vrtxOther2 };
        yPlusQuad.mesh.SetTriangles(yPlusTriangles, 0);

        yPlusQuad.mesh.RecalculateNormals();


        //Quad YPlus Reversed
        Vector3[] yPlusRVertices = new Vector3[4];
        Vector3 vrtxYPlusR1 = new Vector3(0.5f, 0.5f, -(-y2 + 1f));
        Vector3 vrtxYPlusR2 = new Vector3(-0.5f, 0.5f, -(-y1 + 1f));
        Vector3 vrtxYPlusR3 = new Vector3(0.5f, -0.5f, -(-y4 + 1f));
        Vector3 vrtxYPlusR4 = new Vector3(-0.5f, -0.5f, -(-y3 + 1f));
        yPlusRVertices.SetValue(vrtxYPlusR1, 3);
        yPlusRVertices.SetValue(vrtxYPlusR2, 2);
        yPlusRVertices.SetValue(vrtxYPlusR3, 1);
        yPlusRVertices.SetValue(vrtxYPlusR4, 0);
        yPlusQuadReversed.mesh.SetVertices(yPlusRVertices);

        Vector2[] yPlusRUVs = new Vector2[4];
        switch (orientation)
        {
            case Orientation.Neutral:
                yPlusRUVs[0] = new Vector2(0f, 1f);
                yPlusRUVs[1] = new Vector2(0f, 0f);
                yPlusRUVs[2] = new Vector2(1f, 1f);
                yPlusRUVs[3] = new Vector2(1f, 0f);
                break;
            case Orientation.XPlus:
                yPlusRUVs[0] = new Vector2(0f, 1f);
                yPlusRUVs[1] = new Vector2(0f, 0f);
                yPlusRUVs[2] = new Vector2(1f, 1f);
                yPlusRUVs[3] = new Vector2(1f, 0f);
                break;
            case Orientation.XMinus:
                yPlusRUVs[0] = new Vector2(1f, 0f);
                yPlusRUVs[1] = new Vector2(1f, 1f);
                yPlusRUVs[2] = new Vector2(0f, 0f);
                yPlusRUVs[3] = new Vector2(0f, 1f);
                break;
            case Orientation.ZPlus:
                yPlusRUVs[0] = new Vector2(0f, 0f);
                yPlusRUVs[1] = new Vector2(1f, 0f);
                yPlusRUVs[2] = new Vector2(0f, 1f);
                yPlusRUVs[3] = new Vector2(1f, 1f);
                break;
            case Orientation.ZMinus:
                yPlusRUVs[0] = new Vector2(1f, 1f);
                yPlusRUVs[1] = new Vector2(0f, 1f);
                yPlusRUVs[2] = new Vector2(1f, 0f);
                yPlusRUVs[3] = new Vector2(0f, 0f);
                break;
            case Orientation.XPlusZPlus:
                yPlusRUVs[0] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusRUVs[1] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                yPlusRUVs[2] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                yPlusRUVs[3] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                break;
            case Orientation.XPlusZMinus:
                yPlusRUVs[0] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                yPlusRUVs[1] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusRUVs[2] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusRUVs[3] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                break;
            case Orientation.XMinusZPlus:
                yPlusRUVs[0] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                yPlusRUVs[1] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusRUVs[2] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusRUVs[3] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                break;
            case Orientation.XMinusZMinus:
                yPlusRUVs[0] = new Vector2(0.5f + (Mathf.Sqrt(0.5f) / 2), 0.5f);
                yPlusRUVs[1] = new Vector2(0.5f, 0.5f + (Mathf.Sqrt(0.5f) / 2));
                yPlusRUVs[2] = new Vector2(0.5f, 0.5f - (Mathf.Sqrt(0.5f) / 2));
                yPlusRUVs[3] = new Vector2(0.5f - (Mathf.Sqrt(0.5f) / 2), 0.5f);
                break;
        }
        yPlusQuadReversed.mesh.uv = yPlusRUVs;

        int[] yPlusRTriangles = { vrtxOther2, vrtxOther1, vrtxMain2, vrtxOther1, vrtxOther2, vrtxMain1 };
        yPlusQuadReversed.mesh.SetTriangles(yPlusRTriangles, 0);

        yPlusQuadReversed.mesh.RecalculateNormals();
    }

    void CalculateYValues(int[] adjacentFlowValues)
    {
        //for (int i = 0; i < 4; i++) Debug.Log(gameObject.name + " " + adjacentFlowValues[i]);
        switch (flowValue)
        {
            case 8:
                float uniformYValueBis = 1f;
                for (int i = 0; i < 4; i++) yValues[i] = uniformYValueBis;
                break;
            default:
                int[] yValuesPriorityDone = { 0, 0, 0, 0 };
                if(flowValue == 9)
                {
                    if (adjacentFlowValues[4] != -1)
                    {
                        float uniformYValue = 1f;
                        for (int i = 0; i < 4; i++)
                        {
                            yValues[i] = uniformYValue;
                            yValuesPriorityDone[i] = 4;
                        }
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    //Passe de prio A : vérifier les aretes adjacentes de flow supérieur (priority level = 3)
                    if (adjacentFlowValues[i] > flowValue)
                    {
                        //Debug.Log(gameObject.name + " " + i);
                        float[] adjacentYValues;
                        switch (i)
                        {
                            case 0: //X+
                                adjacentYValues = chunkParent.GetYValues(posX + 1, posY, posZ);
                                yValues[0] = adjacentYValues[2];
                                yValues[1] = adjacentYValues[3];
                                yValuesPriorityDone[0] = yValuesPriorityDone[1] = 3;
                                break;
                            case 1: //X-
                                adjacentYValues = chunkParent.GetYValues(posX - 1, posY, posZ);
                                yValues[2] = adjacentYValues[0];
                                yValues[3] = adjacentYValues[1];
                                yValuesPriorityDone[2] = yValuesPriorityDone[3] = 3;
                                break;
                            case 2: //Z+
                                adjacentYValues = chunkParent.GetYValues(posX, posY, posZ + 1);
                                yValues[0] = adjacentYValues[1];
                                yValues[2] = adjacentYValues[3];
                                yValuesPriorityDone[0] = yValuesPriorityDone[2] = 3;
                                break;
                            case 3: //Z-
                                adjacentYValues = chunkParent.GetYValues(posX, posY, posZ - 1);
                                yValues[1] = adjacentYValues[0];
                                yValues[3] = adjacentYValues[2];
                                yValuesPriorityDone[1] = yValuesPriorityDone[3] = 3;
                                break;
                        }
                    }
                    //Passe de prio B : vérifier les aretes adjacentes de flow inférieur (priority level = 2)
                    else if(adjacentFlowValues[i] != -1 && adjacentFlowValues[i] < flowValue)
                    {
                        List<int> yValuesIndexToModify = new List<int>();
                        switch (i)
                        {
                            case 0: //X+
                                if (yValuesPriorityDone[0] < 2) yValuesIndexToModify.Add(0);
                                if (yValuesPriorityDone[1] < 2) yValuesIndexToModify.Add(1);
                                break;
                            case 1: //X-
                                if (yValuesPriorityDone[2] < 2) yValuesIndexToModify.Add(2);
                                if (yValuesPriorityDone[3] < 2) yValuesIndexToModify.Add(3);
                                break;
                            case 2: //Z+
                                if (yValuesPriorityDone[0] < 2) yValuesIndexToModify.Add(0);
                                if (yValuesPriorityDone[2] < 2) yValuesIndexToModify.Add(2);
                                break;
                            case 3: //Z-
                                if (yValuesPriorityDone[1] < 2) yValuesIndexToModify.Add(1);
                                if (yValuesPriorityDone[3] < 2) yValuesIndexToModify.Add(3);
                                break;
                        }
                        for(int j = 0; j < yValuesIndexToModify.Count; j++)
                        {
                            switch (flowValue)
                            {
                                case 9:
                                    yValues[yValuesIndexToModify[j]] = 0.88f;
                                    break;
                                case 7:
                                    yValues[yValuesIndexToModify[j]] = 0.7f;
                                    break;
                                case 6:
                                    yValues[yValuesIndexToModify[j]] = 0.6f;
                                    break;
                                case 5:
                                    yValues[yValuesIndexToModify[j]] = 0.5f;
                                    break;
                                case 4:
                                    yValues[yValuesIndexToModify[j]] = 0.38f;
                                    break;
                                case 3:
                                    yValues[yValuesIndexToModify[j]] = 0.28f;
                                    break;
                                case 2:
                                    yValues[yValuesIndexToModify[j]] = 0.17f;
                                    break;
                                case 1:
                                    yValues[yValuesIndexToModify[j]] = 0.1f;
                                    break;
                            }
                            yValuesPriorityDone[yValuesIndexToModify[j]] = 2;
                        }
                    }
                    //Passe de prio C : le reste des aretes (celles ou y'a de l'air / un bloc / de l'eau avec le même flow) (priority level = 1)
                    else 
                    {
                        List<int> yValuesIndexToModify = new List<int>();
                        bool air = false;
                        switch (i)
                        {
                            case 0: //X+
                                if (yValuesPriorityDone[0] < 1) yValuesIndexToModify.Add(0);
                                if (yValuesPriorityDone[1] < 1) yValuesIndexToModify.Add(1);
                                air = chunkParent.IsThisBlocAir(posX + 1, posY, posZ);
                                break;
                            case 1: //X-
                                if (yValuesPriorityDone[2] < 1) yValuesIndexToModify.Add(2);
                                if (yValuesPriorityDone[3] < 1) yValuesIndexToModify.Add(3);
                                air = chunkParent.IsThisBlocAir(posX - 1, posY, posZ);
                                break;
                            case 2: //Z+
                                if (yValuesPriorityDone[0] < 1) yValuesIndexToModify.Add(0);
                                if (yValuesPriorityDone[2] < 1) yValuesIndexToModify.Add(2);
                                air = chunkParent.IsThisBlocAir(posX, posY, posZ + 1);
                                break;
                            case 3: //Z-
                                if (yValuesPriorityDone[1] < 1) yValuesIndexToModify.Add(1);
                                if (yValuesPriorityDone[3] < 1) yValuesIndexToModify.Add(3);
                                air = chunkParent.IsThisBlocAir(posX, posY, posZ - 1);
                                break;
                        }
                        for (int j = 0; j < yValuesIndexToModify.Count; j++)
                        {
                            switch (flowValue)
                            {
                                case 9:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.88f : 0.88f;
                                    break;
                                case 7:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.7f : 0.77f;
                                    break;
                                case 6:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.6f : 0.67f;
                                    break;
                                case 5:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.5f : 0.56f;
                                    break;
                                case 4:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.38f : 0.44f;
                                    break;
                                case 3:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.28f : 0.32f;
                                    break;
                                case 2:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.17f : 0.22f;
                                    break;
                                case 1:
                                    yValues[yValuesIndexToModify[j]] = air ? 0.1f : 0.1f;
                                    break;
                            }
                            yValuesPriorityDone[yValuesIndexToModify[j]] = 1;
                        }

                        if(adjacentFlowValues[i] == flowValue)
                        {
                            float[] adjacentYValues;
                            switch (i)
                            {
                                case 0: //X+
                                    adjacentYValues = chunkParent.GetYValues(posX + 1, posY, posZ);
                                    if(yValuesPriorityDone[0] < 3 && adjacentYValues[2] > yValues[0])
                                    {
                                        yValues[0] = adjacentYValues[2];
                                        yValuesPriorityDone[0] = 2;
                                    }
                                    if (yValuesPriorityDone[1] < 3 && adjacentYValues[3] > yValues[1])
                                    {
                                        yValues[1] = adjacentYValues[3];
                                        yValuesPriorityDone[1] = 2;
                                    }
                                    break;
                                case 1: //X-
                                    adjacentYValues = chunkParent.GetYValues(posX - 1, posY, posZ);
                                    if (yValuesPriorityDone[2] < 3 && adjacentYValues[0] > yValues[2])
                                    {
                                        yValues[2] = adjacentYValues[0];
                                        yValuesPriorityDone[2] = 2;
                                    }
                                    if (yValuesPriorityDone[3] < 3 && adjacentYValues[1] > yValues[3])
                                    {
                                        yValues[3] = adjacentYValues[1];
                                        yValuesPriorityDone[3] = 2;
                                    }
                                    break;
                                case 2: //Z+
                                    adjacentYValues = chunkParent.GetYValues(posX, posY, posZ + 1);
                                    if (yValuesPriorityDone[0] < 3 && adjacentYValues[1] > yValues[0])
                                    {
                                        yValues[0] = adjacentYValues[1];
                                        yValuesPriorityDone[0] = 2;
                                    }
                                    if (yValuesPriorityDone[2] < 3 && adjacentYValues[3] > yValues[2])
                                    {
                                        yValues[2] = adjacentYValues[3];
                                        yValuesPriorityDone[2] = 2;
                                    }
                                    break;
                                case 3: //Z-
                                    adjacentYValues = chunkParent.GetYValues(posX, posY, posZ - 1);
                                    if (yValuesPriorityDone[1] < 3 && adjacentYValues[0] > yValues[1])
                                    {
                                        yValues[1] = adjacentYValues[0];
                                        yValuesPriorityDone[1] = 2;
                                    }
                                    if (yValuesPriorityDone[3] < 3 && adjacentYValues[2] > yValues[3])
                                    {
                                        yValues[3] = adjacentYValues[2];
                                        yValuesPriorityDone[3] = 2;
                                    }
                                    break;
                            }
                        }
                    }
                }
                break;
        }
    }
    
    void CalculateFlowDirection(int[] adjacentFlowValues)
    {
        int flowDirectionX = 0;
        int flowDirectionZ = 0;
        if(adjacentFlowValues[0] != -1)
        {
            if (adjacentFlowValues[0] > flowValue) flowDirectionX += 1;
            if (adjacentFlowValues[0] < flowValue) flowDirectionX -= 1;
        }
        if (adjacentFlowValues[1] != -1)
        {
            if (adjacentFlowValues[1] > flowValue) flowDirectionX -= 1;
            if (adjacentFlowValues[1] < flowValue) flowDirectionX += 1;
        }
        if (adjacentFlowValues[2] != -1)
        {
            if (adjacentFlowValues[2] > flowValue) flowDirectionZ += 1;
            if (adjacentFlowValues[2] < flowValue) flowDirectionZ -= 1;
        }
        if (adjacentFlowValues[3] != -1)
        {
            if (adjacentFlowValues[3] > flowValue) flowDirectionZ -= 1;
            if (adjacentFlowValues[3] < flowValue) flowDirectionZ += 1;
        }


        if(flowDirectionX == 0 && flowDirectionZ == 0)
        {
            orientation = Orientation.Neutral;
        }
        else if(flowDirectionX == 0 || flowDirectionZ == 0)
        {
            if (flowDirectionX > 0) orientation = Orientation.XPlus;
            if (flowDirectionX < 0) orientation = Orientation.XMinus;
            if (flowDirectionZ > 0) orientation = Orientation.ZPlus;
            if (flowDirectionZ < 0) orientation = Orientation.ZMinus;
        }
        else
        {
            if(flowDirectionX > 0)
            {
                if (flowDirectionZ > 0) orientation = Orientation.XPlusZPlus;
                else orientation = Orientation.XPlusZMinus;
            }
            else
            {
                if (flowDirectionZ > 0) orientation = Orientation.XMinusZPlus;
                else orientation = Orientation.XMinusZMinus;
            }
        }

        switch(orientation)
        {
            case Orientation.Neutral:
                flowForce = new Vector3(0f, 0f, 0f);
                break;
            case Orientation.XPlus:
                flowForce = new Vector3(-1f, 0f, 0f);
                break;
            case Orientation.XMinus:
                flowForce = new Vector3(1f, 0f, 0f);
                break;
            case Orientation.ZPlus:
                flowForce = new Vector3(0f, 0f, -1f);
                break;
            case Orientation.ZMinus:
                flowForce = new Vector3(0f, 0f, 1f);
                break;
            case Orientation.XPlusZPlus:
                flowForce = new Vector3(-1f, 0f, -1f);
                break;
            case Orientation.XPlusZMinus:
                flowForce = new Vector3(-1f, 0f, 1f);
                break;
            case Orientation.XMinusZPlus:
                flowForce = new Vector3(1f, 0f, -1f);
                break;
            case Orientation.XMinusZMinus:
                flowForce = new Vector3(1f, 0f, 1f);
                break;
        }
    }

    void ChangeMaterialOnYPlus()
    {
        if(orientation == Orientation.Neutral)
        {
            yPlusRenderer.material = waterStillMaterial;
            yPlusReversedRenderer.material = waterStillMaterial;
        }
        else if(orientation == Orientation.XPlus || orientation == Orientation.XMinus || orientation == Orientation.ZPlus || orientation == Orientation.ZMinus)
        {
            yPlusRenderer.material = waterFlow16Material;
            yPlusReversedRenderer.material = waterFlow16Material;
        }
        else
        {
            yPlusRenderer.material = waterFlow32Material;
            yPlusReversedRenderer.material = waterFlow32Material;
        }
    }

    void DestroyWaterFlow()
    {
        chunkParent.blocsMatrix[posX, posY, posZ] = 0;
        chunkParent.DestroyBloc(posX, posY, posZ);
    }

    void TransformToSource()
    {
        chunkParent.PlaceBloc(posX, posY, posZ, ListOfAllBlocs.instance.blocs[9]);

        flowValue = 9;
    }

    private enum Orientation
    {
        XPlus,
        XMinus,
        ZPlus,
        ZMinus,
        XPlusZPlus,
        XPlusZMinus,
        XMinusZPlus,
        XMinusZMinus,
        Neutral
    }
}
