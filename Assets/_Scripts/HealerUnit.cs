using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HealerUnit : Unit
{
    [Header("Medic Variables")]
    [SerializeField] float healSpeed = 1f;
    [SerializeField] short healValue;
    [SerializeField] float healRange;
    [SerializeField] NavMeshAgent unitNMAgent;
    [SerializeField] Unit.unitState healerUnitState;
    [SerializeField] CircleCollider2D healCollider2D;
    [SerializeField] LineRenderer healCircleRenderer;
    [SerializeField] LayerMask unitsLayer;
    private WaitForSeconds healResetWaiter;
    private WaitForSeconds healCircleWaiter;
    private List<Unit> targetList = new List<Unit>();
    
    protected override void Awake()
    {
        base.Awake();
        canMove = true;
        unitNMAgent.updateRotation = false; // maintain 2d
        unitNMAgent.updateUpAxis = false;   // maintain 2d
        healResetWaiter = new WaitForSeconds(1f/healSpeed - 0.2f);
        healCircleWaiter  = new WaitForSeconds(0.2f);
        
        healCollider2D.radius = healRange;
        healCircleRenderer.enabled = false;
        healCircleRenderer.startColor = Color.green;
        healCircleRenderer.endColor = Color.green;

        targetList.Clear();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // unit is dead
        if (isAlive == false)
        {
            targetList.Clear();
            return;
        }


        // Sorry but below if statement is purely from AI, I didn't know how to handle
        if (healerUnitState == unitState.Moving) 
        {
            if (!unitNMAgent.pathPending && unitNMAgent.remainingDistance <= unitNMAgent.stoppingDistance)
            {
                if (!unitNMAgent.hasPath || unitNMAgent.velocity.sqrMagnitude == 0f)
                {
                    // Arrived! Now it is safe to go to Idle.
                    changeState(unitState.Idle);
                }
            }
        }

        // healing currently, wait for coroutine to finish
        if (healerUnitState == unitState.Healing) return;

        // when the healing coroutine is finished or nothing to heal around
        if (healerUnitState == unitState.Idle)
        {
            // check if there is a combat unit around
            checkTargets();
            if (targetList.Count > 0)
            {
                changeState(unitState.Healing);
                StartCoroutine(healTargets());
            }
            Debug.Log("targets around: " + targetList.Count);
        }
    }

    public override void moveToPosition(Vector3 mousePosition)
    {
        StopAllCoroutines();
        clearHealCircle();
        changeState(unitState.Moving);
        unitNMAgent.SetDestination(mousePosition);
    }

    public override void changeState(unitState state)
    {
        healerUnitState = state;
    }

    public override void gainAggro(Unit newTarget)
    {
        Vector3 closestPointToTarget = newTarget.getCollider2D().ClosestPoint(transform.position);
        moveToPosition(closestPointToTarget);
    }

    public override void loseAggro()
    {
        targetList.Clear();
        changeState(unitState.Idle);
    }

    IEnumerator healTargets()
    {
        // start healing
        renderHealCircle();
        foreach (Unit target in targetList)
        {
            target.heal(healValue);
            Debug.Log("healing " + target);
        }
        // wait for circle reset
        yield return healCircleWaiter;
        clearHealCircle();

        // wait for heal reset
        yield return healResetWaiter;
        changeState(unitState.Idle);
    }

void checkTargets()
    {
        targetList.Clear();
        Collider2D[] detectedColliderList = Physics2D.OverlapCircleAll(
            transform.position,
            healRange,
            unitsLayer
        );

        foreach (Collider2D detectedCollider in detectedColliderList)
        {
            if (detectedCollider.isTrigger) continue; // this is the attack range collider component of the unit
            if (detectedCollider.gameObject == this.gameObject) continue; // don't consider itself

            
            CombatUnit detectedCombatUnit = detectedCollider.GetComponent<CombatUnit>();
            if (detectedCombatUnit != null && detectedCombatUnit.isUnitAlive()) 
            {
                if (!targetList.Contains(detectedCombatUnit)) targetList.Add(detectedCombatUnit);
            }
        }
    }

    void clearHealCircle() { healCircleRenderer.enabled = false; }
    void renderHealCircle()
    {
        int vertexNum = 12;
        int arcNum    = vertexNum - 2; // 12 vertex for a circle creates 10 arcs -> arc = vertex - 1 & its a loop (again -1)
        healCircleRenderer.positionCount = vertexNum;

        float vertexAngle = 0f;
        float arcAngle = 360f / arcNum;

        for (int i = 0; i < vertexNum; i++)
        {
            // give 0.5f for SRenderer offset -> visually more understandable
            float vertexX = transform.position.x + Mathf.Cos(Mathf.Deg2Rad * vertexAngle) * (healRange + 0.5f);
            float vertexY = transform.position.y + Mathf.Sin(Mathf.Deg2Rad * vertexAngle) * (healRange + 0.5f);

            healCircleRenderer.SetPosition(i, new Vector3(vertexX, vertexY, -1));
            
            vertexAngle += arcAngle;
        }

        healCircleRenderer.enabled = true;
    }
}
