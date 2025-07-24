using Consts;
using LIFULSE.Manager;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class UIStageInfoSide : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private Animator _anim;
    //[Header("스테이지 이미지")]
    //[SerializeField] private ExImage _stageImg;             // 미션정보 없을 경우는 스테이지 이미지 띄우기
    [Header("별정보")]
    [SerializeField] private UIStageStarList _uiStageStarList;
    [Header("적정보")]
    [SerializeField] private UIStageEnemyInfo _uiStageEnemyInfo;

    [Header("편성")]
    [SerializeField] private UIContentTeam _uiContentTeam;
    [Header("기대보상")]
    [SerializeField] private UIStageRewardInfo _uiStageRewardInfo;
    [Header("버튼")]
    [SerializeField] private UIStageButtons _uiStageButtons;

    [Header("스테이지 정보 UI")]
    [SerializeField] private GameObject _roadInfoObj;               // 챌린지 로드 정보 보여줌.
    [SerializeField] private ExTMPUI _stageTitleTMP;
    [SerializeField] private ExTMPUI _stageSubTitleTMP;
    [SerializeField] private ExTMPUI _stageNoTMP;                       // 챌린지 로드 스테이지 넘버. 
    [SerializeField] private GameObject _moveCurrentBtn;         // 현재 위치로 버튼
    [SerializeField] private GameObject _allCelarObj;

    private Stage_TableData _data;
    private bool _isOpen = false;
    public UIContentTeam UIContentTeam { get { return _uiContentTeam; } }
    public bool IsOpen { get { return _isOpen; }set { value = _isOpen; } }

    public void Init()
    {
        _uiStageRewardInfo.init();
    }

    public void InitializeAnim()
    {
        _anim.Play("StageInfoPopupOn", -1, 0);      // 역방향 재생
        _anim.speed = -1; 
        _isOpen = false;
    }

    public void SetStageInfo(Stage_TableData data)
    {
        if (!this.gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        _data = data;

        _uiStageEnemyInfo.Show(data);
        _uiContentTeam.Show(data);
        switch (data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
            case CONTENTS_TYPE_ID.event_main:
                {
                    _roadInfoObj.SetActive(false);
                    if (string.IsNullOrEmpty(data.Stage_Mission))
                    {
                        _uiStageStarList.gameObject.SetActive(false);
                    }
                    else
                    {
                        _uiStageStarList.gameObject.SetActive(true);
                        _uiStageStarList.Show(data);
                    }
                }
                break;
            case CONTENTS_TYPE_ID.manjang:
                {
                    _roadInfoObj.SetActive(true);
                    _uiStageStarList.gameObject.SetActive(false);
                    _stageTitleTMP.ToTableText("str_content_route_title");      //ROUTE
                    _stageSubTitleTMP.ToTableText("str_content_challengeroad_title");       //CHALLENGE ROAD
                    _stageNoTMP.text = data.Stage_Id.ToString("D3");
                    _uiStageButtons.gameObject.SetActive(false);
                }
                break;
            case CONTENTS_TYPE_ID.elemental:    // 
            case CONTENTS_TYPE_ID.gold:             // 골드 던전
            case CONTENTS_TYPE_ID.exp:              // 경험치던전
            case CONTENTS_TYPE_ID.relay_boss:
            case CONTENTS_TYPE_ID.content_class:
                {
                    _roadInfoObj.SetActive(true);
                    _uiStageStarList.gameObject.SetActive(false);
                    _stageTitleTMP.ToTableText("str_ui_stage_01");      //STAGE
                    _stageSubTitleTMP.ToTableText(GetSubTitleString(data.CONTENTS_TYPE_ID));
                    _stageNoTMP.text = data.Stage_Id.ToString("D2");
                    _uiStageButtons.gameObject.SetActive(true);
                }
                break;
            default:
                {
                    _roadInfoObj.SetActive(false);
                    if (string.IsNullOrEmpty(data.Stage_Mission))
                    {
                        _uiStageStarList.gameObject.SetActive(false);
                    }
                    else
                    {
                        _uiStageStarList.gameObject.SetActive(true);
                        _uiStageStarList.Show(data);
                    }
                }
                break;
        }
            
        _uiStageRewardInfo.Show(data);
        SetCombatButtonUI(data);
    }

    // 스테이지 진입 버튼 예외 처리 함수
    private void SetCombatButtonUI(Stage_TableData data)
    {
        switch (data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.manjang:      // 챌린지 로드 
                {
                    _allCelarObj.SetActive(GameInfoManager.Instance.GetAllCelarDungeon(data.CONTENTS_TYPE_ID));
                    if (GameInfoManager.Instance.GetAllCelarDungeon(data.CONTENTS_TYPE_ID))
                    {
                        _uiStageButtons.gameObject.SetActive(false);
                        _moveCurrentBtn.SetActive(false);
                        return;
                    }
                    var isCurrent = GameInfoManager.Instance.GetEnableEnterdDungeon(data.CONTENTS_TYPE_ID, data.Tid);
                    // 진입 가능 한 버튼일 경우믄 _uiStageButtons 보여줌
                    _uiStageButtons.gameObject.SetActive(isCurrent);
                    _moveCurrentBtn.SetActive(!isCurrent);

                    if (isCurrent)
                        _uiStageButtons.Show(data);
                    // 이미 클리어한 스테이지, 진입 불가한 스테이지는 현재 위치로 버튼 추가. 
                }
                break;
            default:
                {
                    _moveCurrentBtn.SetActive(false);
                    _uiStageButtons.gameObject.SetActive(true);
                    _uiStageButtons.Show(data);
                    _allCelarObj.SetActive(false);
                }
                break;
        }
    }

    public void UpdateComabtButtion()
    {
        _uiStageButtons.UpdateComabatUI();
    }

    public void Active()
    {
        if (!this.gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (_anim != null)
        {
            if (_anim.speed <= 0)
                _anim.speed = 1;
            if (!_isOpen)
            {
                _anim.Play("StageInfoPopupOn");
                _isOpen = true;
            }
        }
    }

    public void DeActive()
    {
        if (this.gameObject.activeInHierarchy)
            gameObject.SetActive(false);

        if (_anim != null)
        {
            _anim.Play("StageInfoPopupOn", -1, 0);        // 역방향 재생
            _anim.speed = 0;
            _isOpen = false;
        }
    }

    public void OnClickCloseInfo()
    {
        if (_isOpen)
            DeActive();
    }

    public void OnClickInfosClose()
    {
        // 닫히면서 해야 하는 이벤트
        _uiStageStarList.CloseInfoPopup();        // 별 상세 정보 창 열려있으면 닫고
        _uiStageEnemyInfo.CloseInfoPopup();       // 버프창 열려있으면 닫기
    }

    private string GetSubTitleString(CONTENTS_TYPE_ID type)
    {
        string title = null;
        switch (type)
        {
            case CONTENTS_TYPE_ID.elemental:
                title = "str_ui_content_elemental_title";    //마르티늄 매립지 
                break;
            case CONTENTS_TYPE_ID.gold:
                title = "str_ui_content_gold_title";  //알루나의 전파 
                break;
            case CONTENTS_TYPE_ID.exp:
                title = "str_ui_content_exp_title";   //수송 작전 
                break;
            case CONTENTS_TYPE_ID.content_class:
                title = "str_ui_content_position_title";    //데이터 수집실 
                break;
            case CONTENTS_TYPE_ID.relay_boss:
                title = "str_ui_content_boss_title";    //이름 미정(승급던전) 
                break;
        }
        return title;
    }
}
