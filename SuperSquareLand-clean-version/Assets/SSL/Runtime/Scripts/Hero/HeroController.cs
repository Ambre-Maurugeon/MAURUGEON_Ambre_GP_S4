using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void Update (){
        _entity.SetMoveDirX(GetInputMoveX());

        if (Input.GetKeyDown(KeyCode.Space)){
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
            _entity.Dash();
            //_entity.Dash(_dashState);
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

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.EndVertical();
    }
}