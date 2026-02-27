using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUnit : Unit
{
    [SerializeField] bool canSpawn;
    [SerializeField] List<GameObject> spawnablePrefabs;

    protected override void Awake()
    {
        base.Awake();
        canMove = false;
        if (canSpawn == false)
        {
            spawnablePrefabs = null;
        }
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
    }

    public bool canSpawnUnits(){ return canSpawn; }
    public override List<GameObject> getSpawnablesList(){ return spawnablePrefabs; }
}
