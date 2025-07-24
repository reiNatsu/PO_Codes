using Consts;
using LIFULSE.Manager;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

public class UIChapter : UIBase
{

    //[BoxGroup("[UI_Chapter]")]
    //[SerializeField] private string _reddotKey;         // 스토리 레드닷은 햄버거 메뉴, 로비 메뉴에 안띄워주기 때문에 string으로 받음. 
   // [SerializeField] public RecycleScroll _scroll;

    [Header("[UI_Chapter]")]
    [SerializeField] public ScrollRect _scrollRect;
    [SerializeField] private GameObject _chapterObject;         // 챕터 
    [SerializeField] private Transform _chapterContent;
    [SerializeField] private RectTransform _contentRect;

    [SerializeField] private BoxCollider2D _boxColider;

    [Header("[Pagigng]")]
    [SerializeField] private GameObject _pagingRect;
    [SerializeField] private RectTransform _pagingContent;
    [SerializeField] private ExButton _arrowRight;
    [SerializeField] private ExButton _arrowLeft;

    private List<int> _stageList = new List<int>();

    //private List<ChapterCell> _chapterCellList = new List<ChapterCell>();
    private Dictionary<int, ChapterCell> _chaptersDic= new Dictionary<int, ChapterCell>();// 챔터 넘버, 챕터Cell 아이템 저장
   
    private List<PagingRectItem> _pagingRectList = new List<PagingRectItem>();

    //private List<ChapterCell> _chapterCells;
    //private List<ChapterCellData> _cellDatas = null;

    private int _curNaviIndex = 0;
    private int _depth = 0;
    private SavedStoryStageData _savedData = null;
    public int ClickedChpater { get; set; } = 0;

    public int CurrentChapter
    {
        get => GameInfoManager.Instance.CurrentChapter;
        set => GameInfoManager.Instance.CurrentChapter = value;
    }
    public LEVEL_DIFFICULTY StageDifficulty
    {
        get => GameInfoManager.Instance.StageDifficulty;
        set => GameInfoManager.Instance.StageDifficulty = value;
    }

    private void OnDisable()
    {
        for (int n = 0; n < _pagingContent.childCount; n++)
        {
            _pagingContent.GetChild(n).gameObject.GetComponent<PagingRectItem>().PagingOff();
        } 
    }

    public override void Init()
    {
        base.Init();

        //_scroll.InitData();
       
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        // SettingData();

         UpdateUI();

        //_chapterCells = _scroll.GetCellToList<ChapterCell>();
        //_cellDatas = new List<ChapterCellData>();

        //SetChapterCellList();
    }
    public void UpdateUI()
    {
        SetChapterCellList();
        //SetChaptersUI();
        //SetOpenChapterDimd();
        SetFocusChapter();
    }

    private void SetFocusChapter()
    {
        if (ClickedChpater == 0)
        {
            if (DSPlayerPrefs.HasKey("LastEnterdStage"))
            {
                //_curNaviIndex = DSPlayerPrefs.GetInt(SaveUserStoryData.SaveStoryChapter);
                _savedData = GameInfoManager.Instance.GetSavedData();
                _curNaviIndex = _savedData.ChapterNo;
            }
            else
            {
                _savedData = null;
                if (GameInfoManager.Instance.StageInfo.ClearStageList.Count > 0)
                {
                    //_curNaviIndex = GameInfoManager.Instance.StageInfo.ClearStageList.LastOrDefault().Key;
                  
                    _curNaviIndex = GameInfoManager.Instance.GatCheckChapterIsOpend(CONTENTS_TYPE_ID.stage_main).LastOrDefault();
                }
                else
                {
                    _curNaviIndex = 1;
                }
            }
        }
        else
        {
            _curNaviIndex = ClickedChpater;
        }
      
        Debug.Log("<color=#9efc9e>UIChapter _curNaviIndex is <b>"+_curNaviIndex+"</b></color>");
        FocusChapterCell(_curNaviIndex);
    }

    public void SetChapterCellList()
    {
       
        _stageList = TableManager.Instance.Stage_Table.GetMainChapterList(CONTENTS_TYPE_ID.stage_main);

        for (int n = 0; n < _stageList.Count; n++)
        {
            if (!_chaptersDic.ContainsKey(_stageList[n]))
            {
                GameObject go = GameObject.Instantiate(_chapterObject, _chapterContent);
                ChapterCell uiChapterCell = go.GetComponent<ChapterCell>();

                uiChapterCell.gameObject.name  = "Chapter_"+(n+1);
                List<Content_TableData> contentInfo = TableManager.Instance.Content_Table.GetDataByChapter(_stageList[n]);
                uiChapterCell.Init(contentInfo, _stageList[n], LEVEL_DIFFICULTY.normal);
                //  _chapterCellList[n].InitData(contentInfo, _stageList[n], LEVEL_DIFFICULTY.normal);        // 일단 normal로 보이도 나중에 수정

                _chaptersDic.Add(_stageList[n], uiChapterCell);
            }
            else
            {
                _chaptersDic[_stageList[n]].SetChapterCellData();
            }
        }
    }

    public void SettingData()
    {
        //페이징
        for (int n = 0; n < _pagingContent.childCount; n++)
        {
            _pagingRectList.Add(_pagingContent.GetChild(n).GetComponent<PagingRectItem>());
        }
        //for (int m = 0; m < _chapterContent.childCount; m++)
        //{
        //    _chapterCellList.Add(_chapterContent.GetChild(m).GetComponent<ChapterCell>()); 
        //}
    }

    private void SetOpenChapterDimd()
    {
        foreach (var item in _chaptersDic)
        {
            // TODO Cheat : 스테이지 올 클리어 체크
            if (GameInfoManager.Instance.IsAllOpen)
            {
                _chaptersDic[item.Key].SetDimed(false);
            }
            else
            {
                var openNoList = GameInfoManager.Instance.GatCheckChapterIsOpend(CONTENTS_TYPE_ID.stage_main);
                _chaptersDic[item.Key].SetDimed(true);
                for (int n = 0; n < openNoList.Count; n++)
                {
                    if (openNoList[n] != openNoList.LastOrDefault())
                    {
                        _chaptersDic[openNoList[n]].SetDimed(false);
                    }
                    _chaptersDic[openNoList[n]].SetDimed(false);
                }

            }
        }
    }

    public void OnBeginDrag()
    {
        // 드래그 시작 시 필요한 처리
        Debug.Log("Drag Started");
    }

    public void OnEndDrag(/*PointerEventData eventData*/)
    {
        // 드래그 종료 시 필요한 처리
        Debug.Log("Drag Ended");
        //FocusClosestItem(); // 사용자가 드래그를 끝냈을 때, 가장 가까운 아이템으로 포커스를 이동
    }

    public void OnValueChanged()
    {
        //if (_scrollRect.velocity.magnitude <= 1000)
        //{
        //    FocusClosestItem();
        //}
    }

    public void FocusClosestItem()
    {
        var contentPos = Mathf.Abs(_contentRect.anchoredPosition.x);
        var cellSpacing = _contentRect.GetComponent<HorizontalLayoutGroup>().spacing;
        var cellWidth = _chapterObject.GetComponent<RectTransform>().sizeDelta.x;
        var totalWidth = cellWidth + cellSpacing;
        var pos = Mathf.RoundToInt(contentPos / totalWidth);

        // _chaptersDic의 Key는 1부터 시작하므로, pos를 조정
        pos += 1; // Dictionary의 인덱스 조정
        pos = Mathf.Clamp(pos, 1, _chaptersDic.Count); // 범위 내로 제한
       
        FocusChapterCell(pos);
    }

    public void FocusChapterCell(int chapterNo)
    {
    
        if (_chaptersDic.TryGetValue(chapterNo, out ChapterCell chapterCell))
        {
            var spaceVal = _contentRect.GetComponent<HorizontalLayoutGroup>().spacing;
            var cellWidth = chapterCell.GetComponent<RectTransform>().sizeDelta.x;
            var leftval = _contentRect.GetComponent<HorizontalLayoutGroup>().padding.left;
            var rightval = _contentRect.GetComponent<HorizontalLayoutGroup>().padding.right;
            //var movePosX = (cellWidth + spaceVal) * (chapterNo - 1); // chapterNo는 1부터 시작하므로 인덱스 조정
            //var movePosX = (cellWidth+spaceVal) * (chapterNo-1); // chapterNo는 1부터 시작하므로 인덱스 조정
            var movePosX = (cellWidth * (chapterNo-1)) + (spaceVal * (chapterNo-2));
            Debug.Log("<color=#9efc9e>Focus Chapter Cell("+chapterNo+") movePosX =("+cellWidth+"*"+(chapterNo-1)+") + ("+spaceVal+"*"+(chapterNo-2)+") = "+movePosX+"</color>");
            // 부드럽게 이동하지 않고, 즉시 이동
            _contentRect.anchoredPosition = new Vector2(-movePosX, _contentRect.anchoredPosition.y);
        }
    }

    public void FocusPaging(int num)
    {
        for (int n = 0; n < _pagingRectList.Count; n++)
        {
            if (_pagingRectList[n].IsOn)
            {
                _pagingRectList[n].PagingOff();
            }
        }

        _curNaviIndex = num;
        var pagingspaceval = _pagingContent.GetComponent<HorizontalLayoutGroup>().spacing;
        var pcellsize = _pagingRectList[num].gameObject.GetComponent<RectTransform>().sizeDelta;
        var pmoveposX = (pcellsize.x * (num)) + (pagingspaceval * num);
       
        // 페이징 포커싱
        if (pmoveposX >= _pagingContent.parent.GetComponent<RectTransform>().sizeDelta.x)
        {
            _pagingContent.anchoredPosition = new Vector2(-pmoveposX, 0);
        }
        else
        {
            _pagingContent.anchoredPosition = new Vector2(0, 0);
        }
      
        if (num > 0)
        {
            _arrowLeft.enabled = true;
            _pagingRectList[num].PagingOn();
            _pagingRectList[num-1].PagingOff();
            _pagingRectList[num+1].PagingOff();
        }
        else
        {
            _arrowLeft.enabled = false;
            _pagingRectList[num].PagingOn();
            _pagingRectList[num+1].PagingOff();
        }
    }
    public void OnClickLeftArrow()
    {
        if (_curNaviIndex > 0)
        {
            _arrowLeft.enabled = true;
            _arrowRight.enabled = true;
            _curNaviIndex--;
            FocusChapterCell(_curNaviIndex);
        }
        else
        {
            _arrowRight.enabled = true;
            _arrowLeft.enabled = false;
        }
    }
    public void OnClickRighrArrow()
    {
        if (_curNaviIndex <_stageList.Count-1)
        {
            _arrowRight.enabled = true;
            _arrowLeft.enabled = true;
            _curNaviIndex++;
            FocusChapterCell(_curNaviIndex);
        }
        else
        {
            _arrowLeft.enabled = true;
            _arrowRight.enabled = false;
        }
    }

    

    public override void Refresh()
    {
        base.Refresh();
        Debug.Log("UIoChapter Refresh()");
        UpdateUI();
        //foreach (var chapters in _chaptersDic)
        //{
        //    chapters.Value.SetChapterCellData();
        //}
        //diffDropDown.enabled = true;
    }
    public override void GoBack()
    {
        if (_depth == 0)
            Close();
        else if (_depth == 1)
        {
            //StageMain.SetActive(false);
            //ChapterMain.SetActive(true);
            //diffDropDown.enabled = true;
            _depth = 0;
        }
    }

    public override void Close(bool needCached = true)
    {
        //_pagingRectList.Clear();
        //_chapterCellList.Clear();

        base.Close(needCached);
    }
}
