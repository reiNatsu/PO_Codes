using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BRG_DamageUI;

public class UIPopupHelperSelect : UIPopup
{
    [SerializeField] private AlertInfo _characterListPopup;
    [SerializeField] private UICharacterList _charList;          // 캐릭터 리스트

    private List<CharacterCellData> _charDatas;

    private Action<string> _onClickOk = null;
    private string _title;
    private int _slotIndex = 0;
    private string _selectCharacter;
    public override void Init()
    {
        base.Init();
       
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        base.Show(optionDict);

        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Index, out var title))
            {
                _title = (string)title;
            }
            if (optionDict.TryGetValue(UIOption.Int, out var slotIndex))
            {
                _slotIndex = (int)slotIndex;
            }
            if (optionDict.TryGetValue(UIOption.Action, out var onClickOk))
            {
                _onClickOk = (Action<string>)onClickOk;
            }
        }
        _characterListPopup.Show(PopupButtonType.OK_CANCEL, null, true, popuptitle: _title,
    onClickOk: OnClickOk);

        _charList.SetCharacterClickCallback(OnclickCharacterBtn,null);

        _charList.InitFilter();
        CharacterListInit();
        _charList.SettingInit();
        _charList.SortTypeInit();

        _charList.SetAllDeSelect();
        SetSlotCharacterUI();
    }

    public void SetSlotCharacterUI()
    {
        var slotid = "data_"+_slotIndex;
        if (GameInfoManager.Instance.HelperInfo.Helpers.ContainsKey(slotid)
            && !string.IsNullOrEmpty(GameInfoManager.Instance.HelperInfo.Helpers[slotid].PcTid))
        {
            _selectCharacter = GameInfoManager.Instance.HelperInfo.Helpers[slotid].PcTid;
            _charList.UpdateSelectCell(GameInfoManager.Instance.HelperInfo.Helpers[slotid].PcTid);
        }
        else
            _selectCharacter = null;
    }


    public void CharacterListInit()
    {
        _charList.SetCellData(CharacterListContent.HelperSetting);
        _charDatas = _charList.GetCellData();
        //_charList.createScroll = 
        UpdateCallbackCell();
    }

    private void UpdateCallbackCell()
    {
        for (int i = 0; i<_charDatas.Count; i++)
        {
            DosaInfo info = GameInfoManager.Instance.CharacterInfo.GetDosa(_charDatas[i].Tid);
            _charDatas[i].SetCallback((info) => OnclickCharacterBtn(info));
        }
    }

    public void OnclickCharacterBtn(DosaInfo dosaInfo)
    {
        var slotIndex = "data_"+_slotIndex;
        var helpers = GameInfoManager.Instance.HelperInfo.Helpers;
        foreach (var info in helpers)
        {
            if (/*!string.IsNullOrEmpty(_selectCharacter) &&*/info.Key != slotIndex && info.Value.PcTid.Equals(dosaInfo.Tid))
            {
                UIManager.Instance.ShowToastMessage("str_ui_formation_ban_01");//이미 편성된 체이서 입니다.
                return;
            }

            // 이미 선택되어 있는 캐릭터면 선택 해제
            if (!string.IsNullOrEmpty(_selectCharacter) && _selectCharacter.Equals(dosaInfo.Tid)&&info.Key == slotIndex)
            {
                _charList.UpdateDeSelectCell(_selectCharacter);
                _charList.UpdateAlreadySelectCell(_selectCharacter, false);
                dosaInfo.SettingHelper = false;
                _selectCharacter = string.Empty;
                return;
            }
        }

        if (!string.IsNullOrEmpty(_selectCharacter))
        {
            _charList.UpdateDeSelectCell(_selectCharacter);
            var prevDosa = GameInfoManager.Instance.CharacterInfo.GetDosa(_selectCharacter);
            prevDosa.SettingHelper = false;
        }

        _charList.UpdateSelectCell(dosaInfo.Tid);
        dosaInfo.SettingHelper = true;
        _selectCharacter = dosaInfo.Tid;


        var cell = _charList.GetCell();
        for (int i = 0; i<cell.Count; i++)
        {
            cell[i].UpdateCellData(cell[i].DataIndex);
        }
    }

    public void RefreshCharacterList()
    {
        _charList.Refresh();
    }

    public void OnClickOk()
    {
        if (_onClickOk != null)
            _onClickOk.Invoke(_selectCharacter);

        OnClickClose();
    }
}
