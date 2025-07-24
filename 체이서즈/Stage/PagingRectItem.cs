using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class PagingRectItem : MonoBehaviour
{
    [SerializeField] private int _rectNo;
    [SerializeField] private GameObject _on;
    [SerializeField] private GameObject _off;

    public bool IsOn { get; private set; } = false;

    public void Init( int num)
    {
        _rectNo =num;
    }

    public void PagingOn()
    {
        _on.SetActive(true);
        _off.SetActive(false);
        IsOn = true;
    }
    public void PagingOff()
    {
        _on.SetActive(false);
        _off.SetActive(true);
        IsOn = false;
    }
}
