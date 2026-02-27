using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GridManager : MonoBehaviour
{
    public static GridManager instance { get; private set; }

    public LayerMask unitsLayer;
    public LayerMask outerMapLayer;

    [Header("Map Settings")]
    [SerializeField] private Tilemap groundTilemap;

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

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = new Vector3(0, 0, -10);

        // other initializations
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 getMouseTilemapPosition()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseTilemapPosition = groundTilemap.WorldToCell(mouseWorldPosition);
        mouseTilemapPosition = new Vector3(mouseTilemapPosition.x, mouseTilemapPosition.y, -1);

        return mouseTilemapPosition;
    }

    public bool isTileOccupied(GameObject unitToBePlaced)
    {
        SpriteRenderer unitRenderer = unitToBePlaced.GetComponent<SpriteRenderer>();
        Collider2D[] hits = Physics2D.OverlapBoxAll(unitToBePlaced.transform.position, unitRenderer.size, 0, unitsLayer);

        foreach (Collider2D hit in hits)
        {
            // attack range is hit
            if(hit.isTrigger == true) continue;
            
            // unit body is hit
            else return hit;
        }

        // no unit. so, outer map?
        Collider2D hitOuterMap = Physics2D.OverlapBox(unitToBePlaced.transform.position, unitRenderer.size, 0, outerMapLayer);
        return hitOuterMap;
    }

    public bool isTileOccupied(Vector2 tilePos, Vector2 unitSize)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(tilePos, unitSize, 0, unitsLayer);

        foreach (Collider2D hit in hits)
        {
            // attack range is hit
            if(hit.isTrigger == true) continue;
            
            // unit body is hit
            else return true;
        }

        // no unit. so, outer map?
        Collider2D hitOuterMap = Physics2D.OverlapBox(tilePos, unitSize, 0, outerMapLayer);
        return hitOuterMap != null;
    }
}
