using LIFULSE.Manager;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStageStarList : MonoBehaviour
{
    [Header("정보 팝업")]
    [SerializeField] private GameObject _starsInfoObj;
    [Header("클리어 별")]
    [SerializeField] private List<UIConditionItem> _stars = new List<UIConditionItem>();
    [Header("클리어 별 정보")]
    [SerializeField] private List<UIConditionItem> _starInfos = new List<UIConditionItem>();
    private Stage_TableData _data;
    private List<MissionData> _missiondata = new List<MissionData>();
    private bool _isOpended = false;

    public void Show(Stage_TableData data)
    {
        _data = data;
        _missiondata =  TableManager.Instance.Stage_Mission_Table.GetStageMissionList(_data.Stage_Mission);
        _isOpended = false;
        _starsInfoObj.SetActive(_isOpended);
       
        SetCelarStars(data);
    }

    public void SetCelarStars(Stage_TableData data)
    {
        int missioncount = TableManager.Instance.Stage_Mission_Table.GetMissionCount(data.Tid);
        int[] stars;

        if (data.CONTENTS_TYPE_ID == Consts.CONTENTS_TYPE_ID.event_main)
            stars = GameInfoManager.Instance.EventStoryInfo.GetStars(_data.Tid);
        else
            stars = GameInfoManager.Instance.ClearStarCount(data.Tid);

        for (int n = 0; n < _stars.Count; n++)
        {
            if (n < missioncount)
            {
                _stars[n].gameObject.SetActive(true);
                _starInfos[n].gameObject.SetActive(true);

                if (data.CONTENTS_TYPE_ID == Consts.CONTENTS_TYPE_ID.event_main)
                {
                    if (GameInfoManager.Instance.EventStoryInfo.IsClear(data.Tid))
                    {
                        if (!stars.IsNullOrEmpty())
                            _stars[n].Active(stars[n] == 1);
                    }
                    else
                        _stars[n].Active(false);
                }
                else
                {
                    if (GameInfoManager.Instance.StageInfo.IsClear(data.Tid))
                    {
                        if (!stars.IsNullOrEmpty())
                            _stars[n].Active(stars[n] == 1);
                    }
                    else
                        _stars[n].Active(false);
                }

                bool isOn = !stars.IsNullOrEmpty() && stars[n] == 1;
                _starInfos[n].Init(isOn, _missiondata[n].stage_clear_mission_type, _missiondata[n].stage_clear_mission_value);
            }
            else
            {
                _stars[n].gameObject.SetActive(false);
                _starInfos[n].gameObject.SetActive(false);
            }
        }
    }
    public void OnClickShowStarsInfo()
    {
        if (_isOpended)
            _isOpended = false;
        else
            _isOpended = true;
        _starsInfoObj.SetActive(_isOpended);
    }

    public void CloseInfoPopup()
    {
        if (_isOpended)
        {
            _isOpended = false;
            _starsInfoObj.SetActive(false);
        }
    }
}
