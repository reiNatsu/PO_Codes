using Consts;
using LIFULSE.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILiberationMission : UIBase
{
    [Header("기본 UI")]
    [SerializeField] private ExTMPUI _areaNoTMP;
    [SerializeField] private ExTMPUI _percentTMP;
    [SerializeField] private ExTMPUI _progressBtnTMP;           // 진행중, 완료, 해방 버튼 TMP 
    [SerializeField] private GameObject _progressBtnDimd;           // 진행 상태 버튼 Dimd;
    [SerializeField] private Slider _percentSlider;
    [SerializeField] private GameObject _percentTMPObj;     
    [SerializeField] private GameObject _completeObj;

    [Header("도네이션 팝업")]
    [SerializeField] private UIPopupDonation _uiPopupDonation;

    [Header("퀘스트 리스트")]
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private ExTMPUI _amountTMP;

    [SerializeField] private List<ItemCell> _itemCells = new List<ItemCell>();

    [SerializeField]
    private UILayoutGroup[] _layoutGroup;

    private int _goalValue = 100;

    private List<LiberationMissionCell> _cells;
    private List<LiberationMissionCellData> _cellDatas = new List<LiberationMissionCellData>();

    private string _liberationTid;
    private Liberation_TableData _data;
    private Dictionary<string, LiberationData> _quests;
    public override void Init()
    {
        base.Init();

        _scroll.Init();
        _cellDatas = new List<LiberationMissionCellData>();
        _cells = _scroll.GetCellToList<LiberationMissionCell>();
    }

    private void UpdateLayoutGroup()
    {
        for (int i = 0; i<_layoutGroup.Length; i++)
        {
            _layoutGroup[i].UpdateLayoutGroup();
        }
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _uiPopupDonation.gameObject.SetActive(false);
        _data = new Liberation_TableData();

        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Tid, out var liberationID))
                _liberationTid = liberationID.ToString();
        }

        SetData();
        UpdateUI();
    }

    private void SetData()
    {
        _data = TableManager.Instance.Liberation_Table[_liberationTid];
        _quests = GameInfoManager.Instance.LiberationInfo.GetQuests(_liberationTid);
    }

    private void UpdateUI()
    {
        _areaNoTMP.ToTableText("str_ui_stage_area_02", _data.Content_Chapter_Id);
        SetPercentUI();
        SetRewardUI();
        SetQuestList();
        UpdateLayoutGroup();

        //_completeObj.gameObject.SetActive(GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid));
        if (GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid))
            SetRewardItemUI();
    }

    public void SetPercentUI()
    {
        int percent = GameInfoManager.Instance.GetLiberationPercent(_data.Tid);
        _percentTMP.text = percent.ToString();

        _percentSlider.maxValue = _goalValue;
        _percentSlider.minValue = 0;
        if (percent > _goalValue)
            percent = _goalValue;
        _percentSlider.value = percent;

        // _percentTMP.ToTableText("str_ui_percentage_value_default", percent);
        SetProgressUI(percent);
        _completeObj.gameObject.SetActive(GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid));
        _percentTMPObj.gameObject.SetActive(!GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid));
        UpdateLayoutGroup();
    }
    private void SetProgressUI(int percent)
    {
        string str = null;
        if (percent < _goalValue)
            str = "str_ui_Proceeding";          // 진행중
        else
        {
            if (GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid))
                str = "str_ui_complete_default_001";            // 완료
            else
                str = "str_ui_liberation_button_01";            // 해방
        }
        _progressBtnTMP.ToTableText(str);
        _progressBtnDimd.SetActive(percent < _goalValue || GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid));
    }
    private void SetRewardUI()
    {
        var rewards = _data.Liberation_Reward_All;
        for (int n = 0; n< _itemCells.Count; n++)
        {
            if (n < rewards.Length)
            {
                var rewardData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(rewards[n]);
                _itemCells[n].gameObject.SetActive(true);
                _itemCells[n].UpdateData(rewardData.Item_Tid, rewardData.ITEM_TYPE, ItemCustomValueType.RewardAmount, rewardData.Item_Min);
            }
            else
                _itemCells[n].gameObject.SetActive(false);
        }   
    }

    private void SetQuestList()
    {
        _cellDatas.Clear();

        var list = TableManager.Instance.Liberation_Quest_Table.GetDatas(_data.Tid);
        for (int n = 0; n< list.Count; n++)
        {
            var info = new LiberationData();
            if (_quests != null && _quests.ContainsKey(list[n].Tid))
                info = _quests[list[n].Tid];
            else
                info = null;

            _cellDatas.Add(new LiberationMissionCellData(_data.Content_Chapter_Id, list[n], info));
        }

        for (int i = 0; i <_cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(list.Count);

        SetQuestAmount(list.Count);
    }

    private void SetQuestAmount(int total)
    {
        var currentCount = GameInfoManager.Instance.LiberationInfo.GetCompleteCount(_data.Tid);
        _amountTMP.text = currentCount +" / "+  total.ToString();
    }

    private void SetRewardItemUI()
    {
        for (int n = 0; n< _itemCells.Count; n++)
        {
            if (_itemCells[n].gameObject.activeInHierarchy)
                _itemCells[n].SetActiveAcquired(true);
        }
    }

    public void OpenDonationPopup(Liberation_Quest_TableData data)
    {
        if (_uiPopupDonation == null)
            return;

        _uiPopupDonation.gameObject.SetActive(true);
        _uiPopupDonation.InitData(data, UpdateQuestCells);
    }

    public void UpdateQuestCells()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            if (_cells[i].gameObject.activeInHierarchy)
                _cells[i].UpdateUI();
        }
    }

    public void OnClickReward()
    {
        if (GameInfoManager.Instance.GetLiberationPercent(_data.Tid) <_goalValue)
            return;

        RestApiManager.Instance.RequestUserLiberationGetReward(_data.Tid, (response) => {
            var displayReward = response["result"]["reward"];

            var list = Utils.ToRewardCellDatas(displayReward);
            UIManager.Instance.Show<UILiberationReward>(Utils.GetUIOption(
                UIOption.Index, _data.Content_Chapter_Id,
                UIOption.List , list
                ));
            // 여기서 보상상 아이템에 dimd 
            SetRewardItemUI();
            
            SetPercentUI();
        });
    }

    public void OnClickRejecDonation()
    {
        var str = LocalizeManager.Instance.GetString("str_ui_liberation_donation_itemplz");
        UIManager.Instance.ShowToastMessage(str);
    }
}
