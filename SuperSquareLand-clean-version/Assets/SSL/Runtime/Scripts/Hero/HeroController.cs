using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    private bool _entityWasTouchingGround = false;

    [Header("Coyote Time")]
    [SerializeField] private float _coyoteTimeDuration = 0.2f;
    private float _coyoteTimeCountdown = -1f;

    [Header("Jump Buffer")]
    [SerializeField] private float _jumpBufferDuration =0.2f;
    private float _jumpBufferTimer = 0f;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void Start(){
        _CancelJumpBuffer();
    }

    private void Update (){
        _UpdateJumpBuffer();

        _entity.SetMoveDirX(GetInputMoveX());

        if (_EntityHasExitGround()){
            _ResetCoyoteTime();
        } else {
            _UpdateCoyoteTime();
        }

        if (Input.GetKeyDown(KeyCode.Space)){
            if((_entity.IsTouchingGround || _IsCoyoteTimeActive()) && !_entity.IsJumping){
                _entity.JumpStart();
            } else {
                _ResetJumpBuffer();
            }
        } 

        if(IsJumpBufferActive()){
            if((_entity.IsTouchingGround || _IsCoyoteTimeActive()) && !_entity.IsJumping){
                _entity.JumpStart();
            }
        }

        if(_entity.IsJumpImpulsing){
            if(!Input.GetKey(KeyCode.Space) && _entity.IsJumpMinDurationReached){
                _entity.StopJumpImpulsion();
            }
        }

        if(Input.GetKeyDown(KeyCode.E)){
            _entity.Dash(_entity.dashSettings);
        }

        _entityWasTouchingGround = _entity.IsTouchingGround;
    }

    private float GetInputMoveX(){
        float InputMoveX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q)){
            InputMoveX = -1f;
        }
        if (Input.GetKey(KeyCode.D)){
            InputMoveX = 1f;
        }
        return InputMoveX;
    }

//Jump Buffer
    private void _ResetJumpBuffer(){
        _jumpBufferTimer=0f;
    }

    private void _UpdateJumpBuffer(){
        if(!IsJumpBufferActive()) return;
        _jumpBufferTimer += Time.deltaTime;
    }

    private bool IsJumpBufferActive(){
        return _jumpBufferTimer < _jumpBufferDuration;
    }

    private void _CancelJumpBuffer(){
        _jumpBufferTimer = _jumpBufferDuration;
    }

    private bool _EntityHasExitGround(){
        return _entityWasTouchingGround && !_entity.IsTouchingGround;
    }

//Coyote Time
    private void _UpdateCoyoteTime(){
        if(!_IsCoyoteTimeActive()) return;
        _coyoteTimeCountdown -= Time.deltaTime; // 3 2 1 (décompte décroissant)
    }

    private void _ResetCoyoteTime(){
        _coyoteTimeCountdown = _coyoteTimeDuration;
    }

    private bool _IsCoyoteTimeActive(){
        return _coyoteTimeCountdown > 0f;
    }

//Debug
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label("Jump Buffer Timer = " + _jumpBufferTimer);
        GUILayout.Label("CoyoteTime Countdown = " + _coyoteTimeCountdown);
        GUILayout.EndVertical();
    }
}