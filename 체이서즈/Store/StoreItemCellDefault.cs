using Consts;
using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StoreItemCellDefaultData 
{
    [SerializeField] public Store_TableData storeData;
    [SerializeField] public string Tid;

    public StoreItemCellDefaultData(Store_TableData data)
    {
        storeData = data;
        Tid = data.Tid;
    }
}

public class StoreItemCellDefault : ScrollCell
{
    [Header("[ UI ]")]
    [SerializeField] private ItemCell _itemCell;
    [SerializeField] private ExTMPUI _itemTitle;
    [SerializeField] private ExTMPUI _buyCount;
    [SerializeField] private GameObject _buyObject;
    [Header("[ Item Cost ]")]
    [SerializeField] private UIStoreCost _uiStoreCost;
    [Header("[ UI Sales ]")]
    [SerializeField] private GameObject _saleObj;
    [SerializeField] private GameObject _dimdObj;
    [SerializeField] private ExTMPUI _saleTxt;

    [SerializeField] private UIRedDot _uiRedDot;

    [Header("[ UI Layout Group ]")]
    [SerializeField] private List<UILayoutGroup> _uiLayoutGroup = new List<UILayoutGroup>();

    private UIStore _uiStore;
    private List<StoreItemCellDefaultData> _cellDatas;
    private Dictionary<string, MaterialData> _itemCostDict = new Dictionary<string, MaterialData>();
    private Store_TableData _data;
    private int _cost;

    private string _storeDefine;

    private string _storeTabType;
    private float _checkTime = 1.0f;
    private float _clickTime;
    private int _buyLimitCnt = 0;       // 최대 구매 횟수(테이블 리밋 value)
    //private int _buyItemCount = 0;      // 구매 갯수(차감방식)

    private Reward_TableData _rewardData = new Reward_TableData();
    //public int BuyItemCnt { get { return _buyItemCount; } set { _buyItemCount = value; } }

    public Dictionary<string, MaterialData> ItemCostDict { get { return _itemCostDict; } }
  public string StoreTabType { get { return _storeTabType; } }
    public void UpdateCost(int value)
    {
       // _costValue.text =value.ToString();
    }

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
        UpdateItemData();
    }

    private void UpdateItemData()
    {
        // ItemCell에 정보 넘김
        _rewardData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(_data.Product);
        _itemCell.UpdateData(_rewardData.Item_Tid, _rewardData.ITEM_TYPE, ItemCustomValueType.RewardAmount, _rewardData.Item_Min);

        //SetReddot();
        UpdateUI();
    }

    public void UpdateUI()
    {
       //SetMaxCount();
        _itemTitle.ToTableText(_data.Product_Str);
        // 아이템 가격 
        _uiStoreCost.InitData(_data, isAdFree: CheckAdGetFree());

        SetBuyCountObjUI();
        //UpdateBuyCount();
        // 세일 항목 처리
        SetOnSale(false); 
        // 솔드 아웃 처리
       // _buyItemCount = GameInfoManager.Instance.GetAbleBuyItemCount(_storeTabType, _data.Tid);
        //SetSoldOut(_rewardData.ITEM_TYPE == ITEM_TYPE.character_coin, GameInfoManager.Instance.GetSoldOutItem(_data.Group_Id,_data.Tid));
        for (int n = 0; n< _uiLayoutGroup.Count; n++)
        {
            _uiLayoutGroup[n].UpdateLayoutGroup();
        }
    }

    private void SetBuyCountObjUI()
    {
        if (_rewardData.ITEM_TYPE == ITEM_TYPE.character_coin)  // 캐릭터 조각일때
        {
            _buyObject.SetActive(true);
        }
        else
        {
            if (GameInfoManager.Instance.ItemBuyLimitCount(_data) == 0
               || _data.Purchase_Type.Equals("free") )
            {
                _buyObject.SetActive(false);
            }
            else
            {
                _buyObject.SetActive(true);
            }
        }
        UpdateBuyCount();
    }
    // 아이콘 구매 횟수 count 업데이트
    public void UpdateBuyCount()
    {
        if (_rewardData.ITEM_TYPE == ITEM_TYPE.token)
        {
            var itemdata = TableManager.Instance.Item_Table[_rewardData.Item_Tid];
            if (itemdata == null)
                _buyLimitCnt = 1;
            else
            {
                bool hasDosa = GameInfoManager.Instance.CharacterInfo.HasDosa(itemdata.Item_Use_Effect_Value);
                if (hasDosa)
                    _buyLimitCnt = 0;
                else
                    _buyLimitCnt = 1;
            }
        }
        else    // 캐릭터 조각 아닐때
        {
            //_buyLimitCnt = GameInfoManager.Instance.ItemBuyLimitCount(_data);
            _buyLimitCnt =GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
        }

        // 광고 아이템일 경우
        //if (_data.Purchase_Type.Equals("ad"))
        //{
        //    if (!string.IsNullOrEmpty(_data.Ad_Goods_Tid)&& GameInfoManager.Instance.AccountInfo.CheckEnableAdItem(_data.Ad_Goods_Tid))
        //        _buyLimitCnt = GameInfoManager.Instance.AccountInfo.AdsRewardCount[_data.Ad_Goods_Tid];
        //}

        SetSoldOut(_buyLimitCnt <= 0);
        _buyCount.ToTableText("str_s_goods_bay_limit_01", _buyLimitCnt);
    }
 
    public void SetCellDatas(List<StoreItemCellDefaultData> datas, string type)
    {
        _cellDatas = datas;
        _storeTabType = type;
    }

    public void RefreshCellDatas()
    {
        _cellDatas = new List<StoreItemCellDefaultData>();
    }

    // 세일 항목 처리
    public void SetOnSale(bool isOn)
    {
        if (isOn)
        {
            string saleStr = _saleTxt.text;
            _saleTxt.ToTableText(string.Format(saleStr, "30"));
        }
        _saleObj.SetActive(isOn);
    }

    // 구매 완료 처리
    public void SetSoldOut(bool isDind)
    {
        bool isOn = false;
        if (!GameInfoManager.Instance.IsUnlimitedItem(_data))
        {   // 구매 제한이 있는 경우 
            isOn = isDind;
        }
        else        // 구매 제한이 없는 경우
        {
            isOn = false;
        }
            // 광고일경우
        //    if (_data.Purchase_Type.Equals("ad"))
        //{
        //    if (!string.IsNullOrEmpty(_data.Ad_Goods_Tid) && GameInfoManager.Instance.AccountInfo.CheckEnableAdItem(_data.Ad_Goods_Tid))
        //    {
        //        isOn = !GameInfoManager.Instance.AccountInfo.IsEnableAds(_data.Ad_Goods_Tid);
        //    }
        //}

        _dimdObj.SetActive(isOn);
    }

    private bool CheckAdGetFree()
    {
        bool isfree = false;
        var premiumid = TableManager.Instance.Define_Table["ds_store_month_product"].Opt_01_Str;
        var monthly = GameInfoManager.Instance.EventInfo.Attendance;
        if (monthly != null && monthly.ContainsKey(premiumid))
        {
            DateTime endTime = new DateTime((long)monthly[premiumid].End, DateTimeKind.Utc);
            // DateTime startTime  = new DateTime((long)eventData[_data.Tid].Start, DateTimeKind.Utc);
            var remaindate = endTime - GameInfoManager.Instance.NowResetTime;
            var remainDay = (int)remaindate.TotalDays;
            isfree = remainDay >= 0;
        }
        else
            isfree = false;

        return isfree;
    }

    public void OnClickItem()
    {
        Action endBuy = () => {
            Debug.Log("광고 구매 결과 후 endBuy in storeitemcelldefault");
            UpdateUI();
            _uiStore.UpdateScroll(_data.Group_Id, DataIndex);
        };

        if (_data.Purchase_Type.Equals("ad") && !CheckAdGetFree())
        {
            if (!string.IsNullOrEmpty(_data.Ad_Goods_Tid) || _data.Purchase_Type.Equals("ad"))
            {
                if (GameInfoManager.Instance.AccountInfo.IsEnableAds(_data.Ad_Goods_Tid))
                {
                    var adData = TableManager.Instance.Ad_Table[_data.Ad_Goods_Tid];

#if ((UNITY_ANDROID || UNITY_IOS)&& !UNITY_EDITOR && !UNITY_STANDALONE_WIN)

 Action adAction = () => {
                       LevelPlayManager.Instance.ShowRewardedVideo(adData.Placement, endBuy, () => {
                        UIManager.Instance.ShowToastMessage("str_ui_ad_no_entire_03"); //광고가 소진 되었습니다.
                    });
                   };

Dictionary<UIOption, object> options = Utils.GetUIOption(
          UIOption.Data, _data,
           UIOption.OnClickOk, endBuy,
           UIOption.Bool, CheckAdGetFree(),
           UIOption.Action, adAction);

           UIManager.Instance.Show<UIPopupStoreItemBuy>(options);

                   //  LevelPlayManager.Instance.ShowRewardedVideo(adData.Placement, endBuy, () => {
                   //    var errormsg = "ShowRewardedVideo("+_data.Ad_Goods_Tid+") Faild";
                   //    UIManager.Instance.ShowToastMessage(errormsg);
                   //});
                    //  LevelPlayManager.Instance.ShowRewardedVideo(adData.Placement, endBuy, () => {
                    //    UIManager.Instance.ShowToastMessage("str_ui_ad_no_entire_03"); //광고가 소진 되었습니다.
                    //});
#elif (!UNITY_ANDROID && !UNITY_EDITOR || UNITY_STANDALONE_WIN)
 StringBuilder sb = new StringBuilder();
                    var alerstr = LocalizeManager.Instance.GetString("str_ui_ad_no_entire_02");
                    sb.Append(LocalizeManager.Instance.GetString("str_ui_ad_no_entire_01"));
                    sb.AppendLine();
                    sb.Append("<size=80%>"+alerstr+"</size>");

                    UIManager.Instance.ShowAlert(AlerType.Small,PopupButtonType.OK, message: sb.ToString());
#elif UNITY_EDITOR
                    Action adAction = () => {
                    };

                    Dictionary<UIOption, object> options = Utils.GetUIOption(
                              UIOption.Data, _data,
                               UIOption.OnClickOk, endBuy,
                               UIOption.Bool, true);

                    UIManager.Instance.Show<UIPopupStoreItemBuy>(options);
#endif

                }
            }
        }
        else
        {
            Dictionary<UIOption, object> options = Utils.GetUIOption(
           UIOption.Data, _data,
           UIOption.Bool, CheckAdGetFree(),
            UIOption.OnClickOk, endBuy);

            UIManager.Instance.Show<UIPopupStoreItemBuy>(options);
        }
        
    }
}