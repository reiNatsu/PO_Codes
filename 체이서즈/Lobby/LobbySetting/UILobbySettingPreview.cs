using LIFULSE.Manager;
using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILobbySettingPreview : UIBase
{
    //[SerializeField] private GameObject _changeOn;
    [SerializeField] private UILobbyTypeTab _conversion;
    [SerializeField] private List<UILobbySettingSlot> _slots = new List<UILobbySettingSlot>();

    private int _slotIndex = -1;
    private bool _isRandom = false;
    public override void Refresh()
    {
       
        base.Refresh();
        _slotIndex = -1;
        UpdateUI();
    }
    public override void Hide()
    {
        base.Hide();
        LobbyController.Instance.ActiveDissolve();
    }
    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
       
        
        //LobbyManager.instance.SetRandomLobbySlotNo();
    }
    public override void Init()
    {
        base.Init();
    }

    

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        //_slotIndex = -1;

       
        UpdateUI();
    }

    private void UpdateUI()
    {
        _slotIndex = -1;
        _isRandom = CustomLobbyManager.Instance.IsRandomSelect;
        for (int n = 0;n< _slots.Count; n++)
        {
            int index = n;
            _slots[n].InitSlot(index, (idx) => OnClickSettingSlot(idx));
        }
        
        if (_isRandom)
            _conversion.On();
        else
            _conversion.Off();
        // _changeOn.gameObject.SetActive(_isRandom);
    }

    public void OnClickSetRandom()
    {
        if (_isRandom)
        {
            _isRandom = false;
            _conversion.Off();
        }
        else
        {
            _isRandom = true;
            _conversion.On();
        }
        //_changeOn.gameObject.SetActive(_isRandom);
        //랜덤 선택 api
        CustomLobbyManager.Instance.IsRandomSelect = _isRandom;
        
    }

    public void OnClickSettingSlot(int index)
    {
        if (_slotIndex == index)
            return;

        if(_slotIndex != -1)
            Debug.Log("<color=#9efc9e>_slotIndex != -1</color>");

        _slotIndex = index;

        UIManager.Instance.Show<UILobbySetting>(Utils.GetUIOption(UIOption.Index, index));
    }
}
