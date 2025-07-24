using Consts;
using LIFULSE.Manager;
using Pathfinding;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[System.Serializable]
public class LocalizeAudioManager : ScriptableObject
{
    // 여러 경로를 저장하는 리스트
    [SerializeField]
    private List<string> _baseFolderPaths = new List<string>
    {
        "Assets/06.Sounds/Unit/",
    };

    [SerializeField] private string _findString = "_v_";
    [SerializeField] private string _sequenceVoicePath = "Assets/06.Sounds/Sequence/LC_Voice_Scene";

    [SerializeField] private LocalizeAudioData _localizeAudioData = new LocalizeAudioData();
    

    // SOUND_LANGUAGE_TYPE을 사용하는 딕셔너리

    
    #if UNITY_EDITOR
    [Button]
    public void CacheLocalizeData()
    {
        _localizeAudioData.Clear();
        foreach (var path in _baseFolderPaths)
        {
            //FindAudioClip(path,_findString);
            FindAudioClip(path);
        }
        FindAudioClip(_sequenceVoicePath);
        UnityEditor.EditorUtility.SetDirty(this);
    }

    void FindAudioClip(string path)
    {
        var objGUIDs = AssetDatabase.FindAssets("t:AudioClip ", new[] { path });

        foreach (var guid in objGUIDs)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.Log(prefabPath);
            var assetShotPath = prefabPath.Split("/").Last();
            var assetName = assetShotPath.Split(".").First();
            var lastString = assetShotPath.Split("_").Last().Split(".").First();
            Debug.Log(lastString);
            switch (lastString)
            {
                case "jp":
                    {
                        var originKey = assetName.Replace("_" + lastString, "");
                        if (!_localizeAudioData.ContainsKey(originKey))
                        {
                            _localizeAudioData.Add(originKey, new LocalizeAudioSubData());
                        }
                        _localizeAudioData[originKey][lastString] = assetName;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void FindAudioClip(string path,string findKey = "")
    {
        var objGUIDs = AssetDatabase.FindAssets("t:AudioClip "+ findKey , new []{path});
      
        foreach (var guid in objGUIDs)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.Log(prefabPath);
            var assetShotPath = prefabPath.Split("/").Last();
            var assetName = assetShotPath.Split(".").First();
            var lastString = assetShotPath.Split("_").Last().Split(".").First();
            Debug.Log(lastString);
            switch (lastString)
            {
                case "jp":
                    {
                        var originKey = assetName.Replace("_" + lastString, "");
                        if (!_localizeAudioData.ContainsKey(originKey))
                        {
                            _localizeAudioData.Add(originKey, new LocalizeAudioSubData());
                        }
                        _localizeAudioData[originKey][lastString] = assetName;
                    }
                    break; 
                default:        // ko는 리소스 명 뒤에 ko가 붙지 않음
                    //{
                    //    if (!_localizeAudioData.ContainsKey(assetName))
                    //        _localizeAudioData.Add(assetName, new LocalizeAudioSubData());

                    //    if(!)

                    //    _localizeAudioData[assetName]["ko"] = assetName;
                    //}
                    break;
            }
        }
    }      
#endif

    public string GetCachedData(string soundName, string language)
    {
        
        if (!_localizeAudioData.TryGetValue(soundName,out var data))
        {
            return soundName;
        }

        if (data.TryGetValue(language, out var subData))
        {
            return subData;
        }

        return soundName;
    }
    public string GetCachedData(string soundName)
    {

        #if UNITY_EDITOR
        if (LocalizeManager.Instance == null)
        {
            return soundName;
        }
        #endif
        string language = GameInfoManager.Instance.GetSavedSoundLan();
        
        if (!_localizeAudioData.TryGetValue(soundName,out var data))
        {
            return soundName;
        }

        if (data.TryGetValue(language, out var subData))
        {
            return subData;
        }

        return soundName;
    }
    [Serializable]
    public class LocalizeAudioData : SerializableDictionary<string, LocalizeAudioSubData>
    {

    }
    //[Serializable]
    //public class LocalizeAudioData : SerializableDictionary<string,LocalizeAudioSubData>
    //{

    //}
    [Serializable]
    public class LocalizeAudioSubData : SerializableDictionary<string,string>
    {
        
    }
}