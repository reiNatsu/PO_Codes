using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStoreBuySuccess : UIPopup
{
    [SerializeField] ExTMPUI _infoTxt;

    private Store_TableData _data;
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
       
    }

    private void UpdateUI()
    {
       
    }

    public override void Close(bool needCached = true)
    {
        Refresh();
        base.Close(needCached);
    }
}
