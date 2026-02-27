using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    [Header("Unit Info")]
    [SerializeField] protected string unitName;
    [SerializeField] protected SpriteRenderer unitSRenderer;
    [SerializeField] protected Collider2D unitCollider2D;
    [SerializeField] protected Slider healthBarSlider;
    
    [Header("Unit Stats")]
    [SerializeField] protected short unitMaxHealth;
    [SerializeField] protected short unitCurrentHealth;
    [SerializeField] protected bool isAlive = true;
    [SerializeField] protected bool canMove;

    public enum unitState {Idle, Moving, Attacking, Healing};
    private WaitForSeconds destroyAfter2Sec;
    // Color attributes and statics
    static private Color TINT_RED = new Color(1f, 0.5f, 0.5f, 0.6f);
    static private Color TINT_BLUE = new Color(0.5f, 0.5f, 1f, 0.6f);
    static private Color TINT_YELLOW = new Color(1f, 1f, 0.5f, 0.6f);
    static private Color TINT_BLACK = new Color(0f, 0f, 0f, 0.6f);
    static private Color TINT_GHOSTED = new Color(1f, 1f, 1f, 0.6f);

    // Start is called before the first frame update
    protected virtual void Start()
    {
        healthBarSlider.maxValue = unitMaxHealth;
        healthBarSlider.value = unitMaxHealth;
    }

    // Update is called once per frame
    protected virtual void Update(){}
    protected virtual void Awake()
    {
        unitSRenderer = GetComponent<SpriteRenderer>();
        unitCurrentHealth = unitMaxHealth;
        destroyAfter2Sec = new WaitForSeconds(2f);
    }
    
    // One line methods
    public bool isUnitAlive() { return isAlive; }
    public bool canUnitMove() { return canMove; }
    public Collider2D getCollider2D() { return unitCollider2D;}
    public virtual List<GameObject> getSpawnablesList() { return null; }
    public virtual void moveToPosition(Vector3 mousePosition) {}
    public virtual void changeState(CombatUnit.unitState state) {}
    public virtual void gainAggro(Unit newTarget) {}
    public virtual void loseAggro() {}

    public UnitInformation getUnitInformation()
    {
        UnitInformation tempUnitInformation = null;
        
        // if this is a building and can spawn units
        if (TryGetComponent<BuildingUnit>(out BuildingUnit buildingObject) && buildingObject.canSpawnUnits())
        {
            // if (buildingObject.canSpawnUnits())
            // {
            tempUnitInformation = new UnitInformation(
                unitName, unitSRenderer, GetComponent<Transform>(), buildingObject.getSpawnablesList()
            );
            return tempUnitInformation;
            // }
        }
        // otherwise
        tempUnitInformation = new UnitInformation(
            unitName, unitSRenderer, GetComponent<Transform>()
        );

        return tempUnitInformation;
    }

    // Color methods
    public void setUnitColorTint(string colorName)
    {
        switch (colorName)
        {
            case "red_tint": unitSRenderer.color = TINT_RED; break;
            case "blue_tint": unitSRenderer.color = TINT_BLUE; break;
            case "black_tint": unitSRenderer.color = TINT_BLACK; break;
            case "yellow_tint": unitSRenderer.color = TINT_YELLOW; break;
            case "ghosted": unitSRenderer.color = TINT_GHOSTED; break;
            case "reset": unitSRenderer.color = Color.white; break;
        }
    }

    protected virtual IEnumerator killUnit()
    {
        isAlive = false;
        setUnitColorTint("black_tint");
        
        yield return destroyAfter2Sec;
        Destroy(gameObject);
    }

    public void takeDamage(short damageValue)
    {
        if (unitCurrentHealth <= damageValue)
        {
            unitCurrentHealth = 0;
            healthBarSlider.value = unitCurrentHealth;
            StartCoroutine(killUnit());
        } else
        {
            unitCurrentHealth -= damageValue;
            healthBarSlider.value = unitCurrentHealth;
        }
    }    
    
    public void heal(short healValue)
    {
        // unit is dead, cannot get heal
        if (isAlive == false) return;

        if (unitCurrentHealth + healValue >= unitMaxHealth)
        {
            unitCurrentHealth = unitMaxHealth;
        } else
        {
            unitCurrentHealth += healValue;
        }
        healthBarSlider.value = unitCurrentHealth;
    }
}
