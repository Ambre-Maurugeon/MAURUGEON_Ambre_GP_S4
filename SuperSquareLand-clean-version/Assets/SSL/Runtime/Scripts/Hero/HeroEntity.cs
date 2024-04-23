using UnityEngine;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [SerializeField] private HeroHorizontalMovementSettings _movementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;
    //private bool canDash=true;

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void FixedUpdate()
    {
        if(_AreOrientAndMovementOpposite()){
            _TurnBack();
        } else{   
            _UpdateHorizontalSpeed();
            _ChangeOrientFromHorizontalMovement();
        }
        _ApplyFallGravity();
        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
    }

    private void _UpdateHorizontalSpeed(){
            if(_moveDirX != 0f){
                // if(Input.GetKey(KeyCode.E) && canDash){
                //     _Dash();
                //     canDash=false;
                //     Invoke("_StopDash()", 2f);
                // }
                // else {  
                    _Accelerate();
                //}
            }
            else{
                _Decelerate();
            }
        }
    
    private void _Accelerate(){
        _horizontalSpeed += _movementsSettings.acceleration * Time.fixedDeltaTime;
        if(_horizontalSpeed > _movementsSettings.speedMax){   
            _horizontalSpeed =_movementsSettings.speedMax;
        }
    }

    private void _Decelerate(){
        _horizontalSpeed -= _movementsSettings.deceleration * Time.fixedDeltaTime;
        if(_horizontalSpeed < 0f){   
            _horizontalSpeed = 0f;
        }
    }

    // private void _Dash(){
    //     _horizontalSpeed += _movementsSettings.dash * Time.fixedDeltaTime;
    //     if(_horizontalSpeed > _movementsSettings.dashMax){  
    //         _Accelerate();
    //     }
    // }

    // void _StopDash(){
    //     canDash=true;
    // }

    private void _ChangeOrientFromHorizontalMovement(){
        if(_moveDirX == 0f) return ; // et si pas d'accolades le if execute juste la ligne d'apres 
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _ApplyFallGravity(){
        _verticalSpeed -= _fallSettings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -_fallSettings.fallSpeedMax){
            _verticalSpeed = -_fallSettings.fallSpeedMax;
        }
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed*_orientX;
        _rigidbody.velocity = velocity;
    }

    private void _ApplyVerticalSpeed(){
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

    private void _TurnBack(){
        _horizontalSpeed -= _movementsSettings.turnBackFrictions *Time.fixedDeltaTime;
        if(_horizontalSpeed <0f){
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }

    private bool _AreOrientAndMovementOpposite(){
        return _moveDirX * _orientX <0f;
    }
    
    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label("OrientX = " + _orientX);
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label("Vertical Speed =" + _verticalSpeed);
        GUILayout.EndVertical();
    }
}