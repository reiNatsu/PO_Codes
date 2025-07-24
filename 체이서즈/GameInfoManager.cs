
using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

namespace LIFULSE.Manager
{
    public class CollectionInfo
    {
        public string Tid { get; set; }
        public int Coin { get; set; }
        public int Level { get; set; }
        public CollectionInfo()
        { }
        public CollectionInfo(string tid, int coin, int level)
        {
            Tid = tid;
            Coin = coin;
            Level = level;

            AddCollectionInfoData(tid, coin, level);
        }

        public CollectionInfo(string tid, JToken token)
        {
            AddCollectionInfoData(tid, token);
        }
        public CollectionInfo(string tid, int coin)
        {
            Tid = tid;
            Coin = coin;

            AddCollectionInfoData(tid, coin);
        }

        public void AddCollectionInfoData(string tid, JToken jtoken, int level = 0)
        {
            Dictionary<string, JToken> InfoDic = new Dictionary<string, JToken>();

            InfoDic.Add(tid, jtoken);
        }

    }
    [Serializable]
    public class CurrencyData
    {
        public string Tid;
        public int value;
    }


    public partial class GameInfoManager : Singleton<GameInfoManager>
    {
        [SerializeField] private bool _useCheat = false;

        public bool UseCheat { get { return _useCheat; } set { _useCheat = value; } }

        private Dictionary<ITEM_USE_EFFECT, HashSet<string>> _itemEffectDict = new Dictionary<ITEM_USE_EFFECT, HashSet<string>>();
        private Dictionary<ITEM_ACQUIRE_EFFECT, Dictionary<string, int>> _itemAcqureEffects = new Dictionary<ITEM_ACQUIRE_EFFECT, Dictionary<string, int>>();
        private Dictionary<CombatType, int> _combatTeamIndexDict = new Dictionary<CombatType, int>();

        private int _guideBreakthrough = 0;

        public int GuideBreakthrough
        {
            get => _guideBreakthrough;
            set => _guideBreakthrough = value;
        }
        
        private int _purchasePlayFillCount = 0;
        private bool _needUpdateList = true;

        private NaviQuestInfo _naviQuestInfo = new();
        private AccountInfo _accountInfo = new();
        private AbilityInfo _abilityInfo = new();
        private TicketInfo _ticketInfo = new();
        private CurrencyInfo _currencyInfo = new();
        private OrganizationInfo _organizationInfo = new();
        private StageInfo _stageInfo = new();
        private RewardInfo _rewardInfo = new RewardInfo();
        private PvpInfo _pvpInfo = new PvpInfo();
        private PostInfo _postInfo = new PostInfo();
        private DungeonInfo _dungeonInfo = new DungeonInfo();
        private GuideInfo _guideInfo = new GuideInfo();
        private TotalInfo _totalInfo = new TotalInfo();
        private ChallengeInfo _challengeInfo = new ChallengeInfo();//운명의 시험대
        private ChallengeInfo _trialInfo = new ChallengeInfo();//시련의 시험대
        private SeasonInfo _seasonInfo = new SeasonInfo();

        private CharacterInfo _characterInfo = new();
        private CollectionInfo _collectionInfo = new();
        private FoodInfo _foodInfo = new FoodInfo();
        private MaintenanceInfo _maintenanceInfo = new MaintenanceInfo();
        private TalkInfo _talkInfo = new TalkInfo();
        private EventStoryInfo _eventStoryInfo = new EventStoryInfo();
        private ClanInfo _claninfo = new ClanInfo();
        private HelperInfo _helperinfo = new HelperInfo();
        private SupplyInfo _supplyInfo = new SupplyInfo();

        private EventInfo _eventInfo = new EventInfo();
        private MiniGameInfo _minigameInfo = new MiniGameInfo();
        private HotdealInfo _hotdealInfo = new HotdealInfo();

        private LikeAbilityinfo _likeAbilityinfo = new LikeAbilityinfo();
        private LiberationInfo _liberationInfo = new LiberationInfo();


        private Dictionary<string, CollectionInfo> _collectionDict = new Dictionary<string, CollectionInfo>();

        public Dictionary<CombatType, int> CombatTeamIndex { get => _combatTeamIndexDict; }

        public NaviQuestInfo NaviQuestInfo { get => _naviQuestInfo; }
        public AccountInfo AccountInfo { get => _accountInfo; }
        public AbilityInfo AbilityInfo { get => _abilityInfo; }
        public TicketInfo TicketInfo { get => _ticketInfo; }
        public CurrencyInfo CurrencyInfo { get => _currencyInfo; }
        public OrganizationInfo OrganizationInfo { get => _organizationInfo; }
        public StageInfo StageInfo { get => _stageInfo; }
        public RewardInfo RewardInfo { get => _rewardInfo; }
        public CollectionInfo collectionInfo { get => _collectionInfo; }

        public PvpInfo PvpInfo { get => _pvpInfo; }
        public TotalInfo TotalInfo { get => _totalInfo; }
        public PostInfo PostInfo { get => _postInfo; }
        public TalkInfo TalkInfo { get => _talkInfo; }

        public MaintenanceInfo MaintenanceInfo { get => _maintenanceInfo; }

        public ChallengeInfo ChallengeInfo { get => _challengeInfo; }
        public ChallengeInfo TrialInfo { get => _trialInfo; }

        public DungeonInfo DungeonInfo { get => _dungeonInfo; }
        public GuideInfo GuideInfo { get => _guideInfo; }

        public CharacterInfo CharacterInfo { get => _characterInfo; }

        public Leaderboards Leaderboards { get; private set; } = new Leaderboards();
        public Dictionary<string, CollectionInfo> CollectionDict { get => _collectionDict; }
        public FoodInfo FoodInfo { get => _foodInfo; }
        public ClanInfo ClanInfo { get => _claninfo; }
        public HelperInfo HelperInfo{ get => _helperinfo; }
        public SupplyInfo SupplyInfo { get => _supplyInfo; }
        public EventInfo EventInfo { get => _eventInfo; }
        public MiniGameInfo MiniGameInfo { get => _minigameInfo; }

        public HotdealInfo HotdealInfo { get => _hotdealInfo; }

        public SeasonInfo SeasonInfo { get => _seasonInfo; }
        public EventStoryInfo EventStoryInfo { get => _eventStoryInfo; }

        public LikeAbilityinfo LikeAbilityinfo { get => _likeAbilityinfo; }
        public LiberationInfo LiberationInfo { get => _liberationInfo; }

        public int CheckLoginPeriod { get; set; } = 60; //접속 상태 체크 주기(로그인 상태 체크)

        public bool NeedWelcome { get; set; } = false;
        public bool EnteredLobby { get; set; } = false;
        public bool IsFinishedInit { get; set; } = false;

        public bool IsStageAllOpen { get; set; } = false;
        public bool IsAllContentUnlock { get; set; } = false;

        public string CurrentUiName { get; set; }
        public Action<bool> OnDailyInitializationEvent { get; set; }        // 일일 초기화 이벤트
        public bool IsDaily { get; set; } = false;
        public bool IsDrawing { get; set; } = false;

        public bool IsSkipInteractionSounde { get; set; } = false;
        public bool IsQuit { get; private set; } = false;

        private DateTime _mOpenTime;
        private DateTime _mEndTime;
        private Coroutine _checkMaintenaceIsExitCoroutine;
        private Coroutine _maintenanceCheckCoroutine;
        private Coroutine _maintenanceStartCoroutine;

        private bool _isTime = false;

        // 호감도 정보 테스트 -> 삭제예정
        [SerializeField] public int _curCharacterLAExp;        // 현재 유저 대표 캐릭터 호감도 EXP(수호)
        private int _curCharacterLALevel;      // 현재 유저 대표 캐릭터 호감도 Level(수호)
        public bool IsEndStage { get; set; } = false;
        public Dictionary<string, CollectionInfo> GetCollectionInfoDic()
        {
            return CollectionDict;
        }

        public void SetCollectionInfoDic(string tid, CollectionInfo info)
        {
            if (_collectionDict.ContainsKey(tid))
                _collectionDict[tid] = info;
            else
                _collectionDict.Add(tid, info);

           // AccountInfo.UpdateCollectionStat(info);
        }
        public CollectionInfo SetCollectionMonsterInfo(string tid, int coin, int level = 0)
        {
            _collectionInfo = new CollectionInfo();
            _collectionInfo.Tid = tid;
            _collectionInfo.Coin = coin;
            _collectionInfo.Level = CollectionDict.ContainsKey(tid) ? CollectionDict[tid].Level : 0;

            return _collectionInfo;
        }

        /// <summary>
        /// string is Character TID
        /// </summary>
        private Action<string> _onUpdateMainCharacterEvent;

        public void SetUpItemInfo(JToken items)
        {
            Debug.Log("SetupItemInfo");
            Debug.Log(items.ToString());
            Dictionary<string, ItemInfo> itemDic = new Dictionary<string, ItemInfo>();

            if (items == null || items["M"] == null)
                return;

            var invenP = items["M"].Cast<JProperty>();
            foreach (var inve in invenP)
            {
                if (inve.Value["M"] == null)
                {
                    return;
                }
                var itemTid = inve.Value["M"].Cast<JProperty>();
                foreach (var info in itemTid)
                {
                    var iteminfo = new ItemInfo(info.Name, info.Value.N_Int());
                    if (inve.Name == "onlyone")
                    {
                        itemDic.Add(info.Name, iteminfo);
                    }
                    else
                    {
                        if (!itemDic.ContainsKey(info.Name.ToString()))
                        {
                            itemDic.Add(info.Name.ToString(), iteminfo);
                        }
                        else
                        {
                            itemDic[info.Name].AddAmount(iteminfo.Amount);
                        }
                    }
                }
            }

            SetUpItemInfo(itemDic);
        }

        public void SetUpItemInfo(Dictionary<string, ItemInfo> datas)
        {
            bool containCoin = false;

            _itemInfoDict.Clear();

            foreach (var item in datas)
            {
                var data = TableManager.Instance.Item_Table[item.Key];

                if (data != null)
                {
                    if(data.ITEM_TYPE == ITEM_TYPE.character_coin)
                        containCoin = true;

                    AddAmountItem(data, item.Value.Amount);
                }
            }

            //캐릭터 코인 갯수 체크 => 체이서 레드닷
            CharacterInfo.UpdateCharacterRedDot(null);
        }

        public void UpdateItemInfo(JToken items)
        {
            Debug.Log("SetupItemInfo");
            Debug.Log(items.ToString());
            Dictionary<string, ItemInfo> itemDic = new Dictionary<string, ItemInfo>();
            var invenP = items["M"].Cast<JProperty>();
            foreach (var inve in invenP)
            {
                if (inve.Value["M"] == null)
                {
                    return;
                }
                var itemTid = inve.Value["M"].Cast<JProperty>();
                foreach (var info in itemTid)
                {
                    var iteminfo = new ItemInfo(info.Name, info.Value.N_Int());
                    if (inve.Name == "onlyone")
                    {
                        itemDic.Add(info.Name, iteminfo);
                    }
                    else
                    {
                        if (!itemDic.ContainsKey(info.Name.ToString()))
                        {
                            itemDic.Add(info.Name.ToString(), iteminfo);
                        }
                        else
                        {
                            itemDic[info.Name].AddAmount(iteminfo.Amount);
                        }
                    }
                }

            }
            UpdateItemInfo(itemDic);
        }

        public void UpdateItemInfo(Dictionary<string, ItemInfo> datas)
        {
            foreach (var item in datas)
            {
                var data = TableManager.Instance.Item_Table[item.Key];
                SetAmountItem(data, item.Value.Amount);
            }
        }

        public void AddOnMainCharacterEvent(Action<string> action)
        {
            _onUpdateMainCharacterEvent += action;
        }
        public void RemoveOnMainCharacterEvent(Action<string> action)
        {
            _onUpdateMainCharacterEvent -= action;
        }

        public ChallengeInfo GetChallengeInfo(LEADERBOARDID leaderboardId)
        {
            switch (leaderboardId)
            {
                case LEADERBOARDID.Trial:
                    return _trialInfo;
                default:
                    return _challengeInfo;
            }
        }

        private void GameInfoSetting()
        {
#if LIVE_BUILD || EXTERNAL_BUILD
            UseCheat = false;
#else
            UseCheat = true;
#endif
            NeedWelcome = true;
            IsQuit = false;
            StoreInfoDict = new Dictionary<string, StoreInfo>();
            TicketFillCountDict = new Dictionary<TICKET_TYPE, string>();
            _postInfo.Init();

            SlotMaxCount = TableManager.Instance.Define_Table["ds_inven_slot_default_count"].Opt_01_Int;
            ActionMaxCount = TableManager.Instance.Define_Table["ds_play_energy_max"].Opt_01_Int;
            CheckLoginPeriod = TableManager.Instance.Define_Table["ds_check_login"].Opt_01_Int;

            var tickets = Enum.GetValues(typeof(TICKET_TYPE));

            for (int n = 0; n< tickets.Length; n++)
            {
                string defineTid = null;
                var ticket = (TICKET_TYPE)tickets.GetValue(n);

                switch (ticket)
                {
                    case TICKET_TYPE.i_resource_ticket:
                    case TICKET_TYPE.i_pvp_ticket:
                    case TICKET_TYPE.i_boss_ticket:
                        defineTid = "ds_"+ ticket.ToString().Split("_")[1]+"_ticket";
                        break;
                }

                if (!string.IsNullOrEmpty(defineTid))
                {
                    Define_TableData defineTableData = null;
                    if (TableManager.Instance.Define_Table.TryGetData(defineTid, out defineTableData))
                    {
                        int maxCount = TableManager.Instance.Define_Table[defineTid].Opt_02_Int;
                        string fillStr = " / " + maxCount.ToString();

                        TicketFillCountDict.Add(ticket, fillStr);
                    }

                }

                _helperinfo.SetHelperValue();
            }

        }

        public override void InitializeDirect()
        {
            _instance = this;
            GameInfoSetting();
        }

        public override void SettingDirect()
        {
        }

        public override IEnumerator Initialize()
        {
            _instance = this;
            GameInfoSetting();
            _maintenanceCheckCoroutine = null;
            _maintenanceStartCoroutine = null;
            yield break;
        }

        public override IEnumerator Setting()
        {
            yield break;
        }

        public void UpdateCombatType(CombatType combatType)
        {

        }

        //대표 캐릭터 정보
        public UnitInfo GetMainCharacterInfo()
        {
            return AccountInfo.GetRepresentative;
        }

        public void UpdateRepresentative(string tid)
        {
            AccountInfo.Representative = tid;
            _onUpdateMainCharacterEvent?.Invoke(tid);
        }

        public void AddCurrency(string ticketType, int value)
        {
            _currencyInfo.AddCurrency(ticketType, value);

            var lobby = UIManager.Instance.GetUI<UILobby>();

            if (lobby != null && lobby.gameObject.activeSelf)
                lobby.UpdataGoods();
        }

        public void UpdateCurrency(string ticketType, int value)
        {
            _currencyInfo.UpdateCurrency(ticketType, value);

            var lobby = UIManager.Instance.GetUI<UILobby>();
            var topMenu = UIManager.Instance.GetUI<UITopMenu>();

            if (lobby != null && lobby.gameObject.activeInHierarchy)
                lobby.UpdataGoods();

            if (topMenu != null && topMenu.gameObject.activeInHierarchy)
            {
                if (ticketType.Equals(TICKET_TYPE.i_free_cash.ToString()) || ticketType.Equals(TICKET_TYPE.i_cash.ToString()))
                {
                    ticketType =  TICKET_TYPE.i_cash.ToString();
                    value = GetAmount(ticketType);
                }
                topMenu.UpdateCount(ticketType, value);
            }
        }

        public bool IsActiveItemUseEffect(ITEM_USE_EFFECT effect, string value)
        {
            if (_itemEffectDict.TryGetValue(effect, out var effectValue))
            {
                if (effectValue != null && effectValue.Contains(value))
                    return true;

                return false;
            }

            return false;
        }

        public bool IsActiveItemAcquireEffect(ITEM_ACQUIRE_EFFECT effect, string value)
        {
            if (_itemAcqureEffects.TryGetValue(effect, out var effectValue))
            {
                if (effectValue != null && effectValue.ContainsKey(value))
                    return true;

                return false;
            }

            return false;
        }

        public void AddItemUseEffect(ITEM_USE_EFFECT effect, string value)
        {
            if (!_itemEffectDict.ContainsKey(effect))
                _itemEffectDict.Add(effect, new HashSet<string>());

            _itemEffectDict[effect].Add(value);
        }

        public void RemoveItemUseEffect(ITEM_USE_EFFECT effect, string value)
        {
            if (!_itemEffectDict.ContainsKey(effect))
                return;

            _itemEffectDict[effect].Remove(value);
        }

        public void AddItemAcquireEffect(ITEM_ACQUIRE_EFFECT effect, string value)
        {
            if (effect == ITEM_ACQUIRE_EFFECT.coin && string.IsNullOrEmpty(value))
                return;

            if (!_itemAcqureEffects.ContainsKey(effect))
                _itemAcqureEffects.Add(effect, new Dictionary<string, int>());

            if (!_itemAcqureEffects[effect].ContainsKey(value))
                _itemAcqureEffects[effect].Add(value, 0);

            _itemAcqureEffects[effect][value]++;
        }

        public void RemoveItemAcquireEffect(ITEM_ACQUIRE_EFFECT effect, string value)
        {
            if(string.IsNullOrEmpty(value))
                return;
            if (!_itemAcqureEffects.ContainsKey(effect) || !_itemAcqureEffects[effect].ContainsKey(value))
                return;

            _itemAcqureEffects[effect][value]--;

            if (_itemAcqureEffects[effect][value] == 0)
                _itemAcqureEffects[effect].Remove(value);
        }

        public bool IsNeedUpdateNewCharacter()
        {
            bool result = _needUpdateList;
            _needUpdateList = false;

            return result;
        }

        public int GetCurrency(string ticketType)
        {
            if (_currencyInfo.GetCurrency(ticketType, out var value))
                return value;

            return 0;
        }

        public int GetPurchasePlayFillCount()
        {
            return _purchasePlayFillCount;
        }

        #region[호감도 임시 데이터]
        public int GetLikeabilityLevel()    // 호감도 레벨 
        {
            var data = TableManager.Instance.Define_Table;
            int demo = data["ds_likeability_max_level_exp"].Opt_02_Int;
            _curCharacterLALevel = (_curCharacterLAExp / demo)+1;
            return _curCharacterLALevel;
        }
        public int GetLikeabilityExp()    // 호감도 Exp
        {
            return _curCharacterLAExp;
        }
        #endregion


        // baseTime 받아서 초기화 하기 위해 이벤트 등록 함수 (상점, 퀘스트)
        public void SetupResetTimeEvent()
        {
            // 상점 - 한번만 등록.
            if (!IsAddEvent)
            {
                OnStoreRefreshEvent += StoreRefresh;
                OnDailyInitializationEvent += DailyInitialization;
                Debug.Log("OnDailyInitializationEvent ");
                IsAddEvent = true;

                //var obj = ResourceManager.Instance.Load<GameObject>("UITextMover");
                //_textMover = Instantiate(obj, this.transform).GetComponent<UITextMover>(); 
            }

            //SetCehckMaintenanceEvent();
        }

        //일일 갱신 이벤트 함수
        public void InvokeResetTimeEvent(bool isStore)
        {
            IsDaily = true;
            OnStoreRefreshEvent?.Invoke(isStore);

            RestApiManager.Instance.RequestUserRefreshTicket();
            RestApiManager.Instance.RequestHelperRefreshCount();
            RestApiManager.Instance.RequestPvpGetReward();
            //RestApiManager.Instance.RequestGetDailyPushTime();
            RestApiManager.Instance.AttendanceQuest();
            RestApiManager.Instance.EventCheck((response) =>
            {
                _eventInfo.IsAttendanceChecked = false;
            });
            RestApiManager.Instance.GetLikeAbilityData();

            SetContentsTIcketsRedDot();     // 컨텐츠 티켓 레드닷
           
            if (SceneManager.Instance.SceneState != SceneState.CombatScene)
            {
                if (!IsDrawing)
                {
                    // 추가 : 상점 가챠 진행 중 일때는 안하고, 끝나면
                    OnDailyInitializationEvent?.Invoke(IsDaily);
                }
            }
        }

        //일일 갱신 안내 팝업.
        public void DailyInitialization(bool isDaily)
        {
            IsDaily = false;
            OnDailyInitializationEvent -= DailyInitialization;
            Action onClikc = () =>
            {
                if (SceneManager.Instance.SceneState != SceneState.LobbyScene)
                    SceneManager.Instance.ChangeSceneState(SceneState.LobbyScene);
                else
                {
                    UIManager.Instance.CloseAllUI();
                    UIManager.Instance.Show<UILobby>();
                }
            };

            var uiLoading = UIManager.Instance.GetUI<UILoading>();
            if (SceneManager.Instance.SceneState != SceneState.IntroScene
                || (uiLoading != null && !uiLoading.gameObject.activeInHierarchy))
            {
                UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK,
                       LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                       LocalizeManager.Instance.GetString("str_ui_reset_time_01"),
                       onClickOK: onClikc,
                       onClickClosed: onClikc);
            }
        }


        public void IsDailyInitialization()
        {
            if (IsDaily)
            {
                OnDailyInitializationEvent?.Invoke(IsDaily);
            }
        }

        // 점검 공지 함수
        //처음에 점검 있는 지 체크
        public void SetCehckMaintenanceEvent(Action callback = null)
        {
            bool isInspection = false;
            _maintenanceInfo.Check(() =>
            {

                //if (_maintenanceInfo.IsExist())
                //{
                if (_checkMaintenaceIsExitCoroutine != null)
                {
                    StopCoroutine(_checkMaintenaceIsExitCoroutine);
                    _checkMaintenaceIsExitCoroutine = null;
                }

                Debug.Log("<color=#fa41e9>"+_maintenanceInfo.IsExist()+"</color>");
                if ((_mOpenTime == null || _mOpenTime != _maintenanceInfo.StartTime) &&(_mEndTime == null || _mEndTime != _maintenanceInfo.EndTime))
                {
                    _mOpenTime =  _maintenanceInfo.StartTime;
                    _mEndTime =  _maintenanceInfo.EndTime;
                }

                //if (_mEndTime != null)
                if (_maintenanceInfo.IsExist())
                {
                    RestApiManager.Instance.RequestGetNowTimeNoLogin(() =>
                    {

                        if (NowTime >=  _maintenanceInfo.StartTime && NowTime < _mEndTime)
                        {
                            Debug.Log("DateTime.Now " + NowTime + " StartTime " +  _maintenanceInfo.StartTime);
                            isInspection = true;
                        }

                        //점검 데이터 있는데 점검 전
                        if (NowTime < _mEndTime)
                        {
                            if (_maintenanceCheckCoroutine == null)
                            {
                                _isTime = true;
                                _maintenanceCheckCoroutine = StartCoroutine(CheckMaintenanceTime());
                            }
                        }
                    });
                    //현재 점검 중
                    //if (DateTime.UtcNow.ToAddHours() >=  _maintenanceInfo.StartTime && DateTime.UtcNow.ToAddHours() < _mEndTime)
                    //{
                    //    Debug.Log("DateTime.Now " + DateTime.Now + " StartTime " +  _maintenanceInfo.StartTime);
                    //    isInspection = true;
                    //}

                    ////점검 데이터 있는데 점검 전
                    //if (DateTime.UtcNow.ToAddHours() < _mEndTime)
                    //{
                    //    if (_maintenanceCheckCoroutine == null)
                    //    {
                    //        _isTime = true;
                    //        _maintenanceCheckCoroutine = StartCoroutine(CheckMaintenanceTime());
                    //    }
                    //}

                    Debug.Log("<color=#fa41e9> isInspection   :::   "+isInspection+"</color>");
                    if (isInspection)
                    {
                        Debug.Log("<color=#fa41e9> <<1111>></color>");
                        return;
                    }
                    else
                    {
                        Debug.Log("<color=#fa41e9> <<2222>></color>");
                        callback?.Invoke();
                    }

                    // }
                }
                else
                {
                    if (_checkMaintenaceIsExitCoroutine == null)
                    {
                        _checkMaintenaceIsExitCoroutine= StartCoroutine(CheckIsMaintenanceExit());
                    }

                    callback?.Invoke();
                }
            });
        }

        //5분 마다 점검 데이터 체크
        private void CheckIsMaintenanceUpdate()
        {
            _maintenanceInfo.Check(() =>
            {
                if (_mOpenTime !=  _maintenanceInfo.StartTime  && _mEndTime !=  _maintenanceInfo.EndTime)
                {
                    _mOpenTime =  _maintenanceInfo.StartTime;
                    _mEndTime =  _maintenanceInfo.EndTime;
                }
            });
        }
        IEnumerator CheckIsMaintenanceExit()
        {
            while (!_isTime)
            {
                yield return new WaitForSeconds(60);
                SetCehckMaintenanceEvent();
            }
        }

        IEnumerator CheckMaintenanceTime()
        {
            while (_isTime)
            {
                CheckIsMaintenanceUpdate();
                //if (DateTime.Now >= _maintenanceInfo.GetStartTime() && _maintenanceStartCoroutine == null)
                //{
                //    var period = TableManager.Instance.Define_Table["ds_maintance_popup"].Opt_02_Int;
                //    Debug.Log("<color=#9efc9e>CheckMaintenanceTime() period : "+period+"</color>");
                //    _maintenanceStartCoroutine = StartCoroutine(CheckMaintenanceStartData(period));
                //}

                var period = TableManager.Instance.Define_Table["ds_maintance_popup"].Opt_02_Int;
                RestApiManager.Instance.RequestGetNowTimeNoLogin(() =>
                {

                    SetMaintenanceInfo(period);

                });

                yield return new WaitForSeconds(period);
            }
        }


        private void SetMaintenanceInfo(int period)
        {

            if (NowTime < _maintenanceInfo.StartTime && NowTime >= _maintenanceInfo.GetStartTime())
            {
                //var period = TableManager.Instance.Define_Table["ds_maintance_popup"].Opt_02_Int;

                string str = "";
                var remainTime = _maintenanceInfo.StartTime -NowTime;

                if (remainTime.Minutes <= 5)
                {
                    period = 60;
                }

                //if (remainTime.Minutes > 5)
                //{
                //    period = TableManager.Instance.Define_Table["ds_maintance_popup"].Opt_02_Int;
                //}
                //else
                //{
                //    period = 60;
                //}

                if (_maintenanceInfo.Type == MaintenanceType.Regular)
                {
                    str = LocalizeManager.Instance.GetString("str_ui_maintance_info_01", remainTime.Minutes);
                }
                if (_maintenanceInfo.Type == MaintenanceType.Emergency)
                {
                    str= LocalizeManager.Instance.GetString("str_ui_maintance_info_02", remainTime.Minutes);
                }


                if (remainTime.Minutes > (TableManager.Instance.Define_Table["ds_maintance_popup"].Opt_03_Int/60))
                {
                    GameManager.Instance.ServerCanvas.ShowTextMover(str);
                }
                else
                {

                    if (remainTime.Minutes <= 0)
                    {
                        SetInspectionPopup();
                    }
                }
            }

            if (NowTime >= _maintenanceInfo.StartTime)
            {
                // 코루틴 전부 멈추고
                if (_maintenanceCheckCoroutine != null)
                {
                    StopCoroutine(_maintenanceCheckCoroutine);
                    _maintenanceCheckCoroutine = null;
                }
                if (_maintenanceStartCoroutine != null)
                {
                    StopCoroutine(_maintenanceStartCoroutine);
                    _maintenanceStartCoroutine = null;
                }
                // 점검 팝업 띄우기.

                SetInspectionPopup();
            }
        }

        public void SetInspectionPopup()
        {

            Action onClikc = () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
            };
            var endDate = _maintenanceInfo.EndTime;
            _isTime = false;
            Time.timeScale = 0;
            var title = LocalizeManager.Instance.GetString("str_ui_info_popup_title");
            // var message = LocalizeManager.Instance.GetString("str_ui_maintance_info_03", endDate.Year, endDate.Month, endDate.Day, endDate.Hour, endDate.Minute.ToString());      // 점검 내용
            var message = LocalizeManager.Instance.GetString("str_ui_maintance_info_03", endDate.Year, endDate.Month, endDate.Day, endDate.ToString("HH:mm"));      // 점검 내용
            GameManager.Instance.ServerCanvas.ShowMaintenenacePopup(message, title, onClikc, onClikc);
        }

        public BossInfo GetBossInfo(CONTENTS_TYPE_ID contentId)
        {
            switch (contentId)
            {
                case CONTENTS_TYPE_ID.total:
                    return TotalInfo;
                default:
                    return GetChallengeInfo(contentId.ToLeaderboardId());
            }
        }
        /// <summary>
        /// 서버 데이터를 여러 개 사용해서 레드닷을 업데이트 해야하는 경우<br/>
        /// 로그인 데이터가 모두 로드된 후 호출
        /// </summary>
        public void UpdateReddotInfo()
        {
            // 경쟁탬 보상 관련 레드닷 초기화
            SetBattlefieldReddot();


            // 인연 레드닷
            this.RewardInfo.SetCharacterCollectionReddot();
            this.RewardInfo.UpdateCharacterCollectionReddot();

            // 도사톡 레드닷 초기화
            var roomTableDataArray = TableManager.Instance.DosaTalk_Room_Table.DataArray;
            var roomIndex = 0;
            var groupRoomIndex = 0;

            for (int i = 0; i < roomTableDataArray.Length; i++)
            {
                var roomTableData = roomTableDataArray[i];

                switch (roomTableData.Dosa_Talk_Group_Type)
                {
                    case "private":
                        RedDotManager.Instance.UpdateRedDotDictionary("reddot_dosatalk_room_room", roomIndex++);
                        break;
                    case "group":
                        RedDotManager.Instance.UpdateRedDotDictionary("reddot_dosatalk_grouproom_room", groupRoomIndex++);
                        break;
                }
            }

            // 도사 패스 레드닷 초기화
            var passList = this.EventInfo.GetPassList(); // 패스 탭 초기화

            for (int i = 0; i < passList.Count; i++)
            {
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_pass_tab", i);
            }

            // 도사 관리부 레드닷 업데이트
            //this.RewardInfo.UpdateDosabuReddot();
            // 도사톡 레드닷 업데이트
            this.TalkInfo.UpdateReddotData();
            // 도사 패스 레드닷 업데이트
            this.EventInfo.UpdatePassReddot();
            //SetActiveContentReddot();
            //유료 상점 레드닷 초기화
            GameInfoManager.Instance.SetStoreReddotInfo();
            //무료 상점 레드닷 초기화
            GameInfoManager.Instance.SetCashStoreReddotInfo();
            // 캐릭터 체험 레드닷 업데이트
            this.GuideInfo.UpdateReddotDatas();


            // 컨텐츠 타이머
            SetSeasonContetntTimer();
            // 상점 레드닷 업데이트
            UpdateStoreReddot();

            // 미니게임 미션 레드닷 업데이트
            this.MiniGameInfo.UpdateReddotData();

            // 클랜, 조력자 관련 디파인 테이블 정보 변수로 저장 함수.
            SetHelperDefineInfo();
            // 보급품 리스트 업데이트 코루틴 함수
            //  StartSupplyCoroutine();
            // 컨텐츠 오픈 팝업 봤는지 여부 체크 초기화
            InitializeOpenPopupData();
        }

        public bool CheckIsLocked(string contentIndex)
        {
            if (TableManager.Instance.Lock_Table.GetLockTableData(contentIndex) == null)
            {
                return true;
            }

            bool isOpen = false;
            var data = TableManager.Instance.Lock_Table.GetLockTableData(contentIndex);
            isOpen = CheckOpend(data.Tid);
            return isOpen;
        }

        // 컨텐츠 입장권 남아있는 경우 Reddot 띄우기
        public void SetContentsTIcketsRedDot()
        {
            //추가 체크. => 해당 컨텐츠가 잠겨 있는지 확인.
            var datas = TableManager.Instance.Content_Table.DataArray;

            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].CONTENTS_TAB_TYPE == CONTENTS_TAB_TYPE.none)
                {
                    continue;
                }
                var ticketType = TableManager.Instance.Stage_Table.GetStageNeedCostType(datas[i].CONTENTS_TYPE_ID);
                if (!string.IsNullOrEmpty(ticketType))
                {
                    var value = GetAmount(ticketType);
                    if (datas[i].CONTENTS_TAB_TYPE != CONTENTS_TAB_TYPE.resource
                        && datas[i].CONTENTS_TAB_TYPE != CONTENTS_TAB_TYPE.boss
                        && datas[i].CONTENTS_TAB_TYPE != CONTENTS_TAB_TYPE.pvp)
                    {
                        continue;
                    }

                    if (CheckIsLocked(datas[i].CONTENTS_TAB_TYPE.ToString()) && value >= 0 && IsContetnOpend(datas[i].CONTENTS_TAB_TYPE))
                    {
                        var rdid = "reddot_content_"+datas[i].CONTENTS_TAB_TYPE.ToString()+"_ticket";
                        //RedDotManager.Instance.SetActiveRedDot(rdid, value > 0);
                        //// 시즌이 있는 컨텐츠인 경우 시즌 열렸을때만 티켓 갯수 체크해서 reddot 띄움
                        if (TableManager.Instance.Content_Season_Table.IsSeasosContent(datas[i].CONTENTS_TAB_TYPE.ToString()))
                        {
                            var isTicket = value > 0 && Leaderboards.IsCanEnter(datas[i].CONTENTS_TYPE_ID.ToLeaderboardId());
                            RedDotManager.Instance.SetActiveRedDot(rdid, isTicket);
                        }
                        else     // 시즌이 없는 컨텐츠인 경우 티켓 갯수가 0개 넘으면 레드닷 띄움.
                            RedDotManager.Instance.SetActiveRedDot(rdid, value > 0);
                    }

                }
            }
        }

        // 시험대 => 받을 수 있는 보상이 남아있는지에 대한 레드닷 초기화
        public void SetEnableRewardContent()
        {
            var datas = TableManager.Instance.Content_Table.DataArray;
            int curCount = 0;
            int maxCount = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].CONTENTS_TAB_TYPE == CONTENTS_TAB_TYPE.rank)
                {
                    var leaderboardid = datas[i].CONTENTS_TYPE_ID.ToLeaderboardId();
                    switch (leaderboardid)
                    {
                        case LEADERBOARDID.Challenge:
                            {
                                maxCount = TableManager.Instance.Define_Table["ds_raid_reward_max_count"].Opt_01_Int;
                                curCount = maxCount - ChallengeInfo.RewardedCount;
                                RedDotManager.Instance.SetActiveRedDot("reddot_content_challange_reward", curCount > 0);
                            }
                            break;
                        case LEADERBOARDID.Trial:
                            {
                                maxCount = TableManager.Instance.Define_Table["ds_raid_reward_max_count"].Opt_01_Int;
                                curCount = maxCount - TrialInfo.RewardedCount;
                                RedDotManager.Instance.SetActiveRedDot("reddot_content_trial_reward", curCount > 0);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private bool IsContetnOpend(CONTENTS_TAB_TYPE type)
        {
            int count = 0;

            var datas = TableManager.Instance.Content_Table.GetDataByContentTab(type);
            for (int n = 0; n< datas.Count; n++)
            {
                var conData = datas[n];
                if (!string.IsNullOrEmpty(conData.Str_Error_Guide) && GetLockedIndexs(conData.Str_Error_Guide).Count > 0)
                    count++;
            }   

            return count != datas.Count;
        }

        public bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day;
        }

        // 해당 일의 자정까지만 받을 수 있으므로 
        public long GetRecieveTimeLimit(DateTime now)
        {
            var limittime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            long ticks = limittime.Ticks;
            return ticks;
        }

        public bool IsRecievedTime(DateTime now)
        {
            var openH = TableManager.Instance.Define_Table["ds_daily_push_time"].Opt_01_Int/60;
            var openM = TableManager.Instance.Define_Table["ds_daily_push_time"].Opt_01_Int%60;
            var opentick = new DateTime(now.Year, now.Month, now.Day, openH, openM, 00).Ticks;
            Debug.Log("<color=#a2e920>opentick("+opentick+", "+openH+" : "+openM+") </color>");
            var limittick = GetRecieveTimeLimit(now);
            var nowtick = now.Ticks;
            if (nowtick >= opentick && nowtick <= limittick)
            {
                return true;
            }

            return false;
        }


        // 매일 push 보상 저장 : 오후용

        public void SetDailyPushTimer(long nowtime, long pushtime)
        {
            var period = TableManager.Instance.Post_Table[PUSH_POST_TYPE.p_push_1.ToString()].Post_Period;

            DateTime push = new DateTime(pushtime, DateTimeKind.Utc);
            DateTime now = new DateTime(nowtime, DateTimeKind.Utc);
            //DateTime now = new DateTime(nowtime, DateTimeKind.Utc).AddDays(8);
            //DateTime push = new DateTime(pushtime, DateTimeKind.Utc).AddDays(7);
            var nowIndex = (int)now.DayOfWeek;
            //var nowrewardId = now.DayOfWeek.ToTidDayReward();
            var nowrewardId = TableManager.Instance.Push_Table[nowIndex.ToString()].Reward_Group_Id;
            // 현재 시간 - 푸쉬 시간 > 0 :: 이미 지나서 로그인 했을 경우가 있으므로, 기존에 일일 푸시 보상 받은게 있는지 비교
            // 현재 시간 - 푸쉬 시간 < 0 :: 아직 푸시 시간이 안되었으므로 타이머돌리면 됨.
            if ((nowtime - pushtime < 0) && !IsRecievedTime(now))
            {
                TimerManager.Instance.StartStopwatchEvent(TimerKey.DailyPushTimer, push, () =>
                {
                    SendDailyPush(push);
                }, 10);
            }
            else
            {
                if (IsRecievedTime(now))
                {
                    if (PostInfo.GetPushPostDatas() != null)
                    {
                        var dpplinfo = PostInfo.GetPushPostDatas();
                        var index = "day_"+nowIndex.ToString();
                        if (dpplinfo.ContainsKey(PUSH_POST_TYPE.p_push_1.ToString()))
                        {
                            //if (dpplinfo[PUSH_POST_TYPE.p_push_1.ToString()].ContainsKey(nowrewardId))
                            if (dpplinfo[PUSH_POST_TYPE.p_push_1.ToString()].ContainsKey(index))
                            {
                                DateTime rewardTime = new DateTime(dpplinfo[PUSH_POST_TYPE.p_push_1.ToString()][index], DateTimeKind.Utc);
                                var rewardDay = rewardTime.DayOfWeek.ToTidDayReward();
                                bool isSameDay = IsSameDay(now, rewardTime);
                                Debug.Log("<color=#a2e920>IsSameDay("+now+", "+rewardTime+") = "+isSameDay+"</color>");
                                if (isSameDay)
                                {
                                    Debug.Log("<color=#9efc9e>"+now+"  에 대한 푸시 우편["+nowrewardId+"]를 이미 수령 함.</color>");
                                    return;
                                }
                                else
                                {
                                    SendDailyPush(now);
                                    return;
                                }
                            }
                            else
                            {
                                if (nowtime - pushtime < 0)
                                {
                                    TimerManager.Instance.StartStopwatchEvent(TimerKey.DailyPushTimer, push, () =>
                                    {
                                        SendDailyPush(push);
                                    }, 10);
                                }
                                else
                                {
                                    SendDailyPush(now);
                                }

                            }
                            //else
                            //    SendDailyPush(now);
                        }
                    }

                }
            }
        }

        // 경쟁 컨텐츠 타이머
        public void SetSeasonContetntTimer()
        {
            if (Leaderboards.GetMinVerifyTime() != null)
            {
                DateTime minVerifyTime = Leaderboards.GetMinVerifyTime().VerifyTime;
                TimerManager.Instance.StartStopwatchEvent(TimerKey.ContentVerifyTime, minVerifyTime, () =>
                {
                    //RestApiManager.Instance.RequestLeaderboardGetPrevRankBoard();
                    SetActiveContentReddot();
                }, 10);
            }
        }

        public void SendDailyPush(DateTime date)
        {
            // 푸쉬 시간이 되면 우편 정보 보냄. 
            // 요일별로 알맞은 보상 보내야함. 
            DayOfWeek dayofweek = date.DayOfWeek;
            string rewardId = dayofweek.ToTidDayReward();

            var period = TableManager.Instance.Post_Table["p_push_1"].Post_Period;
            RestApiManager.Instance.RequestGetPushPost("p_push_1"/*, rewardId, period.ToString()*/);
        }


        public string GetSavedSoundLan()
        {

            var savekey = "_selectedLanguage";
            var language = DSPlayerPrefs.GetString(savekey/*, needContainPublicKey: true*/);
            if (string.IsNullOrEmpty(language))
                return SOUND_LANGUAGE_TYPE.ko.ToString();

            return language;
        }

        public List<string> GetProductTids()
        {
            Debug.Log("nowgg GetProductTids 1");

            List<string> tids = new List<string>();

            // 상점 캐시 타입 전부 추가
            var list = TableManager.Instance.Store_Table.GetCashProducts();
            Debug.Log("nowgg GetProductTids 2");

            for (int n = 0; n < list.Count; n++)
            {
                if (list[n].Product_Type == "ad" || list[n].Product_Type == "craft"|| list[n].Product_Type == "like"|| list[n].Product_Type == "cook")
                    continue;

                tids.Add(list[n].Tid);
                Debug.Log("nowgg GetProductTids list add "+ list[n].Tid);
            }

            Debug.Log("nowgg GetProductTids 3");

            // 핫딜 tid 전부 추가
            var hlist = TableManager.Instance.Hot_Deal_Table.GetTids();
            tids.AddRange(hlist);
            Debug.Log("nowgg GetProductTids 4");

            // 패스 tid 전부 추가
            var plist = TableManager.Instance.Pass_Table.GetTids();
            tids.AddRange(plist);
            Debug.Log("nowgg GetProductTids 5");

            return tids;
        }
    }
}

public class EventInfo
{
    private bool _isAttendanceChecked = true;

    public Dictionary<string, PassData> Pass = new Dictionary<string, PassData>();
    public Dictionary<string, AttendanceData> Attendance = new Dictionary<string, AttendanceData>();
    public Dictionary<string, EventData> Event = new Dictionary<string, EventData>();

    private int _pickupPoint = 0; // 픽업 가챠 포인트
    public int PickupPoint
    {
        get => _pickupPoint;
        set => _pickupPoint = value;
    }
    public bool IsAttendanceChecked { get => _isAttendanceChecked; set => _isAttendanceChecked = value; }

    public bool IsEventOpened(string eventid)
    {
        var now = DateTime.Now.Ticks;
        if (Event.ContainsKey(eventid))
        {
            if (now >= Event[eventid].Start && now <= Event[eventid].RewardEnd)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

    public List<(string, int)> UpdateEvent(JToken eventInfo)
    {
        List<(string, int)> result = new List<(string, int)>();
        if (eventInfo == null)
        {
            return result;
        }
        var map = eventInfo["M"];

        var pass = map["pass"];
        if (pass != null)
        {
            var passMap = pass["M"];
            if (passMap != null)
            {
                foreach (JProperty passItem in passMap)
                {
                    var passName = passItem.Name;
                    if (Pass.ContainsKey(passName))
                    {
                        Pass[passItem.Name].UpdatePass(passItem.Value);
                    }
                    else
                    {
                        var newPass = new PassData();
                        newPass.UpdatePass(passItem.Value);
                        Pass[passItem.Name] = newPass;
                    }
                }
            }
        }
        var attendance = map["attendance"];
        if (attendance != null)
        {
            var attendanceMap = attendance["M"];
            if (attendanceMap != null)
            {
                foreach (JProperty attendanceItem in attendanceMap)
                {
                    var attendanceName = attendanceItem.Name;
                    if (Attendance.ContainsKey(attendanceName))
                    {
                        Attendance[attendanceItem.Name].UpdateAttendance(attendanceItem.Value);
                    }
                    else
                    {
                        var newPass = new AttendanceData();
                        newPass.UpdateAttendance(attendanceItem.Value);
                        Attendance[attendanceItem.Name] = newPass;
                    }

                    result.Add((attendanceItem.Name, Attendance[attendanceItem.Name].Prograss));
                }
            }
        }
        var gacha = map["gacha"];
        if (gacha != null)
        {
            var gachaMap = gacha["M"];
            if (gachaMap != null)
            {
                foreach (JProperty gachaItem in gachaMap)
                {
                    var gachaName = gachaItem.Name;
                    if (gachaName!="P")
                    {

                        /*if (Gacha.ContainsKey(gachaName))
                        {
                            Gacha[gachaItem.Name].UpdateGacha(gachaItem.Value);
                        }
                        else
                        {
                            var newgacha = new GachaData();
                            newgacha.UpdateGacha(gachaItem.Value);
                            Gacha[gachaItem.Name] = newgacha;
                        }*/
                    }
                    else
                    {
                        if (gachaName == "P")
                        {
                            _pickupPoint = gachaItem.Value.N_Int();
                        }
                    }
                }
            }
        }

        var eventData = map["event"];

        if (eventData != null)
        {
            var eventMap = eventData["M"];

            if(eventMap != null)
            {
                foreach(JProperty property in eventMap)
                {
                    var tid = property.Name;

                    if (Event.ContainsKey(tid))
                    {
                        Event[tid].UpdateEventData(property.Value);
                    }
                    else
                    {
                        var newEvent = new EventData();

                        newEvent.UpdateEventData(property.Value);
                        Event[tid] = newEvent;
                    }
                }
            }
        }

        return result;
    }

 

    public PassData GetPassData(string groupId)
    {
        if (this.Pass.TryGetValue(groupId, out var passData))
        {
            return passData;
        }

        return null;
    }

    public List<string> GetPassList()
    {
        return this.Pass.Keys.ToList();
    }

    public void UpdatePassReddot()
    {
        if (this.Pass.Count <= 0)
            return;

        var keys = this.GetPassList();

        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var passData = this.Pass[key];
            var isAdditional = !passData.BuyType.Equals("free");
            var isReddotOn = false;

            for(int level = 1; level <= passData.Level; level++)
            {
                if (!passData.Normal.ContainsKey(level) || !passData.Normal[level])
                {
                    isReddotOn = true;
                    break;
                }

                if(isAdditional && (!passData.Premium.ContainsKey(level) || !passData.Premium[level]))
                {
                    isReddotOn = true;
                    break;
                }
            }

            RedDotManager.Instance.SetActiveRedDot("reddot_pass_tab", i, isReddotOn);
        }
    }
}

public class PassData
{
    public long Start;
    public long End;
    public long Last;
    public string BuyType;
    public int Level;
    public int CumulativeExp;
    public int Exp;
    public Dictionary<int, bool> Normal = new Dictionary<int, bool>();
    public Dictionary<int, bool> Premium = new Dictionary<int, bool>();

    public void UpdatePass(JToken passData)
    {
        var pass = passData["M"];
        var start = pass["ST"];
        if (start != null)
        {
            Start = start.N_Long();
        }
        var end = pass["ET"];
        if (end != null)
        {
            End = end.N_Long();
        }
        var buy = pass["B"];
        if (buy != null)
        {
            BuyType = buy.S_String();
        }
        var level = pass["L"];
        if (level != null)
        {
            Level = level.N_Int();
        }
        var cExp = pass["CE"];
        if (cExp != null)
        {
            CumulativeExp = cExp.N_Int();
        }
        var exp = pass["E"];
        if (exp != null)
        {
            Exp = exp.N_Int();
        }
        var normal = pass["NP"];
        if (normal != null)
        {
            var normalList = normal.M_JToken();

            foreach (JProperty normalItem in normalList)
            {
                if(int.TryParse(normalItem.Name.Replace("P", string.Empty), out var value))
                {
                    Normal[value] = normalItem.Value.N_Int() == 1;
                };
            }
        }
        var premium = pass["PP"];
        if (premium != null)
        {
            var premiumList = premium.M_JToken();

            foreach (JProperty premiumItem in premiumList)
            {
                if (int.TryParse(premiumItem.Name.Replace("P", string.Empty), out var value))
                {
                    Premium[value] = premiumItem.Value.N_Int() == 1;
                }
            }
        }
    }
}

public class AttendanceData
{
    public long Start;
    public long End;
    public long Next;
    public int Prograss;
    public void UpdateAttendance(JToken passData)
    {
        var pass = passData["M"];
        var start = pass["ST"];
        if (start != null)
        {
            Start = start.N_Long();
        }
        var end = pass["ET"];
        if (end != null)
        {
            End = end.N_Long();
        }
        var next = pass["NT"];
        if (next != null)
        {
            Next = next.N_Long();
        }
        var prograss = pass["N"];
        if (prograss != null)
        {
            Prograss = prograss.N_Int();
        }

    }
}
public class GachaData
{
    public long Start;
    public long End;
    public long Point;
    public int RewardIndex;

    public void UpdateGacha(JToken gachaData)
    {
        var gacha = gachaData["M"];

        if(gacha != null)
        {
            var start = gacha["ST"];
            if (start != null)
            {
                Start = start.N_Long();
            }
            var end = gacha["ET"];
            if (end != null)
            {
                End = end.N_Long();
            }
            var point = gacha["P"];
            if (point != null)
            {
                Point = point.N_Long();
            }
            var rewardIndex = gacha["R"];
            if(rewardIndex != null)
            {
                RewardIndex = rewardIndex.N_Int();
            }
        }
    }
}

public class EventData
{
    public long Start;
    public long End;
    public long RewardEnd;

    public void UpdateEventData(JToken eventData)
    {
        var eventMap = eventData["M"];

        var start = eventMap["ST"];
        if(start != null)
        {
            Start = start.N_Long();
        }

        var end = eventMap["ET"];
        if (end != null)
        {
            End = end.N_Long();
        }

        var rewardEnd = eventMap["CT"];
        if (rewardEnd != null)
        {
            RewardEnd = rewardEnd.N_Long();
        }
    }
}