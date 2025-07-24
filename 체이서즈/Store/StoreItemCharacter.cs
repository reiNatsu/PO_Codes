using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using AmplifyShaderEditor;
#endif


public class StoreItemCellCharacterData
{
    [SerializeField] public Store_TableData storeData;
    [SerializeField] public string Tid;

    public StoreItemCellCharacterData(Store_TableData data)
    {
        storeData = data;
        Tid = data.Tid;
    }
}

public class StoreItemCharacter : ScrollCell
{
    [SerializeField] private ExImage _itemIcon;            // 상품 아이콘
    [SerializeField] private ExTMPUI _nameTMP;      // 상품 이름
    [SerializeField] private ExTMPUI _limitCountTMP;    // 계정 구매 횟수 
    [SerializeField] private GameObject _onSaleObj;     // 세일 Obj
    [SerializeField] private ExTMPUI _salePriceTMP;     // 세일 가격 
    [SerializeField] private ExTMPUI _remainTimeTMP;        // 남은 시간
    [SerializeField] private GameObject _timeObj;
    [SerializeField] private UIStoreCost _uiStoreCost;
    [SerializeField] private GameObject _dimd;

    [SerializeField] private List<UILayoutGroup> _uiLayoutGroups = new List<UILayoutGroup>();

    private List<StoreItemCellCharacterData> _cellDatas;
    private Store_TableData _data;
    private UIStore _uiStore;
    private int _itemCost;
    private int _holdAmount;
    private string _storeDefine;
   
    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;
        _data = TableManager.Instance.Store_Table[_cellDatas[DataIndex].Tid];
        _uiStore = UIManager.Instance.GetUI<UIStore>();
       // SetReddot();
        UpdateUI();
    }
    public void UpdateUI()
    {
        // 상품 아이콘 상하 적용(원본 사이즈 적용)   
       

        if (!string.IsNullOrEmpty(_data.Product_Image))
        {
            _itemIcon.SetSprite(_data.Product_Image);
        }

        //var costData = TableManager.Instance.Item_Table[_data.Material_01];
        SetTitleUI();
        SetEnableUI();
        SetBuyCount();
        // cost Ui 수정
        //SetCostUI();
        _uiStoreCost.InitData(_data);
     
        _onSaleObj.SetActive(false);
        for (int n = 0; n < _uiLayoutGroups.Count; n++)
        {
            _uiLayoutGroups[n].UpdateLayoutGroup();
        }
    }
    private void SetTitleUI()
    {
        _nameTMP.ToTableText(_data.Product_Str);
    }
    private void SetBuyCount()
    {
        var buyLimitCount =GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid);
        //  _limitCountTMP.text = "계정 구매횟수 "+ buyLimitCount;
        _limitCountTMP.ToTableText("str_s_goods_bay_limit_account_01", buyLimitCount);

        if (string.IsNullOrEmpty(_data.Expired_Time))
            _timeObj.SetActive(false);
        else
            _timeObj.SetActive(buyLimitCount > 0);

    }

    public void SetCellDatas(List<StoreItemCellCharacterData> datas)
    {
        _cellDatas = datas;
    }

    public void SetEnableUI()
    { 
        var rewardData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(_data.Product);
        var itemData = TableManager.Instance.Item_Table[rewardData.Item_Tid];
        if (itemData.ITEM_ETC_SUBGROUP == ITEM_ETC_SUBGROUP.costume)
        {
            bool isDimd = GameInfoManager.Instance.GetAbleBuyItemCount(_data.Group_Id, _data.Tid) == 0;
            _dimd.SetActive(isDimd);
        }
        else
        {
            var tid = itemData.Item_Use_Effect_Value;
            _dimd.SetActive(GameInfoManager.Instance.CharacterInfo.HasDosa(tid));
            //캐릭터 토큰

        }
    }

    public void OnClickPayItem()
    {
        var rewardData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(_data.Product);
        var itemData = TableManager.Instance.Item_Table[rewardData.Item_Tid];
        if (itemData.ITEM_ETC_SUBGROUP == ITEM_ETC_SUBGROUP.costume)
        {
            var costumeData = TableManager.Instance.Costume_Table[itemData.Item_Acquire_Effect_Value];
           
            UIManager.Instance.Show<UISkinInfoPopup>(Utils.GetUIOption(
              UIOption.Data, costumeData,
              UIOption.Data2, _data
              ));
        }
        else
        {
            //캐릭터 토큰
            Action endBuy = () => {
                UpdateUI();
                _uiStore.UpdateScroll(_data.Group_Id, DataIndex);
            };

        
                Dictionary<UIOption, object> options = Utils.GetUIOption(
                    UIOption.Data, _data,
                    UIOption.OnClickOk, endBuy);

                UIManager.Instance.Show<UIPopupStoreItemBuy>(options);
            }
       
        //  _uiStore.ShowPurchaseUI(_data);
    }

}
