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
    
    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround {get; private set;} // = public get(lire) pr heroController, private écriture (HeroEntity)

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling,
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;


    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();

        if(_AreOrientAndMovementOpposite()){
            _TurnBack();
        } else{   
            _UpdateHorizontalSpeed();
            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping){
            _UpdateJump();
        } else {
            if(!IsTouchingGround)
            {    
                _ApplyFallGravity(_fallSettings);
            }
            else {
                _ResetVerticalSpeed();
            }
        }    

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

    private void _ApplyFallGravity(HeroFallSettings settings){
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax){
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }

    private void _ApplyGroundDetection(){
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }

    private void _ResetVerticalSpeed(){
        _verticalSpeed = 0f;
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

    private void _UpdateJump(){
        switch(_jumpState){
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;

            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    private void _UpdateJumpStateImpulsion(){
        _jumpTimer += Time.fixedDeltaTime;
        if(_jumpTimer <_jumpSettings.jumpMaxDuration){
            _verticalSpeed = _jumpSettings.jumpSpeed;
        } else{
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if(!IsTouchingGround){
            _ApplyFallGravity(_jumpFallSettings);
        } else {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    public void JumpStart(){
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }

    public bool IsJumping => _jumpState != JumpState.NotJumping;

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label("OrientX = " + _orientX);
        if(IsTouchingGround){
            GUILayout.Label("OnGround");
        }else{
            GUILayout.Label("InAir");
        }
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label("Vertical Speed =" + _verticalSpeed);
        GUILayout.EndVertical();
    }
}