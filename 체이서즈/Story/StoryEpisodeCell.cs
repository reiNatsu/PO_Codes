using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoryEpisodeCellData
{
    public Stage_TableData Data;

    public StoryEpisodeCellData(Stage_TableData data)
    {
        Data = data;
    }
}

public class StoryEpisodeCell : ScrollCell
{
    [SerializeField] private ExTMPUI _epTMP;
    [SerializeField] private ExTMPUI _titleTMP;
    [SerializeField] private ItemCell _itemCell;
    //[SerializeField] private GameObject _dimdObj;
    [SerializeField] private GameObject _battleIcon;
    [SerializeField] private UIContentLcok _uiContentLock;

    private List<StoryEpisodeCellData> _cellDatas;


    public void SetCellDatas(List<StoryEpisodeCellData> dataList)
    {
        _cellDatas = dataList;
    }
    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;
        UpdateUI();
    }

    private void UpdateUI()
    {
        _epTMP.ToTableText("str_ui_episode_number", _cellDatas[DataIndex].Data.Stage_Id);       //EP. {0} 
        SetTitle();
        SetDimd();
        SetRewardItem();

        _battleIcon.SetActive(_cellDatas[DataIndex].Data.LEVEL_TYPE == LEVEL_TYPE.basic);
    }

    private void SetTitle()
    {
        switch (_cellDatas[DataIndex].Data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.story_event:
                _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
                //if (GameInfoManager.Instance.EventStoryInfo.IsClear(_cellDatas[DataIndex].Data.Tid))
                //    _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
                //else
                //{
                //    if (GameInfoManager.Instance.IsOpened(_cellDatas[DataIndex].Data.Tid))
                //        _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
                //    else
                //        _titleTMP.text = "???????";
                //}
                break;
            case CONTENTS_TYPE_ID.story_main:
                if (GameInfoManager.Instance.StageInfo.IsClear(_cellDatas[DataIndex].Data.Tid))
                    _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
                else
                {
                    if (GameInfoManager.Instance.IsOpened(_cellDatas[DataIndex].Data.Tid))
                        _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
                    else
                        _titleTMP.text = "???????";
                }
                break;
        }
        //if (GameInfoManager.Instance.StageInfo.IsClear(_cellDatas[DataIndex].Data.Tid))
        //    _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
        //else
        //{
        //    if (GameInfoManager.Instance.IsOpened(_cellDatas[DataIndex].Data.Tid))
        //        _titleTMP.ToTableText(_cellDatas[DataIndex].Data.Str_Stage);
        //    else
        //        _titleTMP.text = "???????";
        //}
    }

    private void SetDimd()
    {
        bool isLocked = false;
        int count = 0;


        if (string.IsNullOrEmpty(_cellDatas[DataIndex].Data.Lock_Tid))
            _uiContentLock.gameObject.SetActive(false);
        else
            _uiContentLock.ContentStateUpdate(_cellDatas[DataIndex].Data.Lock_Tid);
    }

    private void SetRewardItem()
    {
        if (_cellDatas[DataIndex].Data.CONTENTS_TYPE_ID ==CONTENTS_TYPE_ID.story_event)
        {
            _itemCell.gameObject.SetActive(false);
        }
        else
        {
            if (_cellDatas[DataIndex].Data.First_Reward_Clear == null ||_cellDatas[DataIndex].Data.First_Reward_Clear.Length == 0)
            {
                _itemCell.gameObject.SetActive(false);
            }
            else
            {
                var rewardID = _cellDatas[DataIndex].Data.First_Reward_Clear.FirstOrDefault();
                var reward = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(rewardID);
                _itemCell.gameObject.SetActive(true);
                _itemCell.UpdateData(reward.Item_Tid, reward.ITEM_TYPE, ItemCustomValueType.RewardAmount, reward.Item_Min);
                _itemCell.SetIsFirstReward(GameInfoManager.Instance.StageInfo.IsClear(_cellDatas[DataIndex].Data.Tid));
                // 이미 클리어 기록이 있으면 Dimd처리
                _itemCell.SetActiveAcquired(GameInfoManager.Instance.StageInfo.IsClear(_cellDatas[DataIndex].Data.Tid));
            }
           
        }
    }

    public void OnClickEnterStory()
    {
        var data = _cellDatas[DataIndex].Data;
        //if (data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.story_main 
        //    ||data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.story_event)
        if (data.LEVEL_TYPE == LEVEL_TYPE.story)
        {
            if (!GameInfoManager.Instance.StageInfo.IsClear(data.Tid))
            {
                // 보유 호패 갯수 체크 -> 나중에 수정 할 필요 있움
                if (GameInfoManager.Instance.InventoryIsFullInStage(data))
                {
                    string str = LocalizeManager.Instance.GetString("str_stage_play_deny_01"); // 보유 호패가 최대여서 스테이지 진행이 불가합니다.
                    UIManager.Instance.ShowToastMessage($"{str}");
                    return;
                }
            }

            string SequenceName = null;
            SequenceName = data.Stage_Scene;
            var splits = SequenceName.Split("_");
            var SheetName = "Sequence_Table_"+ splits[0]+"_" + splits[1];

            GameManager.Instance.StoryPlay(SheetName, SequenceName, () =>
            { },
           () =>
           {
               if (GameInfoManager.Instance.StageInfo.IsClear(data.Tid))
                   return;

               RestApiManager.Instance.RequestStageClear(ResultType.Victory, 0, 0, 0, 1, data.Tid, data.LEVEL_TYPE == LEVEL_TYPE.story, null, null,
                   (response) =>
                   {
                       // 리스트 한번 업데이트 해 주는 이벤트 받아서 실행 해 줘야 함. 
                       //UpdateUI();
                       var uistoryInfo = UIManager.Instance.GetUI<UIStoryInfo>();
                       if (uistoryInfo != null && uistoryInfo.gameObject.activeInHierarchy)
                       {
                           uistoryInfo.SetEpisodeList();
                           uistoryInfo.SetFocusing(data.Stage_Id);
                       }
                       RestApiManager.Instance.UpdateReward(response, true);
                   });
           }
           );
        }
        else
        {
            GameInfoManager.Instance.OnEnterDungeon(data.CONTENTS_TYPE_ID, data.Tid, () => {
                //DosaTeamData storyTeam = new DosaTeamData();
                //storyTeam.SetStoryTeam(data.Stage_Team_Apply);
                //GameInfoManager.Instance.OrganizationInfo.SelectedTeam = storyTeam;
                var team = GameInfoManager.Instance.OrganizationInfo.GetTeam().GetTids();
                RestApiManager.Instance.RequestStageEnter(data.Tid, team, () => {});
            });
        }
    }

    public void OnClickDimd()
    {
        string message = GameInfoManager.Instance.StageLockMessage(_cellDatas[DataIndex].Data.Lock_Tid);
        UIManager.Instance.ShowToastMessage(message);
    }
}
