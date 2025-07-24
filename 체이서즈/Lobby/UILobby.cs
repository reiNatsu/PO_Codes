using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using UnityEngine.UI;

public class UILobby : UIBase
{
    [Header("캐릭터 인터렉션 Talkbox")]
    [SerializeField] private UICharacterInteraction _uiCharacterInteraction;
    [Header("Navi Quest UI")]
    [SerializeField] private UINaviQuest _uiNaviQuest;
    [Header("Main Story UI")]
    [SerializeField] private ExImage _mainImg;
    [SerializeField] private Slider _mainSlider;
    [SerializeField] private ExTMPUI _mainTitle;

    [Header("보급품 버튼")]
    [SerializeField] private UISuppliesButtonObj _uiSupplyButtonObj;

    [Header("Lobby UI Object")]
    [SerializeField] private UILobbySwitchButton _hideBtn;
    [SerializeField] private UILobbySwitchButton _showBtn;
    [SerializeField] private GameObject _rightLobbyUIObj;
    [SerializeField] private GameObject _leftLobbyUIObj;
    [SerializeField] private GameObject _bottomLobbyUIObj;

    [SerializeField] private ExButton _minigameButton;

    [SerializeField] private ExButton _prevBtn;
    [SerializeField] private ExButton _nextBtn;
    

    [SerializeField] private UITicketCounter[] _ticketCounter;
    [SerializeField] private UIAccountInfo _accountInfo;


    [SerializeField] private RectTransform _goodsBG;
    [SerializeField] private RectTransform _goodsTran;

    [SerializeField] private List<UILobbyButton> _uiLobbyButtons = new List<UILobbyButton>();

    [SerializeField] private UIContentLcok[] _uIContentLcoks;
    [SerializeField] private UILayoutGroup[] _uILayoutGroups;

    [SerializeField] private HotdealButton _hotdealButton;
    [SerializeField] private UILobbyBanner _uiLobbyBanner;

    [SerializeField] private List<GameObject> _eventBtnObj;

    private bool _isShow = false;
    public bool IsWelcom { get; set; } = false;
    private bool _isLobbyHide = false;
    private bool _isCheckClanAT = false;
    /// <summary>
    /// 출석부 UI 체크
    /// </summary>
    private AttendanceChecker _attendanceChecker = new AttendanceChecker();
    private HotdealPopupChecker _hotdealChecker = new HotdealPopupChecker();
    private Coroutine _attendanceShowRoutine;
    private Coroutine _hotdealCheckRoutine;

    public Action<string, string> OnWelcomInteracrtion { get; set; }
    public bool IsLobbyHide { get { return _isLobbyHide; } }
    private bool _isFirst = false;              // 처음 로그인 했는지.
    public UISuppliesButtonObj UISuppliesButtonObj { get { return _uiSupplyButtonObj; } }
    public UICharacterInteraction UICharacterInteraction { get { return _uiCharacterInteraction; } }
    public override void Awake()
    {
        _uiType = UIType.Default;
    }

    public void OnDisable()
    {
        if(LobbyController.Instance != null)
            LobbyController.Instance.AdllInteractionSoundOff();
    }

    public override void Refresh()
    {
        base.Refresh();
        LobbyController.Instance.SetCameraLobbyFOV();
        CustomLobbyManager.Instance.RefreshLobby();
        _isShow = GameInfoManager.Instance.StageInfo.ClearStage.ContainsKey("ch01_s02_n");
        UpdateUI();

        _accountInfo.Show();
        
    }

    public override void Init()
    {
        base.Init();
        GameInfoManager.Instance.LoadEnterEventState();         // 이벤트 진입 여부 기기 저장 로드
        _uiLobbyBanner.Initialize();
        // 클랜 레벨 제한 리스트화
        GameInfoManager.Instance.SetClanRestrictionleveLists();

        _hotdealChecker.Initialize();
        
        GameInfoManager.Instance.IsSkipInteractionSounde = false;
        _prevBtn.onClick.AddListener(() =>
        {
            CustomLobbyManager.Instance.PreLobbyData();
        });
        _nextBtn.onClick.AddListener(() =>
        {
            CustomLobbyManager.Instance.NextLobbyData();
        });
    }

    public void UpdateAccountInfo()
    {
        _accountInfo.Show();
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        CustomLobbyManager.Instance.RefreshLobby();
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Bool, out var isSkip))
                CustomLobbyManager.Instance.HideMode = (bool)isSkip;
            else
                CustomLobbyManager.Instance.HideMode = false;
        }
        else
        {
            CustomLobbyManager.Instance.HideMode = SceneManager.Instance.IsGoContent;
        }
        CheckClanAttendance();
        //_isShow = GameInfoManager.Instance.StageInfo.ClearStage.ContainsKey("ch01_s02_n");
        _isShow = true;
        // _audioLanguageTest.gameObject.SetActive(false);
        
        ShowLobbyUI();
        // 컨텐츠 오픈 팝업 여는 함수
        GameInfoManager.Instance.OpenContentUnlock?.Invoke();
    }

    //Todo 신준호 => 홈 버튼을 눌렀을때도 Show() 함수와 같이 동작해야되서 아래 함수로
    //Show 했을때 로직을 여기로 옮김
    private void ShowLobbyUI()
    {
        _accountInfo.Show();

        UpdateUI();

        _uiLobbyBanner.Show();
    }

    public override void Display()
    {
        base.Display();

        CheckedDailyAttendence();
        _uiLobbyBanner.Display();
        ShowHotdeal();
        _hotdealCheckRoutine = StartCoroutine(_hotdealChecker.Check());
        
    }

    public override void Hide()
    {
        base.Hide();
        CustomLobbyManager.Instance.SetExitLobby();
        if (_attendanceShowRoutine != null)
        {
            StopCoroutine(_attendanceShowRoutine);
            _attendanceShowRoutine = null;
        }

        if (_hotdealCheckRoutine != null)
        {
            StopCoroutine(_hotdealCheckRoutine);
            _hotdealCheckRoutine = null;
        }
        _uiLobbyBanner.Hide();
    }

    public void ShowHotdeal()
    {
        _hotdealButton.Show();
    }

    // 클랜 출석 체크 레드닷
    public void CheckClanAttendance()
    {
        if (!GameInfoManager.Instance.ClanInfo.IsJoinClan())
            return;

        var attendanceTime = GameInfoManager.Instance.ClanInfo.GetUserClanConfig().AttendanceTime;
        var dateTime = new DateTime(attendanceTime);

        RedDotManager.Instance.SetActiveRedDot("reddot_clan_attendance", 0, 0, dateTime < GameInfoManager.Instance.DailyResetTime);
    }

    /// <summary>
    /// 출석 여부 체크
    /// </summary>
    public void CheckedDailyAttendence()
    {
        // 게임이 첫 접속일 시
        if (!GameInfoManager.Instance.EventInfo.IsAttendanceChecked && !_attendanceChecker.IsChecked)
        {
            // 튜토리얼 체크
            //var isTutorial = TutorialManager.Instance.IsTutorialing;
            var isTutorial = false;
            if (!TableManager.Instance.Define_Table.TryGetData("ds_attendance_tutorial_step", out var defineTableData))
                return;

            // 튜토리얼 스텝이 8 이상일 경우
            if (TutorialManager.Instance.IsClear(defineTableData.Opt_01_Str) && !isTutorial)
            {
                _attendanceShowRoutine = StartCoroutine(_attendanceChecker.Show());
            }
            GameInfoManager.Instance.SaveEnterEventState(true);
        }
    }

    public void UpdateSuppliesButtonUI()
    {
        _uiSupplyButtonObj.Init();
    }

    public void UpdateUI()
    {
        _uiNaviQuest.InitInfo();
        _uiSupplyButtonObj.Init();
        for (int n = 0; n< _uiLobbyButtons.Count; n++)
        {
            _uiLobbyButtons[n].UpdateReddot();
        }

        _hideBtn.On();
        _showBtn.Off();
        SetLobbyUiShow(true);

        CheckedDailyAttendence();
        _uiLobbyBanner.Display();
        UpdataGoods();
        //CheckContentIsOpened();
        SetMainStageInfo();
        //for (int n = 0; n < _eventBtnObj.Count; n++)
        //{
        //    _eventBtnObj[n].SetActive(true);
        //}
        if (CheckOpendEvent().Count > 0)
        {
            var data = CheckOpendEvent();
            for (int n = 0; n < data.Count; n++)
            {
                var eventdata = TableManager.Instance.Event_Table[data[n]];
                _eventBtnObj[n].GetComponent<ExImage>().SetSprite(eventdata.Event_Banner_Image);
                _eventBtnObj[n].SetActive(GameInfoManager.Instance.EventInfo.IsEventOpened(data[n]));
            }
        }
        else
        {
            for (int n = 0; n < _eventBtnObj.Count; n++)
            {
                _eventBtnObj[n].SetActive(false);
            }
        }

        for (int n = 0; n< _uILayoutGroups.Length; n++)
        {
            _uILayoutGroups[n].UpdateLayoutGroup();
        }
        RedDotManager.Instance.SetActiveRedDot("reddot_event_main", GameInfoManager.Instance.IsEventFirstEnter);
        UpdateMinigameButton();
    }

    public void ShowNaverBanner()
    {
#if UNITY_STANDALONE_WIN
        Application.OpenURL("https://game.naver.com/lounge/DOSA_Guardians/board/1");
#endif
    }

    public void UpdataGoods()
    {
        for (int i = 0; i< _ticketCounter.Length; i++)
        {
            int amount = GameInfoManager.Instance.GetCurrency(_ticketCounter[i].TicketType);
            if (_ticketCounter[i].TicketType.Equals(TICKET_TYPE.i_cash.ToString()) || _ticketCounter[i].TicketType.Equals(TICKET_TYPE.i_free_cash.ToString()))
            {
                amount = GameInfoManager.Instance.GetAmount(_ticketCounter[i].TicketType);
            }
            _ticketCounter[i].Show(amount);
            _ticketCounter[i].UpdateTicket(amount);
            _ticketCounter[i].GetComponent<UILayoutGroup>().UpdateLayoutGroup();
            _ticketCounter[i].gameObject.GetComponent<UILayoutGroup>().UpdateLayoutGroup();
        }

    }

    public void UpdataGoodsUI(string type, int value)
    {
        for (int i = 0; i< _ticketCounter.Length; i++)
        {
            if (_ticketCounter[i].TicketType.ToString().Equals(type))
            {
                _ticketCounter[i].UpdateTicket(value);
            }
        }
    }

    private void UpdateMinigameButton()
    {
        var isShowMinigame = GameInfoManager.Instance.MiniGameInfo.GetMinigameInProgress() != null;

        _minigameButton.gameObject.SetActive(isShowMinigame);
    }

    public void OnClickProfile()
    {
        List<LEADERBOARDID> ids = new List<LEADERBOARDID>() { LEADERBOARDID.Challenge, LEADERBOARDID.Trial, LEADERBOARDID.PvP };
        List<string> keys = new List<string>();

        for (int i = 0; i < ids.Count; i++)
        {
            var setting = GameInfoManager.Instance.Leaderboards.GetSetting(ids[i]);
            int season;

            //시즌 기간에 따라 랭킹 보드 데이터 불러옴
            if (setting.IsEnd())
                season = setting.Version;
            else
                season = setting.Version - 1;

            if (!GameInfoManager.Instance.Leaderboards.IsExistSeasonRankData(ids[i], season))
                keys.Add(ids[i].ToString());
        }

        if (keys.Count > 0)
            RestApiManager.Instance.RequestLeaderboardUpdate(keys, () => UIManager.Instance.Show<UIUserProfile>());
        else
            UIManager.Instance.Show<UIUserProfile>();
    }

    public void OnClickCharacter()
    {
        UIManager.Instance.Show<UICharacterListViewer>();
    }

    public void OnClickStage()
    {
        //UIManager.Instance.Show<UIChapter>();
        UIManager.Instance.Show<UIMainStage>();
    }

    public void OnClickContent()
    {
        UIManager.Instance.Show<UIContent>();
    }
    public void OnClickMenu()
    {
        UIManager.Instance.Show<UIMenu>();
    }
    public void OnClickBanner()
    {
        Application.OpenURL("https://cafe.naver.com/dosaforum");
    }
    public void OnClickQuest()
    {
        UIManager.Instance.Show<UIQuest>();
    }
    public void OnClickTestPopupOpen()
    {
        UIManager.Instance.Show<UIEquip>();
    }
    public void OnClickEvent()
    {
        UIManager.Instance.Show<UIEventPopup>();
    }
    public void OnClickStore()
    {
        UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "none"));
    }
    public void OnClickCashStore()
    {
        UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "cash"));
    }

    public void OnClickPass()
    {
        Debug.Log("Pass Action");
        if (GameInfoManager.Instance.EventInfo.Pass.Count == 0)
        {
            UIManager.Instance.ShowToastMessage("str_ui_pass_ongoing_01"); //진행 중인 패스가 없습니다.
        }
        else
            UIManager.Instance.Show<UISeasonPass>();
    }

    public void OnClickPost()
    {
        var expiredDatas = GameInfoManager.Instance.PostInfo.GetExpiredPostKeys();

        if (expiredDatas.Count > 0)
            RestApiManager.Instance.RequestPostVerifyExpired(expiredDatas, () => UIManager.Instance.Show<UIPost>());
        else
            UIManager.Instance.Show<UIPost>();
    }
    public void OnClickInventory()
    {
        UIManager.Instance.Show<UIInventory>();
    }
    public void OnClickDosaTalk()
    {
        UIManager.Instance.Show<UIDosaTalk>();
    }

    public void OnClickMinigame()
    {
        RestApiManager.Instance.RequestGetNowTime(() =>
        {
            var tid = GameInfoManager.Instance.MiniGameInfo.GetMinigameInProgress();

            if (tid.IsNullOrEmpty())
                return;

            var minigameTableData = TableManager.Instance.Minigame_Table[tid];

            UIManager.Instance.Show($"UIMiniGame_{minigameTableData.Minigame_Group}");
        });
    }

    public void OnClickNoticePopup()
    {
        UIManager.Instance.Show<UINoticePopup>();
    }

    public void OnClickNaverBanner()
    {
        ShowNaverBanner();
    }

    public void OnClickHelperCollection()
    {
        UIManager.Instance.Show<UIHelperCollection>();
    }

    // 이벤트 클릭  
    // TODO :  나중에 story타입 이벤트 별로 생성 해야 할 수 있으니 작게 스크롤로 변경 해 둬야 함. 
    public void OnClickGoEvent(int index)
    {
        //if (_isFirst)
        //{
        //    _isFirst = false;
        //    RedDotManager.Instance.SetActiveRedDot("reddot_event_main", _isFirst);
        //}
        //else
        //    RedDotManager.Instance.SetActiveRedDot("reddot_event_main", false);

        if (CheckOpendEvent().Count > 0)
        {
            var data = CheckOpendEvent();
            UIManager.Instance.Show<UIEventMain>(Utils.GetUIOption(UIOption.Tid, data[0]));
        }
        // UIManager.Instance.Show<UIEventMain>(Utils.GetUIOption(UIOption.Tid, "event_wish_and_promise_01"));
    }

    public void OnClickLobbySetting()
    {
        UIManager.Instance.Show<UILobbySettingPreview>();
    }

    public void OnClickClan()
    {
        UIManager.Instance.Show<UIClanMenu>();
    }

    public void OnClickLobbyUIHide()
    {
        SetLobbyUiShow(false);
        _hideBtn.Off();
        _showBtn.On(); 
    }

    public void OnClickLobbyUIShow()
    {
        SetLobbyUiShow(true);
        _hideBtn.On();
        _showBtn.Off();
    }
    public void OnClickShowLobbyShowBtn()
    {
        if (!_showBtn.Button.gameObject.activeInHierarchy)
            _showBtn.On();
    }

    public void SetLobbyUIState(bool isHide)
    {
        _isLobbyHide = isHide;
     }

    public void SetLobbyUiShow(bool isOn)
    {
        _rightLobbyUIObj.SetActive(isOn);
        _leftLobbyUIObj.SetActive(isOn);
        _bottomLobbyUIObj.SetActive(isOn);
        _prevBtn.gameObject.SetActive(isOn);
        _nextBtn.gameObject.SetActive(isOn);
        _isLobbyHide = !isOn;
    }

    public void ShowSwitchUI()
    {
        if (_isLobbyHide)
        {
            _showBtn.On();
        }
    }

    // 현재 열려있는 이벤트가 있는지 체크 > 열려있는 이벤트 있으면 Event_Table Tid 반환.
    public List<string> CheckOpendEvent()
    {
        List<string> opendlist = new List<string>();
        var now = DateTime.UtcNow.Ticks;
        var data = GameInfoManager.Instance.EventInfo.Event;
        foreach (var eventinfo in data)
        {
            // 이벤트 시작 tick ~ 이벤트 보상 가능 날짜
            if (now >= eventinfo.Value.Start && now < eventinfo.Value.RewardEnd)
            {
                if (!opendlist.Contains(eventinfo.Key))
                    opendlist.Add(eventinfo.Key);
            }
        }
        return opendlist;
    }
    
    public void SetMainStageInfo()
    {
        // 스테이지 테스트
        var bestChapter = GameInfoManager.Instance.StageInfo.BestClearChapter();
        var bChapterData = TableManager.Instance.Content_Table.GetDataByChapter(bestChapter).FirstOrDefault();

        _mainImg.SetSprite(bChapterData.Content_Bg_Icon);
        _mainTitle.ToTableText(bChapterData.Str);
        _mainSlider.minValue = 0;
        _mainSlider.maxValue = GameInfoManager.Instance.GetAllRewardStarCount(bChapterData.Content_Chapter_Id);
        _mainSlider.value = GameInfoManager.Instance.GetRewardStarCount(bChapterData.Content_Chapter_Id);
    }

    public void CheckContentIsOpened()
    {
        if (GameInfoManager.Instance.ContentIsOpenDic.Count > 0)
        {
            var contentDic = GameInfoManager.Instance.ContentIsOpenDic;
            for (int n = 0; n < _uIContentLcoks.Length; n++)
            {
                if (!string.IsNullOrEmpty(_uIContentLcoks[n].TID))
                {
                    if (contentDic.ContainsKey(_uIContentLcoks[n].TID))
                    {
                        _uIContentLcoks[n].IsOpen = contentDic[_uIContentLcoks[n].TID];
                        _uIContentLcoks[n].ContentStateUpdate();
                    }
                }
            }
        }
    }

    public void AllContentUnlock()
    {
        for (int n = 0; n < _uIContentLcoks.Length; n++)
        {
            _uIContentLcoks[n].gameObject.SetActive(false);
        }
    }

    public void OnClickInteractionDimd()
    {
        Debug.Log("<color=#4cd311>UICharacterInteraction  <b>OnClickInteractionDimd</b></color>");
        ShowSwitchUI();
    }

    // 선택 언어 별 사운드 테스트 버튼. > 테스트 후 삭제 예정
    public void OnClickSoundLanTest()
    {
        //_audioLanguageTest.gameObject.SetActive(true);
        //_audioLanguageTest.ShowAudioTestWindow();
    }

    public void ShowCharacterTalkBox(string strid, float delaytime)
    {
        _uiCharacterInteraction.StopTalk();
        _uiCharacterInteraction.SetTalkUI(strid, delaytime);
    }

    public void StopCharacterInteraction()
    {
        _uiCharacterInteraction.StopTalk();
    }

    public void UpdateNavi()
    {
        _uiNaviQuest.InitInfo();
    }
}