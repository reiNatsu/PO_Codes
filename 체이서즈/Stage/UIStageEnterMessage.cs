using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStageEnterMessage : UIBase
{
    [SerializeField] private ExTMPUI _chapterNumberTMP;
    [SerializeField] private ExTMPUI _stageNameTMP;

    private Action _onShow;
    private Action _onEndAnimCallback;

    private string _enterSountKey = "sfx_ui_stage_start";

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        AudioManager.Instance.CashAndPoolAudio(_enterSountKey);

        if (optionDict == null)
            return;

        if (optionDict.TryGetValue(UIOption.Tid, out object tid))
        {
            string stageTid = (string)tid;
            var stageData = TableManager.Instance.Stage_Table[stageTid];
            var mainStageTid = TableManager.Instance.Stage_Table.GetMainStageName(stageData.Theme_Id, stageData.LEVEL_DIFFICULTY, stageData.Stage_Id);
            var stageno = TableManager.Instance.Stage_Table.GetStageNumber(stageData.Theme_Id, stageData.LEVEL_DIFFICULTY, stageData.LEVEL_TYPE, mainStageTid);

            _chapterNumberTMP.gameObject.SetActive(false);
            //if (stageData.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.none && stageData.Theme_Id > 0 && stageno > 0)
            //{
            //    _chapterNumberTMP.gameObject.SetActive(true);
            //    _chapterNumberTMP.text = stageData.Theme_Id + "-" + stageno;
            //}
            //else
            //{
            //    _chapterNumberTMP.gameObject.SetActive(false);
            //}

            _stageNameTMP.text = GameInfoManager.Instance.GetStageName(stageTid);
       
        }

        if (optionDict.TryGetValue(UIOption.Action, out object onShow))
        {
            _onShow = (Action)onShow;
            AudioManager.Instance.PlayAudio(_enterSountKey);
            _onShow?.Invoke();
        }

        if (optionDict.TryGetValue(UIOption.Callback, out object callback))
        {
            _onEndAnimCallback = (Action)callback;
        }
        GameManager.Instance.CombatController.UseController = CombatControllerBase.ControllType.StageEnter;
        //Close();
    }

    public void EndAnimation()
    {
        //테스트가 존재하면 false
        if (_onEndAnimCallback != null)
        {
            AudioManager.Instance.DisposeAudio(_enterSountKey);
            _onEndAnimCallback?.Invoke();    
        }
        else
        {
            GameManager.Instance.CombatController.UseController = CombatControllerBase.ControllType.CanUse;
        }
        
        Close();
    }
}
