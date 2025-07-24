using Consts;
using LIFULSE.Manager;
using Pathfinding;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public partial class UIDungeon : UIBase
{

    [SerializeField] private RecycleScroll _nodeScroll;
    [SerializeField] private UIStageInfoSide _uiStageInfoSide;
    [SerializeField] private ExImage _roadRender;
    //[SerializeField] private RawImage _rawImage;
    [SerializeField] private float _bgSpeed;

    private List<RoadDungeonCell> _cells;
    private List<RoadDungeonCellData> _cellDatas = new List<RoadDungeonCellData>();

    private int _lastOpenedStageIndex = 0;

    private DateTime _endDate;
    private bool _isEnded = false;

    private UILayoutGroup[] _layoutGroupArray;
    private Coroutine _remainTimeRoutine;
    private int _tabIndex = 0;
    private float _originalScrollMin;

    private List<Stage_TableData> _stages = new List<Stage_TableData>();
    private List<Stage_TableData> _totals = new List<Stage_TableData>();

    private float _previousScrollSpeed = 0f; // 이전 스크롤 속도
    private float _adjustedBgSpeed = 0f;     // 속도 조정 값 (스크롤 속도에 맞춰 조정)

    private bool _isOpenInfo = false;
    private Stage_TableData _currentStageData = null;
    private int _lastStageId = 0;
    public override void Init()
    {
        base.Init();

        _nodeScroll.Init();
        _nodeScroll.UseDisableFocusLine = false;
        _cells = _nodeScroll.GetCellToList<RoadDungeonCell>();
        _uiStageInfoSide.Init();
    }
    public override void Display()
    {
        base.Display();
    }

    public override void Hide()
    {
        base.Hide();
    }

    public override void Refresh()
    {
        base.Refresh();
        SetCells();
        if (_isOpenInfo)
        {
            _isOpenInfo = false;
            OnClickRoadCell(_currentStageData.Stage_Id-1,_currentStageData, false);
        }
        else
            _uiStageInfoSide.InitializeAnim();
    }


    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _isOpenInfo = false;
        _totals =  TableManager.Instance.Stage_Table.GetDatas(CONTENTS_TYPE_ID.manjang);
        _lastStageId = _totals.LastOrDefault().Stage_Id;
        _isEnded = false;

        if (optionDict != null && optionDict.TryGetValue(UIOption.Data, out var currentdata))
            _currentStageData = (Stage_TableData)currentdata;

        _uiStageInfoSide.InitializeAnim();
        SetRoadBackGround();
        SetCells();

        if (_currentStageData != null)
            OnFocusCurrentRoad(_currentStageData);
    }

    public void SetRoadBackGround()
    {
        _nodeScroll.ScrollRect.onValueChanged.AddListener((value) =>
        {
            float scrollSpeed = _nodeScroll.ScrollRect.velocity.x;
            float vectorX = _nodeScroll.GetcurrentVector();
            var startIndex = _nodeScroll.GetStarIndex();
            float result = 0f;

            if (vectorX < 0)
                result =Mathf.Abs(vectorX);
            else
                result = 0;

            if (Mathf.Abs(scrollSpeed) > 15.0f)
            {
                //result = result * 0.5f;
                result = result *0.5f * 0.2f* 0.01f; // 0.5는 기존 코드, 0.2는 1/5 속도
                _roadRender.material.SetFloat("_Progress", result);
            }
        });
    }




    public void SetCells()
    {
        _stages.Clear();
        _cellDatas.Clear();
        for (int n = 0; n < _totals.Count; n++)
        {
            var stageInfo = _totals[n];
            _cellDatas.Add(new RoadDungeonCellData(stageInfo));
            _stages.Add(stageInfo);
        }
        
        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }
        _nodeScroll.ActivateCells(_totals.Count);
        _originalScrollMin = _nodeScroll.GetContentObjWidth();
        SetResizeNodeScroll();
    }


    private void OnFocusCurrentRoad(Stage_TableData data)
    {
        //if (data.Stage_Id > 1)
        //{
        //    if (data.Stage_Id >= _totals.Count - 4)
        //        SetNodeListSize(data.Stage_Id);
        //}
        //if (data.Stage_Id >= _totals.Count - 4)
        //    SetNodeListSize(data.Stage_Id);
        if(data.Stage_Id > 0)
            SetNodeListSize(data.Stage_Id);

        float padding = 0.0f;
        int targetStage = 0;
        if (data.Stage_Id != _lastStageId)
        {
            if (data.Stage_Id ==  _totals.FirstOrDefault().Stage_Id)
            {
                padding = 0.3f;
                targetStage = data.Stage_Id - 1;
            }
            else
            {
                padding = 0.3f;
                targetStage = data.Stage_Id - 2;
            }
        }
        else
        {
            padding = 0.2f;
            if(!_isOpenInfo)
                targetStage = data.Stage_Id - 4;
            else
                targetStage = data.Stage_Id - 3;
        }
        _nodeScroll.FocusLine(targetStage, padding: -_nodeScroll.GetCustomAddValue(padding), isCustom: true, isMoveSmooth: true);
       

        //if (_currentStageData.Stage_Id > 0)
        //{
        //    float padding = 0.0f;
        //    int targetStage = 0;
        //    if (data.Stage_Id != _lastStageId)
        //    {
        //        if (data.Stage_Id ==  _totals.FirstOrDefault().Stage_Id)
        //        {
        //            padding = 0.3f;
        //            targetStage = data.Stage_Id - 1;
        //        }
        //        else
        //        {
        //            padding = 0.3f;
        //            targetStage = data.Stage_Id - 2;
        //        }
        //    }
        //    else
        //    {
        //        padding = 0.3f;
        //        targetStage = data.Stage_Id - 3;
        //    }

        //    _nodeScroll.FocusLine(targetStage, padding: -_nodeScroll.GetCustomAddValue(padding), isCustom: true, isMoveSmooth: true);
        //}
    }

    public void OnClickRoadCell(int index ,Stage_TableData data, bool goCurrent = false)
    {
        _currentStageData = data;
        if (!_isOpenInfo)
        {
            _uiStageInfoSide.Active();
            _isOpenInfo = true;
        }
        _tabIndex = data.Stage_Id;
        OnFocusCurrentRoad(data);

        //_tabIndex = data.Stage_Id;
        //if (data.Stage_Id > 1)
        //{
        //    if (data.Stage_Id >= _totals.Count - 4)
        //        SetNodeListSize(data.Stage_Id);
        //}

        //if (_currentStageData.Stage_Id > 0)
        //{
        //    float padding = 0.0f;
        //    int targetStage = 0;
        //    if (data.Stage_Id != _lastStageId)
        //    {
        //        if (data.Stage_Id ==  _totals.FirstOrDefault().Stage_Id)
        //        {
        //            padding = 0.3f;
        //            targetStage = data.Stage_Id - 1;
        //        }
        //        else
        //        {
        //            padding = 0.3f;
        //            targetStage = data.Stage_Id - 2;
        //        }
        //    }
        //    else
        //    {
        //        padding = 0.3f;
        //        targetStage = data.Stage_Id - 3;
        //    }

        //    _nodeScroll.FocusLine(targetStage, padding: -_nodeScroll.GetCustomAddValue(padding), isCustom: true, isMoveSmooth: true);
        //}

        //if (data.Stage_Id != _lastStageId)
        //    _nodeScroll.FocusLine(data.Stage_Id - 2, padding: -_nodeScroll.GetCustomAddValue(0.4f), isCustom: true, isMoveSmooth: true);
        //else
        //    _nodeScroll.FocusLine(data.Stage_Id - 3, padding: -_nodeScroll.GetCustomAddValue(0.4f), isCustom: true, isMoveSmooth: true);
        

        //if (index > 0)
        //    _nodeScroll.SetCustomAnchored(70, 0);

        for (int n = 0; n < _cellDatas.Count; n++)
        {
            var no = n;
            _cellDatas[n].SetIsSelected(no == index);
        }
        for (int i = 0; i < _cells.Count; i++)
        {
            if (_cells[i].gameObject.activeInHierarchy)
                _cells[i].SetIsSelected();
        }

        _uiStageInfoSide.SetStageInfo(data);
    }

    private void SetNodeListSize(int stageid)
    {
        int index = 4;
        var lastStage = _totals.LastOrDefault().Stage_Id;
        //if (stageid < lastStage - 3)
        //    index = 1;
        //else
        //    index = 2;
        float width = 0.0f;
        var cellwidth = _nodeScroll.GetCellObjectWidth() + _nodeScroll.Spacing.x;

        if (_originalScrollMin < 0)
            width = _originalScrollMin - (cellwidth*index);
        else
            width = _originalScrollMin + (cellwidth*index);

        _nodeScroll.ReSizeContent(width);
        // _scroll.ResetContentSize(contents.x);
    }


    // 스테이지 정보 팝업에서 보여줄 현재 위치로 함수
    public void OnClickCurrentRoad()
    {
        Stage_TableData current = null;
        var type = CONTENTS_TYPE_ID.manjang;
        var clearlist = GameInfoManager.Instance.DungeonInfo.GetClearList(type);

        if (clearlist.Count > 0)
        { 
            // 클리어 리스트가 있는 경우. 
            var lastClearInfo = TableManager.Instance.Stage_Table[clearlist.LastOrDefault()];
            if (!lastClearInfo.Tid.Equals(_totals.LastOrDefault().Tid))
            {
                // 클리어 스테이지 중 마지막 스테이지가, 해당 컨텐츠의 마지막 스테이지가 아닌 경우
                var info = TableManager.Instance.Stage_Table.GetData(type, lastClearInfo.Stage_Id);
                current = info;
            }
            else
                current = lastClearInfo;
        }
        else
            current = _totals.FirstOrDefault();

        Debug.Log("<color=#85f6db>current  "+current.Tid+"("+current.Stage_Id+")</color>");

        OnClickRoadCell(current.Stage_Id-1, current, true);
    }

    private void SetResizeNodeScroll()
    {
        int index = Math.Abs(_cells.Count / 2);
       
        float width = 0.0f;
        var cellwidth = _nodeScroll.GetCellObjectWidth();
        var addWidth = Math.Abs(Screen.width / 2) +cellwidth;
        if (_originalScrollMin < 0)
            width = _originalScrollMin - (_nodeScroll.Padding.right);
        else
            width = _originalScrollMin + (_nodeScroll.Padding.right);

        _nodeScroll.ReSizeContent(width);
    }

    public void OnClickCloseInfo()
    {
        RefreshCell();

        if (_isOpenInfo)
        {

            int targetIndex = 0;
            if (_currentStageData.Stage_Id == _totals.LastOrDefault().Stage_Id)
            {
                targetIndex =_totals.LastOrDefault().Stage_Id - _nodeScroll.ShowMaxCount;

                _nodeScroll.FocusLine(targetIndex, padding: 0, isCustom: true);
                SetResizeNodeScroll();
            }

            //if (_tabIndex == _lastStageId)
            //{
            //    //_nodeScroll.ReSizeContent(_originalScrollMin);
            //    targetIndex = _lastStageId - _nodeScroll.ShowMaxCount;
            //    _nodeScroll.FocusLine(targetIndex,isCustom: true, isMoveSmooth: true);

            //    SetResizeNodeScroll();
            //}

            //_nodeScroll.Refresh();
            //_nodeScroll.UpdateContentSize(_totals.Count);
            _isOpenInfo = false;
            _uiStageInfoSide.DeActive();
        }
    }


    public void RefreshCell()
    {
        for (int n = 0; n < _cellDatas.Count; n++)
        {
            _cellDatas[n].SetIsSelected(false);
        }

        for (int i = 0; i < _cells.Count; i++)
        {
            if (_cells[i].gameObject.activeInHierarchy)
                _cells[i].SetIsSelected();
        }
    }

    #region[안씀]
    //public void UpdateData()
    //{
    //    var stageDataList = TableManager.Instance.Stage_Table.GetDatas(CONTENTS_TYPE_ID.manjang);

    //    foreach (var stageTableData in stageDataList)
    //    {
    //        _dataList.Add(new ContentBossCellData(stageTableData.Tid));
    //    }

    //    _dataList.Reverse();
    //}

    /// <summary>
    /// UI 업데이트
    /// </summary>
    //public void UpdateUI()
    //{
    //    // 레이아웃 업데이트
    //    if(_layoutGroupArray != null)
    //    {
    //        foreach(var layoutGroup in _layoutGroupArray)
    //        {
    //            layoutGroup.UpdateLayoutGroup();
    //        }
    //    }
    //}

    ///// <summary>
    ///// 스크롤 포커싱
    ///// </summary>
    ///// <param name="index">포커싱할 셀 인덱스</param>
    //public void Focus(int index)
    //{
    //    var content = _nodeScroll.ContentRect;
    //    var viewportHeight = _nodeScroll.GetComponent<RectTransform>().rect.height;
    //    var contentHeight = content.rect.height;

    //    var padding = _nodeScroll.Padding.top;
    //    var spacing = _nodeScroll.Spacing.y;
    //    var cellSize = _cellList.First().GetComponent<RectTransform>().rect.height;

    //    index = index - 2;

    //    var value = Mathf.Clamp((cellSize * index + spacing * index + padding) / (contentHeight), 0f, 1f);

    //    _nodeScroll.OnValueChagedScrollbar(value);
    //}

    ///// <summary>
    ///// 가장 마지막으로 열린 스테이지 업데이트
    ///// </summary>
    //private void UpdateLastOpenStage()
    //{
    //    var clearList = GameInfoManager.Instance.DungeonInfo.GetClearList(CONTENTS_TYPE_ID.manjang);
    //    var index = 0;

    //    if(clearList != null)
    //    {
    //        index = Mathf.Clamp(_dataList.Count - clearList.Count - 1, 0, _dataList.Count - 1);
    //    }

    //    _lastOpenedStageIndex = index;
    //}

    ///// <summary>
    ///// 타이머 UI 업데이트
    ///// </summary>
    ///// <param name="endDate"></param>
    //private void UpdateTimerUI(DateTime endDate)
    //{
    //    var timeSpan = endDate.GetRemainTime();

    //    if (_remainTimeTmp != null)
    //    {
    //        if (timeSpan.Days > 0)
    //        {
    //            _remainTimeTmp.text = "str_ui_time_count_d".ToTableArgs("str_ui_manjangcave_reset_01".ToTableText(), timeSpan.Days, timeSpan.Hours);
    //        }
    //        else if (timeSpan.Hours > 0)
    //        {
    //            _remainTimeTmp.text = "str_ui_time_count_h".ToTableArgs("str_ui_manjangcave_reset_01".ToTableText(), timeSpan.Hours);
    //        }
    //        else if (timeSpan.Minutes > 0)
    //        {
    //            _remainTimeTmp.text = "str_ui_time_count_m".ToTableArgs("str_ui_manjangcave_reset_01".ToTableText(), timeSpan.Minutes);
    //        }
    //        else
    //        {
    //            _remainTimeTmp.text = "str_ui_time_count_s".ToTableArgs("str_ui_manjangcave_reset_01".ToTableText(), timeSpan.Seconds);
    //        }
    //    }
    //}

    //private void UpdateLayer()
    //{
    //    foreach(var cell in _cellList)
    //    {
    //        cell.transform.SetAsFirstSibling();
    //    }
    //}

    ///// <summary>
    ///// 남은시간 타이머
    ///// </summary>
    ///// <param name="endDate">종료일자</param>
    ///// <returns></returns>
    //private IEnumerator Timer(DateTime endDate)
    //{
    //    while (true)
    //    {
    //        var timeSpan = endDate.GetRemainTime();
    //        WaitForSeconds waitSeconds = null;

    //        if (timeSpan.Days > 0 || timeSpan.Hours > 0)
    //        {
    //            waitSeconds = new WaitForSeconds((float)timeSpan.TotalSeconds % 3600);
    //        }
    //        else if (timeSpan.Minutes > 0)
    //        {
    //            waitSeconds = new WaitForSeconds((float)timeSpan.TotalSeconds % 60);
    //        }
    //        else
    //        {
    //            waitSeconds = new WaitForSeconds(1f);
    //        }

    //        if (timeSpan.TotalSeconds > 0)
    //        {
    //            UpdateTimerUI(endDate);
    //        }
    //        else
    //        {
    //            _remainTimeTmp.text = "종료된 시즌입니다.";
    //            _isEnded = true;
    //            yield break;
    //        }

    //        yield return waitSeconds;
    //    }
    //}
    #endregion

}
