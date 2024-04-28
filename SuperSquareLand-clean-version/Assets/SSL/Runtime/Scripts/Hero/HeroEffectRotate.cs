using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEffectRotate : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    [Header("Object to Rotate")]
    [SerializeField] private Transform _objectToRotate;

    [Header("Rotate Loop")]
    [SerializeField] private float rotationPeriod;
    [SerializeField] private float rotationFactor = 10f;

    private float _rotateTimer;
    private float _currentRotation;

    private void Update(){
        if(_entity.IsTouchingGround && !_entity.IsDashing){
            if(_entity.IsHorizontalMoving){
                _rotateTimer += Time.deltaTime;
                float percent = Mathf.PingPong(_rotateTimer, rotationPeriod)/rotationPeriod;
                float newRotation = Mathf.Lerp(-rotationFactor, rotationFactor, percent);
                _SetObjectToRotateDelta(newRotation);
            } else{
                _SetObjectToRotateDelta(0f);
            }
        } else{
            _SetObjectToRotateDelta(0f);
        }
    }

    private void _SetObjectToRotateDelta(float rotation){
        Vector3 newEulerAngles = _objectToRotate.localEulerAngles;
        newEulerAngles.z -= _currentRotation;
        _currentRotation = rotation;
        newEulerAngles.z  += _currentRotation;
        _objectToRotate.localEulerAngles = newEulerAngles;
    }
}
