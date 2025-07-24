using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class StoryListCellData
{
    public CONTENTS_TYPE_ID TypeID;
    public Story_Group_TableData StoryGroupData;

    public StoryListCellData(CONTENTS_TYPE_ID type, Story_Group_TableData data )
    {
        TypeID = type;
        StoryGroupData = data;
    }
}

public class StoryListCell : ScrollCell
{
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExTMPUI _numberTMP;
    [SerializeField] private ExTMPUI _numberTMP2;
    [SerializeField] private ExTMPUI _typeTMP = null;
    [SerializeField] private ExTMPUI _typeTMP2 = null;
    [SerializeField] private ExTMPUI _titleTMP;
    [SerializeField] private ExTMPUI _descTMP;
    //[SerializeField] private GameObject _dimd;
    [SerializeField] private ExButton _button;
    [SerializeField] private UIContentLcok _uiContentLock = null;



    private List<StoryListCellData> _cellDatas;

    protected override void Init()
    {
        base.Init();
    }

    public void SetCellDatas(List<StoryListCellData> dataList)
    {
        _cellDatas = dataList;
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;

        UpdateUI();
    }

    // RecycleScroll 에서 셀로 쓰일 때 UI, info부분은 안보임.
    private void UpdateUI()
    {
        // 스토리 챕터cell 잠금

        SetInfoUI(false);
        SetChapterIsLocked(true);
        this.gameObject.SetActive(_cellDatas[DataIndex].StoryGroupData.Story_Disable == 0);
        SetIcon(_cellDatas[DataIndex].StoryGroupData.Chapter_Icon_Id);
        SetNumberIndex(_cellDatas[DataIndex].StoryGroupData.Str_Chapter);
        SetTypeIndex(_cellDatas[DataIndex].StoryGroupData.Chapter_Str);
        _button.onClick.AddListener(() => OnClickStoryCell());
    }

    //  RecycleScroll 이외에 사용 될 때. Info부분 보임
    public void UpdateInfoUII(Story_Group_TableData data)
    {
        SetInfoUI(true);
        SetChapterIsLocked(false);
        SetInfoData(data.Str_Chapter_Title_Name, data.Str_Chapter_Summarize);
        SetIcon(data.Chapter_Icon_Id);
        SetNumberIndex(data.Str_Chapter);
        SetTypeIndex(data.Chapter_Str);
    }
    // 잠금 체크 :: 리스트에서 보여 줄 때만 체크
    private void SetChapterIsLocked(bool isCheck)
    {
        if (_uiContentLock == null)
            return;

        if (isCheck)
        {
            bool isLocked = false;
            if (string.IsNullOrEmpty(_cellDatas[DataIndex].StoryGroupData.Open_Value_Tid))
                _uiContentLock.gameObject.SetActive(false);
            else
                _uiContentLock.ContentStateUpdate(_cellDatas[DataIndex].StoryGroupData.Open_Value_Tid);


            if (_uiContentLock.LockBtn.gameObject.activeInHierarchy)
                _button.gameObject.SetActive(!_uiContentLock.IsOpen);
            else
                _button.gameObject.SetActive(true);
        }
        else
            _uiContentLock.ContentStateUpdate();
        //_dimd.SetActive(false);
    }

    private void SetIcon(string name)
    {
        _icon.SetSprite(name);
    }
    private void SetNumberIndex(string index)
    {
        _numberTMP.ToTableText(index);
        _numberTMP2.ToTableText(index);
    }
    private void SetTypeIndex(string index)
    {
        if (_typeTMP == null && _typeTMP2 == null)
            return;

        _typeTMP.ToTableText(index);
        _typeTMP2.ToTableText(index);
    }

    private void SetInfoUI(bool isOn)
    {
        _titleTMP.gameObject.SetActive(isOn);
        if (_descTMP != null)
            _descTMP.gameObject.SetActive(isOn);
        
    }
    private void SetInfoData(string title,string info)
    {
        _titleTMP.ToTableText(title);
        if (_descTMP != null)
            _descTMP.ToTableText(info);
        Debug.Log("<color=#be29ec>_decsTMP.IsUseConvertText("+_descTMP.IsUseRubyText+")</color>");
        if (_descTMP.gameObject.activeInHierarchy && _descTMP.IsUseRubyText)
        {
            Debug.Log("<color=#be29ec>_decsTMP.preferredHeightt("+_descTMP.preferredHeight+")</color>");
            //CheckAndAddScroll();
        }
    }

    //bool 체크되어있고
    //텍스트 변경됐을 때 호출되는 이벤트 => 넘쳤어


    public void OnClickStoryCell()
    {
       var list = TableManager.Instance.Stage_Table.GetStoryDatas(_cellDatas[DataIndex].TypeID, _cellDatas[DataIndex].StoryGroupData.Theme_Id);

        var isOpenStage = list.FirstOrDefault();

        for (int n = 0; n < list.Count; n++)
        {
            if (!list[n].Tid.Equals(list.FirstOrDefault().Tid)&& !GameInfoManager.Instance.StageInfo.IsClear(list[n].Tid) && GameInfoManager.Instance.StageInfo.IsClear(list[n-1].Tid))
            {
                isOpenStage = list[n];
                break;
            }
        }

        UIManager.Instance.Show<UIStoryInfo>(Utils.GetUIOption(
            UIOption.Data, _cellDatas[DataIndex].StoryGroupData,
            UIOption.EnumType, _cellDatas[DataIndex].TypeID,
            UIOption.Data2, isOpenStage
            ));
     }

    public void OnClickStoryDimd()
    {
        //var message = LocalizeManager.Instance.GetString(_cellDatas[DataIndex].StoryGroupData.Str_Error_Message);
        //UIManager.Instance.ShowToastMessage(message);
    }

    private void CheckAndAddScroll()
    {

        //if (_decsScrollRect == null)
        //    CreateScrollRect();
        // TMP_Text의 preferredHeight를 가져옵니다.
        //float textHeight = _decsTMP.preferredHeight;
        //float containerHeight = _decsTMP.gameObject.GetComponent<RectTransform>().rect.height;

        //if (textHeight > containerHeight)
        //{
        //    _decsScrollRect.movementType = ScrollRect.MovementType.Elastic;
        //}
        //else
        //{
        //    _decsScrollRect.movementType = ScrollRect.MovementType.Clamped;
        //}

        // 텍스트가 컨테이너 높이를 초과하는지 확인
        //if (textHeight > containerHeight)
        //{
        //    // ScrollRect가 없으면 추가합니다.
        //    if (_decsScrollRect == null)
        //    {
        //        CreateScrollRect();
        //    }
        //}
        //else
        //{
        //    // 텍스트가 길지 않으면 ScrollRect를 비활성화
        //    if (_decsScrollRect != null)
        //    {
        //        _decsScrollRect.gameObject.SetActive(true);
        //    }
        //}
    }

    //private void CreateScrollRect()
    //{
    //    RectTransform descRect = _decsTMP.gameObject.GetComponent<RectTransform>();

    //    // 부모에 ScrollRect를 감쌀 새로운 GameObject를 생성
    //    GameObject scrollObj = new GameObject("ScrollRectParent");
    //    scrollObj.transform.SetParent(_decsTMP.gameObject.GetComponent<RectTransform>().parent); // 부모에 추가

    //    // ScrollRect 컴포넌트를 추가
    //    _decsScrollRect = scrollObj.AddComponent<ScrollRect>();

    //    // RecT2D 컴포넌트 추가.
    //    scrollObj.AddComponent<RectMask2D>();
    //    var rectMask = scrollObj.GetComponent<RectMask2D>();
    //    rectMask.softness = new Vector2Int(10, 10);

    //    // ScrollRect 설정
    //    _decsScrollRect.horizontal = false; // 수평 스크롤을 비활성화
    //    _decsScrollRect.vertical = true; // 수직 스크롤 활성화

    //    //RectTransform tmpRectTransform = _decsTMP.gameObject.GetComponent<RectTransform>();
    //    RectTransform tmpRectTransform = _decsTMP.gameObject.GetComponent<RectTransform>();

    //    RectTransform scrollRectTransform = _decsScrollRect.GetComponent<RectTransform>();
    //    scrollRectTransform.anchorMin = tmpRectTransform.anchorMin;
    //    scrollRectTransform.anchorMax = tmpRectTransform.anchorMax;
    //    scrollRectTransform.pivot = tmpRectTransform.pivot;
    //    scrollRectTransform.sizeDelta = tmpRectTransform.sizeDelta;
    //    scrollRectTransform.anchoredPosition = tmpRectTransform.anchoredPosition;

    //    _decsScrollRect.content = tmpRectTransform;

    //    // ScrollRect의 Viewport 설정
    //    GameObject viewportObj = new GameObject("Viewport");
    //    viewportObj.transform.SetParent(scrollObj.transform);
    //    RectTransform viewportTransform = viewportObj.AddComponent<RectTransform>();

    //    //decsTMP.gameObject.transform.SetParent(viewportObj.transform, false);
    //    _decsTMP.transform.SetParent(viewportObj.transform, false);

    //    viewportTransform.anchorMin = new Vector2(0, 0); // 아래 왼쪽
    //    viewportTransform.anchorMax = new Vector2(1, 1); // 위 오른쪽
    //    viewportTransform.pivot = new Vector2(1, 1); // 중앙에 피벗
    //    viewportTransform.sizeDelta = Vector2.zero; // 사이즈는 부모에 의해 stretch 될 것
    //    viewportTransform.offsetMin =  Vector2.zero;     // left, bottom
    //    viewportTransform.offsetMax =  Vector2.zero;     // right,  top

    //    // 카피한 텍스트 영역 설정
    //    _decsTMP.gameObject.SetActive(true);
    //    ContentSizeFitter descSizeFilter = _decsTMP.gameObject.AddComponent<ContentSizeFitter>();
    //    descSizeFilter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
    //    descRect.anchorMin = new Vector2(0, 1);
    //    descRect.anchorMax = new Vector2(1, 0);
    //    descRect.pivot = new Vector2(0.5f, 0.5f);

    //    viewportTransform.anchoredPosition = scrollRectTransform.anchoredPosition;

    //    // Viewport에 Image 추가
    //    Image viewportImage = viewportObj.AddComponent<Image>();
    //    viewportImage.color = new Color(0, 0, 0, 0); // 투명하게 설정

    //    // ScrollRect의 Viewport 필드에 할당
    //    _decsScrollRect.viewport = viewportTransform;

    //    // ScrollRect가 활성화되도록 설정
    //    _decsScrollRect.enabled = true;

    //    tmpRectTransform.anchorMin = new Vector2(0, 0);
    //    tmpRectTransform.anchorMax = new Vector2(1, 1);
    //    tmpRectTransform.pivot = new Vector2(0.5f, 0.5f); // 텍스트가 위로 정렬되도록 설정
    //    tmpRectTransform.anchoredPosition = Vector2.zero;
    //}
}
