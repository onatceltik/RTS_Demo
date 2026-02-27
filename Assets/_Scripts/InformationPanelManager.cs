using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;

public class InformationPanelManager : MonoBehaviour
{
    [Header("Subbed to")]
    [SerializeField] UnitCommandManagerRevised unitSelectionManager;
    
    [Header("Refers to")]
    [SerializeField] UnitSpawnManager unitSpawnManager;

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI uiUnitName;
    [SerializeField] Image uiUnitImage;

    [Header("Spawn Button Related")]
    [SerializeField] GameObject spawnButtonPrefab;
    [SerializeField] Transform contentContainer;
    [SerializeField] TextMeshProUGUI uiUnitProductionsTitle;
    List<GameObject> spawnButtonList;
    Unit shownUnit = null;

    // Start is called before the first frame update
    void Start()
    {
        if (unitSelectionManager != null)
        {
            unitSelectionManager.OnUnitSelect += showUnitInfo;
            unitSelectionManager.OnUnitDeselect += clearUnitInfo;
        }
        uiUnitName.SetText(string.Empty);
        uiUnitImage.enabled = false;
        spawnButtonList = new List<GameObject>();
    }

    void Update()
    {
        // check if the object is alive
        if (shownUnit != null && !shownUnit.isUnitAlive()) clearUnitInfo(EventArgs.Empty);
    }

    void showUnitInfo(Unit unitToBeShown)
    {
        clearUnitInfo(EventArgs.Empty);

        // This is absolutely a trash design and I am not proud of this,
        // but giving the object itself was the fastest way 
        // to inform the information panel on the life of the unit.
        // I need to redesign the information passing system utilizing events,
        // but it will take time and possibly generate errors,
        // and I don't want to risk the project. :'(
        // So if it works, don't fix it.
        shownUnit = unitToBeShown;
        UnitInformation unitInfo = unitToBeShown.getUnitInformation();

        uiUnitName.SetText(unitInfo.unitName);
        uiUnitImage.sprite = unitInfo.unitSRenderer.sprite;
        uiUnitImage.enabled = true;

        if (unitInfo.spawnableUnits != null && unitInfo.spawnableUnits.Count > 0)
        {
            uiUnitProductionsTitle.enabled = true;
            
            foreach (GameObject spawnableUnit in unitInfo.spawnableUnits)
            {
                GameObject buttonObject = Instantiate(spawnButtonPrefab, contentContainer);
                buttonObject.transform.localScale = Vector3.one;
                spawnButtonList.Add(buttonObject);

                TextMeshProUGUI buttonObjectText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonObjectText != null) buttonObjectText.SetText(spawnableUnit.name);

                Image buttonObjectImage = buttonObject.GetComponentInChildren<Image>();
                if (buttonObjectImage != null)
                {
                    buttonObjectImage.sprite = spawnableUnit.GetComponent<SpriteRenderer>().sprite;
                    buttonObjectImage.type = Image.Type.Simple;
                }

                Button buttonObjectComp = buttonObject.GetComponentInChildren<Button>();                
                if (buttonObjectComp != null) buttonObjectComp.onClick.AddListener(() => unitSpawnManager.spawnUnit(unitInfo, spawnableUnit));
            }
        }
    }

    void clearUnitInfo(EventArgs e)
    {
        uiUnitName.SetText(string.Empty);
        uiUnitImage.enabled = false;
        uiUnitProductionsTitle.enabled = false;
        destroyButtons();
    }

    void destroyButtons()
    {
        if (spawnButtonList != null)
        {
            foreach (GameObject spawnButton in spawnButtonList)
            {
                if (spawnButton != null) Destroy(spawnButton);
            }
            spawnButtonList.Clear();
        }
    }
}
