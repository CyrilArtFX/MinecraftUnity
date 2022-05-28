using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private Text fpsText;
    [SerializeField] private Text tpsText;

    private int fps = 0;
    private float fpsTime = 0;
    private int tps = 0;
    private float tpsTime = 0;


    private bool showState = false;

    public void ChangeState(bool newState)
    {
        showState = newState;
    }

    private void Update()
    {
        fps++;
        fpsTime += Time.deltaTime;
        if(fpsTime >= 1f)
        {
            fpsText.text = "FPS : " + fps.ToString();
            fpsTime -= 1f;
            fps = 0;
        }
    }

    private void FixedUpdate()
    {
        tps++;
        tpsTime += Time.fixedDeltaTime;
        if (tpsTime >= 1f)
        {
            tpsText.text = "TPS : " + tps.ToString();
            tpsTime -= 1f;
            tps = 0;
        }


        fpsText.gameObject.SetActive(showState);
        tpsText.gameObject.SetActive(showState);
    }
}
