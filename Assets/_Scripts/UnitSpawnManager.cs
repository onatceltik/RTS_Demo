using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

// using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class UnitSpawnManager : MonoBehaviour
{
    public static UnitSpawnManager instance { get; private set; }
    [SerializeField] GridManager gridManager;
    
    // side offsets to snap soldiers to the grid
    Vector2 bottomOffset = new Vector2(0.5f, -0.5f); // (+, -)
    Vector2 rightOffset  = new Vector2(0.5f, 0.5f); // (+, +)
    Vector2 topOffset    = new Vector2(-0.5f, 0.5f); // (-, +)
    Vector2 leftOffset   = new Vector2(-0.5f, -0.5f); // (-, -)

    private void Awake()
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
    
    public void spawnUnit(UnitInformation spawnerUnitInfo, GameObject spawningUnitPrefab)
    {
        UnityEngine.Debug.Log("spawning unit pos: " + spawnerUnitInfo.unitTransform.position);
        UnityEngine.Debug.Log("spawning unit size: " + spawnerUnitInfo.unitSRenderer.size);
        UnityEngine.Debug.Log("spawned unit: " + spawningUnitPrefab);


        // area check
        Vector2? availablePosition = getAvailablePlace(
            spawnerUnitInfo.unitTransform.position,
            spawnerUnitInfo.unitSRenderer.size
        );

        // place unit
        if (availablePosition != null)
        {
            GameObject unitInstance = Instantiate(spawningUnitPrefab);
            Vector3 availablePosition3D = availablePosition.Value;
            availablePosition3D.z = -1;

            // change the unit's layer to buildings layer from preview layer
            int layerID = LayerMask.NameToLayer("Units");
            Transform unitTransform = unitInstance.GetComponent<Transform>();
            unitTransform.gameObject.layer = layerID;
            unitTransform.transform.position = availablePosition3D;
            
            // change its hovering color tint
            SpriteRenderer unitSRenderer = unitInstance.GetComponent<SpriteRenderer>();
            unitSRenderer.sortingLayerName = "Units";

            // enable navmesh components when placing
            if (unitInstance.TryGetComponent<NavMeshObstacle>(out NavMeshObstacle unitNMObstacle)) unitNMObstacle.enabled = true;
            if (unitInstance.TryGetComponent<NavMeshAgent>(out NavMeshAgent unitNMAgent)) unitNMAgent.enabled = true; 
        }
        else
        {
            // show a text to user 
            Debug.Log("trigger alert");
            AlertMessageManager.triggerAlert(AlertMessageManager.AlertType.NoEmptySpaceOnSpawn);
        }
    }

    Vector2? getAvailablePlace(Vector3 spawnerPosition, Vector2 spawnerSize)
    {
        // Assuming the producable soldier units have 1x1 size, otherwise this code is needed to be revise
        Vector2 soldierSize = new Vector2(0.9f, 0.9f);

        // convert position and size to int
        Vector2 spawnerPosition2 = Vector2Int.RoundToInt(spawnerPosition);
        float halfWidth  = Vector2Int.RoundToInt(spawnerSize).x/2;
        float halfHeight = Vector2Int.RoundToInt(spawnerSize).y/2;

        // spawner vertices
        Vector2Int bottomLeftVertex  = Vector2Int.RoundToInt(spawnerPosition2 + new Vector2(-halfWidth, -halfHeight));
        Vector2Int bottomRightVertex = Vector2Int.RoundToInt(spawnerPosition2 + new Vector2(halfWidth, -halfHeight));
        Vector2Int topLeftVertex     = Vector2Int.RoundToInt(spawnerPosition2 + new Vector2(-halfWidth, halfHeight));
        Vector2Int topRightVertex    = Vector2Int.RoundToInt(spawnerPosition2 + new Vector2(halfWidth, halfHeight));

        // first try bottom
        Vector2 tempPosition = bottomLeftVertex + bottomOffset;
        for (float i = bottomLeftVertex.x; i <= bottomRightVertex.x; i++, tempPosition.x++)
        {
            if (!gridManager.isTileOccupied(new Vector2(tempPosition.x, tempPosition.y), soldierSize))
            {
                return tempPosition;
            }
        }
        Debug.Log("No available pos. on bottom");

        // then try right
        tempPosition = bottomRightVertex + rightOffset;
        for (float i = bottomRightVertex.y; i <= topRightVertex.y; i++, tempPosition.y++)
        {
            if (!gridManager.isTileOccupied(new Vector2(tempPosition.x, tempPosition.y), soldierSize))
            {
                return tempPosition;
            }
        }
        Debug.Log("No available pos. on right");

        // then try top
        tempPosition = topRightVertex + topOffset;
        for (float i = topRightVertex.x; i >= topLeftVertex.x; i--, tempPosition.x--)
        {
            if (!gridManager.isTileOccupied(new Vector2(tempPosition.x, tempPosition.y), soldierSize))
            {
                return tempPosition;
            }
        }
        Debug.Log("No available pos. on top");

        // then try top
        tempPosition = topLeftVertex + leftOffset;
        for (float i = topLeftVertex.y; i >= bottomLeftVertex.y; i--, tempPosition.y--)
        {
            if (!gridManager.isTileOccupied(new Vector2(tempPosition.x, tempPosition.y), soldierSize))
            {
                return tempPosition;
            }
        }
        Debug.Log("No available pos. on left");

        return null;
    }
}
