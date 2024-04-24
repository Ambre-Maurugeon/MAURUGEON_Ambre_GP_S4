using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

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

        if (Input.GetKeyDown(KeyCode.Space)){
            if(_entity.IsTouchingGround && !_entity.IsJumping){
                _entity.JumpStart();
            } else {
                _ResetJumpBuffer();
            }
        } 

        if(IsJumpBufferActive()){
            if(_entity.IsTouchingGround && !_entity.IsJumping){
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

//Debug
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label("Jump Buffer Timer = " + _jumpBufferTimer);
        GUILayout.EndVertical();
    }
}