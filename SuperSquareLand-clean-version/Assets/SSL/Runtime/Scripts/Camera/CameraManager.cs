using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set;}

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Profile System")]
    [SerializeField] private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;
    public CameraProfile CurrentCameraProfile => _currentCameraProfile;

    //Transition
    private float _profileTransitionTimer = 0f;
    private float _profileTransitionDuration = 0f;
    private Vector3 _profileTransitionStartPosition;
    private float _profileTransitionStartSize;

    //Follow
    private Vector3 _profileLastFollowDestination;

    //Damping
    private Vector3 _dampedPosition;

    //Delimitation cam
    private Rect boundsRect;
    private Vector2 worldHalfScreenSize;
    private Vector2 worldScreenSize;

    //AutoScroll
    private Vector3 origine;
    private Vector3 destination = Vector3.zero;
    private float autoScrollSpeed; 

    //Follow Offset
    private float _previousOrientX = 1;
    private HeroEntity _entity;

    private void Awake(){
        Instance = this;
        _entity = FindObjectOfType<HeroEntity>();
    }

    private void Start(){
        _InitToDefaultProfile();

        if(_currentCameraProfile.ProfileType == CameraProfileType.AutoScroll) _SetAutoScroll();
    }

//private Vector3 _previousPosition;
    private void Update(){

        Vector3 nextPosition = _FindCameraNextPosition();
        nextPosition = _ClampPositionIntoBounds(nextPosition);
        nextPosition = _ApplyDamping(nextPosition);

        // Vector3 delayedPosition = Vector3.Lerp(_previousPosition, nextPosition, _currentCameraProfile._followOffsetDamping);
        // delayedPosition = _ApplyDamping(delayedPosition);

        if(_IsPlayingProfileTransition() && _currentCameraProfile.ProfileType != CameraProfileType.AutoScroll){
            _profileTransitionTimer += Time.deltaTime;
            Vector3 transitionPosition = _CalculateProfileTransitionPosition(nextPosition);
            _SetCameraPosition(transitionPosition);
            float transitionSize = _CalculateProfileTransitionCameraSize(_currentCameraProfile.CameraSize);
            _SetCameraSize(transitionSize);
        }
        else{     
            _SetCameraPosition(nextPosition);
            _SetCameraSize(_currentCameraProfile.CameraSize);
        }

        //_previousPosition = nextPosition;
    }


//Delimitation Camera
    private void _GetCameraInfo(){
        boundsRect = _currentCameraProfile.BoundsRect;
        Vector3 worldBottomLeft = _camera.ScreenToWorldPoint(new Vector3(0f,0f));
        Vector3 worldTopRight = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth,_camera.pixelHeight));
        Vector2 worldScreenSize = new Vector2(worldTopRight.x - worldBottomLeft.x, worldTopRight.y - worldBottomLeft.y);
        worldHalfScreenSize = worldScreenSize / 2f;
    }

    private Vector3 _ClampPositionIntoBounds(Vector3 position){
        if(!_currentCameraProfile.HasBounds) return position;

        _GetCameraInfo();

        if(position.x >boundsRect.xMax - worldHalfScreenSize.x){
            position.x = boundsRect.xMax - worldHalfScreenSize.x;
        }

        if(position.x < boundsRect.xMin + worldHalfScreenSize.x){
            position.x = boundsRect.xMin + worldHalfScreenSize.x;
        }

        if(position.y >boundsRect.yMax - worldHalfScreenSize.y){
            position.y = boundsRect.yMax - worldHalfScreenSize.y;
        }

        if(position.y < boundsRect.yMin + worldHalfScreenSize.y){
            position.y = boundsRect.yMin + worldHalfScreenSize.y;
        }

        return position;
    }


    

//Damping 
    private Vector3 _ApplyDamping(Vector3 position){
        if(_currentCameraProfile.UseDampingHorizontally)
        {
            _dampedPosition.x = Mathf.Lerp(
                _dampedPosition.x,
                position.x,
                _currentCameraProfile.HorizontalDampingFactor * Time.deltaTime
            );
        } else {
            _dampedPosition.x = position.x;
        }

        if(_currentCameraProfile.UseDampingVertically){
            _dampedPosition.y = Mathf.Lerp(
                _dampedPosition.y,
                position.y,
                _currentCameraProfile.VerticalDampingFactor  * Time.deltaTime
            );
        } else{
            _dampedPosition.y = position.y;
        }

        return _dampedPosition;
    }


    private void _SetCameraDampedPosition(Vector3 position){
        _dampedPosition.x=position.x;
        _dampedPosition.y=position.y;
    }

//
private Vector3 _FindCameraNextPosition(){
    //Target To follow
    if(_currentCameraProfile.ProfileType == CameraProfileType.FollowTarget){
        if (_currentCameraProfile.TargetToFollow != null){
            CameraFollowable targetToFollow = _currentCameraProfile.TargetToFollow;
            if(_entity._orientX != _previousOrientX){
                _profileLastFollowDestination.x = Mathf.Lerp(_profileLastFollowDestination.x, targetToFollow.FollowPositionX, _currentCameraProfile._followOffsetDamping*Time.deltaTime);
                Debug.Log("je leeeerp de " + _profileLastFollowDestination.x + " à " + targetToFollow.FollowPositionX);
            }
            else{
                _profileLastFollowDestination.x = targetToFollow.FollowPositionX;
                //return _profileLastFollowDestination;
            }
            _profileLastFollowDestination.y = targetToFollow.FollowPositionY;
            _previousOrientX = _entity._orientX;
            //_profileLastFollowDestination = new Vector3(_profileLastFollowDestination.x,_profileLastFollowDestination.y,_profileLastFollowDestination.z);
            return _profileLastFollowDestination;
        }
    }
    //AutoScroll
    else if(_currentCameraProfile.ProfileType == CameraProfileType.AutoScroll){
        _LaunchAutoScroll();
    }
    return _currentCameraProfile.Position;
}

//AutoScroll
private void _SetAutoScroll(){
    _GetCameraInfo();
    origine = new Vector3(boundsRect.xMin + worldHalfScreenSize.x, boundsRect.yMin + worldHalfScreenSize.y, _currentCameraProfile.myPosition.z);
    _currentCameraProfile.myPosition=origine;
    destination = new Vector3(boundsRect.xMax - worldHalfScreenSize.x, boundsRect.yMax - worldHalfScreenSize.y, _currentCameraProfile.Position.z);
}

private void _LaunchAutoScroll()
{
    // autoScrollSpeed += Mathf.Clamp01(origine.x/destination.x)*0.0005f*_currentCameraProfile._autoScrollHorizontalSpeed; 

    //de là à là en x à cette vitesse horizontale x
    float scrollX = Mathf.Lerp(_currentCameraProfile.myPosition.x, destination.x, _currentCameraProfile._autoScrollHorizontalSpeed/100 * Time.deltaTime);

    //de là à là en y à cette vitesse horizontale y
    float scrollY = Mathf.Lerp(_currentCameraProfile.myPosition.y, destination.y, _currentCameraProfile._autoScrollVerticalSpeed/100 * Time.deltaTime);

    Vector3 newPosition = new Vector3 (scrollX, scrollY, _currentCameraProfile.myPosition.z);
    _currentCameraProfile.UpdatePosition(newPosition);
}


//Transition
    //Play Transition
    private void _PlayProfileTransition(CameraProfileTransition transition){
        _profileTransitionStartPosition = _camera.transform.position;
        _profileTransitionStartSize = _camera.orthographicSize;

        _profileTransitionTimer = 0f;
        _profileTransitionDuration= transition.duration;
    }

    private bool _IsPlayingProfileTransition(){
        return _profileTransitionTimer < _profileTransitionDuration;
    }

//Set Transition
    private float _CalculateProfileTransitionCameraSize(float endSize){
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        float startSize = _profileTransitionStartSize;
        return Mathf.Lerp(startSize, endSize, percent);
    }

    private Vector3 _CalculateProfileTransitionPosition(Vector3 destination){
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        Vector3 origin = _profileTransitionStartPosition;
        return Vector3.Lerp(origin, destination, percent);
    }

//Changement de cam
    public void EnterProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null){ //transition est un para optionnel
        _currentCameraProfile = cameraProfile;
        if(transition != null){
            _PlayProfileTransition(transition);
        }
        _SetCameraDampedPosition(_FindCameraNextPosition());
    }

    public void ExitProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null){
        if(_currentCameraProfile != cameraProfile) return;
        _currentCameraProfile = _defaultCameraProfile;

        if(transition != null){
            _PlayProfileTransition(transition);
        }
        _SetCameraDampedPosition(_FindCameraNextPosition());
    }

//Set Camera
    private void _SetCameraPosition(Vector3 position){
        Vector3 newCameraPosition = _camera.transform.position;
        newCameraPosition.x = position.x;
        newCameraPosition.y = position.y; 
        _camera.transform.position = newCameraPosition;  
    }

    private void _SetCameraSize(float size){
        _camera.orthographicSize = size;
    }

    private void _InitToDefaultProfile(){
        _currentCameraProfile = _defaultCameraProfile;
        _SetCameraPosition(_currentCameraProfile.Position);
        _SetCameraSize(_currentCameraProfile.CameraSize);
        _SetCameraDampedPosition(_ClampPositionIntoBounds(_FindCameraNextPosition()));
    }

}