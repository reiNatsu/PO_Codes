using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILiberation : MonoBehaviour
{
    [SerializeField] private GameObject _completeMark;              // 배경 완료 마크
    [SerializeField] private ExTMPUI _completeTMP;                      // 완료 % TMP; 
    [SerializeField] private UIRedDot _uiRedDot;

    private Liberation_TableData _data;
    public void Init(int chapterID)
    {
        _data = TableManager.Instance.Liberation_Table.GetData(chapterID);
        _uiRedDot.UpdateRedDot("reddot_area_liberations", chapterID-1, chapterID-1);
        SetLiberationUI();
    }

    public void SetLiberationUI()
    {
        int percent = GameInfoManager.Instance.GetLiberationPercent(_data.Tid);
        _completeTMP.ToTableText("str_ui_percentage_value_default", percent);

        _completeMark.SetActive(GameInfoManager.Instance.LiberationInfo.IsReward(_data.Tid));
    }

    public void OnClickLiberation()
    {
        UIManager.Instance.Show<UILiberationMission>(Utils.GetUIOption(UIOption.Tid, _data.Tid));
    }
}
