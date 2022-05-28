using UnityEngine;

public class FaceOptimiser : MonoBehaviour
{
    [SerializeField] GameObject faceYPlus;
    [SerializeField] GameObject faceYMinus;
    [SerializeField] GameObject faceXPlus;
    [SerializeField] GameObject faceXMinus;
    [SerializeField] GameObject faceZPlus;
    [SerializeField] GameObject faceZMinus;

    public bool isTransparent = false;
    public bool isWater = false;
    public GameObject fallingBloc;
    public int blocIndex;

    public void UpdateFaces(bool yPlus, bool yMinus, bool xPlus, bool xMinus, bool zPlus, bool zMinus)
    {
        faceYPlus.SetActive(yPlus);
        faceYMinus.SetActive(yMinus);
        faceXPlus.SetActive(xPlus);
        faceXMinus.SetActive(xMinus);
        faceZPlus.SetActive(zPlus);
        faceZMinus.SetActive(zMinus);
    }

    public void UpdateYPlus(bool yPlus)
    {
        faceYPlus.SetActive(yPlus);
    }

    public void UpdateYMinus(bool yMinus)
    {
        faceYMinus.SetActive(yMinus);
    }

    public void UpdateXPlus(bool xPlus)
    {
        faceXPlus.SetActive(xPlus);
    }

    public void UpdateXMinus(bool xMinus)
    {
        faceXMinus.SetActive(xMinus);
    }

    public void UpdateZPlus(bool zPlus)
    {
        faceZPlus.SetActive(zPlus);
    }

    public void UpdateZMinus(bool zMinus)
    {
        faceZMinus.SetActive(zMinus);
    }
}
