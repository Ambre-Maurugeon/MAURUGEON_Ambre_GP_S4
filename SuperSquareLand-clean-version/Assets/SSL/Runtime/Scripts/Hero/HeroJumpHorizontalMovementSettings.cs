using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class HeroJumpHorizontalMovementSettings
{
    public float acceleration = 20f;
    public float deceleration = 1f;

    public float turnBackFrictions = 200f;
    public float speedMax=7f;
}
