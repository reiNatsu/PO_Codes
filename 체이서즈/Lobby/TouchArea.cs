using Consts;
using LIFULSE.Manager;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TouchArea : MonoBehaviour
{
    //[SerializeField] private AudioSource _source;
    [SerializeField] private CharacterData[] _characterMainMenuTypes;
    [SerializeField] private TouchAreaObject _obj;

    private string _soundKey;
    private HashSet<string> _soundKeys = new HashSet<string>();
    private AudioPoolData _audioData = null;
    public string SoundKey { get { return _soundKey; } }
    [OnInspectorInit]
    void Init()
    {
        _obj = GetComponentInParent<TouchAreaObject>();
        //_source = GetComponentInParent<AudioSource>();
    }


    private void OnDisable()
    {
        StopAudio();
        foreach (var soundKey in _soundKeys)
        {
            AudioManager.Instance.DisposeAudio(soundKey);    
        }
        _soundKeys.Clear();
        
    }
    public void StopAudio()
    {
        //if (!string.IsNullOrEmpty(_soundKey))
        //    AudioManager.Instance.StopAudio(_soundKey);
        //if(AudioManager.Instance.GetAudioPoolData(_soundKey) != null)
        if(!string.IsNullOrEmpty(_soundKey))
            AudioManager.Instance.StopAudio(_soundKey);
        _soundKey = null;
    }
    public void PlayAudio(string audiostr)
    {
        if (!string.IsNullOrEmpty(_soundKey))
            AudioManager.Instance.StopAudio(_soundKey);

        if (string.IsNullOrEmpty(audiostr))
            return;
        if (!GameInfoManager.Instance.IsSkipInteractionSounde)
        {
            var type = GameInfoManager.Instance.GetSavedSoundLan();
            var audioClip = AudioManager.Instance.LocalizeAudioManager.GetCachedData(audiostr, type);
            _soundKey = audioClip;
            AudioManager.Instance.PoolPlayAudio(_soundKey);
        }
    }


    public void PlayAnimation()
    {
        //2024-12-08 동연 추가.  스테이지 종료 후 다시 노드 화면으로 이동 시, 로비 캐릭터 인터렉션 나오는 부분 예외처리.
        var randIndex = Random.Range(0, _characterMainMenuTypes.Length);

        if (randIndex >= _characterMainMenuTypes.Length)
            return;

        var select = _characterMainMenuTypes[randIndex];
        _obj.SetTrigger(select.Key.ToString());
        /*var clip = select.Clip;
        if (clip != null && _source != null && !GameInfoManager.Instance.IsSkipInteractionSounde)
        {
            _source.clip = clip;
            _source.Play();
        }*/
    }



    /*[Serializable]
    public class CharacterInteractions
    {
        public INTERACTION_TYPE Key;
        public AudioClip Clip;
    }*/


    [Serializable]
    public class CharacterData
    {
        public CharacterMainMenuType Key;
        //public AudioClip Clip;
    }

    
}
