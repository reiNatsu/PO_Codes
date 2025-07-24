using Consts;
using LIFULSE.Manager;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class QuestCellData
{
    [SerializeField] public QuestData QuestData;

    public QuestCellData(QuestData questdata)
    {
        QuestData = questdata;
    }
}

public class QuestCell : ScrollCell
{
    [Header("[ UI ]")]
    [SerializeField] private ExTMPUI _tagText;
    [SerializeField] private ExTMPUI _questTitle;
    [SerializeField] private ExTMPUI _questSubTitle;

    [Header("[ Target UI ]")]
    [SerializeField] private ExTMPUI _goalValue;
    [SerializeField] private ExTMPUI _currentValue;
    [SerializeField] private Slider _targetSlider;

    [Header("[ UI Layout Group ]")]
    [SerializeField] private UILayoutGroup _uiLayoutGroup;
    [SerializeField] private List<ItemCell> _rewardItemList;

    [Header("[ Quest State Buttons ]")]
    [SerializeField] private GameObject _progressBtn;
    [SerializeField] private GameObject _receiveBtn;

    [Header("[ Quest Reward UI ]")]
    [SerializeField] private GameObject _cellObj;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private HorizontalLayoutGroup _layout;


    [SerializeField] private GameObject _dim;

    private Quest_TableData _data;
    private List<QuestCellData> _cellDatas;

    // 보상 아이템 RecycleScroll
    private List<ItemCell> _itemCells;

    private int _questIndex = 0;
    private QUEST_TYPE _questType;
    private RewardState _rewardState;

    public int GetQuestIndex()
    {
        return _questIndex;
    }

    protected override void Init()
    {
        base.Init();

        _itemCells = new List<ItemCell>();
        _itemCells.Add(_cellObj.GetComponent<ItemCell>());

        Utils.SetResolutionUI(this.gameObject);
    }


    public void SetCellDatas(List<QuestCellData> datas, QUEST_TYPE type)
    {
        _cellDatas = datas;
        _questType = type;

        // _tagText.ToTableText(GameInfoManager.Instance.ReturnQuestTypeString(_questType));
       
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;

        UpdateData();
        UpdateSliderValue();

        CheckQuestStatus((RewardState)_cellDatas[DataIndex].QuestData.Status);
        if (this.gameObject.activeInHierarchy)
        {
            
        }
        
        UpdateUI();
    }

    private void UpdateData()
    {
        if (TableManager.Instance.Quest_Table[_cellDatas[DataIndex].QuestData.Tid] == null)
        {
            Debug.LogError("퀘스트 테이블 데이터 null " + _cellDatas[DataIndex].QuestData.Tid);
        }
        else
        {
            _data = TableManager.Instance.Quest_Table[_cellDatas[DataIndex].QuestData.Tid];
        }

        _rewardState = (RewardState)_cellDatas[DataIndex].QuestData.Status;
        _tagText.ToTableText(GameInfoManager.Instance.ReturnQuestTypeString(_data.QUEST_TYPE));
    }

    private void UpdateSliderValue()
    {
        _targetSlider.minValue = 0;
        _targetSlider.maxValue = _data.Completed_Value;
        _targetSlider.value = _cellDatas[DataIndex].QuestData.Value;
        //Debug.Log("<color=#9efc9e>["+(_cellDatas[DataIndex].QuestData.Tid)+"] Quest Slider Value "+_cellDatas[DataIndex].QuestData.Value+"/"+ _data.Completed_Value)+"</color>");
    }

    private void UpdateUI()
    {
        var cellData = _cellDatas[DataIndex];
        if (Utils.ToItemCellDatas(_data.Quest_Reward) == null)
        {
            Debug.Log("<color=#9efc9e>reward is Null = "+_data.Tid+"</color>");
            //return;
        }
        var rewarddata = Utils.ToItemCellDatas(_data.Quest_Reward);

        if (rewarddata == null || rewarddata.Count == 0)
        {
            Debug.Log("퀘스트 리워드 없음 " + _data.Quest_Reward);
            return;
        }

        if (rewarddata.Count > _itemCells.Count)
            CreateCell(rewarddata.Count - _itemCells.Count);

        UpdateReward(rewarddata);

        _currentValue.text = string.Format("{0:#,0}", _cellDatas[DataIndex].QuestData.Value);

        // _questTitle.ToTableText(_data.Str_Quest);
        SetQuestTitleUI();   // 퀘스트 제목 표기
        SetQuestSubTItleUI();       // 퀘스트 서브 타이틀(행동목표) 표기

        _goalValue.text = string.Format("{0:#,0}" ,_data.Completed_Value);

        _uiLayoutGroup.UpdateLayoutGroup();
      //  UpdateRewardItemList();
    }

    private void SetQuestTitleUI()
    {
       
        switch (_data.QUEST_TYPE)
        {
            case QUEST_TYPE.feat:
                _questTitle.ToTableText(_data.Str_Quest, _data.Quest_Type_Value);
                break;
            default:
                _questTitle.ToTableText(_data.Str_Quest);
                break;
        }
    }

    private void SetQuestSubTItleUI()
    {
        if (_data.Quest_Condition_Type.Equals(QUEST_CONDITION_TYPE.Stage.ToString()))
        {
            if (!string.IsNullOrEmpty(_data.Quest_Condition_Value) && TableManager.Instance.Stage_Table[_data.Quest_Condition_Value] != null)
            {
                var stageData = TableManager.Instance.Stage_Table[_data.Quest_Condition_Value];
                StringBuilder sb = new StringBuilder();
                switch (stageData.CONTENTS_TYPE_ID)
                {
                    case CONTENTS_TYPE_ID.stage_main:
                       
                        sb.Append(stageData.LEVEL_DIFFICULTY.ToConvertLevel());
                        sb.Append(" ");
                        sb.Append(stageData.Theme_Id);
                        sb.Append("-");
                        sb.Append(stageData.Stage_Id);
                        _questSubTitle.ToTableText(_data.Str_Desc_Quest, sb.ToString());
                        break;
                    case CONTENTS_TYPE_ID.stage_sub:
                        sb.Append(stageData.LEVEL_DIFFICULTY.ToConvertLevel());
                        sb.Append(" ");
                        sb.Append(stageData.Theme_Id);
                        sb.Append("-");
                        sb.Append(stageData.Stage_Id);
                        sb.Append(" sub");
                        sb.Append(stageData.Extra_Id);
                        _questSubTitle.ToTableText(_data.Str_Desc_Quest, sb.ToString());
                        break;
                    default:
                        _questSubTitle.ToTableText(_data.Str_Desc_Quest, stageData.Stage_Id);
                        break;
                }
                //var stageno = TableManager.Instance.Stage_Table.GetStageNumber(stageData.Theme_Id, stageData.LEVEL_DIFFICULTY, stageData.LEVEL_TYPE, stageData.Tid);
                //_questSubTitle.ToTableText(_data.Str_Desc_Quest, stageData.Theme_Id, stageno);
            }
        }
        else
            _questSubTitle.ToTableText(_data.Str_Desc_Quest, string.Format("{0:#,0}", _data.Completed_Value));
    }

    private void CreateCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_cellObj, _layout.transform);
            var itemCell = newObj.GetComponent<ItemCell>();

            _itemCells.Add(itemCell);
        }
    }

    private void UpdateReward(List<ItemCellData> itemCellDatas)
    {
        if (itemCellDatas.Count > 3)
        {
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _layout.padding.right = 30;
        }
        else
        {
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _layout.padding.right = 0;
        }

        for (int i = 0; i < _itemCells.Count; i++)
        {
            if (i < itemCellDatas.Count)
                _itemCells[i].UpdateData(itemCellDatas[i].Tid, itemCellDatas[i].ItemType, itemCellDatas[i].CustomValueType, itemCellDatas[i].RewardAmount);
            else
                _itemCells[i].gameObject.SetActive(false);
        }
    }

    private void UpdateRewardItemList()
    {
        
        //var rewardLists = TableManager.Instance.Reward_Table.GetDatas(_data.Quest_Reward);

        //for (int n = 0; n< _rewardItemList.Count; n++)
        //{
        //    if (n < rewardLists.Count)
        //        _rewardItemList[n].SetStageList(rewardLists[n].Item_Tid, rewardLists[n].ITEM_TYPE, ItemCustomValueType.RewardAmount, rewardLists[n].Item_Min);
        //    else
        //        _rewardItemList[n].gameObject.SetActive(false);
        //}
    }

    private void CheckQuestStatus(RewardState state)
    {
        // 퀘스트 상태  0: 진행중 1: 성공 2: 수령
        switch (state)
        {
            case RewardState.DeActive:         // 진행중
                {
                    _dim.SetActive(false);
                    SetButtonStateUI(false);
                    SetRecivedRewardsUI(false);
                }
                //787878
                break;
            case RewardState.Active:           // 성공
                {
                    _dim.SetActive(false);
                    SetButtonStateUI(true);
                    SetRecivedRewardsUI(false);
                }
                break;
            case RewardState.Received:          // 수령
                {
                    _dim.SetActive(true);
                    SetButtonStateUI(true);

                    SetRecivedRewardsUI(true);
                }
                break;
        }
    }
    private void SetRecivedRewardsUI(bool isOn)
    {
        for (int n = 0; n< _rewardItemList.Count; n++)
        {
            //_rewardItemList[n].SetActiveDimmed(isOn, false);
            _rewardItemList[n].UpdateDimmedImage(isOn);
        }
    }
    private void SetButtonStateUI(bool isRecive)
    {
        _progressBtn.SetActive(!isRecive);
        _receiveBtn.SetActive(isRecive);
    }

    public void OnClickRewardButton()
    {
        if (_data.QUEST_TYPE == QUEST_TYPE.feat)
        {
            var clanId = GameInfoManager.Instance.ClanInfo.GetUserClanId();

            RestApiManager.Instance.RequestQuestReward(_data.Group_Id, _data.QUEST_TYPE, clanId, eventid: null, callBack:(result) =>
            {
                UIQuest uiQuest = UIManager.Instance.GetUI<UIQuest>();
                if (uiQuest != null && uiQuest.isActiveAndEnabled)
                {
                    uiQuest.SetQuestEvent(result["result"]["reward"]);
                }
                uiQuest.UpdateTab(uiQuest.SelectedTab);
                GameInfoManager.Instance.EventInfo.UpdatePassReddot();
            });
        } 
        else
        {
            List<string> rewardids = new List<string>();
            var clanId = GameInfoManager.Instance.ClanInfo.GetUserClanId();

            RestApiManager.Instance.RequestQuestReward(_cellDatas[DataIndex].QuestData.Tid, _data.QUEST_TYPE, clanId, eventid: null, callBack: (result) =>
            {
                UIQuest uiQuest = UIManager.Instance.GetUI<UIQuest>();
                if (uiQuest != null && uiQuest.isActiveAndEnabled)
                {
                    uiQuest.SetQuestEvent(result["result"]["reward"]);
                }
                uiQuest.UpdateTab(uiQuest.SelectedTab);
                GameInfoManager.Instance.EventInfo.UpdatePassReddot();
            });
        }

        
    }

}
