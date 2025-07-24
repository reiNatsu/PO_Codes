using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILobbySwitchButton : MonoBehaviour
{
    [SerializeField] private GameObject _button;
    [SerializeField] private bool _isAnim;

    private Coroutine _coroutine;
    public GameObject Button { get { return _button; } }

    public void OnDisable()
    {
        if (_isAnim)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }

    public void On()
    {
        _button.SetActive(true);

        if (_isAnim)
        {
            if (_coroutine == null)
                _coroutine = StartCoroutine(ButtonOffEvent());
        }
    }

    public void Off()
    {
        if (_isAnim)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
       
        _button.SetActive(false);
       
    }

    IEnumerator ButtonOffEvent()
    {
        yield return new WaitForSeconds(4.0f);
        Off();
    }
}
