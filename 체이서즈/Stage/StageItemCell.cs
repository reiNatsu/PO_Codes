using Consts;
using LIFULSE.Manager;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class StageItemCell : MonoBehaviour
{
    [Header("Info UI")]
    [SerializeField] private bool _isClear;
    [SerializeField] private ExTMPUI _index;
    [SerializeField] private ExImage _complete;
    [SerializeField] private GameObject _dimd;
    [SerializeField] private ExImage _boss;

    [Header("Main Stage UI")]
    [SerializeField] private ExImage _mStageImg;
    [SerializeField] private ExImage[] _stars;
    [SerializeField] private GameObject _starObj;

    [Header("Sunb Stage UI")]
    [SerializeField] private SubStageItem[] _subStage;

    private int _stageNo = 0;
    private int _openSubStageNo = 0;
    private Stage_TableData _data;
    private List<Stage_TableData> _subDatas;
    private string _stageIndex;
    private Action _storyClearCallback;

    public void InitData(Stage_TableData data, Action storyClearCallback)
    {
        EnableDimd(true);
        _data = data;

        for (int s = 0; s< _stars.Length; s++)
        {
            _stars[s].enabled = false;
        }

        _storyClearCallback = storyClearCallback;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 메인 스테이지 UI
        _mStageImg.SetSprite(_data.Stage_Bg);
        CountStageStars(_data.Tid);
        // Dimd 처리  
        //_stageIndex = "S"+_data.Theme_Id +"_"+_data.Stage_Id;

        _stageNo = TableManager.Instance.Stage_Table.GetStageNumber(_data.Theme_Id, _data.LEVEL_DIFFICULTY, _data.LEVEL_TYPE, _data.Tid);
        string str = string.Empty;
        if (_data.LEVEL_TYPE == LEVEL_TYPE.story)
        {
            str = "str_story_main_number_default";// STORY {0}
        }
        else
        {
            str = "str_stage_main_number_default";      // STAGE {0}
        }
        _index.ToTableText(str, _stageNo);            // STAGE {0}

        // 아이콘 처리 → 보스아이콘, 스토리 아이콘
        SetBossIcon();

        // 서브 스테이지 생성
        _subDatas = new List<Stage_TableData>();
        _subDatas = TableManager.Instance.Stage_Table.GetSubStages(_data.Theme_Id, _data.Stage_Id, CONTENTS_TYPE_ID.stage_sub, _data.LEVEL_DIFFICULTY);
        SetSubStages(); // 서브스테이지 체크. → 메인스테이지 클리어시 ex1,2 노출. 이전 ex 스테이지 미 클리어시 dimd
        UpdateSubAnimation();


        CheckThisStageAllClear();
    }

    private void SetBossIcon()
    {
        if (_data.LEVEL_TYPE == LEVEL_TYPE.boss)
        {
            _boss.gameObject.SetActive(true);
            _boss.SetSprite("UI_StageNode_Boss");
            _starObj.SetActive(true);
        }
        else if (_data.LEVEL_TYPE == LEVEL_TYPE.story)
        {
            _boss.gameObject.SetActive(true);
            _boss.SetSprite("UI_StageNode_Story");
            // 스토리 스테이지면 별Object 숨기기.
            _starObj.SetActive(false);
        }
        else
        {
            _boss.gameObject.SetActive(false);
            _starObj.SetActive(true);
        }
    }


    private void SetSubStages()
    {
        //GameInfoManager.Instance.StageInfo.IsClear();
        if (GameInfoManager.Instance.StageInfo.IsClear(_data.Tid))
        {
            if (_subDatas.Count > 0)
            {
                for (int n = 0; n < _subDatas.Count; n++)
                {
                    _subStage[n].gameObject.SetActive(true);
                    _subStage[n].InitData(_subDatas[n], _stageNo);
                    _subStage[0].SetSubStageDimd(false);// 메인 스테이지가 깨지면 ex1은 오픈
                    if (GameInfoManager.Instance.StageInfo.IsClear(_subDatas.FirstOrDefault().Tid))
                    {
                        _subStage[1].SetSubStageDimd(false);
                    }
                    else
                    {
                        _subStage[1].SetSubStageDimd(true);
                    }
                    //_subStage[n].PlayAnimation();
                }
            }
            else
            {
                SubstagesHide();
            }
        }
        else
        {
            SubstagesHide();
        }
    }

    public void SubstagesHide()
    {
        for (int n = 0; n < _subStage.Length; n++)
        {
            _subStage[n].gameObject.SetActive(false);
        }
    }

    public void UpdateSubAnimation()
    {
        for (int n = 0; n < _subDatas.Count; n++)
        {
            _subStage[n].PlayAnimation();
            //if (_subStage[n].gameObject.activeInHierarchy)
            //{
            //    _subStage[n].PlayAnimation();
            //}
        }
    }

    public void CountStageStars(string tid)
    {
        var stars = GameInfoManager.Instance.ClearStarCount(tid);
        if (!stars.IsNullOrEmpty())
        {
            // 별 갯수가 있는 경우
            // 마지막 항목은 스테이지 클리어 횟수니까 -1한 만큼
            int starCount = stars.Length -1;
            for (int n = 0; n <starCount; n++)
            {
                if (stars[n] == 1)
                {
                    _stars[n].enabled = true;
                }
                else
                {
                    _stars[n].enabled = false;
                }
            }
        }
        else
        {
            // 별 갯수가 없는 경우.
        }
    }

    // main, ex1, ex2 전부 클리어 + main, ex1, ex2 전부 MaxStar 했을 경우 체크 이미지 노출
    private void CheckThisStageAllClear()
    {
        // 서브 스테이지가 있는 경우
        if (_subDatas.Count > 0)
        {
            // 메인 클리어,maxStar + 서브 1 클리어,maxStar + 서브 2 클리어,maxStar
            if (CheckIsComplete(_data.Tid)
                &&CheckIsComplete(_subDatas[0].Tid)&&CheckIsComplete(_subDatas[1].Tid))
            {
                _complete.enabled = true;
            }
            else
            {
                _complete.enabled = false;
            }
        }
        else     // 서브 스테이지 없는 경우
        {
            if (_data.LEVEL_TYPE == LEVEL_TYPE.story)    // 스토리 스테이지 인 경우
            {
                _complete.enabled = GameInfoManager.Instance.StageInfo.IsClear(_data.Tid);
            }
            else         // 스토리 스테이지가 아닌 경우
            {
                _complete.enabled = CheckIsComplete(_data.Tid);
            }
        }

    }

    public bool CheckIsComplete(string stageTid)
    {
        if (GameInfoManager.Instance.StageInfo.IsClear(stageTid) &&GameInfoManager.Instance.StageInfo.IsMaxStar(stageTid))
            return true;

        return false;
    }

    public bool CheckIsMaxStars()
    {
        List<bool> _isSubMaxStars = new List<bool>();
        if (_subDatas.Count > 0)
        {
            for (int n = 0; n < _subDatas.Count; n++)
            {
                _isSubMaxStars.Add(GameInfoManager.Instance.StageInfo.IsMaxStar(_subDatas[n].Tid));
            }
        }
        else
        {
            _isSubMaxStars = null;
        }
        if (GameInfoManager.Instance.StageInfo.IsMaxStar(_data.Tid))
        {
            if (_isSubMaxStars == null)
            {
                return true;
            }
            else
            {
                if (_isSubMaxStars.Contains(false))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            return false;
        }
    }

    public void EnableDimd(bool isOn)
    {
        _dimd.SetActive(isOn);
    }

    public void OnClickStage()
    {
        if (_data.LEVEL_TYPE == LEVEL_TYPE.story)
        {
            if (!GameInfoManager.Instance.StageInfo.IsClear(_data.Tid))
            {
                if (GameInfoManager.Instance.InventoryIsFullInStage(_data))
                {
                    string str = LocalizeManager.Instance.GetString("str_stage_play_deny_01"); // 보유 호패가 최대여서 스테이지 진행이 불가합니다.
                    UIManager.Instance.ShowToastMessage($"{str}");
                    return;
                }
            }
                
            string SequenceName = null;
            SequenceName = _data.Stage_Scene;
            var splits = SequenceName.Split("_");

            var SheetName = "Sequence_Table_"+ splits[0]+"_" + splits[1];

            GameManager.Instance.StoryPlay(SheetName, SequenceName, () =>
            { },
            () =>
            {
                if (GameInfoManager.Instance.StageInfo.IsClear(_data.Tid))
                    return;

                RestApiManager.Instance.RequestStageClear(ResultType.Victory, 0, 0, 0, 1, _data.Tid, _data.LEVEL_TYPE == LEVEL_TYPE.story, null, null,
                    (response) =>
                    {
                        RestApiManager.Instance.UpdateReward(response, true);
                        CheckThisStageAllClear();
                        _storyClearCallback?.Invoke();
                 
                    });
            }
            );

        }
        else
        {
            // SceneManager.Instance.LoadCombatScene(_data.Stage_Scene);
            // 정보 창 열기
            Dictionary<UIOption, object> optionDict = Utils.GetUIOption(
               //UIOption.Data, _data
               UIOption.Tid, _data.Tid
                );

            //var uistage = UIManager.Instance.GetUI<UIStage_old>();
            //if (uistage != null && uistage.gameObject.activeInHierarchy)
            //{
            //    //uistage.EnterdStageData[_data.LEVEL_DIFFICULTY] = _data.Stage_Id;
            //    uistage.SetEnteredStageLevel(_data.LEVEL_DIFFICULTY, _data.Stage_Id);
            //}

            UIManager.Instance.Show<UIStageInfo>(optionDict);
        }
    }


    public void OnClickDimdStage()
    {
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK, message: SetIsNotOpenIndex());
    }

    private string SetIsNotOpenIndex()
    {
        Stage_TableData data = new Stage_TableData();
        if (TableManager.Instance.Stage_Table.GetPrevStageData(_data.Theme_Id, _data.Stage_Id, _data.CONTENTS_TYPE_ID, _data.LEVEL_DIFFICULTY) != null)
        {
            data = TableManager.Instance.Stage_Table.GetPrevStageData(_data.Theme_Id, _data.Stage_Id, _data.CONTENTS_TYPE_ID, _data.LEVEL_DIFFICULTY);
        }
        var stageno = TableManager.Instance.Stage_Table.GetStageNumber(data.Theme_Id, data.LEVEL_DIFFICULTY, data.LEVEL_TYPE, data.Tid);
        var index = string.Empty;

        if (data.LEVEL_TYPE == LEVEL_TYPE.story)
        {
            index = LocalizeManager.Instance.GetString("str_story_main_number_default", stageno);// STORY {0}
        }
        else
        {
            index = LocalizeManager.Instance.GetString("str_stage_main_number_default", stageno);// STAGE {0}
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("[ ");
        sb.Append(index);
        sb.Append("] ");
        sb.Append("str_content_total_difficulty_guide_02".ToTableText()); //클리어 시 해금
        //sb.Append(_data.Theme_Id);
        //sb.Append("-");
        //sb.Append((_data.Stage_Id-1));
        //sb.Append("클리어 시 해금");


        return sb.ToString();

    }
}
