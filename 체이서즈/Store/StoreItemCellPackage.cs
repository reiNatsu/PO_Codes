using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class StoreItemCellPackageData
{
    [SerializeField] public Store_TableData StoreData;
    [SerializeField] public string Tid;


    public StoreItemCellPackageData(Store_TableData data)
    {
        StoreData = data;
        Tid = data.Tid;
    }
}

public class StoreItemCellPackage : ScrollCell
{
    [SerializeField] private ExImage _itemImg;
    [SerializeField] private ExTMPUI _title;
    // [SerializeField] private ExTMPUI _costTxt;
    [SerializeField] private UIStoreCost _uiStoreCost;
    [SerializeField] private ExTMPUI _mark;
    [SerializeField] private GameObject _timerObj;
    [SerializeField] private ExTMPUI _timerText;
    [SerializeField] private GameObject _dimd;
    [SerializeField] private GameObject _buttonDimd;

    [SerializeField] private GameObject _ablePurchaseObject;
    [SerializeField] private ExTMPUI _ableBuyIndex;


    [SerializeField] private UILayoutGroup[] _uiLayoutGroup;

    private List<StoreItemCellPackageData> _cellDatas;
    private Store_TableData _data;
    private UIStore _uiStore;

    private int _ableCount = 0; // 구매 가능 횟수
    private string _storeTabType;

    protected override void Init()
    {
        base.Init();
        _uiStore = UIManager.Instance.GetUI<UIStore>();
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;

        var data = TableManager.Instance.Store_Table[_cellDatas[DataIndex].Tid];
        _data = data;

        //SetReddot();
        UpdateUI();
    }

    private void OnDisable()
    {

        
    }


    public void UpdateUI()
    {
        if (_data.Product_Image_Use_Type == 1)
        {
            _itemImg.SetSprite(_data.Product_Image.ToString());
        }

        SetMarkUI(_data.Mark_Type);
        SetTimerUI();
        _title.ToTableText(_data.Product_Str); 
        //_costTxt.text = "￦ "+_data.Use_Cost;
        _uiStoreCost.InitData(_data);

        SetAblePurchase();
        SetItemOnSale();
        for (int n = 0; n <_uiLayoutGroup.Length; n++ )
        {
            _uiLayoutGroup[n].UpdateLayoutGroup();
        }
    }

    public void SetItemOnSale()
    {
        if (!GameInfoManager.Instance.IsUnlimitedItem(_data) && GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid) == 0)
        {
            _dimd.SetActive(true);
            _buttonDimd.SetActive(true);
        }
        else
        {
            _dimd.SetActive(false);
            _buttonDimd.SetActive(false);
        }
    }

    private void SetAblePurchase()
    {
        var countStr = _uiStore.SetAblePurchaseStr(_data);
        if (!string.IsNullOrEmpty(countStr))
        {
            if (!_ablePurchaseObject.activeInHierarchy)
            {
                _ablePurchaseObject.SetActive(true);
            }
            _ableBuyIndex.text = countStr;
            var maxCount = GameInfoManager.Instance.ItemBuyLimitCount(_data);
            var ableCount = GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
            _ableBuyIndex.ToTableText(countStr, ableCount);
            //_ableMaxIndex.text = GameInfoManager.Instance.ItemBuyLimitCount(_data).ToString();
            //_ableCountIndex.text  = GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid).ToString();
        }
        else
        {
            _ablePurchaseObject.SetActive(false);
            //_ableBuyIndex.gameObject.SetActive(false);
        }
    }

    private void SetMarkUI(string index)
    {
        if (!index.Equals("none"))
        {
            _mark.gameObject.SetActive(true);
            _mark.text = index;
        }
        else {
            _mark.gameObject.SetActive(false);
        }
        string textStr;
        if (index.Equals("new"))
        {
            textStr = "#007bff";
        }
        else
        {
            textStr = "#dc3545";
        }
        ColorUtility.TryParseHtmlString(textStr, out Color new_color);
        _mark.color = new_color;
    }

    private void SetTimerUI()
    {
        if (string.IsNullOrEmpty(_data.Expired_Time))
        {
            _timerObj.SetActive(false);
            return;
        }
        _timerObj.SetActive(true);
      
        SetRemainTimeUI();
    }

    private TimeSpan GetItemRemainTime()
    {
        var period = _data.Expired_Time;
        string format = "MM/dd/yyyy HH:mm";
        CultureInfo provider = CultureInfo.InvariantCulture;

        DateTime expiretime = DateTime.ParseExact(period, format, provider, DateTimeStyles.AssumeLocal);
        return expiretime.GetRemainTime();
    }

    private void SetRemainTimeUI()
    {
        //var timeSpan = expiretime.GetRemainTime();
        var timeSpan = GetItemRemainTime();

        if (_timerText != null)
        {
            if (timeSpan.Days > 0)
            {
                _timerText.text = "str_ui_post_remain_time_02".ToTableArgs(timeSpan.Days, timeSpan.Hours) + " " + "str_ui_store_remain".ToTableText();
            }
            else
            {
                if (timeSpan.Hours > 0)
                {
                    _timerText.text = "str_ui_post_remain_time_03".ToTableArgs(timeSpan.Hours) + " " + "str_ui_store_remain".ToTableText();
                }
                else
                {
                    _timerText.text = "str_ui_post_remain_time_04".ToTableArgs(timeSpan.Minutes) + " " + "str_ui_store_remain".ToTableText();
                    //if (timeSpan.Minutes > 0)
                    //{
                    //    _timerText.text = "str_ui_post_remain_time_04".ToTableArgs(timeSpan.Minutes) + " 남음";
                    //}
                    //else
                    //{
                    //    // str = string.Format(" {0:D2}초", time.Seconds);
                    //    _timerText.text = "str_ui_post_remain_time_05".ToTableArgs(timeSpan.Seconds) + " 남음";
                    //}
                }
            }
        }
    }
    public void SetCellDatas(List<StoreItemCellPackageData> datas)
    {
        _cellDatas = new List<StoreItemCellPackageData>();
        _cellDatas = datas;
    }

    public void OnClickPackageItem()
    {
        Action onClikc = () =>
        {
            UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "cash"));
            //_uiStore.UpdateScroll(_data.Group_Id, _data.Order);
        };

        if (!string.IsNullOrEmpty(_data.Expired_Time))
        {
            var expiredtime = GetItemRemainTime();
            var now = DateTime.UtcNow.ToAddHours().Ticks;
            if (expiredtime.Ticks < 10)
            {
                UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK,
                     LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                     "판매가 종료된 상품입니다.",
                     onClickOK: onClikc,
                     onClickClosed: onClikc);
                return;
            }
        }

      
            _uiStore.ShowUIPopupStoreBuyInfo(_data, DataIndex);
        //_uiStore.ShowPurchaseUI(_data);
    }
}
