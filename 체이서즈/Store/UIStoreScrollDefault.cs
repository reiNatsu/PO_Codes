using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStoreScrollDefault : MonoBehaviour
{
    [SerializeField] private RecycleScroll _scroll;
    [Header("Rfresh Obj")]   // vertical 스크롤 유형 상점 refresh Object
    [SerializeField] private ExTMPUI _storeTitle;
    [SerializeField] private ExTMPUI _refreshBtnTxt;
    [SerializeField] private UIStoreRefreshTimer _uiStoreRefreshTimer;

    [Header("Items List")]
    [SerializeField] private List<Store_TableData> _storeItemList = new List<Store_TableData>();

    private List<StoreItemCellDefault> _storeCells;
    private List<StoreItemCellDefaultData> _cellDatas = null;

    
    private Store_Option_TableData _soData;
    private UIStore _uiStore;
    private Coroutine _coroutine;

    private string _storeTab;       // 보따리 상점, 패키지 상점, 유료상점등 구분
    private string _storeType;      // 상점,교환소,제작소 구분
    //private string _baseRefreshInfoIndex;

    private long _remainTime;
    private DateTime _nowTime;

    private Vector2 _currentAnchor = Vector2.zero;

    public void Init()
    {
        _scroll.Init();
        
    }

    public void InitData(string tab, int index, bool isRefresh)
    {
        _storeTab = tab;

        _storeCells = _scroll.GetCellToList<StoreItemCellDefault>();
        _cellDatas = new List<StoreItemCellDefaultData>();
        _soData = TableManager.Instance.Store_Option_Table.GetStoreOprionData(_storeTab);

        _uiStore = UIManager.Instance.GetUI<UIStore>();
        _currentAnchor = _scroll.ContentRect.anchoredPosition;
        if (isRefresh)
            _currentAnchor = Vector2.zero;
        // CheckNowTimeToLong();
        UpdateUI();
        UpdateData(index);
    }

    private void UpdateUI()
    {
        // 상점 상단 이름. 
        if(_storeTitle != null)
            _storeTitle.ToTableText(_soData.Store_Str_Tid);

        // 갱신 UI
        //UpdateRefreshUI();

    }

    public void UpdateData(int index)
    {
        _cellDatas.Clear();
        //_storeItemList = GameInfoManager.Instance.GetSotreItemsList(_storeTab);
        _storeItemList = GameInfoManager.Instance.CheckExpiredTime(_storeTab);
        for (int n = 0; n < _storeItemList.Count; n++)
        {
            _cellDatas.Add(new StoreItemCellDefaultData(_storeItemList[n]));
        }
        for (int i = 0; i < _storeCells.Count; i++)
        {
            _storeCells[i].SetCellDatas(_cellDatas, _storeTab);
        }
        int showCount = 0;
        //_scroll.ActivateCells(_cellDatas.Count);
        _scroll.ActivateCells(GameInfoManager.Instance.GetShowListCount(_soData.Tid));
        _scroll.ContentRect.anchoredPosition = _currentAnchor;
        //var focusline = _scroll.SetScrollLineToItem(index);
        //_scroll.FocusLine(focusline);
    }


    //private void UpdateRefreshUI()
    //{
    //    if (_soData.Refresh == 0)
    //    { // 초기화 불가능
    //        //_refreshInfoObj.SetActive(false);
    //        _uiStoreRefreshTimer.gameObject.SetActive(false);
    //    }
    //    if (_soData.Refresh == 1)
    //    {
    //        _uiStoreRefreshTimer.gameObject.SetActive(true);
    //        _uiStoreRefreshTimer.ShowTimer(TimerKey.RefreshDayTimer, _soData.REFRESH_COST_TYPE);
    //    }
    //}
    // 즉시 갱신 버튼
    public void OnClickResetBtn()
    {
        if (GameInfoManager.Instance.CheckRemainRefreshCount(_soData.Tid) != 0)
        {
            Dictionary<UIOption, object> options = Utils.GetUIOption(
             UIOption.Data, _soData,
             UIOption.Object, this,
             //UIOption.Tid, _uiStore.GetCostData(_soData.Refresh_Item,_soData.REFRESH_COST_TYPE),
             UIOption.Tid, GameInfoManager.Instance.GetCostData(_soData.Refresh_Item, _soData.REFRESH_COST_TYPE),
             UIOption.Value, GameInfoManager.Instance.GetRefreshCost(_soData).ToString()
             );
            UIManager.Instance.Show<UIPopupStoreRefresh>(options);
        }
        else
        {
            string str = LocalizeManager.Instance.GetString("str_ui_shop_failed_refresh");
            UIManager.Instance.ShowToastMessage($"{str}");
        }
        // 재화 충분하면 보내고 아니면 안보내기 
    }
}
  