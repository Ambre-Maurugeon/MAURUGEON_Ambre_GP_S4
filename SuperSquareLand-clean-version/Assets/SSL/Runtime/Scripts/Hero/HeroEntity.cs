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
    [SerializeField] private float _dashCooldown;

    [HideInInspector]
    public bool IsDashing = false;

    [HideInInspector] 
    public HeroDashSettings dashSettings;

    private float _dashTimer;
    private float _dashCooldownTimer=0f;
    private TrailRenderer tr;

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    public float _orientX = 1f;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;
    
    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround {get; private set;}

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Sliding")]
    [SerializeField] private float _normalSlidingVerticalSpeed = -1.2f;
    [SerializeField] private float _downSlidingVerticalSpeed = -10f;

    [Header("Jump")]
    [SerializeField]private HeroJumpSettings[] _allJumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;
    [SerializeField] private HeroHorizontalMovementsSettings _jumpHorizontalMovementSettings;
    private HeroJumpSettings _jumpSettings;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling,
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer;

    [HideInInspector]
    public int index = 0;


    [Header("Wall")]
    [SerializeField] private WallDetector _wallDetector;
    public bool IsTouchingWall {get; private set;}
    
    [Header("Wall Jump")]
    [SerializeField] private HeroWallJumpSettings _wallJumpSettings;
    [HideInInspector]
    public bool IsWallJumping ;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

//Camera Follow
    public CameraFollowable _cameraFollowable;
    private CameraProfile _cameraProfile;

    private void Awake(){
        tr = GetComponent<TrailRenderer>();
        
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraProfile = FindObjectOfType<CameraProfile>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x + _orientX * _cameraProfile._followOffsetX;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;
    }

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        _UpdateCameraFollowPosition();
        _ApplyWallDetection();
        
        horizontalMovementSettings = _GetCurrentHorizontalMovementSettings();
        dashSettings = _GetCurrentDashSettings();

        if(_AreOrientAndMovementOpposite()){
            if(IsJumping){ 
                _TurnBack(_jumpHorizontalMovementSettings);
            }
            else{
                _TurnBack(horizontalMovementSettings);
            }
        } else{
            if(IsDashing){
                Dash(dashSettings, horizontalMovementSettings); 
            } else if (IsJumping){
                _UpdateHorizontalSpeed(_jumpHorizontalMovementSettings);
            }   
            else{
                _UpdateHorizontalSpeed(horizontalMovementSettings);
            }

            _ChangeOrientFromHorizontalMovement();
        }
        
        if (IsJumping && !IsDashing)
        {
            _UpdateJump();
        }
        else {
            if(!IsTouchingGround && !IsSliding) 
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
          
        if(IsWallJumping){
            _UpdateWallJump();
        } 
        
        if(IsTouchingGround || IsSliding ) index = 0;

        _UpdateDashCooldown();
        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
        
    }

//Horizontal Mvt
    public bool IsHorizontalMoving => _moveDirX !=0f;

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementsSettings settings){
        if(IsHorizontalMoving)
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

//Horizontal Settings
    private void _ResetHorizontalSpeed(){
        _horizontalSpeed = 0f;
    }

     private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed*_orientX;
        _rigidbody.velocity = velocity;

        if(IsTouchingWall){
            _ResetHorizontalSpeed() ;
        }
    }

    //IsTouchingGround vrai ou faux ? Si vrai : Si faux
    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementSettings(){
        return IsTouchingGround ? _groundHorizontalMovementsSettings : _airHorizontalMovementsSettings;
    }

//Orient
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
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

//Jump
    public bool CanJump => IsTouchingGround || index < _allJumpSettings.Length;
    //public int _currentJumpIndex => (IsTouchingGround || IsSliding) ? 0 : index;

    public HeroJumpSettings _GetJumpSettings(){
        if (index >= _allJumpSettings.Length){
            _jumpState = JumpState.Falling;
        }
        else{
            _jumpSettings = _allJumpSettings[index];
        }

        return _jumpSettings;
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
        _jumpSettings = _GetJumpSettings();
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


//Dash
    public void Dash(HeroDashSettings _dashSettings, HeroHorizontalMovementsSettings _mvtSettings){
        
        _dashTimer += Time.fixedDeltaTime;
        if(_dashTimer < _dashSettings.duration){
            IsDashing = true; 
            _ResetVerticalSpeed();
            _horizontalSpeed = _dashSettings.speed;
            
            tr.emitting = true;

        } else {
            _stopDash();
            _horizontalSpeed = _mvtSettings.speedMax;   // qd le dash est fini reprendre le speed max
        }
    }

    private void _stopDash(){
        IsDashing = false;
        _dashTimer = 0;

        tr.emitting = false;
        
        _dashCooldownTimer = _dashCooldown;
    }

    //DashCooldown

    private HeroDashSettings _GetCurrentDashSettings(){
        return IsTouchingGround ?  _dashSettingsOnGround :  _dashSettingsInAir;
    }

    private void _UpdateDashCooldown(){
        if (_dashCooldownTimer > 0f) {
            _dashCooldownTimer -= Time.fixedDeltaTime; 
        }
    }

    public bool CanDash => _dashCooldownTimer <= 0f;

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
    private float _wallJumpTimer;

    public void WallJumpStart(){
        _orientX = -_orientX; 
        _wallJumpTimer = 0f; 
        _UpdateWallJump(); 
    }

    private void _UpdateWallJump(){
        _wallJumpTimer += Time.fixedDeltaTime;
        if(_wallJumpTimer < _wallJumpSettings.wallJumpDuration){
            _horizontalSpeed = _wallJumpSettings.wallJumpHorizontalSpeed;
            _verticalSpeed = _wallJumpSettings.wallJumpVerticalSpeed;
        } else {
            _ApplyFallGravity(_fallSettings); 
        }
    }

//Camera
    private void _UpdateCameraFollowPosition(){
        _cameraFollowable.FollowPositionX = _rigidbody.position.x + _orientX * _cameraProfile._followOffsetX;

        if(IsTouchingGround && !IsJumping){
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }

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
        GUILayout.Label("Sauts restants = " + (_allJumpSettings.Length-index).ToString());
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