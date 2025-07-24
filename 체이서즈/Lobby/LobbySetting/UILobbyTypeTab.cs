using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILobbyTypeTab : MonoBehaviour
{
    [SerializeField] private SELCET_CATEGORY _selectcategory;
    [SerializeField] private GameObject _on;
    [SerializeField] private GameObject _off;
    //SerializeField] private string _title;
    [SerializeField] private ExButton _button;
    [SerializeField] private Animator _anim;

    private bool _inOn = false;

    public SELCET_CATEGORY SelectCategory{ get { return _selectcategory; } }
    public ExButton Button { get => _button; }
    private void Awake()
    {
        //if (_anim == null)
        //    _anim = this.GetComponent<Animator>();
        Off();
    }
    public void InitTab(Action onClikc)
    {
        _button.onClick.AddListener(() => onClikc());
    }

    public void InitTab(int index,Action<int> onClikc)
    {
        if (_anim != null)
        {
            _anim.Play("MenuButton_Off", -1, 1);
            _anim.speed = 0;
        }
        else
            _on.SetActive(false);
        _button.onClick.AddListener(()=>onClikc(index));
    }

    public void On()
    {
        if (_anim != null)
        {
            if (_anim.speed == 0)
                _anim.speed = 1;

            _anim.Play("MenuButton_On");
        }
        else
        {
            _on.SetActive(true);
            _off.SetActive(false);
        }
        _inOn = true;
    }
    public void Off()
    {
        if (_anim != null)
        {
            //_anim.Play("MenuButton_Off");
            //_anim.speed = 0;
            _anim.Play("MenuButton_Off", -1, 1);
            _anim.speed = 0;
        }
        else
        {
            _on.SetActive(false);
            _off.SetActive(true);
        }
        _inOn = false;
    }

    public void OnClickConversion()
    {
        if (_inOn)
            Off();
        else
            On();
    }

    public void Refresh()
    {
        _anim.Play("MenuButton_Off", -1, 1);
        _anim.speed = 0;
    }

    private void OnDisable()
    {
    
    }

}
