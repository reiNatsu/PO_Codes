using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkinCameraSetting : ScriptableObject
{
    [Header("===== Camera Setting ======")]
    [SerializeField] private Vector3 _position;
    [SerializeField] private Vector3 _rotation;
    [SerializeField] private Vector3 _scale;

    [Header("===== 캐릭터 y값 보정 ======")]
    [SerializeField] private float _yValue;

    [Header("fieldOfView")]
    [Header("fieldOfViewAxis Vertical 기준으로 값 입력 시 Horizontal값으로 변경 ")]
    [SerializeField] private float _fieldOfView;

    [Header("===== 적용값 확인 ======")]
    [SerializeField] private Vector3 _resultCamPos;

    public float FieldOfView { get { return _fieldOfView; } }
    public Vector3 Pos { get { return _position; } }
    public Vector3 Rot { get { return _rotation; } }
    public float YValue { get { return _yValue; } }
    public Vector3 ResultPos { get { return _resultCamPos; } }
    public void SetCamera()
    {
        Camera maincamera = LobbyController.Instance.MainCam;
        _position = maincamera.GetComponent<Transform>().position;
        _scale = maincamera.GetComponent<Transform>().localScale;
        _fieldOfView = maincamera.fieldOfView;
    }


    public void UpdateResultCmaPos(Camera cam)
    {
        _resultCamPos = cam.transform.position;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SkinCameraSetting))]
public class AudioClipCacheManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SkinCameraSetting manager = (SkinCameraSetting)target;
        if (GUILayout.Button("Change MainCamera Values"))
        {
            if (LobbyController.Instance != null)
                LobbyController.Instance.SetMainCameraValues();
        }
  
    }
}
#endif