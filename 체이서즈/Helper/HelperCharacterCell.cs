using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HelperCharacterCell : MonoBehaviour
{
    [Header("HelperCell UI")]
    //[SerializeField] private CharacterCell _characterCell;
    [SerializeField] private ExImage _icon;
    [SerializeField] private GameObject _infoObj;
    [SerializeField] private GameObject _noInfoObj;
    [SerializeField] private ExTMPUI _name;
    [SerializeField] private ExImage _rewardImg;
    [SerializeField] private ExTMPUI _rewardTMP;
    [SerializeField] private GameObject _rewardDimd;
    [SerializeField] private GameObject _settingDimd;
    [SerializeField] private ExTMPUI _organizationTMP;


    [SerializeField] private int _helperCellIndex;
    [SerializeField] private ExButton _setButton;

    private Coroutine _tmpCoroutine = null;     // 보상 value 업데이트

    public ExButton SetButton { get { return _setButton; } }

    private HelperData _helperData;

    private void OnDisable()
    {
        StopAllCoroutine();
    }

    public void InitData(HelperData helperData)
    {
        _helperData = helperData;
        if (string.IsNullOrEmpty(_helperData.PcTid))
        {
            StopAllCoroutine();
        }

        // 여기서 한번 사용 횟수 받아오기.

        UpdateUI(_helperData.PcTid);
    }

  
    public void UpdateUI(string iconId)
    {
        SetInfoUI(!string.IsNullOrEmpty(iconId));
        //_infoObj.SetActive(true);
        //_noInfoObj.SetActive(false);

        _settingDimd.gameObject.SetActive(false);
        
        _rewardDimd.gameObject.SetActive(_helperData.ReceivedTime == 0 || string.IsNullOrEmpty(iconId));

        if (!string.IsNullOrEmpty(iconId) && _helperData.ReceivedTime > 0)
        {
            if (GetResetTime(_helperData.ReceivedTime) < GameInfoManager.Instance.HelperInfo.HelperSetTerm)
            {
                SetRewardButtonDimd(GetRewardAmount() <= 0);
            }
        }

        if (!string.IsNullOrEmpty(iconId))
        {
            var table = TableManager.Instance.Character_PC_Table[iconId];
            CharacterCellData charactercelldata = new CharacterCellData(table.Tid);
            //_characterCell.UpdateInfo(charactercelldata);
            _icon.gameObject.SetActive(true);
            _icon.SetSprite(table.Char_Icon_Square_01.ToCostumeKey(table.Tid));
            _name.ToTableText(table.Str);
            _organizationTMP.ToTableText("str_ui_team_reset_formation_001");     // 재편성
            _rewardTMP.text =GetRewardAmount().ToString();
            if (_tmpCoroutine == null)
                _tmpCoroutine = StartCoroutine(SetRewardTMP());
        }
        else
        {
            //_icon.SetSprite("UI_Cha_Non");
            _icon.gameObject.SetActive(false);
            _name.ToTableText("str_ui_count_button_02");
            _rewardTMP.text = "0";
            _organizationTMP.ToTableText("str_ui_team_set_formation_001");     // 편성
        }
    }

    private void SetInfoUI(bool isOn)
    {
        _infoObj.SetActive(isOn);
        _noInfoObj.SetActive(!isOn);
    }

    private double GetResetTime(long targetTime)
    {
        double time = 0;
        if (targetTime > 0)
            time = GameInfoManager.Instance.NowTime.Subtract(new DateTime(targetTime)).TotalSeconds;

        return time;
    }

    private double GetRewardAmount()
    {
        //double quotient = Math.Truncate(GetResetTime(_helperData.ReceivedTime)  / GameInfoManager.Instance.HelperInfo.HelperSetTerm);
        double quotient = Math.Floor(GetResetTime(_helperData.ReceivedTime)  / GameInfoManager.Instance.HelperInfo.HelperSetTerm);
        int useReward = TableManager.Instance.Define_Table["ds_helper_default"].Opt_02_Int;
        var useCount = _helperData.UsedCount * useReward;

        var rewardAmount = quotient * GameInfoManager.Instance.HelperInfo.HelperRewardAmount;
        if (rewardAmount < 0)
            rewardAmount = 0;
        return rewardAmount + useCount;
    }

    IEnumerator SetRewardTMP()
    {
        var maxval = GameInfoManager.Instance.HelperInfo.HelperSetTerm;
        TimeSpan current = DateTime.Now - new DateTime(_helperData.ReceivedTime, DateTimeKind.Utc);
        var value = 0;
        if (maxval > (int)current.TotalSeconds)
            value = maxval - (int)current.TotalSeconds;
        else
            value= maxval;
        yield return new WaitForSecondsRealtime(value);
        _rewardTMP.text = GetRewardAmount().ToString();
        SetRewardButtonDimd(GetRewardAmount() <= 0);

        if (_tmpCoroutine != null)
        {
            StopCoroutine(_tmpCoroutine);
            _tmpCoroutine = null;
        }
        if (_tmpCoroutine == null)
            _tmpCoroutine = StartCoroutine(SetRewardTMP());
    }
    // 조력자 편성 버튼
    public bool OnClickSetHelpler()
    {
        if (string.IsNullOrEmpty(_helperData.PcTid))
            return true;

        if (new DateTime(_helperData.UpdateTime) != DateTime.MinValue && GetResetTime(_helperData.UpdateTime) <GameInfoManager.Instance.HelperInfo.HelperSetTerm)
        {
            UIManager.Instance.ShowToastMessage("str_clan_helper_set_deny_01"); //조력자는 변경 후 1분 뒤에 다시 변경 가능합니다.
            return false;
        }
        if (GetRewardAmount() > 0)
        {
            UIManager.Instance.ShowToastMessage("str_clan_helper_set_deny_02"); // 조력자 보상 수령 후 편성 변경이 가능합니다.
            return false;
        }
        return true;
    }

    // 조력자 보상
    public void OnClickGetReward()
    {
        //토스토로 할 지 딤드로 할 지 (미편성)
        if (string.IsNullOrEmpty(_helperData.PcTid))
        {
            UIManager.Instance.ShowToastMessage("str_ui_toast_message_receive_deny_01");        //수령 가능한 보상이 없습니다.
            return;
        }
        if (GetResetTime(_helperData.ReceivedTime) < GameInfoManager.Instance.HelperInfo.HelperSetTerm)
        {
            SetRewardButtonDimd(GetRewardAmount() <=0);
            UIManager.Instance.ShowToastMessage("str_clan_helper_set_deny_01"); //조력자는 변경 후 1분 뒤에 다시 변경 가능합니다.
            return;
        }
     
        var slotKey = "data_" + _helperCellIndex;
        RestApiManager.Instance.RequestHelperGetReward(slotKey, ShowRewardResult);
    }


    private void ShowRewardResult(JObject response)
    {
        var rewardResult = response["result"]["rewardResult"];
        if (rewardResult == null)
            return;

        StopAllCoroutine();
        _rewardTMP.text = "0";
        SetRewardButtonDimd(true);
        //시간, 사용 횟수, 보상 갯수
        var time = rewardResult["time"].ToObject<int>();
        var count = rewardResult["count"].ToObject<int>();
        var timeReward = rewardResult["timeReward"].ToObject<int>();
        var countReward = rewardResult["countReward"].ToObject<int>();

        if (_tmpCoroutine == null)
            _tmpCoroutine = StartCoroutine(SetRewardTMP());
       
        UIManager.Instance.Show<UIPopupHelperReward>(Utils.GetUIOption(
            UIOption.Time, time,
            UIOption.Count, count,
            UIOption.Value, timeReward,
            UIOption.Value2, countReward));
    }

    private void SetRewardButtonDimd(bool isOn)
    {
        _rewardDimd.SetActive(isOn);
    }

    public void OnClickRewardDimd()
    {
        UIManager.Instance.ShowToastMessage("str_ui_toast_message_receive_deny_01");            //수령 가능한 보상이 없습니다.
    }

    public void StopAllCoroutine()
    {
        if (_tmpCoroutine != null)
        {
            StopCoroutine(_tmpCoroutine);
            _tmpCoroutine = null;
        }
      
    }
}
