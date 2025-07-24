using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

//임시 재화
[Serializable]
public class UiStoreScrollInfo
{
    public string storeOption;
    public GameObject storeOnjects;
}

public class UIStore : UIBase
{
    [Header("Store Scroll Objects")]
    [SerializeField] private GameObject _storeDefault;  // 일반 인게임 상점 
    [SerializeField] private GameObject _storeSubscribe;  // 월정액 타입 상점
    [SerializeField] private GameObject _storePackage;  // 유료 상점(패키지상점, 크리스탈상점)
    [SerializeField] private GameObject _storeCharacter;    // 캐릭터 상점(스킨, 캐릭터)
    [SerializeField] private GameObject _subtabsObj;

    [SerializeField] private GameObject _paymenyRegulationObj;
    [SerializeField] private GameObject _refreshStoreObj;

    //체험하기
    [SerializeField] private GameObject _experienceObj;

    [Header("UITabs")]
    [SerializeField] private UITabMenu _uiTabMenu;
   
    [BoxGroup("Tab")]
    [SerializeField] private  List<UIPvpTab> _uisubTab = new List<UIPvpTab>();


    [SerializeField] private Dictionary<int, string> _storeMainMenuDic = new Dictionary<int, string>();
    [SerializeField] public Dictionary<string, Dictionary<string, UIStoreMenuTab>> _storeSubMenuDic = new Dictionary<string, Dictionary<string, UIStoreMenuTab>>();

    [SerializeField] private UIStoreRefreshTimer _uiStoreRefreshTimer;

    //[Header("UI Layout Group")]
    //[SerializeField] private UILayoutGroup _uiLayoutGroup;

    private Dictionary<string, SequenceCharacter> _sequenceCharacterList = new Dictionary<string, SequenceCharacter>();

    private UIStoreScrollDefault _scrollDefalut;
    private UIStoreScrollSubscribe _scrollSubscribe;
    private UIStoreScrollCharacter _scrollCharacter;
    private UIStoreScrollPackage _scrollPackage;

    private Store_Table _sTable;
    //public ToggleGroup _tGroup;
    private Store_Option_Table _storeOptionData;

    public Coroutine _timerCheckCoroutine;

    private bool _showNativePopup = false;

    private string _curTabType;
    private bool _isEnd;
    private int _selectedMainTab;
    private int _prevSelectedMainTab = -1;
    private int _selectedStoreType;

    private string _selectedSubTab = "";
    private int _selectedIndex = 0;

    private int _tabIndex = -1;
    private int _selectItemCellIndex = 0; 

    //private UIStoreBg _storeBg;

    public string SelectedTab { get { return _selectedSubTab; } set { _selectedSubTab = value; } }
    public Coroutine TimerCheckCoroutine { get; set; }
    private List<Store_MainOption_Table> _enableMaintabs = new List<Store_MainOption_Table>();

    //구매 후 클라이언트 콜백 처리
    private Action<JObject> _purchaseCompleted;
    private Action _purchaseEnd;
    
    private void OnDisable()
    {
      // 여기 정리 필요 → sub 탭 정리 하면서 정리 하자.
        _uiStoreRefreshTimer.IsDisableObject();
        //_slotIndex = -1;
        //_selectedIndex = 0;
       
        SetTimerCoroutine(false);
    }


    public override void Init()
    {
        base.Init();
        if (_sTable == null)
        {
            _sTable = TableManager.Instance.Store_Table;
        }
        _scrollDefalut =_storeDefault.GetComponent<UIStoreScrollDefault>();
        _scrollDefalut.Init();

        _scrollSubscribe = _storeSubscribe.GetComponent<UIStoreScrollSubscribe>();
        //_scrollSubscribe.Init();

        _scrollCharacter = _storeCharacter.GetComponent<UIStoreScrollCharacter>();
        _scrollCharacter.Init();

        _scrollPackage = _storePackage.GetComponent<UIStoreScrollPackage>();
        _scrollPackage.Init();

        _storeOptionData = TableManager.Instance.Store_Option_Table;
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _storeMainMenuDic.Clear();
        _uiTabMenu.SetResetTabsList();
        
        var stringtype = "";
        if (optionDict.TryGetValue(UIOption.Index, out var storeType))
        {
            stringtype = storeType.ToString();
        }
        //상점 탭 포커싱

        SetStoreMainTabs(stringtype);
        string tabIndex = null;
        if (optionDict != null && optionDict.TryGetValue(UIOption.Tid, out var optionID))
        {
            var info = TableManager.Instance.Store_Option_Table[optionID.ToString()];
            tabIndex = info.Main_Option_Group_Tid;
            _selectedIndex = info.Order;
        }
        else
        {
            // tabIndex = TableManager.Instance.Store_MainOption_Table.FirstStoreOptionTid();
            //tabIndex = _storeMainMenuDic.FirstOrDefault().Value; .
            tabIndex = _storeMainMenuDic.FirstOrDefault().Value;
            _selectedIndex = 0;
        }
       
        //SetStoreMainTabs(stringtype);

        foreach (var main in _storeMainMenuDic)
        {
            if (main.Value.Equals(tabIndex))
            {
                _selectedMainTab = main.Key;
            }
        }
       


        SetIsEndTimer();

        if (!_isEnd)
        {
            SetTimerCoroutine(true);
        }
        else
        {
            GameInfoManager.Instance.InvokeResetTimeEvent(true);
        }

        _tabIndex = -1;
       // _uiTabMenu.RefreshTab(_selectedMainTab);
        UpdateUI();
    }

    public void UpdateUI()
    {
        _uiTabMenu.RefreshTab(_selectedMainTab);
        // 여기서 포커싱
        _uiTabMenu.SetFocusTab(_selectedMainTab);
        _prevSelectedMainTab = _selectedMainTab;

        if(!_storeMainMenuDic.ContainsKey(_selectedMainTab))
            Debug.LogError("상점 메인 탭 데이터 없음 Index " + _selectedIndex + " MainMenuDic Count " + _storeMainMenuDic.Count + " : " + _selectedMainTab);

        SetSubTabsUI(_storeMainMenuDic[_selectedMainTab], _selectedIndex);
    }
    public IEnumerator IsTimerEndCheck()
    {
        yield return new WaitUntil(() => TimerManager.Instance.IsEndTimer(TimerKey.RefreshDayTimer));
    }

    public void SetStoreMainTabs(string storeType)
    {
        _storeMainMenuDic.Clear();
        _uiTabMenu.SetResetTabsList();
        //var mainTabs = TableManager.Instance.Store_MainOption_Table.GetStoreMainOptions(storeType);
        var mainTabs = TableManager.Instance.Store_MainOption_Table.SortedStoreMainOptions(storeType);
        RectTransform uiTabMenuRect = _uiTabMenu.gameObject.GetComponent<RectTransform>();
        for (int n = 0; n < mainTabs.Count; n++)
        {
            var index = n;
            if (!_storeMainMenuDic.ContainsKey(index))
            {
                var tabData = TableManager.Instance.Store_MainOption_Table[mainTabs[n].Tid];
                //var tab = GameObject.Instantiate(_uiTab, _uiTabMenu.GetComponent<RectTransform>());

                _storeMainMenuDic.Add(index, mainTabs[n].Tid);

                var title = TableManager.Instance.Store_MainOption_Table[mainTabs[n].Tid].Store_Str_Tid;
                UiTabInfo info = new UiTabInfo(title);
                _uiTabMenu.SetUITabsList(info);
            }
            _uiTabMenu.GetCell(n).SetTabRedDotInfo(SetMainTabReddotId(storeType), index, SetReddotParentIndex(storeType));
        }
        _uiTabMenu.SetUITabMenu(OnClickMainTab);
      
    }

    public void SetSubTabsUI(string mainname, int subIndex)
    {
        // 1. 탭이 바뀌면 subtab 전부 끄고 
        for (int n = 0; n< _uisubTab.Count; n++)
        {
            _uisubTab[n].gameObject.SetActive(false);
        }
        //2. 선택된 mainoption탭의 sub탭 갯수 만큼 키고 , text 변경

        List<int> list = new List<int>();
        // var subTabs = TableManager.Instance.Store_Option_Table.GetSortedOptions(mainname);
        var subTabs = GameInfoManager.Instance.CheckIsSubTabDisable(mainname);
        var mainTabIndex = _storeMainMenuDic.FirstOrDefault(x => x.Value == mainname).Key;
        for (int n = 0; n < subTabs.Count; n++)
        {
            var subTablist = GameInfoManager.Instance.GetSotreItemsList(subTabs[n]);
            //var items = GameInfoManager.Instance.CheckExpiredTime(subTabs[n]);
            //if (GameInfoManager.Instance.GetSotreItemsList(subTabs[n]) == null ||GameInfoManager.Instance.GetSotreItemsList(subTabs[n]).Count == 0)
            var index = 0;
            var data = TableManager.Instance.Store_Option_Table[subTabs[n]];

            _uisubTab[n].gameObject.SetActive(true);
            _uisubTab[n].TabTId = subTabs[n];
            _uisubTab[n].SetTabTitle(data.Store_Str_Tid);
            list.Add(n);
            _uisubTab[n].Active(false);

            //if (items == null || items.Count == 0)
            //{
            //    _uisubTab[n].Active(false);
            //}
            //else
            //{
            //    _uisubTab[n].gameObject.SetActive(true);
            //    _uisubTab[n].TabTId = subTabs[n];
            //    _uisubTab[n].SetTabTitle(data.Store_Str_Tid);
            //    list.Add(n);
            //    _uisubTab[n].Active(false);
            //}
            index = n;
            // _uisubTab[n].SetRedDot(data.Order, mainname, mainTabIndex);
            _uisubTab[n].SetRedDot(index, mainname, mainTabIndex);
        }
        //if (_isClickMain)
        //{
        //    _selectedIndex = list.FirstOrDefault();
        //}
        _uisubTab[_selectedIndex].Active(true);
        OnClickSubTab(_selectedIndex);
    }
    private string SetMainTabReddotId(string storeType) 
    {
        string str = string.Empty;
        switch (storeType)
        {
            case "none":
                str = "reddot_store_main_tab";
                break;
            case "cash":
                str = "reddot_cashstore_main_tab";
                break;
        }
        return str;
    }
    private int SetReddotParentIndex(string storeType)
    {
        int index = 0;
        switch (storeType)
        {
            case "none":
                index = 6;
                break;
            case "cash":
                index = 5;
                break;
        }
        return index;
    }
    // 상점 메뉴 탭에 따라 상점 리스트 변경 => StoreTab.cs에서 호출 해서 쓰임.
    public void UpdateTab(string tab)
    {
        var selecttabdata = TableManager.Instance.Store_Option_Table[tab];
        _selectedStoreType = TableManager.Instance.Store_Option_Table[tab].Store_Type;
        SetRefreshObjUI(selecttabdata);
        //_storePay.SetActive(false);
        switch (selecttabdata.Store_Type)
        {
            case 1:
                _storeDefault.SetActive(true);
                _storeSubscribe.SetActive(false);
                _storeCharacter.SetActive(false);
                _storePackage.SetActive(false);
                break;
            case 2:
                _storeDefault.SetActive(false);
                _storeSubscribe.SetActive(true);
                _storeCharacter.SetActive(false);
                _storePackage.SetActive(false);
                break;
            case 3:
                _storeDefault.SetActive(false);
                _storeSubscribe.SetActive(false);
                _storeCharacter.SetActive(true);
                _storePackage.SetActive(false);
                break;
            case 4:
                _storeDefault.SetActive(false);
                _storeSubscribe.SetActive(false);
                _storeCharacter.SetActive(false);
                _storePackage.SetActive(true);
                break;
        }
        if (selecttabdata.Store_Payment_Regulation == 1)
        {
            _paymenyRegulationObj.SetActive(true);
        }
        else
        {
            _paymenyRegulationObj.SetActive(false);
        }
        SetTickets(_selectedSubTab);

        UpdateStoreUI(_selectedSubTab);
        UpdateScroll(_selectedSubTab, refresh: true);
    }

    public void UpdateScroll(string tid, int index = 0, bool refresh = false)
    {
        _subtabsObj.SetActive(_selectedStoreType != 2);
        switch (_selectedStoreType)
        {
            case 1:
                _scrollDefalut.InitData(tid, index, refresh);
                break;
            case 2:
                _scrollSubscribe.InitData(tid, index, refresh);
                break;
            case 3:
                _scrollCharacter.InitData(tid, index);
                break;
            case 4:
                _scrollPackage.InitData(tid, index, refresh);
                break;
        }
    }

    public void SetTickets(string tabName)
    {
        List<string> tickets = new List<string>();
        var data = TableManager.Instance.Store_Option_Table[tabName];
        if (!string.IsNullOrEmpty(data.Store_Ticket_Type_01))
        {
            tickets.Add(data.Store_Ticket_Type_01);
        }
        if (!string.IsNullOrEmpty(data.Store_Ticket_Type_02))
        {
            tickets.Add(data.Store_Ticket_Type_02);
        }
        if (!string.IsNullOrEmpty(data.Store_Ticket_Type_03))
        {
            tickets.Add(data.Store_Ticket_Type_03);
        }

        UpdateTopMenu(HELP_LOCATION_TYPE.none, tickets.ToArray());
    }

    public void UpdateStoreUI(string tid)
    {
        //체험하기 버튼 활성화
        if (!string.IsNullOrEmpty(tid) && tid.Equals("store_char_01"))
            _experienceObj.SetActive(true);
        else
            _experienceObj.SetActive(false);

        foreach (var sequencelist in _sequenceCharacterList)
        {
            sequencelist.Value.gameObject.SetActive(false);
        }

    }

    public void OnClickMainTab(int index)
    {
         _selectedMainTab = index;
        _tabIndex = -1;
        _selectedIndex = 0;
        //_uiTabMenu.SetFocusTab(_selectedMainTab);
        SetSubTabsUI(_storeMainMenuDic[_selectedMainTab], _selectedIndex);
    }
    // main tab이 바뀔때마다 콜
   
    public void OnClickGoCharacterGuide()
    {
        UIManager.Instance.Show<UICharacterGuide>();
    }

    public void OnClickSubTab(int index)
    {
        if (_tabIndex == index)
            return;

        _tabIndex = index;
        
        for (int i = 0; i < _uisubTab.Count; i++)
        {
            _uisubTab[i].Active(i == _tabIndex);
        }
        _selectedSubTab = _uisubTab[index].TabTId;
         UpdateTab(_uisubTab[index].TabTId);
    }

    private void SetRefreshObjUI(Store_Option_TableData opdata)
    {
        if (opdata.Refresh == 0)
        { // 초기화 불가능
            //_refreshInfoObj.SetActive(false);
            _uiStoreRefreshTimer.gameObject.SetActive(false);
        }
        if (opdata.Refresh == 1 || !string.IsNullOrEmpty(opdata.Store_Refresh_Timer))
        {
            _uiStoreRefreshTimer.gameObject.SetActive(true);
            SetRefreshTimer(opdata);
        }
    }

    public void OnClickMoreGuid()
    {
        UIManager.Instance.Show<UIPopupStoreGuid>();
    }

    // 공용 : 구매 제한 텍스트 표시
    public string SetAblePurchaseStr(Store_TableData data)
    {
        string str = null;
        if (data.Use_Limit == 0 &&data.Use_Daily == 0 && data.Use_Weekliy == 0 && data.Use_Month == 0)
        {
            return null;
        }
        if (data.Use_Limit != 0)
        {
            str = "str_s_goods_bay_limit_account_01";
        }
        if (data.Use_Daily != 0)
        {
            str = "str_s_goods_bay_limit_daily_01";
        }
        if (data.Use_Weekliy != 0)
        {
            str = "str_s_goods_bay_limit_weekliy_01";
        }
        if (data.Use_Month != 0)
        {
            str = "str_s_goods_bay_limit_month_01";
        }

        return str;
    }

    // ━━━━━━━ 상점Refresh 관련 ━━━━━━━
    public void SetIsEndTimer()
    {
        _isEnd = TimerManager.Instance.IsEndTimer(TimerKey.RefreshDayTimer);
        if (_isEnd)
        {
            Debug.Log("<color=#4cd311>SetIsEndTimer 〓 "+_isEnd+"</color>");
        }

    }
    public void SetTimerCoroutine(bool isStart)
    {
        if (isStart)
        {
            _timerCheckCoroutine = StartCoroutine(IsTimerEndCheck());
        }
        else
        {
            if (_timerCheckCoroutine != null)
            {
                StopCoroutine(_timerCheckCoroutine);
                _timerCheckCoroutine = null;
            }
            SetIsEndTimer();
        }
    }

    public override void Refresh()
    {
        base.Refresh();
        //UIManager.Instance.GetUI<UIIllust>();
        //UIManager.Instance.Show<UIStoreBg>(Utils.GetUIOption(UIOption.Object));
        
        Debug.Log("Refresh UIStore _selectedMainTab("+_selectedMainTab+")");
        _uiTabMenu.RefreshTab(_selectedMainTab);
        UpdateScroll(_selectedSubTab);
    }

    public override void Hide()
    {
        base.Hide();
       
        //_storeBg.Close();
    }
    public override void Close(bool needCached = true)
    {
        //UIManager.Instance.Close<UIStoreBg>();
        //UIManager.Instance.Close<UIIllust>();
        _tabIndex = -1;
        _selectedIndex = 0;
        _selectedMainTab = 0;
        _uiTabMenu.RefreshTab(_selectedMainTab);
        base.Close(needCached);
    }

    public void SetRefreshTimer(Store_Option_TableData optiondata)
    {
        DateTime dataTime = DateTime.UtcNow.ToAddHours();
        if(!string.IsNullOrEmpty(optiondata.Refresh_Reset_Type))
        {
            if (optiondata.Refresh_Reset_Type.Equals("daily"))
            {
                dataTime =  TimerManager.Instance.GetTime(TimerKey.RefreshDayTimer);
            }

            if (optiondata.Refresh_Reset_Type.Equals("weekly"))
            {
                dataTime =  TimerManager.Instance.GetTime(TimerKey.RefreshWeekTimer);
            }

            if (optiondata.Refresh_Reset_Type.Equals("month"))
            {
                dataTime =  TimerManager.Instance.GetTime(TimerKey.RefreshMonthTimer);
            }
        }


        if (!string.IsNullOrEmpty(optiondata.Store_Refresh_Timer))
        {
            var type = TableManager.Instance.Content_Season_Table[optiondata.Store_Refresh_Timer];
            dataTime = GameInfoManager.Instance.Leaderboards.GetSetting(type.Leaderboard_Id).NextOpenTime;
        }

        _uiStoreRefreshTimer.ShowTimer(dataTime, optiondata);
    }

    public void SetShowNativePopup(bool isActive)
    {
        _showNativePopup = isActive;
    }

    public void Purchase(int steamstoreId, string storeTid, Action endEvent = null)
    {
#if !UNITY_STANDALONE_WIN
        if (RestApiManager.Instance.GetLoginType() == LoginType.ADMIN || _showNativePopup)
        {
            Debug.LogError($"Purchase Failed - ShowNativePopup : {_showNativePopup}");
            return;
        }
#endif
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR && !UNITY_ANDROID)

        _showNativePopup = true;
#endif
        if (UIManager.Instance != null)
            UIManager.Instance.ShowUIBlock();

        if (NowGGManager.Instance.IsNowGGActive)
        {
#if UNITY_EDITOR
            Debug.Log("<color=#9efc9e>LOG++++++++블루스텍 ::: 구매 가능 => 유니티라 안됨</color>");
            _showNativePopup = true;
#else
            Debug.Log("<color=#9efc9e>LOG++++++++블루스텍 ::: 구매 가능</color>");
            NowGGManager.Instance.PurchaseProduct(storeTid, (response) => { CompletePurchase(); }, () =>
            {
                _showNativePopup = false;
                endEvent?.Invoke();
            });
#endif
            Debug.LogError("Purchase 0" + storeTid);
        }
        else
        {
            Debug.Log("<color=#9efc9e>LOG++++++++블루스텍 아님.  noqGG 구매 안됨</color>");

            Action onClickEnd = () =>
            {
                _showNativePopup = false;
                endEvent?.Invoke();
            };
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR && !UNITY_ANDROID)
Debug.Log("<color=#9efc9e>LOG++++++++UNITY_STANDALONE_WIN</color>");
        var storeData = TableManager.Instance.Store_Table[storeTid];

        SteamworksManager.Instance.Transaction(storeTid, (response) => { CompletePurchase(); }, () =>
        {
            _showNativePopup = false;
            endEvent?.Invoke();
        });
#elif ((UNITY_ANDROID|| UNITY_IOS) && !UNITY_EDITOR && !UNITY_STANDALONE_WIN)
            Debug.Log("<color=#9efc9e>LOG++++++++UNITY_ANDROID</color>");
  IAPManager.Instance.Purchase(IAPType.Store, steamstoreId, storeTid, (response) => { CompletePurchase(); }, () =>
            {
                _showNativePopup = false;
                endEvent?.Invoke();
            });
#else
            IAPManager.Instance.Purchase(IAPType.Store, steamstoreId, storeTid, (response) => { CompletePurchase(onClickEnd); });
#endif
        }
    }

    private void CompletePurchase(Action endEvent = null)
    {
        //_selectedSubTab = _uisubTab[_slotIndex].TabTId;
        //UpdateScroll(_selectedSubTab);
        Debug.Log("<color=#9efc9e>상품 구매 완료 CompletePurchase() 호출 </color>");
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK, 
            message: "str_store_purchase_complete_01".ToTableText(),
            onClickOK: endEvent);
        // UpdateUI();
        _selectedSubTab = _uisubTab[_tabIndex].TabTId;

        UIManager.Instance.CloseUIBlock();
#if UNITY_STANDALONE_WIN
        UIManager.Instance.CloseUIBlockStore();
#endif
        UpdateScroll(_selectedSubTab, _selectItemCellIndex);
        //_interaction.BuyAction();
    }

    public void ShowPurchaseUI(Store_TableData data)
    {
        if (RestApiManager.Instance.GetLoginType() == LoginType.GUEST)
        {
            Action onClickOk = () =>
            {
                ShowUIPopupStoreBuyInfo(data, _selectItemCellIndex);
            };

            UIManager.Instance.ShowAlert(AlerType.Middle, PopupButtonType.OK_CANCEL,
                LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                LocalizeManager.Instance.GetString("str_ui_guest_stoar_01"),
                onClickOK:onClickOk);
        }
        else
            ShowUIPopupStoreBuyInfo(data, _selectItemCellIndex);
    }

    public void ShowUIPopupStoreBuyInfo(Store_TableData data, int index)
    {
        _selectItemCellIndex =index;
        var storeType = TableManager.Instance.Store_Option_Table[data.Group_Id].Store_Type;
        Dictionary<UIOption, object> options = Utils.GetUIOption(
             UIOption.Data, data,
             UIOption.Int, storeType,
              UIOption.Bool, true 
             );

        UIManager.Instance.Show<UIPopupStoreBuyInfo>(options);
    }
    public void ShowUIPopupSubscribePopup(Store_TableData data, int index)
    {
        _selectItemCellIndex =index;
        var storeType = TableManager.Instance.Store_Option_Table[data.Group_Id].Store_Type;
        Dictionary<UIOption, object> options = Utils.GetUIOption(
             UIOption.Data, data
             );

        UIManager.Instance.Show<UIPopupStoreSubscribe>(options);
    }
}
