using LIFULSE.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class UIStageResultStars : MonoBehaviour
{
    [SerializeField] private GameObject _starsinfo;     // ? 클릭시 스테이지 별 보상 조건 확인 위한 object
   // [SerializeField] private ExImage[] _starOns; 
    [SerializeField] private List<UIConditionItem> _conditions = new List<UIConditionItem>();
    [SerializeField] private Transform _starsContent;
    // stage_main, stage_sub → 애니메이션으로 오브젝트가 조정 되어서 나눠서 처리.
    [BoxGroup("One Star")]
    [SerializeField] private GameObject _oneStar;           // 별이 한개인 오브젝트 켜질때
    [BoxGroup("Three Star")]
    [SerializeField] private List<GameObject> _threeStarList = new List<GameObject>();       // 별이 3개인 오브젝트 켜질때


    private Stage_TableData _data;
    private List<MissionData> _missionList = new List<MissionData>();
    private int[] _stars;

    private bool _isOpen = false;

    private void Awake()
    {

    }

    public void Show(int[] stars, string stageId)
    {
        _stars = new int[stars.Length];
        _stars = stars;
        gameObject.SetActive(true);
        _starsinfo.SetActive(false);
        _data = TableManager.Instance.Stage_Table[stageId];
        _missionList = TableManager.Instance.Stage_Mission_Table.GetStageMissionList(_data.Stage_Mission);

        var missioncount = TableManager.Instance.Stage_Mission_Table.GetMissionCount(stageId);
        UpdateThreeStarsUI();
        //if (missioncount > 1)
        //    UpdateThreeStarsUI();
        //else
        //    UpdateOneStarUI();

        //if (_data.CONTENTS_TYPE_ID == Consts.CONTENTS_TYPE_ID.stage_sub)
        //{
        //    UpdateOneStarUI();
        //}
        //else
        //{
        //    UpdateThreeStarsUI();
        //}

    }

    private void UpdateThreeStarsUI()
    {
        for (int n = 0; n < _missionList.Count; n++)
        {
            if (_missionList[n].stage_clear_mission_type != Consts.STAGE_CLEAR_MISSION_TYPE.none)
            {
                _threeStarList[n].transform.parent.gameObject.SetActive(true);
                if (_stars[n] == 1)
                {
                    _threeStarList[n].SetActive(true);
                }
                else
                {
                    _threeStarList[n].SetActive(false);
                }
                UpdateStarConditions(n, true);
            }
            else
            {
                UpdateStarConditions(n, false);
                _threeStarList[n].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateOneStarUI()
    {
        for (int n = 0; n < _missionList.Count; n++)
        {
            if (_missionList[n].stage_clear_mission_type != Consts.STAGE_CLEAR_MISSION_TYPE.none)
            {
                if (_stars[n] == 1)
                {
                    _oneStar.SetActive(true);
                }
                else
                {
                    _oneStar.SetActive(false);
                }
                UpdateStarConditions(n, true);
            }
            else
            {
                UpdateStarConditions(n, false);
            }
        }
    }

    private void UpdateStarConditions(int index,bool isActive)
    {
        _conditions[index].gameObject.SetActive(isActive);

        if (isActive)
        {
            _conditions[index].Init(_stars[index], _missionList[index].stage_clear_mission_type, _missionList[index].stage_clear_mission_value);
        }
    }

    public void OnClickStarsInfo()
    {
        _isOpen = true;
        if (_isOpen)
        {
            _starsinfo.SetActive(true);
            _isOpen = false;
        }
        else
        {
            _starsinfo.SetActive(false);
        }

        UpdateThreeStarsUI();
        //if (_data.CONTENTS_TYPE_ID == Consts.CONTENTS_TYPE_ID.stage_sub)
        //{
        //    UpdateOneStarUI();
        //}
        //else
        //{
        //    UpdateThreeStarsUI();
        //}
    }
    public void OnClickStarsInfoClose()
    {
        _starsinfo.SetActive(false);
    }

    public void Close()
    {
        _starsinfo.SetActive(false);
        gameObject.SetActive(false);
    }
}


