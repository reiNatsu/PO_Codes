using Consts;
using LIFULSE.Manager;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class StageNodeCellData
{
    public Stage_TableData StageData;
    public bool IsLastStage;
    public bool IsSelected = false;

    public StageNodeCellData(Stage_TableData data, bool islast)
    {
        StageData = data;
        IsLastStage = islast;
    }

    public void SetIsSelected(bool isSelected)
    {
        IsSelected = isSelected;
    }
}

public class StageNodeCell : ScrollCell
{
    //
    [SerializeField] private ExTMPUI _stageNoTMP;
    [SerializeField] private GameObject _clearObj;
    [SerializeField] private GameObject _mainTMPObj;

    [SerializeField] private GameObject _nextLine = null;   
    [SerializeField] private GameObject _selected;
    [SerializeField] private UIContentLcok _uiContentLock = null;

    [Header("보스 스테이지 ")]
    [SerializeField] private GameObject _bossObj = null;
    [SerializeField] private ExTMPUI _bossStageNoTMP = null;
    [SerializeField] private GameObject _bossTMPObj = null;

    //[Header("서브 스테이지 노드")]
    //[SerializeField] private List<StageNodeCell> _subStages = new List<StageNodeCell>();
    //[SerializeField] private List<GameObject> _subLines = new List<GameObject>();

    [Header("클리어 별")]
    [SerializeField] private List<UIConditionItem> _stars = new List<UIConditionItem>();
    [Header("보스 클리어 별")]
    [SerializeField] private List<UIConditionItem> _bossstars = new List<UIConditionItem>();

    private List<StageNodeCellData> _cellDatas;
    private Stage_TableData _data;

    protected override void Init()
    {
        base.Init();

    }
    public void SetCellDatas(List<StageNodeCellData> dataList)
    {
        _cellDatas = dataList;
    }
    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;

        _data = _cellDatas[DataIndex].StageData;
       
        //SetSubStage();
        UpdateUI();
    }

    // Recycle4Scroll이 아니라 sub스테이지와 데이터 업데이트 용으로 쓰일 때 사용.
    public void InitNodeData(Stage_TableData data)
    {
        _data = data;
        UpdateUI();
    }

    public void UpdateUI()
    {
        _selected.SetActive(false);
        CheckStageOpend();
        SetBossUI();
        SetSelectUI();
        if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_main)
            _stageNoTMP.text = _data.Stage_Id.ToString("D2");

        _clearObj.SetActive(false);
        //if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_sub)
        //    _stageNoTMP.text = string.Format("{0}-{1}", _data.Stage_Id, _data.Extra_Id);

        //if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_sub)
        //    _clearObj.SetActive(GameInfoManager.Instance.StageInfo.IsClear(_data.Tid));
        //else


        // 이 노드가 해당 레벨의 스테이지 중 마지막 노드이면 nextLine 꺼주기. 
        if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_main)
            _nextLine.SetActive(!_cellDatas[DataIndex].IsLastStage);

        if(_data.LEVEL_TYPE == LEVEL_TYPE.boss)
            SetCelarStars(_bossstars);
        else
            SetCelarStars(_stars);
    }

    public void SetBossUI()
    {
        if (_bossObj != null)
        {
            _bossObj.SetActive(_data.LEVEL_TYPE == LEVEL_TYPE.boss);
            _bossStageNoTMP.text = _data.Stage_Id.ToString("D2");
        }

        if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_main)
        {
            bool isBoss = _data.LEVEL_TYPE == LEVEL_TYPE.boss;
            _mainTMPObj.SetActive(!isBoss);
            _bossTMPObj.SetActive(isBoss);
        }
    }

   
    //public void SetSubStage()
    //{
    //    if (_data.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.stage_main)
    //        return;

    //    var subs = TableManager.Instance.Stage_Table.GetSubStages(_data.Theme_Id, _data.Stage_Id, CONTENTS_TYPE_ID.stage_sub, _data.LEVEL_DIFFICULTY);
    //    for (int n = 0; n< _subStages.Count; n++)
    //    {
    //        if (n < subs.Count)
    //        {
    //            _subStages[n].gameObject.SetActive(true);
    //            _subStages[n].InitNodeData(subs[n]);
    //            _subLines[n].SetActive(true);
    //        }
    //        else
    //        {
    //            _subStages[n].gameObject.SetActive(false);
    //            _subLines[n].SetActive(false);
    //        }
    //    }
    //}

    public void CheckStageOpend()
    {
        if (_uiContentLock == null)
            return;

        if (string.IsNullOrEmpty(_data.Lock_Tid))
            _uiContentLock.gameObject.SetActive(false);
        else
            _uiContentLock.ContentStateUpdate(_data.Lock_Tid);
    }
    public void SetCelarStars(List<UIConditionItem> starInfos)
    {
        int missioncount = TableManager.Instance.Stage_Mission_Table.GetMissionCount(_data.Tid);

        for (int n = 0; n < _stars.Count; n++)
        {
            if (n < missioncount)
            {
                starInfos[n].gameObject.SetActive(true);
                if (GameInfoManager.Instance.StageInfo.IsClear(_data.Tid))
                    SetMissionState(n);
                else
                    starInfos[n].Active(false);
            }
            else
                starInfos[n].gameObject.SetActive(false);
        }
    }

    private void SetMissionState(int index)
    {
        var stars = GameInfoManager.Instance.ClearStarCount(_data.Tid);
        if (!stars.IsNullOrEmpty())
        {
            if(_data.LEVEL_TYPE != LEVEL_TYPE.boss)
                _stars[index].Active(stars[index] == 1);
            else
                _bossstars[index].Active(stars[index] == 1);
        }
    }

    public void OnClickStageNode()
    {
        //UIManager.Instance.Show<UIStageInfo>(Utils.GetUIOption( UIOption.Tid, _data.Tid));
        var uiStage = UIManager.Instance.GetUI<UIStage>();
        if (uiStage != null && uiStage.gameObject.activeInHierarchy)
            uiStage.OpenStageInfo(_data);

    }
    public void OnClickLock()
    {
        var uiStage = UIManager.Instance.GetUI<UIStage>();
        if (uiStage != null && uiStage.gameObject.activeInHierarchy)
            uiStage.OnClickCloseInfo();

        string message = GameInfoManager.Instance.StageLockMessage(_data.Lock_Tid);
        UIManager.Instance.ShowToastMessage(message);
    }

    public void ActiveSelected(Stage_TableData data)
    {
        if (data == null)
        {
            _selected.SetActive(false);
            return;
        }

        if (_data.Tid.Equals(data.Tid))
            _selected.SetActive(true);
        else
            _selected.SetActive(false);
    }

    public void SetSelectUI()
    {
        _selected.SetActive(_cellDatas[DataIndex].IsSelected);
    }
}
