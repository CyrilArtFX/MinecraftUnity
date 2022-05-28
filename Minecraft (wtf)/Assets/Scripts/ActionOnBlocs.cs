using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionOnBlocs : MonoBehaviour
{
    [SerializeField, Range(0f, 20f)] float Range = 8f;
    [SerializeField] Camera playerCamera;
    [SerializeField] ItemSelector itemSelector;
    [SerializeField] private LayerMask blocLayer, blocPlacableLayer, fallingBlocLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && PauseMenu.instance.stepsSincePauseEnded > 10)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(playerCamera.pixelWidth / 2, playerCamera.pixelHeight / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Range, blocLayer))
            {
                if (hit.collider.gameObject.layer == fallingBlocLayer) return;
                hit.collider.gameObject.transform.parent.GetComponent<ChunkManager>().DestroyBloc((int)(hit.collider.gameObject.transform.localPosition.x), (int)(hit.collider.gameObject.transform.localPosition.y), (int)(hit.collider.gameObject.transform.localPosition.z));
            }
        }

        if (Input.GetMouseButtonDown(1) && PauseMenu.instance.stepsSincePauseEnded > 10)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(playerCamera.pixelWidth / 2, playerCamera.pixelHeight / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Range, blocPlacableLayer))
            {
                if(itemSelector.currentBlocSelected.index != 0)
                {
                    if (hit.collider.gameObject.layer == fallingBlocLayer) return;
                    hit.collider.gameObject.transform.parent.parent.GetComponent<ChunkManager>().PlaceBloc((int)(hit.transform.parent.localPosition.x + (hit.transform.localPosition.x * 2)), (int)(hit.transform.position.y + hit.transform.localPosition.y), (int)((hit.transform.parent.localPosition.z + (hit.transform.localPosition.z * 2))), itemSelector.currentBlocSelected);
                }
            }
        }
    }
}
