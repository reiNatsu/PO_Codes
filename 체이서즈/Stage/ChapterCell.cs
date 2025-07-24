using Consts;
using LIFULSE.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor.ShaderGraph;
#endif
using UnityEngine;
using UnityEngine.UI;

public class ChapterCellData
{
    [SerializeField] public Content_TableData Data;
    [SerializeField] public string Tid;
    [SerializeField] public int ChapterID;
    [SerializeField] public LEVEL_DIFFICULTY Level;
   

    public ChapterCellData(Content_TableData data)
    {
        Data = data;
        Tid = data.Tid;
        ChapterID =data.Content_Chapter_Id;
        Level = (LEVEL_DIFFICULTY)data.Content_Chapter_Group;
      
    }
}

// 스테이지 페이지에서 보이는 가로형 스크롤바 아이템
public class ChapterCell : MonoBehaviour
{
    [SerializeField] private ExButton _chapterBtn;

    [Header("Lock")]
    [SerializeField] private UIContentLcok _uiContentLock;

    [Header("RedDot Key")]
    [SerializeField] private UIRedDot _uiReddot;
    [SerializeField] private ChapterItem _chapterItem;
  
    private int _chapterNo= 0;
    private int _stageNo;
    private LEVEL_DIFFICULTY _chapterLevel;
    private List<Content_TableData> _chapterContentData = new List<Content_TableData>();
    private Content_TableData _data;

    private List<ChapterCellData> _cellDatas;

    public ExButton Button { get => _chapterBtn; }

    private void Awake()
    {
        SetDimed(true);
    }

    public void Init(List<Content_TableData> datas, int no, LEVEL_DIFFICULTY level)
    {
        _data = datas.FirstOrDefault(); 
        SetChapterCellData();
    }

    public void SetChapterCellData()
    {
        _chapterNo = _data.Content_Chapter_Id;
        _chapterLevel = (LEVEL_DIFFICULTY)_data.Content_Chapter_Group;
        _uiReddot.UpdateRedDot("reddot_story_chapter", _chapterNo-1);

        GameInfoManager.Instance.CurrentChapter = _chapterNo;
           UpdateUI();
    }

    private void UpdateUI()
    {
        SetIsOpenChapter();
        _chapterItem.InitData(_data, OnClickChapterDimmed);

        //var titleStr = LocalizeManager.Instance.GetString(_data.Str).Split(".")[1];
        // 게이지 
        var total = GameInfoManager.Instance.GetAllRewardStarCount(_chapterNo);
        var get = GameInfoManager.Instance.GetRewardStarCount(_chapterNo);
    }

    private void SetIsOpenChapter()
    {
        if (_chapterNo > 1)
        {
            if (string.IsNullOrEmpty(_data.Str_Error_Guide))
                _uiContentLock.ContentStateUpdate();
            else
                _uiContentLock.ContentStateUpdate(_data.Str_Error_Guide);
            //var openValue = TableManager.Instance.Stage_Table.GetMainStageListByLevel(_chapterNo-1, CONTENTS_TYPE_ID.stage_main, LEVEL_DIFFICULTY.normal).LastOrDefault().Tid;
            //// if (GameInfoManager.Instance.StageInfo.IsClear(openValue))
            //SetDimed(!GameInfoManager.Instance.StageInfo.IsClear(openValue));
        }
        else
            _uiContentLock.ContentStateUpdate();
            //SetDimed(false);
    }

    public void OnClickChpaterCell()
    {
        var enterlevel = LEVEL_DIFFICULTY.normal;
        var enterstage = 1;
        Stage_TableData laststageData = TableManager.Instance.Stage_Table.GetData(_chapterNo, enterstage, enterlevel);
        if (GameInfoManager.Instance.StageInfo.ClearStageList.ContainsKey(_chapterNo)
            && GameInfoManager.Instance.StageInfo.ClearStageList[_chapterNo].Count >0)
        {
            enterlevel = GameInfoManager.Instance.StageInfo.ClearStageList[_chapterNo].Keys.LastOrDefault();
            laststageData = TableManager.Instance.Stage_Table[GameInfoManager.Instance.StageInfo.ClearStageList[_chapterNo][enterlevel].LastOrDefault()];
        }

        UIManager.Instance.Show<UIStage>(Utils.GetUIOption(
            UIOption.Index, _chapterNo
            ,UIOption.Data, laststageData,
            UIOption.Bool, true));
        
    }

    public void OnClickChapterDimmed()
    {
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK,
                 LocalizeManager.Instance.GetString("str_ui_info_popup_title"),
                 SetIsNotOpenIndex());
    }
    private string SetIsNotOpenIndex()
    {
        var  titledata = _chapterContentData = TableManager.Instance.Content_Table.GetDataByChapter(_chapterNo-1);

        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(LocalizeManager.Instance.GetString(titledata[(int)_chapterLevel].Str.ToString()));
        sb.Append("] ");
        sb.Append("str_content_total_difficulty_guide_02".ToTableText()); //클리어 시 해금

        return sb.ToString();

    }

    public void SetDimed(bool isOn)
    {
        _chapterItem.SetChapterItemDimed(isOn);
    }
}
