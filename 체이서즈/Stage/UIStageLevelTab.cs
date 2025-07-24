using Consts;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_EDITOR
using UnityEditor.Build;
#endif
using UnityEngine;

public class UIStageLevelTab : MonoBehaviour
{
    [SerializeField] private GameObject _on;
   // [SerializeField] private GameObject _locked;
    [SerializeField] private ExButton _button;
    [SerializeField] private UIContentLcok _uiContentLock;
    [SerializeField] private Animator _anim = null;

    [Header("이벤트에서 사용")]
    [SerializeField] private ExTMPUI _indexTMP = null;
    [SerializeField] private bool _isStory = false;
    [SerializeField] private LEVEL_DIFFICULTY _levle = LEVEL_DIFFICULTY.none;
    private bool _isLocked = false;
    private bool _isEvent = false;
    private int _eventLevelIndex = 0;
    private Action<int> _onClickLevelTab = null;

    public ExButton Button { get { return _button; } set { value = _button; } }
    
    public void InitTab()
    {
        if (_anim != null)
        {
            //_anim.Play("DifficultyBtn_Active", -1, 1); // 애니메이션 끝에서 시작
            //_anim.speed = -1; // 역방향 재생
            _anim.Play("DeActive", 0, 1f);// Layer 0에서 끝에서부터 역방향 재생
            _anim.speed = -1; // 역방향 재생
        }
        _isEvent = false;
    }

    public void InitLock(string tid)
    {
        _uiContentLock.ContentStateUpdate(tid);
    }

    public void SetIsLock(bool isLocked)
    {
        _isLocked = isLocked;
       // _locked.SetActive(isLocked);
    }

    public void Active()
    {
        if (_isLocked)
            return;

        if (_anim != null)
        {
            if (_anim.speed <= 0)
                _anim.speed = 1;
            _anim.Play("Active");
        }

        if (_isEvent)
            _indexTMP.SetColor("#FFFFFF");
    }
    public void DeActive()
    {
        if (_anim != null)
        {
            _anim.Play("DeActive");
        }

        if (_isEvent)
            _indexTMP.SetColor("#808080");
    }

    public void ActiveLock(bool isLock)
    {
       // _locked.SetActive(isLock);
    }
    public void OnClikcLock()
    { 
    
    }


    public void InitStoryTab(int index)
    {
        _isEvent= true;
        _eventLevelIndex = index;
        if (_isStory)
        {
            if (_indexTMP != null)
                _indexTMP.text = "STORY";
        }
       else //(_type != LEVEL_TYPE.story)
        {
            var level = _levle.ToStringTid();
            if (_indexTMP != null)
                _indexTMP.ToTableText(level);
        }
        if (_anim != null)
        {
            _anim.Play("DeActive", 0, 1f);// Layer 0에서 끝에서부터 역방향 재생
            _anim.speed = -1; // 역방향 재생
        }
        SetContenLockUI();
    }
    public void SetContenLockUI()
    {
        if (string.IsNullOrEmpty(_uiContentLock.TID))
            _uiContentLock.gameObject.SetActive(false);
        else
            _uiContentLock.ContentStateUpdate();
    }

}
