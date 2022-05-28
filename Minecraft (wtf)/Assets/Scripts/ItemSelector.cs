using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour
{
    [Header("Start Inventory Bar")]
    [SerializeField] private List<int> inventoryBarBlocsIndex = new List<int>(9);

    [Header("UI Settings")]
    [SerializeField] private Image ItemSelectorSprite;
    [SerializeField] private List<Image> iconImages = new List<Image>(9);

    [HideInInspector] public Bloc currentBlocSelected;

    private int currentSelect = 0;
    private int desiresChange = 0;

    private void Start()
    {
        for(int i = 0; i < 9; i++)
        {
            iconImages[i].sprite = ListOfAllBlocs.instance.blocs[inventoryBarBlocsIndex[i]].icon;
        }
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y > 0) desiresChange = -1;
        if (Input.mouseScrollDelta.y < 0) desiresChange = 1;
    }

    private void FixedUpdate()
    {
        if(desiresChange != 0)
        {
            currentSelect += desiresChange;
            desiresChange = 0;
            if (currentSelect < 0) currentSelect = 8;
            if (currentSelect > 8) currentSelect = 0;
        }

        ItemSelectorSprite.rectTransform.anchoredPosition = new Vector2((currentSelect - 4) * 68, 0);
        currentBlocSelected = ListOfAllBlocs.instance.blocs[inventoryBarBlocsIndex[currentSelect]];
    }
}


