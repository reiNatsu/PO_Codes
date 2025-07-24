using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISearchClan : MonoBehaviour
{
    [SerializeField] private TMP_InputField _searchClanTMP;

    private string _inputText;
    public string InputText { get { return _inputText; } }

    private void Start()
    {
        SetClanSearchUI(false);

    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void OnClickCloseSearchClan()
    {
        //var result = _searchClanTMP.text;
        //SetSearchClasnTMP(result);

        //_searchClanTMP.text = "";
        var uiClan = UIManager.Instance.GetUI<UIClan>();
        if (uiClan != null && uiClan.gameObject.activeInHierarchy)
        {
            var result = _searchClanTMP.text;
            //uiClan.SetSearchTMP(result);
            uiClan.UpdateClanData(result);
            //uiClan.UpdateClanList();
        }
     
        SetClanSearchUI(false);
    }

   
    private void SetClanSearchUI(bool isOn)
    {
        if (this.gameObject != null && gameObject.activeInHierarchy == !isOn)
            gameObject.SetActive(isOn);
    }

}
