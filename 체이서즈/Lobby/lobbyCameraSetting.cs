using UnityEngine;

public class lobbyCameraSetting : ScriptableObject
{
    [Header("===== Camera Transform ======")]
    [SerializeField] private Vector3 _position;
    [SerializeField] private Vector3 _rotation;
    [SerializeField] private Vector3 _scale;

    [Header("===== Camera Projection ======")]
    [SerializeField] private bool _isOrthographic;
    [SerializeField] private float _orthographicSize;
    [Header("===== Camera Projection Clipping ======")]
    [SerializeField] private float _fieldOfView;
    [SerializeField] private float _fieldOfViewAxis;
    [SerializeField] private float _near;
    [SerializeField] private float _far;
    [Header("===== Character Pos ======")]
    [SerializeField] private float _characterPos = 2f;


    public Vector3 Position { get { return _position; } }
    public Vector3 Roatation { get { return _rotation; } }
    public Vector3 Scale { get { return _scale; } }
    public bool IsOrthographic { get { return _isOrthographic; } }
    public float OrthographicSize { get { return _orthographicSize; } }
    public float FieldOfView { get { return _fieldOfView; } }
    public float FieldOfViewAxis { get { return _fieldOfViewAxis; } }
    public float Near { get { return _near; } }
    public float Far { get { return _far; } }
    public float CharacterPos { get { return _characterPos; } }

    public void GetCameraSetting(Vector3 pos, Vector3 rot, Vector3 scale
        ,float fov, float fova, float near, float far, bool isOrtho, float orthSize)
    {
        _isOrthographic = isOrtho;
        if (isOrtho)
        {
            _orthographicSize = orthSize;
        }
        _position = pos;
        _rotation = rot;
        _scale = scale;
        _fieldOfView = fov; 
        _fieldOfViewAxis = fova;
        _near = near; 
        _far = far;
    }
}
