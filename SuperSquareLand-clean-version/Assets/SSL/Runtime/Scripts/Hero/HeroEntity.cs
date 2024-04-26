using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_movementsSettings")]

    [SerializeField] private HeroHorizontalMovementsSettings _groundHorizontalMovementsSettings;
    [SerializeField] private HeroHorizontalMovementsSettings _airHorizontalMovementsSettings;

    [HideInInspector]
    public HeroHorizontalMovementsSettings horizontalMovementSettings;

    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Dash")]
    [FormerlySerializedAs("_dashSettings")]
    [SerializeField] private HeroDashSettings _dashSettingsOnGround;
    [SerializeField] private HeroDashSettings _dashSettingsInAir;

    [HideInInspector]
    public bool IsDashing = false;

    [HideInInspector] 
    public HeroDashSettings dashSettings;

    private float _dashTimer;
    private TrailRenderer tr;

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

    [Header("Sliding")]
    [SerializeField] private float _normalSlidingVerticalSpeed = -1.2f;
    [SerializeField] private float _downSlidingVerticalSpeed = -10f;

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;
    [SerializeField] private HeroJumpHorizontalMovementSettings _jumpHorizontalMovementSettings;
    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling,
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer;


    [Header("Wall")]
    [SerializeField] private WallDetector _wallDetector;
    public bool IsTouchingWall {get; private set;}
    
    [Header("Wall Jump")]
    [SerializeField] private HeroWallJumpSettings _wallJumpSettings;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;


    private void Awake(){
        tr = GetComponent<TrailRenderer>();
    }

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        _ApplyWallDetection();
        
        horizontalMovementSettings = _GetCurrentHorizontalMovementSettings();
        dashSettings = _GetCurrentDashSettings();

        if(_AreOrientAndMovementOpposite()){
            _TurnBack(horizontalMovementSettings);
        } else{
            if(IsDashing){
                Dash(dashSettings, horizontalMovementSettings); //à vérif
            } else{
                _UpdateHorizontalSpeed(horizontalMovementSettings);
            }

            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping && !IsDashing){
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

        if(IsSliding){
            if(Input.GetKey(KeyCode.S)){
                _ApplySlidingGravity(_downSlidingVerticalSpeed);
            }else{
                _ApplySlidingGravity(_normalSlidingVerticalSpeed);
            }
        }

        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
        
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementsSettings settings){
        if(_moveDirX != 0f)
        {
            _Accelerate(settings); 
        }
        else
        {
            _Decelerate(settings);
        }
    }
    
    private void _Accelerate(HeroHorizontalMovementsSettings settings){
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if(_horizontalSpeed > settings.speedMax){   
            _horizontalSpeed =settings.speedMax;
        }
    }

    private void _Decelerate(HeroHorizontalMovementsSettings settings){
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if(_horizontalSpeed < 0f){   
            _horizontalSpeed = 0f;
        }
    }

//Dash
    private HeroDashSettings _GetCurrentDashSettings(){
        return IsTouchingGround ?  _dashSettingsOnGround :  _dashSettingsInAir;
    }

    public void Dash(HeroDashSettings _dashSettings, HeroHorizontalMovementsSettings _mvtSettings){
        
        _dashTimer += Time.fixedDeltaTime;
        if(_dashTimer < _dashSettings.duration){
            IsDashing = true; 
            _ResetVerticalSpeed();
            _horizontalSpeed = _dashSettings.speed;
            
            tr.emitting = true;
        }
        else {
            IsDashing = false;
            tr.emitting = false;
            _dashTimer = 0;
            _horizontalSpeed = _mvtSettings.speedMax;
        }
    }

//
    private void _ChangeOrientFromHorizontalMovement(){
        if(_moveDirX == 0f) return ; // et si pas d'accolades le if execute juste la ligne d'apres 
        _orientX = Mathf.Sign(_moveDirX);
    }

//Vertical Settings
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

    private void _ApplyVerticalSpeed(){
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

     private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed*_orientX;
        _rigidbody.velocity = velocity;
    }

    //IsTouchingGround vrai ou faux ? Si vrai : Si faux
    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementSettings(){
        return IsTouchingGround ? _groundHorizontalMovementsSettings : _airHorizontalMovementsSettings;
    }

    private void _TurnBack(HeroHorizontalMovementsSettings settings){
        _horizontalSpeed -= settings.turnBackFrictions *Time.fixedDeltaTime;
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

        // if(IsSliding){
        //     if(Input.GetKey(KeyCode.Escape)){
        //         _verticalSpeed= WallDetector.orientDetection * 7;
        //         _horizontalSpeed= 5;
        //         Debug.Log("je devrais avoir fait le walljump");
        //     }
        // }
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    //Jump

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

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;
    public bool IsJumping => _jumpState != JumpState.NotJumping;

    //Slide
    private void _ApplyWallDetection(){
        IsTouchingWall = _wallDetector.DetectWallNearBy();
    }

    public bool IsSliding => IsTouchingWall && !IsTouchingGround;

    private void _ApplySlidingGravity(float slidingVerticalSpeed){
        _ResetVerticalSpeed();
        _verticalSpeed = slidingVerticalSpeed;
    }

    //Wall Jump

    // if(isSliding && Input.GetKey(KeyCode.Escape)){
    //     _verticalSpeed=;
    //     _horizontalSpeed=;
    // }

    // //si il slide 
    // if input.GetKey(KeyCode.Escape){

    // }


    //Debug
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
        if(IsDashing){
            GUILayout.Label($"Dash Speed = {dashSettings.speed}");
        } else{
            GUILayout.Label($"Dash Speed = {0}");
        }
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label("Vertical Speed =" + _verticalSpeed);
        GUILayout.EndVertical();
    }
}