using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;
using LIFULSE.Manager;
//using UnityEditor.Searcher;

public class UIConditionItem : MonoBehaviour
{
    [SerializeField] private ExImage _star;
    [SerializeField] private ExTMPUI _text = null;

    private int _missionaStar;
    private STAGE_CLEAR_MISSION_TYPE _missionType;
    private int _missionValue;


    private void OnDisable()
    {
        //_star.enabled = false;
        //if (_text != null)
        //    _text.SetColor("#8A8A8A");
    }

    // 특정 오브젝트 껐다 키기만 할 때.
    public void Active(bool isOn)
    {
        if (!_star.gameObject.activeInHierarchy)
            _star.gameObject.SetActive(true);

        _star.enabled = isOn;
    }

    public void Init(int star, STAGE_CLEAR_MISSION_TYPE type, int value)
    {
        _missionaStar = star;
        _missionType = type;
        _missionValue = value;

        UpdateUI();
    }

    public void Init(bool isOn, STAGE_CLEAR_MISSION_TYPE type, int value)
    {
        Active(isOn);
        _missionType = type;
        _missionValue = value;

        _text.ToTableText(_missionType.ToStringTid(), _missionValue);
        if(isOn)
            _text.SetColor("#FFFFFF");
        else
            _text.SetColor("#808080");
    }


    private void UpdateUI()
    {
        _text.ToTableText(_missionType.ToStringTid(), _missionValue);
        if (_missionaStar == 1)
        {
            _star.enabled = true;
            _text.SetColor("#FFFFFF");
        }
        else
        {
            _star.enabled = false;
            _text.SetColor("#808080");
        }
    
    }


    // 미션 한글 텍스트 (임시. 테이블이 안보입니다....)
    private string SetMissionText(STAGE_CLEAR_MISSION_TYPE type)
    {
        string text = "";
        switch (type)
        {
            case STAGE_CLEAR_MISSION_TYPE.mission_star:
                text= "별 획득 여부";
                break;
            case STAGE_CLEAR_MISSION_TYPE.clear:
                text= "스테이지 클리어";
                break;
            case STAGE_CLEAR_MISSION_TYPE.time_limit:
                text= "제한 시간 안에 스테이지 클리어";
                break;
            case STAGE_CLEAR_MISSION_TYPE.die:
                text= "스테이지 클리어까지 죽은 횟수";
                break;
            case STAGE_CLEAR_MISSION_TYPE.use_skill:
                text= "스킬 한번이라도 사용하기";
                break;
            case STAGE_CLEAR_MISSION_TYPE.use_heal:
                text= "힐 한번이라도 사용하기";
                break;
            case STAGE_CLEAR_MISSION_TYPE.no_use_skill:
                text= "스킬 사용하지 않고 스테이지 클리어";
                break;
            case STAGE_CLEAR_MISSION_TYPE.no_use_heal:
                text= "힐 사용하지 않고 스테이지 클리어";
                break;
            case STAGE_CLEAR_MISSION_TYPE.use_itembox:
                text= "오브젝트 상자를 열기";
                break;
            case STAGE_CLEAR_MISSION_TYPE.tier_grade_limit:
                text= "등급 제한 캐릭터로만 클리어";
                break;
        }
        return text;
    }

}
