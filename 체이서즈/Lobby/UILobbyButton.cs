using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(ExImage))]
public class UILobbyButton : MonoBehaviour
{
    [SerializeField] private string _buttonName;
    [SerializeField] private UIRedDot _uiReddot;


    public void UpdateReddot()
    {
        _uiReddot.UpdateRedDot();
    }
    
}
