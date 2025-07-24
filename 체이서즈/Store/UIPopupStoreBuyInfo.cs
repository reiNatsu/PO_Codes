using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupStoreBuyInfo : UIPopup
{
    [SerializeField] private ExTMPUI _itemExplain;
    [SerializeField] private GameObject _rewardSlide;
    [SerializeField] private GameObject _rewardIndividual;
    [SerializeField] private ExTMPUI _additionalRewardText;
    [SerializeField] private ExImage _productImg;
    [SerializeField] private ExTMPUI _productName;
    [SerializeField] private ExTMPUI _productCount;
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private ExButton _dimdBtn;

    [Header("[ Daily Reward ]")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private HorizontalLayoutGroup _layout;
    [SerializeField] private GameObject _cellObj;

    [SerializeField] private GameObject _dailyRewardArea;
    [SerializeField] private List<ItemCell> _immedeatilyRewardItems = new List<ItemCell>();
    //[SerializeField] private List<ItemCell> _dailyRewardItems = new List<ItemCell>();
    [SerializeField]
    private UILayoutGroup[] _layoutGroup;

    private void UpdateLayoutGroup()
    {
        for (int i = 0; i<_layoutGroup.Length; i++)
        {
            _layoutGroup[i].UpdateLayoutGroup();
        }
    }

    private List<PackageRewardItem> _cells;
    private List<PackageRewardItemData> _cellDatas = null;
    private List<Reward_TableData> _rewardList = new List<Reward_TableData>();
    private List<ItemCell> _itemCells;

    private int _type;
    private UIStore _uiStore;
    private Store_TableData _data;

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _scroll.Init();
        _cells = _scroll.GetCellToList<PackageRewardItem>();
        _cellDatas = new List<PackageRewardItemData>();

        _itemCells = new List<ItemCell>();
        _itemCells.Add(_cellObj.GetComponent<ItemCell>());

        _data = null;
        SetInitialiseRewardCells();       // 보상 아이템 오브젝트 초기화.

        if (optionDict.TryGetValue(UIOption.Data, out var data))
        {
            _data = (Store_TableData)data;
        }
        if (optionDict.TryGetValue(UIOption.Int, out var type))
        {
            _type = (int)type;
        }
        if (optionDict.TryGetValue(UIOption.Bool, out var isOn))
        {
            if (_dimdBtn != null)
            {
                _dimdBtn.onClick.AddListener(() => Close((bool)isOn));
            }
        }

        _uiStore = UIManager.Instance.GetUI<UIStore>();
        UpdateUI();
    }

    private void SetInitialiseRewardCells()
    {
        for (int n = 0; n < _immedeatilyRewardItems.Count; n++)
        {
            _immedeatilyRewardItems[n].gameObject.SetActive(false);
        }
        //for (int n = 0; n < _dailyRewardItems.Count; n++)
        //{
        //    _dailyRewardItems[n].gameObject.SetActive(false);
        //}
        for (int i = 0; i <_itemCells.Count;i++)
        {
            _itemCells[i].gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        _itemExplain.ToTableText(_data.Product_Str_Desc);
        SetRewardTypeUI();
        SetProductImageUI();
        if (_uiStore != null && _uiStore.isActiveAndEnabled)
        {
            var countStr = _uiStore.SetAblePurchaseStr(_data);
            if (!string.IsNullOrEmpty(countStr))
            {
                var maxCount = GameInfoManager.Instance.ItemBuyLimitCount(_data);
                var ableCount = GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
                _productCount.ToTableText(countStr, ableCount, maxCount);
            }
            else
            {
                _productCount.gameObject.SetActive(false);
            }
        }

        UpdateLayoutGroup();
    }

    private void SetProductImageUI()
    {
        // 여기서 상품 이름도 같이
        _productName.ToTableText(_data.Product_Str);

        if (!string.IsNullOrEmpty(_data.Product_Popup_Image))
        {
            _productImg.SetSprite(_data.Product_Popup_Image);
        }
        else
        {
            var productdata = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(_data.Product);
            //var imgStr = _uiStore.GetCostData(productdata.Item_Tid, (ITEM_TYPE)productdata.ITEM_TYPE);
            var imgStr = GameInfoManager.Instance.GetCostData(productdata.Item_Tid, (ITEM_TYPE)productdata.ITEM_TYPE);
            _productImg.SetSprite(imgStr);
        }
    }

    private void SetRewardTypeUI()
    {
        // List<Reward_TableData> rewardData = new List<Reward_TableData>();
        _rewardList = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
        switch (_type)
        {
            case 2:
                {
                    SetRewardItemArea(false);
                    SetSubRewardItems();
                    // rewardData = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
                }
                break;
            case 3:
                {
                    SetRewardItemArea(true);
                    //rewardData = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
                    //SetMainRewardItems(rewardData);
                    SetRewardListUI();
                }
                break;
            case 4:
                {
                    SetRewardItemArea(true);
                    SetRewardListUI();
                }
                break;
        }
        SetMainRewardItems(_rewardList);
      
    }

    private void SetMainRewardItems(List<Reward_TableData> datas)
    {
        // 구매즉시
        int count = 0;
        if (datas.Count > 2)
        {
            count = 2;
        }
        else
        {
            count=datas.Count;
        }

        for (int n = 0; n < count; n++)
        {
            _immedeatilyRewardItems[n].gameObject.SetActive(true);
            _immedeatilyRewardItems[n].UpdateData(datas[0].Item_Tid, datas[0].ITEM_TYPE, ItemCustomValueType.RewardAmount, datas[0].Item_Min);
            //_immedeatilyRewardItems[n].SetFreeCashUI();
        }
    }

    private void SetSubRewardItems()
    {
        _dailyRewardArea.SetActive(true);
       
        if (_data.Purchase_Type.Equals("none"))
        {
            _dailyRewardArea.SetActive(false);
            return;
        }

        switch (_type)
        {
            case 2:  // sub
                {
                    List<ItemCellData> itemDatas = new List<ItemCellData>();
                    _additionalRewardText.ToTableText("str_store_daily_reward_01");
                    var dailyRewardId = TableManager.Instance.Attendance_Reward_Table.GetDatas(_data.Tid).FirstOrDefault();
                    itemDatas = Utils.ToItemCellDatas(dailyRewardId.Attendance_Reward);
                    //var dailyRewardGroupId = TableManager.Instance.Reward_Table.GetDatas(_data.Product);

                    //for (int n = 0; n <dailyRewardGroupId.Count; n++)
                    //{
                    //   // _dailyRewardItems[n].gameObject.SetActive(true);
                    //    // 매일 획득

                    //    //var itemDatas = Utils.ToItemCellDatas(dailyRewardId.Attendance_Reward);

                    //    //var dailyRewardData = TableManager.Instance.Reward_Table.GetDatas(dailyRewardId.Attendance_Reward);
                    //    //_dailyRewardItems[n].UpdateData(dailyRewardData[n].Item_Tid, dailyRewardData[n].ITEM_TYPE, ItemCustomValueType.RewardAmount, dailyRewardData[n].Item_Min);
                    //    ////_dailyRewardItems[n].SetFreeCashUI();
                    //}

                    if (itemDatas.Count > _itemCells.Count)
                        CreateCell(itemDatas.Count - _itemCells.Count);

                    UpdateReward(itemDatas);
                }
                break;
            //case 3:
            //    {
            //        _additionalRewardText.ToTableText("str_store_first_product_reward_01");
            //        if (!_data.Purchase_Type.Equals("none"))
            //        {
            //            var rewardData = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
            //            int count = 0;
            //            if (rewardData.Count > 2)
            //            {
            //                count = 2;
            //            }
            //            else
            //            {
            //                count=rewardData.Count;
            //            }
            //            for (int n  = 0; n < count; n++)
            //            {
            //                _dailyRewardItems[n].UpdateData(rewardData[n].Item_Tid, rewardData[n].ITEM_TYPE, ItemCustomValueType.RewardAmount, rewardData[n].Item_Min);
            //            }
            //        }
            //    }
            //    break;
            default:
                break;
        }

    }

    private void CreateCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_cellObj, _layout.transform);
            var itemCell = newObj.GetComponent<ItemCell>();

            _itemCells.Add(itemCell);
        }
    }

    private void UpdateReward(List<ItemCellData> itemCellDatas)
    {
        if (itemCellDatas.Count > 3)
        {
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _layout.padding.right = 15;
        }
        else
        {
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _layout.padding.right = 0;
        }

        for (int i = 0; i < _itemCells.Count; i++)
        {
            if (i < itemCellDatas.Count)
            {
                _itemCells[i].UpdateData(itemCellDatas[i].Tid, itemCellDatas[i].ItemType, itemCellDatas[i].CustomValueType, itemCellDatas[i].RewardAmount);
                // _itemCells[i].SetFreeCashUI();
            }
            else
                _itemCells[i].gameObject.SetActive(false);
        }
    }


    private void SetRewardListUI()
    {
        _cellDatas.Clear();
        //var rewardData = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
        for (int n = 0; n <_rewardList.Count; n++)
        {
            _cellDatas.Add(new PackageRewardItemData(_rewardList[n]));
        }
        for (int i = 0; i < _cellDatas.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }

        _scroll.ActivateCells(_rewardList.Count);
    }

    private void SetRewardItemArea(bool isSlide)
    {
        _rewardSlide.SetActive(isSlide);
        _rewardIndividual.SetActive(!isSlide);
    }
                              
    public void OnClickPurchase()
    {
        Action onClikc = () =>
        {
            Debug.Log("인앱 구매 PurchaseEndCallback");
            //_uiStore.UpdateScroll(_data.Group_Id, _data.Order);
            UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "cash"));
            Close();
        };

        if (GameInfoManager.Instance.CheckIsInventoryFull(_rewardList))
        {
            string str = LocalizeManager.Instance.GetString("str_purchase_deny_01");  // 보유 칩셋이 최대여서 아이템 구매가 불가합니다.
            UIManager.Instance.ShowToastMessage($"{str}");
            return;
        }


        // if (_data.Purchase_Type.Equals("none") && _data.Use_Cost == 0 && _data.Material_01_Value == 0)  
        if (_data.Purchase_Type.Equals("free"))
        {
            RestApiManager.Instance.RequestStoreUseStore(_data.Tid, "1", _data.Store_Type, 
               (res) => 
               {
                   _uiStore.UpdateScroll(_data.Group_Id);
                   Close();
               });
        }
        else
        {

            UnityEngine.Debug.Log("상점 구매 @@@ _0");
            if (!NowGGManager.Instance.IsNowGGActive && !IAPManager.Instance.IsInit())
            {
                UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK,
                       LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                       "IAPManager.Instance.IsInit() = false",
                       onClickOK: onClikc,
                       onClickClosed: onClikc);
                return;
            }

            // 구매 기간이 지정되어있는 한정구매 상품 확인
            if (!string.IsNullOrEmpty(_data.Expired_Time))
            {
                DateTime expiretime = DateTime.ParseExact(_data.Expired_Time, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
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
            platformTid = _data.Steam_Item_Id;
#else
            platformTid = _data.Steam_Item_Id;
#endif

            if (RestApiManager.Instance.GetLoginType() == LoginType.GUEST)
            {
                if(_uiStore == null)
                {
                    Debug.LogError("UIStore null@@@@@@@");
                    return;
                }

                if(_data == null)
                {
                    Debug.LogError("store data null @@@@@@@");
                    return;
                }

                Action onClickOk = () =>
                {
                    UnityEngine.Debug.Log("상점 구매 @@@ _101");
                    _uiStore.Purchase(platformTid, _data.Tid);
                    OnClickClose();
                };

                UIManager.Instance.ShowAlert(AlerType.Middle, PopupButtonType.OK_CANCEL,
                    LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                    LocalizeManager.Instance.GetString("str_ui_guest_stoar_01"),
                    onClickOK: onClickOk);
            }
            else
            {
                UnityEngine.Debug.Log("상점 구매 @@@ _102");
                _uiStore.Purchase(platformTid, _data.Tid);
                OnClickClose();
            }
            // _uiStore.Purchase(_data.Tid);
        }
    }

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
        SetInitialiseRewardCells();
    }
}
