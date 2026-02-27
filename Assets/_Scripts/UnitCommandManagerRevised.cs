using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitCommandManagerRevised : MonoBehaviour
{
    public static UnitCommandManagerRevised instance { get; private set; }

    [SerializeField] UnitPlacementManager unitPlacementManager;
    [SerializeField] Unit detectedUnit = null;
    [SerializeField] Unit hoveredUnit = null; // this is unnecessary right now but deleting it will create problems so I left it as it is (check this later)
    [SerializeField] Unit selectedUnit = null;
    [SerializeField] List<Unit> selectedUnitList = null;
    [SerializeField] LayerMask unitsLayer;
    [SerializeField] RectTransform selectionBox;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] float mouseDragThreshold;
    bool canHover;
    bool mousePressedOnUI;

    [SerializeField] bool isDragging;
    Vector2 mouseLeftButtonPressedPos;

#region Events
    // public event Action<UnitInformation> OnUnitSelect;
    public event Action<Unit> OnUnitSelect;
    public event Action<EventArgs> OnUnitDeselect;
#endregion

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

    // Start is called before the first frame update
    void Start()
    {
        if (unitPlacementManager != null){
            unitPlacementManager.OnPlacementStart += deselectUnit;
            unitPlacementManager.OnPlacementStart += deselectMultipleUnits;
        }
        selectedUnitList = new List<Unit>();
        canHover = true;
        mousePressedOnUI = false;
    }

    // Update is called once per frame
    void Update()
    {
        // check if placement manager is not working this frame (conflicts with selectUnit())
        if (unitPlacementManager.placementLock() == true)
        {
            if (selectedUnit != null) deselectUnit();
            if (selectedUnitList != null && selectedUnitList.Count > 0) deselectMultipleUnits();
            return;
        }

        // check if mouse is on the UI (side panels)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            dehoverUnit(); // just to be safe
            
            // right-clicked on UI, currently nothing happens
            if (Input.GetMouseButtonDown(1)) return;

            // left-button pressed while on UI
            if (Input.GetMouseButtonDown(0))
            {
                mousePressedOnUI = true;
            }
            // left-button released while on UI
            if (Input.GetMouseButtonUp(0))
            {
                // if while placing a unit, it doesn't even get here
                clearDragging();
            }
            return;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // check if mouse is on a unit or on the ground
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D detectedCollider = Physics2D.OverlapPoint(mouseWorldPosition, unitsLayer);

        // check what the player is doing right now: left-click, right-click, dragging

        // the player pressed the left button first time
        // might just click or drag, save the mouse position to check th
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = false;
            mousePressedOnUI = false;
            mouseLeftButtonPressedPos = Input.mousePosition; // record screen space coord (learn more on those spaces!)
        }

        // left-button is held
        else if (Input.GetMouseButton(0) && !mousePressedOnUI)
        {
            // is already dragging
            if (isDragging) resizeSelectionBox(Input.mousePosition);

            // started dragging
            else if (Vector2.Distance(mouseLeftButtonPressedPos, Input.mousePosition) > mouseDragThreshold)
            {
                isDragging = true;
                canHover = false;
                selectionBox.gameObject.SetActive(true);
            }
        } 
        

        // the player released the left button
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging) // was dragging
            {
                // select multiple units
                deselectMultipleUnits();
                selectMultipleUnits(mouseWorldPosition);

                // reset the selection box
                clearDragging();
            }
            else // clicked on the board
            {
                if (detectedCollider == null || detectedCollider.isTrigger == true)
                {
                    dehoverUnit();
                    deselectUnit();
                    deselectMultipleUnits();
                } else
                {
                    detectedUnit = detectedCollider.GameObject().GetComponent<Unit>();
                    selectUnit();
                }
            }
        }


        // right-button is clicked
        if (Input.GetMouseButtonDown(1))
        {
            // clicked on an empty space -> move there
            if (hoveredUnit == null)
            {
                // there is only one soldier selected
                if (selectedUnit != null)
                {
                    selectedUnit.loseAggro();
                    selectedUnit.moveToPosition(mouseWorldPosition);
                }
                // there are many soldiers selected
                else if (selectedUnitList.Count() > 0)
                {
                    foreach (Unit unitInList in selectedUnitList)
                    {
                        unitInList.loseAggro();
                        unitInList.moveToPosition(mouseWorldPosition);
                    }
                }
            }
            // clicked on a unit -> attack it
            else
            {
                // there is only one soldier selected
                if (selectedUnit != null && hoveredUnit.isUnitAlive())
                {
                    selectedUnit.gainAggro(hoveredUnit);
                }
                // there are many soldiers selected
                else if (selectedUnitList.Count() > 0 && hoveredUnit.isUnitAlive() && !selectedUnitList.Contains(hoveredUnit))
                {
                    foreach (Unit unitInList in selectedUnitList)
                    {
                        unitInList.gainAggro(hoveredUnit);
                    }
                }
            }
        }

        // mouse is on an empty space
        if (detectedCollider != null && detectedCollider.isTrigger == false)
        {
            detectedUnit = detectedCollider.GameObject().GetComponent<Unit>();
            if (detectedUnit != hoveredUnit && canHover) hoverUnit();
        } else
        {
            detectedUnit = null; // reset the pointer
            dehoverUnit();
        }
    }

    void clearDragging()
    {
        isDragging = false;
        canHover = true;
        selectionBox.gameObject.SetActive(false);
        selectionBox.sizeDelta = new Vector2(0,0);
        selectionBox.position = new Vector2(0,0);
    }

    void resizeSelectionBox(Vector2 currMouseScreenPosition)
    {
        float minX = Mathf.Min(mouseLeftButtonPressedPos.x, currMouseScreenPosition.x);
        float minY = Mathf.Min(mouseLeftButtonPressedPos.y, currMouseScreenPosition.y);
        float maxX = Mathf.Max(mouseLeftButtonPressedPos.x, currMouseScreenPosition.x);
        float maxY = Mathf.Max(mouseLeftButtonPressedPos.y, currMouseScreenPosition.y);

        Vector2 lowerLeftVertex  = new Vector2(minX, minY);
        Vector2 selectionBoxSize = new Vector2(maxX-minX, maxY-minY);

        // if not divides, canvas scale breaks the selection box transformation
        selectionBox.sizeDelta = selectionBoxSize / mainCanvas.scaleFactor;
        selectionBox.position = lowerLeftVertex + (selectionBoxSize/2f); // middle point
    }

    void selectMultipleUnits(Vector2 mouseWorldPosition)
    {
        deselectUnit();

        // get selection box world size
        Vector2 selectionBoxWorldPivotVertex = Camera.main.ScreenToWorldPoint(mouseLeftButtonPressedPos);
        Vector2 selectionBoxWorldSize = new Vector2(
            Mathf.Abs(selectionBoxWorldPivotVertex.x - mouseWorldPosition.x), 
            Mathf.Abs(selectionBoxWorldPivotVertex.y - mouseWorldPosition.y)
        );

        Collider2D[] detectedColliderList = Physics2D.OverlapBoxAll(
            Camera.main.ScreenToWorldPoint(selectionBox.position),
            selectionBoxWorldSize,
            0f,
            unitsLayer
        );

        foreach (Collider2D detectedCollider in detectedColliderList)
        {
            if (detectedCollider.isTrigger) continue; // this is the attack range collider component of the unit

            Unit detectedUnit = detectedCollider.GameObject().GetComponent<CombatUnit>();
            if (detectedUnit == null) detectedUnit = detectedCollider.GameObject().GetComponent<HealerUnit>();

            if (detectedUnit != null && detectedUnit.isUnitAlive()) // it is a combat unit
            {
                detectedUnit.setUnitColorTint("blue_tint");
                selectedUnitList.Add(detectedUnit);
            }
        }
    }

    void deselectMultipleUnits()
    {
        foreach (Unit unitInList in selectedUnitList)
        {
            unitInList.setUnitColorTint("reset");
        }
        selectedUnitList.Clear();
    }

    void selectUnit()
    {
        dehoverUnit();
        deselectUnit();
        deselectMultipleUnits();

        if (detectedUnit != null)
        {
            selectedUnit = detectedUnit;
            if(detectedUnit.isUnitAlive()) selectedUnit.setUnitColorTint("blue_tint");
        }

        // OnUnitSelect?.Invoke(selectedUnit.getUnitInformation());
        OnUnitSelect?.Invoke(selectedUnit);
    }
    
    void deselectUnit()
    {
        if (selectedUnit == null) { return; }
        if(selectedUnit.isUnitAlive()) selectedUnit.setUnitColorTint("reset");
        OnUnitDeselect?.Invoke(EventArgs.Empty);
        selectedUnit = null;
    }

    void hoverUnit()
    {
        if (detectedUnit != null) // aka another unit is hovered
        {
            dehoverUnit();
            hoveredUnit = detectedUnit;
            if(detectedUnit.isUnitAlive()) hoveredUnit.setUnitColorTint("yellow_tint");
        }
    }

    void dehoverUnit()
    {
        if (hoveredUnit == null) return;
        if (hoveredUnit == selectedUnit || selectedUnitList.Contains(hoveredUnit)) hoveredUnit.setUnitColorTint("blue_tint");
        else if(hoveredUnit.isUnitAlive()) hoveredUnit.setUnitColorTint("reset");
        hoveredUnit = null;
    }
}
