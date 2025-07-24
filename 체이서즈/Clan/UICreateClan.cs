
using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICreateClan : UIBase
{
    #region string
        List<char> _consonant = new List<char>()
        {
             'ㄱ','ㄲ','ㄴ','ㄷ','ㄸ','ㄹ','ㅁ','ㅂ','ㅃ',
             'ㅅ','ㅆ','ㅇ','ㅈ','ㅉ','ㅊ','ㅋ','ㅌ','ㅍ','ㅎ','ㅏ','ㅐ','ㅑ','ㅒ','ㅓ','ㅔ','ㅕ','ㅖ',
             'ㅗ','ㅘ','ㅙ','ㅚ','ㅛ','ㅜ','ㅝ','ㅞ',
            'ㅟ','ㅠ','ㅡ','ㅢ','ㅣ'
        };
    #endregion

    [Header("더미 값")]
    [SerializeField] private string _neddGoodID;
    [SerializeField] private int _needGoodValue;

    [Header("UI Object")]
    [SerializeField] private TMP_InputField _clanNameTMP;
    [SerializeField] private ExTMPUI _needValueTMP;
    [SerializeField] private ExTMPUI _holdValueTMP;

    private List<string> _banString = new List<string>();
    private int _holdValue = 0;

    public override void Init()
    {
        base.Init();
    }
    private void Start()
    {
       
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _clanNameTMP.onValueChanged.AddListener(
        (word) =>
        {
            _clanNameTMP.text = TrimStringWithLengthOfByte(Utils.GetStringInputException(word), 16);
        });
        _clanNameTMP.text = "";
        _banString.Clear();
        var tableData = TableManager.Instance.Ban_Table;

        for (int i = 0; i<tableData.ArrayCount; i++)
        {
            _banString.Add(tableData.GetDataByIndex(i).Ko);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        _needValueTMP.text = _needGoodValue.ToString();

        SetHoldGood();
    }
    private void SetHoldGood()
    {
        _holdValue = GameInfoManager.Instance.GetCurrency(_neddGoodID);
        if (_neddGoodID.Equals(TICKET_TYPE.i_cash.ToString()) ||_neddGoodID.Equals(TICKET_TYPE.i_free_cash.ToString()))
        {
            _holdValue = GameInfoManager.Instance.GetAmount(_neddGoodID);
        }

        _holdValueTMP.text = _holdValue.ToString();
        if (_holdValue < _needGoodValue)
            _needValueTMP.SetColor("#FF0000");
        else    //3A3A3A
            _needValueTMP.SetColor("#3A3A3A");

    }

    public void OnClickCreateClan()
    {
        var name = _clanNameTMP.text;
        if (string.IsNullOrEmpty(name))
        {
            string str = LocalizeManager.Instance.GetString("str_clan_create_deny_msg_04");    //클랜명은 2~8글자 사이로 입력해야 합니다.
            UIManager.Instance.ShowToastMessage($"{str}");
            return;
        }
        if (_holdValue < _needGoodValue )
        {
            string str = LocalizeManager.Instance.GetString("str_clan_create_deny_msg_01");    // 곡옥이 부족합니다.
            UIManager.Instance.ShowToastMessage($"{str}");
            return;
        }
        if (!GameInfoManager.Instance.CheckCanJoinClanTime(GameInfoManager.Instance.ClanInfo.UserClanData, isCreate:true))
        {
            return;
        }
      
        var stringLen = GetStrintByte(_clanNameTMP.text);
        Debug.Log($"닉네임 스트링 {stringLen}");

        if (CheckBadLanguage(_clanNameTMP.text))
        {
            UIManager.Instance.ShowToastMessage("str_ui_profile_swearguide_01");        //금칙어가 포함되어 있습니다.
            return;
        }
        if (CheckConsonant())
        {
            UIManager.Instance.ShowToastMessage("str_ui_profile_formguide_01");         //사용 불가능한 양식입니다.
            return;
        }

        if (stringLen <4)
        {
            UIManager.Instance.ShowToastMessage("str_clan_create_deny_msg_04");     //클랜명은 2~8글자 사이로 입력해야 합니다.
            return;
        }

        int grade = (int)MemberGrade.master;
        ClanConfig clanconfig = GameInfoManager.Instance.ClanInfo.GetUserClanConfig();


        RestApiManager.Instance.RequestClanCreate(name, clanconfig, (response) => {

            // 결과가 ""로 오면 이미 동일한 이름의 클랜이 있는 것이므로 토스메세지 보여주고 return 시키기
            //  if (string.IsNullOrEmpty(response["result"].ToString()))
          
            RestApiManager.Instance.CheckIsEmptyResult(/*ClanState.Overlapped, */ response, () => {
                UIManager.Instance.ShowToastMessage("str_clan_create_complete_msg");// 클랜이 생성되었습니다
                GameInfoManager.Instance.IsCheckAttendance = false;
                GameInfoManager.Instance.TransferClanInfo(GameInfoManager.Instance.IsCheckAttendance);
            });
        });
    }

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
        _clanNameTMP.text = string.Empty;
    }


    public void OnInputSelect(bool isHide)
    {
        if (!isHide && !string.IsNullOrEmpty(_clanNameTMP.text))
            _clanNameTMP.placeholder.gameObject.SetActive(false);

        _clanNameTMP.placeholder.gameObject.SetActive(!isHide);
    }
    #region[길드 이름 검사]
    public string TrimStringWithLengthOfByte(string sVal, int iByteLength)
    {
        string sTemp = "";
        string sRet = "";
        int iByteLen = 0;

        for (int i = 0; i < sVal.Length; i++)
        {
            string sStrOfCurIndex = sVal.Substring(i, 1);
            sTemp = sTemp + sStrOfCurIndex;
            iByteLen += Mathf.Min(Encoding.UTF8.GetByteCount(sStrOfCurIndex), 2);
            if (iByteLen > iByteLength)
            {
                sRet = sTemp.Substring(0, sTemp.Length - 1);
                break;
            }
            else sRet = sTemp;
        }
        return sRet;
    }

    private bool CheckBadLanguage(string textValue)
    {
        textValue = textValue.Replace(" ", string.Empty);
        textValue = textValue.Replace("\n", string.Empty);
        for (int i = 0; i< _banString.Count; i++)
        {
            if (textValue.Contains(_banString[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 자음 모음만 있는지 체크
    /// </summary>
    /// <returns></returns>
    public bool CheckConsonant()
    {
        for (int i = 0; i<_consonant.Count; i++)
        {
            if (_clanNameTMP.text.Contains(_consonant[i]))
            {
                return true;
            }
        }
        return false;
    }

    private int GetStrintByte(string str)
    {
        char[] charObj = str.ToCharArray();
        int resultByte = 0;
        for (int i = 0; i< charObj.Length; i++)
        {
            byte oF = (byte)((charObj[i] & 0xff00)>>7);
            byte oB = (byte)((charObj[i] & 0x00ff));

            if (oF == 0)
            {
                resultByte++;
            }
            else
            {
                resultByte += 2;
            }
        }
        return resultByte;
    }
    #endregion
}
