using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class UIStage : UIBase
{
    [Header("Stage Name TMPs")]
    [SerializeField] private ExTMPUI _stageNumberTMP;
    [SerializeField] private ExTMPUI _stagetitleTMP;
    [Header("StageNode")]
    [SerializeField] private RecycleScroll _scroll;

    [Header("별보상")]
    [SerializeField] private UIStarReward _uiStarReward;
    [SerializeField] private ExTMPUI _starRewardTMP;

    [Header("스테이지 정보 팝업")]
    [SerializeField] private UIStageInfoSide _uiStageInfoSide;      // 오른쪽에서 보여지는 스테이지 인포창

    [Header("해방 임무 정보")]
    [SerializeField] private UILiberation _uiLiberation;         

    [Header("Level Button")]
    [SerializeField] private List<UIStageLevelTab> _levelTabs = new List<UIStageLevelTab>();

    private List<StageNodeCellData> _cellDatas = new List<StageNodeCellData>();
    private List<StageNodeCell> _cells;

    private Dictionary<int, List<Stage_TableData>> _stages = new Dictionary<int, List<Stage_TableData>>();
    private Stage_TableData _currentStageData = null;

    private int _chapterIndex = 0;
    private int _levelIndex;
    private bool _isOpenInfo = false;

    private float _originalScrollMinX = 0.0f;       // 스크롤 오리지널 가로 길이.
    private int _clickedStageNo = 0;
    private int _stageCount = 0;

    private int _openStageId = -1;
    private bool _isSkipOpenInfo = false;
    private Coroutine _coroutine = null;

    public UIStageInfoSide UIStageInfoSide { get { return _uiStageInfoSide; } }
    public override void Close(bool needCached = true)
    {
        if (_uiStageInfoSide != null && _uiStageInfoSide.IsOpen)
            _uiStageInfoSide.DeActive();

        ActiveInfoWindow(false);
        _stages.Clear();
        base.Close(needCached);
    }

    //public void OnEnable()
    //{
    //    _isSkipOpenInfo = false;
    //    if (_isOpenInfo)
    //    {
    //        _isOpenInfo = false;
    //        OpenStageInfo(_currentStageData);
    //    }
    //    else
    //    {
    //        _uiStageInfoSide.InitializeAnim();
    //        if (_cells == null || _cells.Count == 0)
    //            return;

    //        //for (int i = 0; i < _cells.Count; i++)
    //        //{
    //        //    if (_cells[i].gameObject.activeInHierarchy)
    //        //        _cells[i].ActiveSelected(null);
    //        //}
    //        if (_currentStageData.Stage_Id > 2)
    //        {
    //            int targetIndex = 0;
    //            if (_currentStageData.Stage_Id == _stages[_levelIndex].LastOrDefault().Stage_Id)
    //                targetIndex = _currentStageData.Stage_Id -2;
    //            else
    //                targetIndex =  _currentStageData.Stage_Id -1;

    //            _scroll.FocusLine(targetIndex, padding: -_scroll.GetCustomAddValue(1.3f), isCustom: true, isMoveSmooth: true);
    //        }
    //        SetSelectUI();
    //        _isSkipOpenInfo = false;
    //    }
    //}

    public override void Refresh()
    {
        base.Refresh();
        _isSkipOpenInfo = false;
        if (_isOpenInfo)
        {
            _isOpenInfo = false;
            OpenStageInfo(_currentStageData);
        }
        else
        {
            _uiStageInfoSide.InitializeAnim();
            //for (int i = 0; i < _cells.Count; i++)
            //{
            //    if (_cells[i].gameObject.activeInHierarchy)
            //        _cells[i].ActiveSelected(null);
            //}
            if (_currentStageData.Stage_Id > 2)
            {
                int targetIndex = 0;
                if (_currentStageData.Stage_Id == _stages[_levelIndex].LastOrDefault().Stage_Id)
                    targetIndex = _currentStageData.Stage_Id -2;
                else
                    targetIndex =  _currentStageData.Stage_Id -1;

                _scroll.FocusLine(targetIndex, padding: -_scroll.GetCustomAddValue(1.3f), isCustom: true, isMoveSmooth: true);
            }
            SetSelectUI(-1);
            _isSkipOpenInfo = false;
        }
        _uiLiberation.Init(_chapterIndex);
    }

    public void SetResetInfoWindow()
    {
        if (_isOpenInfo)
        {
            _isOpenInfo = false;
            OnClickCloseInfo();
        }

        _uiStageInfoSide.InitializeAnim();
    }

    public override void Init()
    {
        base.Init();

        _scroll.Init();
        _cellDatas = new List<StageNodeCellData>();
        _cells = _scroll.GetCellToList<StageNodeCell>();

        _uiStageInfoSide.Init();
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _isOpenInfo = false;
        _uiStageInfoSide.InitializeAnim();
        ActiveInfoWindow(false);
        if (optionDict != null)
        {
            // 챕터 넘버
            if (optionDict.TryGetValue(UIOption.Index, out var chapterindex))
                _chapterIndex = (int)chapterindex;
        }

        SetStageDatas();
        SetLevelButtons();
        _levelIndex = -1;
        //OnClickLevelButton(0);
        _clickedStageNo = 0;
        if (optionDict != null && optionDict.TryGetValue(UIOption.Data, out var stageData))
        {
            var data = (Stage_TableData)stageData;
            OnClickLevelButton((int)data.LEVEL_DIFFICULTY);
            if (optionDict.TryGetValue(UIOption.Bool, out var isSkip))
                _isSkipOpenInfo = (bool)isSkip;
            else
                _isSkipOpenInfo = false;
            OpenStageInfo(data);
        }
        else
            OnClickLevelButton(0);

        UpdateUI();
        CheckHelper();
        _uiLiberation.Init(_chapterIndex);

        if (_coroutine == null)
            _coroutine = StartCoroutine(ShowUnLockPopup());
    }

    IEnumerator ShowUnLockPopup()
    {
        yield return new WaitForEndOfFrame();
        GameInfoManager.Instance.OpenContentUnlock?.Invoke();

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private void ActiveInfoWindow(bool isOn)
    {
        //if (_uiStageInfoSide.gameObject.activeInHierarchy != isOn)
            _uiStageInfoSide.gameObject.SetActive(isOn);
    }

    private bool CheckIsOpened(string lockKey)
    {
        bool isOpen = false;

        if (string.IsNullOrEmpty(lockKey))
            isOpen = true;
        else
        {
            if (TableManager.Instance.Lock_Table[lockKey] != null)
                isOpen= GameInfoManager.Instance.GetLockedIndexs(lockKey).Count == 0;
            else
                isOpen = true;
        }
        return isOpen;
    }

    public void OpenStageInfo(Stage_TableData data)
    {
        // RecycleScroll의 Content의 길이를 늘려줬다가, 닫을떄 원상복귀 시켜주시.
        _currentStageData = data;
        _openStageId = data.Stage_Id;
        bool isOpen = CheckIsOpened(data.Lock_Tid);
        if (GameInfoManager.Instance.IsAllOpen)
            isOpen = true;

        if (!_isSkipOpenInfo)
        { 
            if (!_isOpenInfo && isOpen)
            {
                _uiStageInfoSide.Active();
                _isOpenInfo = true;
                ActiveInfoWindow(true);
            }

            float addvalue = 0.25f;
            if (data.Stage_Id > 1)
                SetNodeListSize(data.Stage_Id, data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_main);

            if (_currentStageData.Stage_Id > 0)
            {
                float padding = 0.0f;
                if (data.Stage_Id == _stages[(int)data.LEVEL_DIFFICULTY].LastOrDefault().Stage_Id)
                {
                    if (data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_main)
                        padding = 0.8f;
                    else
                        padding = 0.6f;
                }
                else if (data.Stage_Id == _stages[(int)data.LEVEL_DIFFICULTY].FirstOrDefault().Stage_Id)
                    padding = 0.05f;
                else
                    padding =0.7f;
                //_scroll.FocusLine(_stageDatas[_levelIndex].Stage_Id-1,padding: -70,isCustom:true, isMoveSmooth:true);
                _scroll.FocusLine(_currentStageData.Stage_Id-1, padding: -_scroll.GetCustomAddValue(padding), isCustom: true, isMoveSmooth: true);
            }
            SetSelectUI(_currentStageData.Stage_Id -1);
        }
        else
        {
            if (_currentStageData.Stage_Id > 2)
            {
                int targetIndex = 0;
                if (_currentStageData.Stage_Id == _stages[(int)data.LEVEL_DIFFICULTY].LastOrDefault().Stage_Id)
                    targetIndex = _currentStageData.Stage_Id -2;
                else
                    targetIndex =  _currentStageData.Stage_Id -1;

                _scroll.FocusLine(targetIndex, padding: -_scroll.GetCustomAddValue(0.7f), isCustom: true, isMoveSmooth: true);
            }
            SetSelectUI(-1);
            _isSkipOpenInfo = false;
        }

        _uiStageInfoSide.SetStageInfo(data);
        if(!_isOpenInfo)
            ActiveInfoWindow(false);
    }

    private void SetNodeListSize(int stageid, bool isMain)
    {
        float index = 1.2f;
        if (!isMain)
            index = 1.5f;
        var lastStage = TableManager.Instance.Stage_Table.GetLastStageData(_chapterIndex, (LEVEL_DIFFICULTY)_levelIndex, CONTENTS_TYPE_ID.stage_main).Stage_Id;
        //if (stageid < lastStage - 3)
        //    index = 1;
        //else
        //    index = 2;
        float width = 0.0f;
        var cellwidth = _scroll.GetCellObjectWidth() + _scroll.Spacing.x;

        if (_originalScrollMinX < 0)
            width = _originalScrollMinX - (cellwidth*index);
        else
            width = _originalScrollMinX + (cellwidth*index);

        _scroll.ReSizeContent(width);
        // _scroll.ResetContentSize(contents.x);
    }
    private void SetResizeNodeScroll()
    {
        int index = Math.Abs(_cells.Count / 2);

        float width = 0.0f;
        var cellwidth = _scroll.GetCellObjectWidth();
        var addWidth = Math.Abs(Screen.width / 2) +cellwidth;
        if (_originalScrollMinX < 0)
            width = _originalScrollMinX - (_scroll.Padding.right);
        else
            width = _originalScrollMinX + (_scroll.Padding.right);

        _scroll.ReSizeContent(width);
    }
    public void UpdateUI()
    {
        //SetTotalStarReward();
       // _uiStarReward.Show(_stages[_levelIndex].FirstOrDefault().Tid, "reddot_story_rewards");
    }

    // 스테이지 노드 세팅
    private void SetStageNode(int levelIndex)
    {
        _cellDatas.Clear();
        var list = _stages[levelIndex];
        var str = TableManager.Instance.Content_Table.GetData(list.FirstOrDefault().CONTENTS_TYPE_ID, list.FirstOrDefault().Theme_Id, levelIndex);
        _stageNumberTMP.ToTableText("str_stage_area_name_01", list.FirstOrDefault().Theme_Id.ToString("D2"));
        if (string.IsNullOrEmpty(str))
            _stagetitleTMP.gameObject.SetActive(false);
        else
        {
            _stagetitleTMP.gameObject.SetActive(true);
            _stagetitleTMP.ToTableText(str);
        }
           
        _stageCount = list.Count;
        for (int n = 0; n < list.Count; n++)
        {
            var isLast = n == list.Count-1;
            _cellDatas.Add(new StageNodeCellData(list[n], isLast));
        }

        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(_stages[levelIndex].Count);
        _originalScrollMinX = _scroll.GetContentObjWidth();
        SetResizeNodeScroll();
    }

    private void SetTotalStarReward()
    {
        var totalStars = GameInfoManager.Instance.GetStarCount(_chapterIndex, (LEVEL_DIFFICULTY)_levelIndex);
        var curStars = GameInfoManager.Instance.GetStarAchievedCount(_chapterIndex, (LEVEL_DIFFICULTY)_levelIndex);
        _starRewardTMP.text = string.Format("<color=#9b8cff>{0}</color> / {1}", curStars, totalStars);
    }

    private void SetStageDatas()
    {
        //스테이지 정보들 저장
        if (_stages == null)
            _stages = new Dictionary<int, List<Stage_TableData>>();

        //var list = TableManager.Instance.Stage_Table.GetDatas(_chapterIndex);
        // 해당 챕터/레벨의 메인 스테이지 리스트만 불러옴. => 서브 스테이지는 StageNodeCell 에서 관리 
        var list = TableManager.Instance.Stage_Table.GetDatas(_chapterIndex, CONTENTS_TYPE_ID.stage_main);
        for (int n = 0; n< list.Count; n++)
        {
            // var index = GetStageType(list[n]);
            var index = (int)list[n].LEVEL_DIFFICULTY;
            if (!_stages.ContainsKey(index))
                _stages.Add(index, new List<Stage_TableData>());

            _stages[index].Add(list[n]);
        }
    }
    // 레벨 버튼 세팅
    private void SetLevelButtons()
    {
        for (int n = 0; n< _levelTabs.Count; n++)
        {
            var index = n;
            _levelTabs[index].InitTab();
            LEVEL_DIFFICULTY level = (LEVEL_DIFFICULTY)index;
            var contentTid = "content_main_chapter_"+level.ToString()+"_"+_chapterIndex.ToString("D2");
            string lockTid = null;
            if (TableManager.Instance.Content_Table[contentTid] == null)
                lockTid = null;
            else
                lockTid = TableManager.Instance.Content_Table[contentTid].Str_Error_Guide;
            _levelTabs[index].InitLock(lockTid);
          //  _levelTabs[index].SetIsLock(!isOpenLevel(index));
        }
    }

    //Todo 신준호 => 해당 씬으로 오면 현재 팀에 있는 헬퍼 제거
    private void CheckHelper()
    {
        if (_currentStageData == null)
            return;

        var infoManager = GameInfoManager.Instance;
        var teamIndex = infoManager.OrganizationInfo.GetPresetIndex(_currentStageData.CONTENTS_TYPE_ID);
        var teamData = infoManager.OrganizationInfo.GetTeam(_currentStageData.CONTENTS_TYPE_ID, teamIndex);

        //서버로 보낼 데이터
        var charList = new DosaInfo[4];

        if(teamData != null)
        {
            if (teamData.DosaInfos != null)
            {
                for (int i = 0; i< teamData.DosaInfos.Length; i++)
                {
                    if (teamData.DosaInfos[i] == null)
                    {
                        continue;
                    }

                    if (teamData.DosaInfos[i].HelperTargetKey.IsNullOrEmpty())
                    {
                        //조력자 캐릭터가 아닌 경우
                        charList[i] = teamData.DosaInfos[i];
                    }
                    else
                    {
                        charList[i] = null;
                    }
                }
            }
            teamData.DosaInfos = charList;
            infoManager.OrganizationInfo.SetTeam(_currentStageData.CONTENTS_TYPE_ID, teamIndex, teamData);
        }
    }

    private bool isOpenLevel(int index)
    {
        var laststage = _stages[index].LastOrDefault().Tid;
        bool isOpend = GameInfoManager.Instance.StageInfo.IsClear(laststage);
        if (_chapterIndex == 1 && index == 0)
            isOpend = true;

        return isOpend;
    }

    public void OnClickLevelButton(int index)
    {
        //if (!isOpenLevel(index))
        //    return;

        if (_levelIndex == index)
            return;

        if (_levelIndex != -1)
            _levelTabs[_levelIndex].DeActive();

        _levelTabs[index].Active();
        _levelIndex = index;

        _uiStarReward.Show(_stages[_levelIndex].FirstOrDefault().Tid, "reddot_story_rewards");
        SetStageNode(_levelIndex);
        OnClickCloseInfo();
    }

    public void OnClickCloseInfo()
    {
        //ResetCells();
        SetSelectUI(-1);
        if (_isOpenInfo)
        {
            _scroll.ReSizeContent(_originalScrollMinX);
            //if (_openStageId > 1)
            //    _scroll.SetCustomAnchored(70, 0);
            _isOpenInfo = false;

            _uiStageInfoSide.DeActive();
            ActiveInfoWindow(false);
            SetResizeNodeScroll();
        }
    }
    public void ResetCells()
    {

        //for (int i = 0; i < _cells.Count; i++)
        //{
        //    if (_cells[i].gameObject.activeInHierarchy)
        //        _cells[i].ActiveSelected(null);
        //}
    }

    public void SetSelectUI(int index = -1)
    {
        for (int i = 0; i < _cellDatas.Count; i++)
        {
            if (index > -1)
                _cellDatas[i].SetIsSelected(i==index);
            else
                _cellDatas[i].SetIsSelected(false);
        }


        for (int m = 0; m < _cells.Count; m++)
        {
            if (_cells[m].gameObject.activeInHierarchy)
                _cells[m].SetSelectUI();
        }
    }


    public void SetRefreshScrollUI()
    {
        SetSelectUI(-1);
        _scroll.ReSizeContent(_originalScrollMinX);
        SetResizeNodeScroll();
    }
}
