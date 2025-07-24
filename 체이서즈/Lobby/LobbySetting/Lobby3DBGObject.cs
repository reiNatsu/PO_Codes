
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Lobby3DBGObject : MonoBehaviour
{
    [SerializeField] private GameObject[] _switchObject;
    [SerializeField] private lobbyCameraSetting _lobbycamerasetting;

    public lobbyCameraSetting lobbyCameraSetting { get => _lobbycamerasetting; }
    private string _exportPath = "Assets/12.SettingAsset/LobbyCameraSetting";


    public void SwitchObject(bool b)
    {
        foreach (var obj in _switchObject)
        {
            obj.SetActive(b);
        }
    }

    public void LoadOrCreateScriptableObject()
    {
        string createclassname = "lobbycamerasetting_" + this.gameObject.name;
        string assetPath = _exportPath + "/" + createclassname + ".asset";

#if UNITY_EDITOR
        // 에디터 환경에서 ScriptableObject 로드
        _lobbycamerasetting = AssetDatabase.LoadAssetAtPath<lobbyCameraSetting>(assetPath);
        if (_lobbycamerasetting == null)
        {
            // 해당 ScriptableObject가 없으면 생성
            _lobbycamerasetting = CreateScriptableObject<lobbyCameraSetting>(createclassname, assetPath);
            SettingCameraData();
        }
#else
        // 런타임 환경에서 Resources 폴더에서 ScriptableObject 로드
        _lobbycamerasetting = Resources.Load<lobbyCameraSetting>(createclassname);
#endif

        if (_lobbycamerasetting == null)
        {
            Debug.LogError("Failed to load or create lobbyCameraSetting.");
        }
    }

#if UNITY_EDITOR
    private T CreateScriptableObject<T>(string createclassname, string assetPath) where T : ScriptableObject
    {
        T obj = ScriptableObject.CreateInstance<T>();

        if (!Directory.Exists(_exportPath))
        {
            Directory.CreateDirectory(_exportPath);
        }

        // 지정된 경로에 ScriptableObject 생성
        AssetDatabase.CreateAsset(obj, assetPath);
        Debug.Log(createclassname + " created successfully");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(this);

        return obj;
    }


    // --- 버튼으로 생성 ---
    [Button("set lobbycamerasetting ")]
    public void OnClickCameraSetting(Action onSaveEvent = null)
    {
       
        if (_lobbycamerasetting == null)
        {
            var inputclasstype = "lobbyCameraSetting";
            var createclassname = "lobbycamerasetting_"+this.gameObject.name;
            var obj = ScriptableObject.CreateInstance(inputclasstype);

            if (obj == null)
            {
                EditorUtility.DisplayDialog("Error", inputclasstype + " is't exist", "Ok");
                Debug.LogError(inputclasstype + " is't exist");
            }
            else
            {
                if (!Directory.Exists(Application.dataPath + "/0_Script"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/0_Script");
                }
                if (!Directory.Exists(Application.dataPath + "/0_Script/_TempCreatedAsset"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/0_Script/_TempCreatedAsset");
                }

                AssetDatabase.CreateAsset((ScriptableObject)obj, _exportPath + "/" + createclassname + ".asset");
                Debug.Log(inputclasstype + " create success");


                _lobbycamerasetting = AssetDatabase.LoadAssetAtPath<lobbyCameraSetting>(_exportPath + "/" + createclassname + ".asset");
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
          
        }

        onSaveEvent?.Invoke();
    }

    [Button("Update camera data")]
    public void SettingCameraData()
    {
        if (_lobbycamerasetting == null)
        {
            Debug.LogError("_lobbycamerasetting 가 없습니다. 먼저 생성해주세요");
            return;
        }
        
        Camera foundCamera = GetComponentInChildren<Camera>(true);
        if (foundCamera != null)
        {
            Vector3 pos = foundCamera.transform.position;
            Vector3 rot = foundCamera.transform.eulerAngles;
            //Vector3 scale = foundCamera.transform.localScale;
            float fieldOfView = foundCamera.fieldOfView;
            float fieldOfViewAxis = foundCamera.fieldOfView;
            float near = foundCamera.nearClipPlane;
            float far = foundCamera.farClipPlane;
            bool isOrthographic = foundCamera.orthographic;
            float orthographicSize = foundCamera.orthographicSize;

            _lobbycamerasetting.GetCameraSetting(pos, rot, Vector3.one, fieldOfView, fieldOfViewAxis, near, far, isOrthographic, orthographicSize);
#if UNITY_EDITOR
            // Mark the _lobbycamerasetting asset as dirty to ensure changes are recognized
            EditorUtility.SetDirty(_lobbycamerasetting);

            // Save all modified assets
            AssetDatabase.SaveAssets();
#endif
        }
        else
        {
            Vector3 pos = Vector3.zero;
            Vector3 rot = new Vector3(0, 0, 0);
            Vector3 scale = Vector3.one;
            float fieldOfView = 60f;
            float near = 0.3f;
            float far = 8000;
            bool isOrthographic = false;
            float orthographicSize = 5.0f;

            _lobbycamerasetting.GetCameraSetting(pos, rot, scale, fieldOfView, fieldOfView, near, far, isOrthographic, orthographicSize);
#if UNITY_EDITOR
            // Mark the _lobbycamerasetting asset as dirty to ensure changes are recognized
            EditorUtility.SetDirty(_lobbycamerasetting);

            // Save all modified assets
            AssetDatabase.SaveAssets();
#endif
        }

    }

#endif
}

