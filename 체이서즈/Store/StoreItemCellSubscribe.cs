using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemCellSubscribeData
{
    [SerializeField] public Store_TableData StoreData;
    [SerializeField] public string Tid;

    public StoreItemCellSubscribeData(Store_TableData data)
    {
        StoreData = data;
        Tid = data.Tid;
    }
}

public class StoreItemCellSubscribe : ScrollCell
{
    [Header("[ UI ]")]
    [SerializeField] private ExTMPUI _itemTitle;
    [SerializeField] private ExImage _bannerImg;        // 배너 이미지 
    [SerializeField] private ExTMPUI _buyCountInfo;
    [SerializeField] private ExTMPUI _remainIndex;
    [SerializeField] private GameObject _alreadyBuy;
    [Header("[ Cost ]")]
    [SerializeField] private ExButton _buyButton;
    [SerializeField] private UIStoreCost _uiStoreCost;
    [SerializeField] private GameObject _buttonDimd;
    [SerializeField] private List<UILayoutGroup> _uiLayoutGroup = new List<UILayoutGroup>();
    [Header("Compensation UI")]
    [SerializeField] private ItemCell _immediateItem;
    [SerializeField] private ItemCell _dailyItem;

    [SerializeField] private List<ItemCell> _immItems = new List<ItemCell>();
    [SerializeField] private List<ItemCell> _dailyItems = new List<ItemCell>();

    [SerializeField] private UIRedDot _uiRedDot;

    //[BoxGroup("Tap")]
    private List<StoreItemCellSubscribeData> _cellDatas;
    private Dictionary<string, List<Reward_TableData>> _rewardsDic = new Dictionary<string, List<Reward_TableData>>();
    private List<PackageItemCell> _rewardList = new List<PackageItemCell>();

    private Store_TableData _data;
    private int _itemCost;
    private int _holdAmount;
    private UIStore _uiStore;
    private bool _isPayItem = false;
    private int _maxCount;

    private List<ItemCell> _itemCells;

    private string _storeDefine;

    protected override void Init()
    {
        base.Init();
        //_itemCells = new List<ItemCell>();
        //_itemCells.Add(_cellObj.GetComponent<ItemCell>());
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;
        var data = TableManager.Instance.Store_Table[_cellDatas[DataIndex].Tid];
        _data = data;
        _maxCount =  TableManager.Instance.Define_Table["ds_subscribe_max_time"].Opt_01_Int;

        //SetRewardItemListData(data.Product);
        List<Reward_TableData> rewards = new List<Reward_TableData>();
        rewards = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
        if (!_rewardsDic.ContainsKey(_data.Tid))
        {
            _rewardsDic.Add(_data.Tid, rewards);
        }
        else
        {
            _rewardsDic[_data.Tid] = rewards;
        }

        _uiStore = UIManager.Instance.GetUI<UIStore>();
        //UpdateUI();
    }

    public void UpdateInfo(Store_TableData data)
    {
        for (int n = 0; n< _immItems.Count; n++)
        {
            _immItems[n].gameObject.SetActive(false);
        }
        for (int i = 0; i< _immItems.Count; i++)
        {
            _dailyItems[i].gameObject.SetActive(false);
        }


        _data = data;
        _maxCount =  TableManager.Instance.Define_Table["ds_subscribe_max_time"].Opt_01_Int;

        //SetRewardItemListData(data.Product);
        List<Reward_TableData> rewards = new List<Reward_TableData>();
        rewards = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
        if (!_rewardsDic.ContainsKey(_data.Tid))
        {
            _rewardsDic.Add(_data.Tid, rewards);
        }
        else
        {
            _rewardsDic[_data.Tid] = rewards;
        }

        _uiStore = UIManager.Instance.GetUI<UIStore>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        _itemTitle.ToTableText(_data.Product_Str);
        _bannerImg.SetSprite(_data.Product_Image);

        // 구매 버튼, 횟수 UI 업데이트
        //SetCostUI();
        _uiStoreCost.InitData(_data, _buyButton);
        // 보상 리스트 UI 업데이트 
        SetRewardsUI();
        // 남은 일수, max 기간 버튼 처리 UI 업데이트
        SetRemainUI();
        for (int n = 0; n <_uiLayoutGroup.Count; n++)
        {
            _uiLayoutGroup[n].UpdateLayoutGroup();
        }
    }

    private void SetRemainUI()
    {
        var eventData = GameInfoManager.Instance.EventInfo.Attendance;
        if (eventData != null && eventData.ContainsKey(_data.Tid))
        {
            DateTime endTime = new DateTime((long)eventData[_data.Tid].End, DateTimeKind.Utc);
            // DateTime startTime  = new DateTime((long)eventData[_data.Tid].Start, DateTimeKind.Utc);
            var remaindate = endTime - GameInfoManager.Instance.NowResetTime;
            var remainDay = (int)remaindate.TotalDays;
            if (remainDay < 0)
            {
                _alreadyBuy.SetActive(false);
                _remainIndex.ToTableText("str_s_purchased_not_01");         // 미구매 
                _buttonDimd.SetActive(false);
            }
            else
            {
                _alreadyBuy.SetActive(true);
                _remainIndex.ToTableText("str_s_purchased_left_01", remainDay);         // {0}일 남음 
                if (remainDay >= _maxCount)
                    _buttonDimd.SetActive(true);
                else
                    _buttonDimd.SetActive(false);
            }
        }
        else
        {
            _alreadyBuy.SetActive(false);
            _remainIndex.ToTableText("str_s_purchased_not_01");         // 미구매 
            _buttonDimd.SetActive(false);
        }
    }
    private void SetRewardsUI()
    {
        // 구매 즉시 획득
        var immRewards = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
        for (int n = 0; n< immRewards.Count; n++)
        {
            var info = immRewards[n];
            if (n < _immItems.Count)
            {
                _immItems[n].gameObject.SetActive(true);
                _immItems[n].UpdateData(info.Item_Tid, info.ITEM_TYPE, ItemCustomValueType.RewardAmount, info.Item_Min);
                _immItems[n].IsUseItemPopup = false;
                _immItems[n].SetFreeCashUI();
            }
            else
                _immItems[n].gameObject.SetActive(false);
        }

        // 매일 획득
        if (TableManager.Instance.Attendance_Reward_Table.GetDatas(_data.Tid) == null)
            return;

        var dailyRewardId = TableManager.Instance.Attendance_Reward_Table.GetDatas(_data.Tid).FirstOrDefault();
        var dailyRewardData = TableManager.Instance.Reward_Table.GetDatas(dailyRewardId.Attendance_Reward);
        for (int j  = 0; j< dailyRewardData.Count; j++)
        {
            var data = dailyRewardData[j];
            if (j < _dailyItems.Count)
            {
                _dailyItems[j].gameObject.SetActive(true);
                _dailyItems[j].UpdateData(data.Item_Tid, data.ITEM_TYPE, ItemCustomValueType.RewardAmount, data.Item_Min);
                _dailyItems[j].IsUseItemPopup = false;
                _dailyItems[j].SetFreeCashUI();
            }
            else
                _dailyItems[j].gameObject.SetActive(false);
        }
    }


    public void SetCellDatas(List<StoreItemCellSubscribeData> datas)
    {
        _cellDatas = datas;
    }

    public void OnClickSubscribeItem()
    {
        _uiStore.ShowUIPopupSubscribePopup(_data, DataIndex);
        //_uiStore.ShowPurchaseUI(_data);
    }
    public void OnClickDimdButton()
    {
        string title = LocalizeManager.Instance.GetString(_data.Product_Str);
        string str = LocalizeManager.Instance.GetString("str_store_subscribe_error_01", title, _maxCount);      //{0}월정액 기간은 {1}일을 초과할 수 없습니다.
        UIManager.Instance.ShowToastMessage($"{str}");
    }
}
