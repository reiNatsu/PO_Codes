
using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public enum INPUT_NAME_OTHER
{ 
   Name = 16,
   Notice = 100
}

public class CheckBadInputText : MonoBehaviour
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

    [SerializeField] private TMP_InputField _clanNameTMP;
    [SerializeField] private INPUT_NAME_OTHER _inputType;
    private List<string> _banString = new List<string>();
    private Action _onClick = null;

    private void Start()
    {
        Debug.Log("<color=#4cd311> "+gameObject.transform.parent.name+"(<b> " +(int)_inputType+ ")</b></color>");

        _clanNameTMP.onValueChanged.AddListener(
(word) =>
{
    //_clanNameTMP.text = TrimStringWithLengthOfByte(Regex.Replace(word, @"[^0-9a-zA-Z ㄱ-ㅎㅏ-ㅣ가-힣ㆍᆢ]", ""), 16);
    _clanNameTMP.text = TrimStringWithLengthOfByte(Utils.GetStringInputException(word), (int)_inputType);
    Debug.Log("<color=#4cd311> 입력 중········(<b> " +_clanNameTMP.text.Length+ ")</b></color>");
});
        _clanNameTMP.text = "";
        _banString.Clear();
        var tableData = TableManager.Instance.Ban_Table;

        for (int i = 0; i<tableData.ArrayCount; i++)
        {
            _banString.Add(tableData.GetDataByIndex(i).Ko);
        }
    }

    public void InitAction(Action onClick)
    {
        _onClick = onClick;
    }

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
        if (_inputType == INPUT_NAME_OTHER.Name)
        {
            textValue = textValue.Replace(" ", string.Empty);
            textValue = textValue.Replace("\n", string.Empty);
        }
       
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
            //if (_inputField.text.Contains(_consonant[i]))
            //{
            //    return true;
            //}
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


    public void OnClickCheckInput()
    {
        var name = _clanNameTMP.text;
        if (!string.IsNullOrEmpty(name))
        {
            var stringLen = GetStrintByte(_clanNameTMP.text);
            Debug.Log($"입력된 스트링 {stringLen}");

            if (CheckBadLanguage(_clanNameTMP.text))
            {
                UIManager.Instance.ShowToastMessage("str_ui_profile_swearguide_01");
                return;
            }
            if (CheckConsonant())
            {
                UIManager.Instance.ShowToastMessage("str_ui_profile_formguide_01");
                return;
            }

            if (stringLen <4)
            {
                UIManager.Instance.ShowToastMessage("str_ui_profile_formguide_01");
                return;
            }
            _onClick?.Invoke();
        }
        else
        {
            string str = LocalizeManager.Instance.GetString("str_ui_shop_failed_enoughgoods");// 재화가 부족합니다.
            UIManager.Instance.ShowToastMessage($"{str}");
            return;
        }

    }
}
