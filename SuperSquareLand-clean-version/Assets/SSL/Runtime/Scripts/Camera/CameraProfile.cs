using UnityEngine;

public enum CameraProfileType {
    Static = 0,
    FollowTarget,
    AutoScroll
}

public class CameraProfile : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private CameraProfileType _profileType = CameraProfileType.Static;

    [Header("Follow")]
    [SerializeField] private CameraFollowable _targetToFollow;
    public float _followOffsetX=8f;
    public float _followOffsetDamping=1.5f;

    [Header("Damping")]
    [SerializeField] private bool _useDampingHorizontally;
    [SerializeField] private float _horizontalDampingFactor=5f;
    [SerializeField] private bool _useDampingVertically;
    [SerializeField] private float _verticalDampingFactor=5f;

    [Header("Bounds")]
    [SerializeField] private bool _hasBounds;
    [SerializeField] private Rect _boundsRect = new Rect(0f,0f,10f,10f);

    [Header("AutoScroll")]
    public float _autoScrollHorizontalSpeed; //{get; private set;}
    public float _autoScrollVerticalSpeed; //{get; private set;}

    private Camera _camera;
    public float CameraSize => _camera.orthographicSize;
    public Vector3 Position => _camera.transform.position;

    //Target Camera
    public CameraProfileType ProfileType => _profileType;
    public CameraFollowable TargetToFollow => _targetToFollow;

    //Damping
    public bool UseDampingHorizontally => _useDampingHorizontally;
    public float HorizontalDampingFactor => _horizontalDampingFactor;
    public bool UseDampingVertically => _useDampingVertically;
    public float VerticalDampingFactor => _verticalDampingFactor;

    //Bounds
    public bool HasBounds => _hasBounds;
    public Rect BoundsRect => _boundsRect;


    private void Awake(){
        _camera = GetComponent<Camera>();
        if( _camera != null ){
            _camera.enabled=false;
        }
    }

    private void OnDrawGizmosSelected(){
        if(!_hasBounds) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_boundsRect.center, _boundsRect.size);
    }

    //Autoscroll
    public void UpdatePosition(Vector3 newPosition){
        transform.position = newPosition;
    }

        public Vector3 myPosition
    {
        get => _camera.transform.position;
        set => _camera.transform.position = value;
    }
}

