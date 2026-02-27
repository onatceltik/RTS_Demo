using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitPlacementManager : MonoBehaviour
{
    public static UnitPlacementManager instance { get; private set; }
    [SerializeField] GridManager gridManager;

    // placement logic
    [SerializeField] GameObject unitInstance;
    [SerializeField] bool isPlacing = false;
    [SerializeField] bool placementDone = false;

#region Events
    public event Action OnPlacementStart;
#endregion

    private void Awake() // Will be used only when it is called unlike Update()
    {
        // Singleton Initialization
        // '-> There should be only one map: recreate it if already exists, create if not
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlacing) return;
        
        if (placementDone) // the unit is placed, only wait for relasing left mouse button
        {
            if (Input.GetMouseButtonUp(0))
            {
                isPlacing = false;
                placementDone = false;
                unitInstance = null;
            }
            StartCoroutine(waitFrameEnding());
            return;
        }
        
        if (unitInstance == null) return; // just to be safe

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC is pressed while placing");
            haltPlacement();
            return;
        }
        
        // check if mouse is on the UI (side panels)
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // make instance follow the mouse position
        Vector3 tilemapPosition = gridManager.getMouseTilemapPosition();
        unitInstance.transform.position = gridOffsetFix(tilemapPosition);

        if (!gridManager.isTileOccupied(unitInstance))
        {
            // Tile is empty
            unitInstance.GetComponent<Unit>().setUnitColorTint("ghosted");
            if (Input.GetMouseButtonDown(0)) placeUnit();
        }
        else
        {
            // Tile is occupied
            unitInstance.GetComponent<Unit>().setUnitColorTint("red_tint");
            if (Input.GetMouseButtonDown(0)) AlertMessageManager.triggerAlert(AlertMessageManager.AlertType.InvalidPlacement);
        }
    }

    public void startPlacement(GameObject unitPrefab)
    {
        OnPlacementStart?.Invoke();

        // if there is already a unit instatiated
        if (isPlacing == true) Destroy(unitInstance);

        isPlacing = true;
        unitInstance = Instantiate(unitPrefab);
        unitInstance.GetComponent<Unit>().setUnitColorTint("ghosted");
    }

    public void placeUnit()
    {
        // change the unit's layer to buildings layer from preview layer
        int layerID = LayerMask.NameToLayer("Units");
        Transform unitTransform = unitInstance.GetComponent<Transform>();
        unitTransform.gameObject.layer = layerID;
        
        // change its hovering color tint
        unitInstance.GetComponent<Unit>().setUnitColorTint("reset");
        SpriteRenderer unitSRenderer = unitInstance.GetComponent<SpriteRenderer>();
        unitSRenderer.sortingLayerName = "Units";

        // enable navmesh components when placing
        if (unitInstance.TryGetComponent<NavMeshObstacle>(out NavMeshObstacle unitNMObstacle)) unitNMObstacle.enabled = true;
        if (unitInstance.TryGetComponent<NavMeshAgent>(out NavMeshAgent unitNMAgent)) unitNMAgent.enabled = true; 
        
        // Make the flag change wait for the end of the 
        // frame to not conflict with unit selection
        placementDone = true;
    }

    void haltPlacement()
    {
        if (unitInstance != null) Destroy(unitInstance);
        isPlacing = false;
        placementDone = false;
        unitInstance = null;
    }

    public bool placementLock()
    {
        return isPlacing;
    }

    // if a unit's size, either single axis or both, is odd value it stays inbetween cells.
    // decrease the position value by 0.5 to snap it back to the grid.    
    public Vector3 gridOffsetFix(Vector3 tilemapPosition)
    {
        if (unitInstance != null)
        {
            SpriteRenderer unitSRenderer = unitInstance.GetComponent<SpriteRenderer>();
            int unitSizeX = Mathf.RoundToInt(unitSRenderer.size.x);
            int unitSizeY = Mathf.RoundToInt(unitSRenderer.size.y);

            // x size is odd
            if (unitSizeX % 2 == 1) tilemapPosition.x += 0.5f;
            
            // y size is odd
            if (unitSizeY % 2 == 1) tilemapPosition.y += 0.5f;
        }

        return tilemapPosition;
    }

    private IEnumerator waitFrameEnding()
    {
        yield return new WaitForEndOfFrame();
    }
}
