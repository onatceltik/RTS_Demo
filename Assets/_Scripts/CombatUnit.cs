using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CombatUnit : Unit
{
    [Header("Combat Variables")]
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] short attackDamage;
    [SerializeField] float attackRange;
    [SerializeField] NavMeshAgent unitNMAgent;
    [SerializeField] Unit.unitState combatUnitState;
    [SerializeField] CircleCollider2D attackCollider2D;
    [SerializeField] LineRenderer attackLineRenderer;
    private WaitForSeconds attackResetWaiter;
    private WaitForSeconds attackLineWaiter;
    private Unit target;
    
    protected override void Awake()
    {
        base.Awake();
        canMove = true;
        unitNMAgent.updateRotation = false; // maintain 2d
        unitNMAgent.updateUpAxis = false;   // maintain 2d
        attackResetWaiter = new WaitForSeconds((1f / attackSpeed) - 0.2f);
        attackLineWaiter = new WaitForSeconds(0.2f);
        
        attackCollider2D.radius = attackRange;
        attackLineRenderer.positionCount = 2;
        attackLineRenderer.enabled = false;
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
            target = null;
            return;
        }

        // unit is in combat
        if (combatUnitState == unitState.Attacking)
        {
            return; // do nothing in attacking state to be safe, it may mess up the states otherwise
        }
        
        // Sorry but below if statement is purely from AI, I didn't know how to handle
        if (combatUnitState == unitState.Moving) 
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
 
        if (target == null)
        {
            changeState(unitState.Idle);
        }
        else // attacks
        {
            if (!target.isUnitAlive())
            {
                loseAggro();
            }
            // unit's not moving and have a target
            else if (attackCollider2D.IsTouching(target.getCollider2D()))
            {
                // target is in range -> attack
                unitNMAgent.velocity = Vector3.zero;
                unitNMAgent.ResetPath();
                StartCoroutine(attackTarget());
            }
            // unit stopped because it couldn't reach the target
            // or not in the range -> try to reach it
            else
            {
                gainAggro(target);
            }
        }
    }

    public override void moveToPosition(Vector3 mousePosition)
    {
        changeState(unitState.Moving);
        unitNMAgent.SetDestination(mousePosition);
    }

    public override void changeState(unitState state)
    {
        combatUnitState = state;
    }

    public override void gainAggro(Unit newTarget)
    {
        // cancel attack coroutines
        // it carries over the attacks to the next target if current one dies
        StopAllCoroutines();
        target = newTarget;
        Vector3 closestPointToTarget = target.getCollider2D().ClosestPoint(this.transform.position);
        moveToPosition(closestPointToTarget);
    }

    public override void loseAggro()
    {
        target = null;
        changeState(unitState.Idle);
    }

    IEnumerator attackTarget()
    {
        // start attacking
        changeState(unitState.Attacking);
        renderAttackLine();
        target.takeDamage(attackDamage);
        yield return attackLineWaiter;
        clearAttackLine();

        // wait for attack reset
        yield return attackResetWaiter;
        changeState(unitState.Idle);
    }

    void clearAttackLine() { attackLineRenderer.enabled = false; }
    void renderAttackLine()
    {
        if (target != null)
        {
            attackLineRenderer.SetPosition(0, transform.position);
            attackLineRenderer.SetPosition(1, target.transform.position);
            attackLineRenderer.enabled = true;
        }
    }
}
