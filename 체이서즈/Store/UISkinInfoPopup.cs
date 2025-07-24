using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing.Extension;

public class UISkinInfoPopup : UIPopup
{

    [SerializeField] private ExImage _mainImg;
    [SerializeField] private ExTMPUI _nameTMP;
    [SerializeField] private ExTMPUI _infoTMP;
    [SerializeField] private ExTMPUI _characterNameTMP;
    [SerializeField] private ExImage _characterImg;
    [SerializeField] private ExTMPUI _ownedTMP;
    [SerializeField] private ExTMPUI _okBtnTMP;

    [SerializeField] private List<UILayoutGroup> _uILayoutGroup;

    private Costume_TableData _data;
    private Store_TableData _storeData;

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    public override void Init()
    {
        base.Init();
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Data, out var costumData))
                _data = (Costume_TableData)costumData;
            
            if (optionDict.TryGetValue(UIOption.Data2, out var storeData))
                _storeData = (Store_TableData)storeData;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        SetCahracterInfo();
        _mainImg.SetSprite(_storeData.Product_Popup_Image);
        _nameTMP.ToTableText(_storeData.Product_Str);
        _infoTMP.ToTableText(_storeData.Product_Str_Desc);
        _okBtnTMP.ToTableText("str_s_button_bay_05", _storeData.Use_Cost.ToDollarString());
        SetIsOwend();

        for (int n = 0; n< _uILayoutGroup.Count; n++)
        {
            _uILayoutGroup[n].UpdateLayoutGroup();
        }
    }

    private void SetIsOwend()
    {
        bool isOwend = GameInfoManager.Instance.CharacterInfo.GetDosa(_data.Costume_Character) != null;
        if (isOwend)
            _ownedTMP.ToTableText("str_ui_costume_button_02");          //  보유중
        else
            _ownedTMP.ToTableText("str_ui_costume_button_03");          //  미보유
    }

    private void SetCahracterInfo()
    {
        Character_PC_TableData character = TableManager.Instance.Character_PC_Table[_data.Costume_Character];
        _characterImg.SetSprite(character.Char_Icon_Square_02);
        _characterNameTMP.ToTableText(character.Str);
    }


    public void OnClickInfo()
    {
        UIManager.Instance.Show<UISkinPage>(Utils.GetUIOption(
            UIOption.Tid, _data.Costume_Character,
            UIOption.Data, _data,
            UIOption.Bool, true
            ));
    }

    public void OnClickPurchase()
    {
        Action onClikc = () =>
        {
            //_uiStore.UpdateScroll(_data.Group_Id, _data.Order);
            UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "cash"));
            Close();
        };

        var uiStore = UIManager.Instance.GetUI<UIStore>();
        if (uiStore == null)
            return;

        if (_storeData.Purchase_Type.Equals("free"))
        {
            RestApiManager.Instance.RequestStoreUseStore(_data.Tid, "1", _storeData.Store_Type,
            (res) =>
               {
                   uiStore.UpdateScroll(_storeData.Group_Id);
                   Close();
               });
        }
        else
        {

            UnityEngine.Debug.Log("상점 구매 @@@ _0");
            if (!NowGGManager.Instance.IsNowGGActive &&!IAPManager.Instance.IsInit())
            {
                UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK,
                       LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                       "IAPManager.Instance.IsInit() = false",
                       onClickOK: onClikc,
                       onClickClosed: onClikc);
                return;
            }

            // 구매 기간이 지정되어있는 한정구매 상품 확인
            if (!string.IsNullOrEmpty(_storeData.Expired_Time))
            {
                DateTime expiretime = DateTime.ParseExact(_storeData.Expired_Time, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
                var remaintiem = expiretime.GetRemainTime();
                if (remaintiem.Ticks <10)
                {
                    UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK,
                        LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                        "판매가 종료된 상품입니다.",
                        onClickOK: onClikc,
                        onClickClosed: onClikc);
                    return;
                }
            }

            var platformTid = -1;
#if UNITY_STANDALONE_WIN
            platformTid = _storeData.Steam_Item_Id;
#else
            platformTid = _storeData.Steam_Item_Id;
#endif

            if (RestApiManager.Instance.GetLoginType() == LoginType.GUEST)
            {
                if (uiStore == null)
                {
                    Debug.LogError("UIStore null@@@@@@@");
                    return;
                }

                if (_data == null)
                {
                    Debug.LogError("store data null @@@@@@@");
                    return;
                }

                Action onClickOk = () =>
                {
                    UnityEngine.Debug.Log("상점 구매 @@@ _1001");
                    uiStore.Purchase(platformTid, _storeData.Tid, () => { Close(); });
                    OnClickClose();
                };

                UIManager.Instance.ShowAlert(AlerType.Middle, PopupButtonType.OK_CANCEL,
                    LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                    LocalizeManager.Instance.GetString("str_ui_guest_stoar_01"),
                    onClickOK: onClickOk);
            }
            else
            {
                UnityEngine.Debug.Log("상점 구매 @@@ _1002");
                uiStore.Purchase(platformTid, _storeData.Tid);
                OnClickClose();
            }
        }
    }
}
