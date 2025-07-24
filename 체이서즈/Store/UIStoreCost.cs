using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStoreCost : MonoBehaviour
{
    [SerializeField] private ExImage _costImg;
    [SerializeField] private ExTMPUI _costText;
    [SerializeField] private UILayoutGroup _uiLayoutGroup;
    [SerializeField] public bool _isPurchase = false;

    private ExButton _button;
    private Store_TableData _data;
    private UIStore _uiStore;

    public void InitData(Store_TableData data, ExButton button = null, bool isAdFree = false)
    {
        _data = data;
        _button = button;

        _uiStore = UIManager.Instance.GetUI<UIStore>();

        //_costText.gameObject.SetActive(!data.Purchase_Type.Equals("ad"));
        _costImg.gameObject.SetActive(data.Purchase_Type.Equals("ad") || data.Purchase_Type.Equals("none"));
        switch (data.Purchase_Type)
        {
            case "cash":
                _costText.ToTableText("str_s_button_bay_05", _data.Use_Cost.ToDollarString());
                break;
            case "none":
                _costText.text = string.Format("{0:n0}", _data.Material_01_Value);
                _costImg.SetSprite(GameInfoManager.Instance.GetCostData(_data.Material_01, _data.MATERIAL_01_TYPE));
                break;
            case "ad":
                {
                    if (!isAdFree)
                    {
                        _costText.ToTableText("str_ui_ad_view");
                        _costImg.SetSprite("UI_StoreADIcon");
                    }
                    else
                    {
                        _costText.ToTableText("str_ui_cash_text_002");
                        _costImg.gameObject.SetActive(false);
                    }
                }
                break;
            case "free":
                _costText.ToTableText("str_ui_cash_text_002");
                break;
        }

        SetUnablePurchaseUI();
        _uiLayoutGroup.UpdateLayoutGroup();
    }

    private void SetUnablePurchaseUI()
    {
        var storeOption = TableManager.Instance.Store_Option_Table[_data.Group_Id];

        if (_button != null)
        {
            // var purchaseCount = GameInfoManager.Instance.GetItemPurchaseCount(_data.Tid);
            var purchaseCount = 0;
            if (!_isPurchase)
            {
                _button.enabled =  false;
                ColorUtility.TryParseHtmlString("#C63328", out Color cleat_color);
                _costText.color = cleat_color;
                _costText.text = "구매 불가";
                return;
            }
            //if ((_data.Use_Daily !=0 && purchaseCount == _data.Use_Daily)||(_data.Use_Weekliy !=0 && purchaseCount == _data.Use_Weekliy)||(_data.Use_Limit !=0 && purchaseCount == _data.Use_Limit))
            //{
            //    _button.enabled =  false;
            //    ColorUtility.TryParseHtmlString("#C63328", out Color cleat_color);
            //    _costText.color = cleat_color;
            //    _costText.text = "구매 불가";
            //    return;
            //}

            _button.enabled = true;
        }

    }
}
