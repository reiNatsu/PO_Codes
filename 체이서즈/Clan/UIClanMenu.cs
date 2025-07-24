using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClanMenu : UIBase
{

    public override void Show(Dictionary<UIOption, object> optionDict)
    {

        UpdateUI();
    }

    public void UpdateUI()
    {
        //var clanid = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
        //_helperDimd.SetActive(string.IsNullOrEmpty(clanid));
    }

 
    // 클랜 버튼 클릭
    public void OnClickGoClan()
    {
        GameInfoManager.Instance.IsCheckAttendance = true;
        GameInfoManager.Instance.CheckIsUpdateUserConfig(GameInfoManager.Instance.CheckClanConfig) ;
        Close();
    }

    // 조력자 버튼 클릭
    public void OnClickGoHelper()
    {
        Debug.Log("<color=#4cd311> 클랜 조력자 버튼 클릭</color>");
        UIManager.Instance.Show<UIHelperCharacter>();
        Close();
    }

    // 조력자 dimd 클릭
    public void OnClickHelperDimd()
    {

        Debug.Log("<color=#4cd311> 클랜 조력자 딤드 클릭</color>");
        UIManager.Instance.ShowToastMessage("str_clan_helper_can_use_check_01"); //조력자는 클랜 가입 후 이용 가능합니다.
    }
}
