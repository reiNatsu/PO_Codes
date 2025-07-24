using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UIPopupStoreItemBuy : UIPopup
{
    [SerializeField] private ItemCell _itemCell;

    [Header("Item Info")]
    [SerializeField] private GameObject _ableBuyObj;
    [SerializeField] private ExTMPUI _ableBuyIndex;        // 구매 가능 횟수 (limit count - 되는 방식)
    [SerializeField] private ExTMPUI _itemName;                 // 아이템 이름
    [SerializeField] private ExTMPUI _itemHoldCount;            // 보유 아이템 수량

    [Header("Buy Info")]
    [SerializeField] private UICounter _uiCounter;
    [SerializeField] private ExTMPUI _itemExplain;              // 아이템 설명
    [SerializeField] private ExImage _costTypeIcon;             // 총 구매 가격 재화 아이콘
    [SerializeField] private ExTMPUI _totalCostTxt;                // 총 구개 가격 Index
    [SerializeField] private GameObject _storeItemCost;
    [SerializeField] private Transform _storeItemCostParent;

    [SerializeField] private ExImage _currentTypeIcon;
    [SerializeField] private ExTMPUI _currentCostTxt;

    [SerializeField] private List<UILayoutGroup> _uiLayoutGroup = new List<UILayoutGroup>();


    private UIStore _uiStore;
    private Store_TableData _data;
    private int _countMinBuyItem = 1;             // 최소 구매 수량 : 1
    private int _countMaxBuyItem;                // 최대 구매 수량 : 스토어 아이템 테이블에서 제한값
    private int _buyLimitCount;
   // private int _countBuyItem;                       // 왼쪽 영역 구매 횟수 카운트
    private int _updateBuyCount;                 // 수량 증감 카운트 숫자
    private int _itemCost;                              // 아이템 가격.(원래 가격)
    private int _ablePurchaseCount;
    private int _holdAmount;                              // 해당 재화 보유량
    private int _totalCost;                               // 총 가격
    
    private string _itemAbleBuyCoountIndex;
    private Reward_TableData _rewardTableData;
    private Action _onClickOk;

    private bool _adfree = false;
    private Action _adAction = null;

    public override void Refresh()
    {
        base.Refresh();

    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _data = null;
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Data, out var data))
                _data = (Store_TableData)data;
            if (optionDict.TryGetValue(UIOption.OnClickOk, out var onclikcOk))
                _onClickOk = (Action)onclikcOk;
            if (optionDict.TryGetValue(UIOption.Bool, out var adfree))
                _adfree = (bool)adfree;
            if (optionDict.TryGetValue(UIOption.Action, out var adAction))
                _adAction = (Action)adAction;
        }
     

        _uiStore = UIManager.Instance.GetUI<UIStore>();
       
        UpdateItemCell();
        UpdateUI();
    }

    private void UpdateItemCell()
    {
        // ItemCell에 정보 넘김
        _rewardTableData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(_data.Product);
        _itemCell.UpdateData(_rewardTableData.Item_Tid, _rewardTableData.ITEM_TYPE, ItemCustomValueType.RewardAmount, _rewardTableData.Item_Min);
    }

    private void UpdateUI()
    {
        //_countMaxBuyItem = SetMaxBuyCount();
        var maxdefine = TableManager.Instance.Define_Table["ds_store_max_buy_count"];
        if (maxdefine.Opt_01_Str.Equals("use"))
        {
            if (SetMaxBuyCount() > maxdefine.Opt_01_Int || SetMaxBuyCount() == 0)
                _countMaxBuyItem = maxdefine.Opt_01_Int;
            else
                _countMaxBuyItem= SetMaxBuyCount();
        }
        _holdAmount = GameInfoManager.Instance.GetAmount(_data.Material_01, Enum.GetName(typeof(ITEM_TYPE), _data.MATERIAL_01_TYPE));
        SetItemInfoLeft();

        // 아이템 설명
        //_itemExplain.ToTableText(_data.Product_Str_Desc);
        SetItemExplain(_rewardTableData);
         
        // 정보
        SetItemCostUI();
       
    }

    private void SetItemExplain(Reward_TableData rewarddata)
    {
        // item_use_table에 데이터가 있으면 해당 효과도 붙여서 설명 보여줌, 없으면 기본 설명만 보여줌

        if (!TableManager.Instance.Item_Use_Table.IsUseItem(rewarddata.Item_Tid))
        {
            // 아이템 설명
            _itemExplain.ToTableText(_data.Product_Str_Desc);
        }
        else
        {
            var cookdata = TableManager.Instance.Item_Use_Table[rewarddata.Item_Tid];
            var effectValue = cookdata.UPGRADE_TYPE== Consts.OPT_TYPE.rate || cookdata.UPGRADE_TYPE== Consts.OPT_TYPE.add_rate ? $"{cookdata.Upgrade_Value * 0.01f}%" : $"{cookdata.Upgrade_Value}";
            var cookeffecttxt = "str_ui_use_cook_effect_01".ToTableArgs($"{cookdata.EFFECT_APPLY.ToTableText().ToTableText()} {effectValue}");//$" {cookdata.EFFECT_APPLY.ToTableText().ToTableText()} {effectValue}";

            StringBuilder sb = new StringBuilder();
            sb.Append(LocalizeManager.Instance.GetString(_data.Product_Str_Desc));
            sb.Append("\n");
            sb.Append("\n");
            sb.Append(LocalizeManager.Instance.GetString("str_ui_sng_Building_ability"));
            sb.Append(" : ");
            sb.Append(cookeffecttxt);
           // sb.Append(" 증가");

            _itemExplain.text = sb.ToString();
        }

    }
    private void SetItemInfoLeft()
    {
        _updateBuyCount = _countMinBuyItem;
        // Item Info 정보들
        _itemName.ToTableText(_data.Product_Str);

        SetEnableBuyCountUI();

        
        int holdAmount =
            GameInfoManager.Instance.GetAmount(_rewardTableData.Item_Tid, Enum.GetName(typeof(ITEM_TYPE), _rewardTableData.ITEM_TYPE));
        if (_rewardTableData.ITEM_TYPE !=ITEM_TYPE.token)
            _itemHoldCount.ToTableArgs(holdAmount);
        else
        {
            var itemtable = TableManager.Instance.Item_Table[_rewardTableData.Item_Tid];
            if (!GameInfoManager.Instance.CharacterInfo.HasDosa(itemtable.Item_Use_Effect_Value))
                _itemHoldCount.ToTableText("str_ui_costume_button_03");     // 미보유
            else
                _itemHoldCount.ToTableText("str_ui_have_character");     // 보유 체이서
        }
    }

    private int SetMaxBuyCount()
    {
        int count = 0;
        if (_rewardTableData.ITEM_TYPE == ITEM_TYPE.character_coin)  // 캐릭터 조각일 경우
        {
            // var isHold = GameInfoManager.Instance.CharacterInfo.HasDosa(_rewardTableData.Item_Tid);
            var characterid = GameInfoManager.Instance.GetCharacterId(_rewardTableData.Item_Tid);
            var isHold = GameInfoManager.Instance.CharacterInfo.HasDosa(characterid);
            //기존과 달라저서 처리 필요함
            var maxCoin = TableManager.Instance.Character_Breakthrough_Table.GetMaxLimit();
            var pieceCount = 0;
            if (isHold)
                pieceCount = GameInfoManager.Instance.GetEnableCharacterCoin(_rewardTableData.Item_Tid, _rewardTableData.Item_Min);
            else
                pieceCount = maxCoin +1;

            if (_data.Use_Daily == 0 &&_data.Use_Weekliy == 0 &&_data.Use_Limit == 0 && _data.Use_Month == 0)
                count = pieceCount;
            else
            {
                var limitCount = GameInfoManager.Instance.ItemBuyLimitCount(_data);
                if (GameInfoManager.Instance.GetAmount(_data.Tid) + limitCount >= maxCoin)
                    count = 0;
                else
                    count =  GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
            }
        }
        else    // 명패 아닐때
        {
            if (_rewardTableData.ITEM_TYPE.IsEquip())
            {
                if (GameInfoManager.Instance.IsUnlimitedItem(_data))
                    count = 40;
                else
                    count = GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
            }
            else
            {
                count = GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
            }
        }
        return count;
    }

    private void SetEnableBuyCountUI()
    {
        var limitCount = GameInfoManager.Instance.ItemBuyLimitCount(_data);
        if (_rewardTableData.ITEM_TYPE != ITEM_TYPE.character_coin)
        {
            if (GameInfoManager.Instance.ItemBuyLimitCount(_data) <= 0
           || _data.Purchase_Type.Equals("free"))
            {
                // 구매 횟수 안보여줌. > 무료 아이템 이거나 무제한 구매 아이콘이거나.
                _ableBuyObj.SetActive(false);
            }
            else
            {
                _ableBuyObj.SetActive(true);
            }
        }
        else
        {
            _ableBuyObj.SetActive(true);
        }

        _ableBuyIndex.ToTableText("str_s_goods_bay_limit_01", _countMaxBuyItem);
    }
    private void SetItemCostUI()
    {
        _costTypeIcon.gameObject.SetActive(!_data.Purchase_Type.Equals("free") && !_data.Purchase_Type.Equals("ad"));
        _costTypeIcon.SetSprite(GameInfoManager.Instance.GetCostData(_data.Material_01, _data.MATERIAL_01_TYPE));
        _currentTypeIcon.SetSprite(GameInfoManager.Instance.GetCostData(_data.Material_01, _data.MATERIAL_01_TYPE));
        _itemCost = _data.Material_01_Value;
        _totalCost =  _itemCost;
        if (_data.Purchase_Type.Equals("free") )
            _totalCostTxt.ToTableText("str_ui_cash_text_002");
        else if(_data.Purchase_Type.Equals("ad"))
            _totalCostTxt.ToTableText("str_ui_ad_view");
        else
            _totalCostTxt.text = string.Format("{0:n0}", _totalCost);
        _currentCostTxt.text = string.Format("{0:n0}", _holdAmount);
        UpdateLayoutGroups();

        if (_countMaxBuyItem == 0)
        {
            _countMinBuyItem = 1;
            if (_holdAmount < _itemCost)
                _countMaxBuyItem = 1;
            else
                _countMaxBuyItem =_holdAmount /_itemCost;
        }
        else
        {
            if (_itemCost > _holdAmount)
            {
                _countMaxBuyItem = 1;
            }
            else
            {
                var totalConstValue = _itemCost *_countMaxBuyItem;
                if (totalConstValue > _holdAmount)
                {
                    _countMaxBuyItem = _holdAmount /_itemCost;
                }

            }
        }
        //if (_data.Purchase_Type.Equals("none") && _data.Use_Cost == 0 && _data.Material_01_Value ==0)
        if (_data.Purchase_Type.Equals("free"))
        {
            _uiCounter.Init(1, 1, 1, null);
        }
        else
        {
            _uiCounter.Init(_countMinBuyItem, _countMaxBuyItem, _countMinBuyItem, null, UpdateUICounter);
        }

        SetTotalCostUI();
    }

    public void UpdateLayoutGroups()
    {
        for (int n = 0; n< _uiLayoutGroup.Count; n++)
        {
            _uiLayoutGroup[n].UpdateLayoutGroup();
        }
    }

    public void UpdateUICounter(int count)
    {
        if (!_data.Purchase_Type.Equals("free") && !_data.Purchase_Type.Equals("ad"))
        {
            if (_itemCost > _holdAmount)
            {
                count = _countMinBuyItem;
            }
            _totalCost = _itemCost * count;
            if (_totalCost > _holdAmount)
            {
                _countMaxBuyItem =_holdAmount /_itemCost;
            }
            // _totalCostTxt.ToTableText(_totalCost.ToString());
            _totalCostTxt.text = string.Format("{0:n0}", _totalCost);
        }
        SetTotalCostUI();
    }


    public void SetTotalCostUI()
    {
        if (!_data.Purchase_Type.Equals("free") && !_data.Purchase_Type.Equals("ad"))
        {
            if (_totalCost > _holdAmount)       // 구매 불가인 경우
                _totalCostTxt.SetColor("#c36262");
            else
                _totalCostTxt.SetColor("#373739");
        }
        else
            _totalCostTxt.SetColor("#373739");
        UpdateLayoutGroups();
    }
  
    public void OnClickBuyBtn()
    {

        var rewardList = TableManager.Instance.Reward_Table.GetDatas(_data.Product);
        Debug.Log("<color=#9efc9e>UIPopupStoreItemBuy OnClickBuyBtn Is Full Inventory?("+GameInfoManager.Instance.CheckIsInventoryFull(rewardList)+")</color>");
        if (GameInfoManager.Instance.CheckIsInventoryFull(rewardList))   // 보유 호패가 최대여서 아이템 구매가 불가합니다.
        {
            string str = string.Empty;
            if (_holdAmount <_totalCost)
            {
                str = LocalizeManager.Instance.GetString("str_ui_shop_failed_enoughgoods");     // 재화가 부족합니다.
            }
            else
            {
                str = LocalizeManager.Instance.GetString("str_purchase_deny_01"); // 보유 호패가 최대여서 스테이지 진행이 불가합니다.
            }
            UIManager.Instance.ShowToastMessage($"{str}");
            return;
        }

        switch (_data.Purchase_Type)
        {
            case "free":
                {
                    RestApiManager.Instance.RequestStoreUseStore(_data.Tid, "1", _data.Store_Type,
                      (res) =>
                      {
                          _uiStore.UpdateScroll(_data.Group_Id);
                          _onClickOk?.Invoke();
                          OnClickClose();
                      });
                }
                break;
            case "ad":
                {
                    if (_adfree)
                    {
                        RestApiManager.Instance.RequestStoreUseStore(_data.Tid, "1", _data.Store_Type,
                        (res) =>
                        {
                            _uiStore.UpdateScroll(_data.Group_Id);
                            _onClickOk?.Invoke();
                            OnClickClose();
                        });
                    }
                    else
                    {
                        Debug.Log("광고 구매 결과 후 _adAction P");
                        _adAction?.Invoke();
                        Debug.Log("광고 구매 결과 후 _adAction E + _onClickOk P");
                        _onClickOk?.Invoke();
                        Debug.Log("광고 구매 결과 후 _onClickOk E");
                        OnClickClose();
                    }
                }
                break;
            default:
                {
                    if (_holdAmount >=_totalCost)
                    {
                        RestApiManager.Instance.RequestStoreUseStore(_data.Tid, _uiCounter.Count.ToString(), _data.Store_Type,
                            (res) =>
                            {
                                _onClickOk?.Invoke();
                                OnClickClose();
                            });
                    }
                    else
                    {
                        string str = LocalizeManager.Instance.GetString("str_ui_shop_failed_enoughgoods");// 재화가 부족합니다.
                        UIManager.Instance.ShowToastMessage($"{str}");
                    }
                }
                break;
        }
        //if (_data.Purchase_Type.Equals("none") && _data.Use_Cost == 0 && _data.Material_01_Value ==0)
        //if (_data.Purchase_Type.Equals("free") ||_data.Purchase_Type.Equals("ad"))
        //{
        //    RestApiManager.Instance.RequestStoreUseStore(_data.Tid, "1", _data.Store_Type,
        //           (res) =>
        //           {
        //               _uiStore.UpdateScroll(_data.Group_Id);
        //           });
        //}
        //else if (_holdAmount >=_totalCost)
        //{
        //    RestApiManager.Instance.RequestStoreUseStore(_data.Tid, _uiCounter.Count.ToString(), _data.Store_Type,
        //        (res) =>
        //        {
        //            _onClickOk?.Invoke();
        //        });
        //}
        //else
        //{
        //        string str = LocalizeManager.Instance.GetString("str_ui_shop_failed_enoughgoods");// 재화가 부족합니다.
        //    UIManager.Instance.ShowToastMessage($"{str}");
        //}
    }


    public override void Close(bool needCached = true)
    {
        Refresh();
        base.Close(needCached);
    }
}
