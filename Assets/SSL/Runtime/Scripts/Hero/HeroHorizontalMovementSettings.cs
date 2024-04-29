using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class HeroHorizontalMovementSettings
{
    public float acceleration = 20f;
    public float speedMax = 5f;

    [Header("Horizontal Movements")]
    [SerializeField] private HeroHorizontalMovementSettings _movementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    private float _Accelerate()
    {
        _horizontalSpeed += _movementsSettings.acceleration * Time.fixedDeltaTime;
    }

    private float _UpdateHorizontalSpeed()
    {
        if (_moveDirX != 0f)
        {
            _Accelerate();
        } else
        {
            _horizontalSpeed = 0f;
        }
    }

    private void FixedUpdate() 
    {
        _UpdateHorizontalSpeed();

    }
}
