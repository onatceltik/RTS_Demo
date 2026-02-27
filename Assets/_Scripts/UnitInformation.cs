using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitInformation
{
    public string unitName;

    public SpriteRenderer unitSRenderer;
    public Transform unitTransform;
    
    public List<GameObject> spawnableUnits;

    // for units that cannot spawn soldiers
    public UnitInformation(
        string _unitName,
        SpriteRenderer _unitSRenderer,
        Transform _unitTransform
    )
    {
        unitName = _unitName;
        unitSRenderer = _unitSRenderer;
        unitTransform = _unitTransform;
        spawnableUnits = null;
    }
    
    // for units that can spawn soldiers
    public UnitInformation(
        string _unitName,
        SpriteRenderer _unitSRenderer,
        Transform _unitTransform,
        List<GameObject> _spawnableUnits
    )
    {
        unitName = _unitName;
        unitSRenderer = _unitSRenderer;
        unitTransform = _unitTransform;
        spawnableUnits = _spawnableUnits;
    }
}