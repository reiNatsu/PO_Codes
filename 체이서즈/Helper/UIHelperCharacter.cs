using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIHelperCharacter : UIBase
{
    [SerializeField] private List<HelperCharacterCell> _helperCells;

    [Header("캐릭터 편성 팝업")]
    //[SerializeField] private AlertInfo _characterListPopup;
    //[SerializeField] private UIPopupHelperSelect _characterListPopup;
    //[SerializeField] private UICharacterList _charList;          // 캐릭터 리스트
    

    private List<CharacterCellData> _charDatas;
    private int _clickIndex;
    private string _selectCharacter;

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    
    public override void Init()
    {
        base.Init();

       // CharacterListInit();
    }

    public override void Refresh()
    {
        base.Refresh();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _clickIndex = -1;

        SetHelperCellDatas();
    }


    public void SetHelperCellDatas()
    {
        var helpers = GameInfoManager.Instance.HelperInfo.Helpers;
        Debug.Log("<color=#4cd311>helpers<b>("+helpers.Count+")</b></color>");
        for (int n = 0; n< _helperCells.Count; n++)
        {
            var index = n;
            _helperCells[n].InitData(helpers["data_" +index.ToString()]);
            if (_helperCells[n].SetButton != null)
                _helperCells[n].SetButton.onClick.AddListener(() => OnClickHelperCell(index));
        }
    }

    public void OnClickHelperCell(int index)
    { 
        Action<string> onClickOK = (selectCharacter) => {
            var helpers = GameInfoManager.Instance.HelperInfo.Helpers;
            
            var slotindex = "data_"+_clickIndex;
            if (!string.IsNullOrEmpty(selectCharacter))
            {
                foreach (var info in helpers)
                {
                    if (!string.IsNullOrEmpty(selectCharacter) && info.Key != slotindex && info.Value.PcTid.Equals(selectCharacter))
                    {
                        UIManager.Instance.ShowToastMessage("str_ui_formation_ban_01");//이미 편성된 체이서 입니다.
                        return;
                    }
                }
            }
            OnClickEndSetting(selectCharacter);
        };
        _clickIndex = index;

        if (!_helperCells[index].OnClickSetHelpler())
            return;
        Debug.Log("<color=#4cd311><b>("+_helperCells[_clickIndex].name+")</b></color>");


        var popuptitle = LocalizeManager.Instance.GetString("str_ui_helper_set_title");
        //_characterListPopup.gameObject.SetActive(true);

        UIManager.Instance.Show<UIPopupHelperSelect>(Utils.GetUIOption(UIOption.Index, popuptitle
            , UIOption.Int, _clickIndex
            , UIOption.Action, onClickOK));

    }

    public void OnClickEndSetting(string selectCharacter)
    {
        // DosaInfo => 선택 된 도사 설정.

        HelperData helperData = null;

        if (!string.IsNullOrEmpty(selectCharacter))
            helperData = new HelperData(selectCharacter);

        var key = "data_" + _clickIndex.ToString();

     
        RestApiManager.Instance.RequestHelperSetData(new Dictionary<string, HelperData> { { key, helperData } },
              (response) =>
              {
                  _helperCells[_clickIndex].InitData(GameInfoManager.Instance.HelperInfo.Helpers[key]);
                  _selectCharacter = null;
                  //_charList.Refresh();
                  var uiHelperSettingPopup = UIManager.Instance.GetUI<UIPopupHelperSelect>();
                  if (uiHelperSettingPopup != null && uiHelperSettingPopup.gameObject.activeInHierarchy)
                  {
                      uiHelperSettingPopup.RefreshCharacterList();
                      uiHelperSettingPopup.OnClickClose();
                  }
                     
                  //_characterListPopup.Close();
              });
      
    }
}
