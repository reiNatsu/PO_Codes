using LIFULSE.Manager;
using System;
using UnityEngine;

public class UILobbySettingSlot : MonoBehaviour
{
    
    [SerializeField] private ExTMPUI _slotNumber;
    [SerializeField] private GameObject _slotNonData;
    [SerializeField] private GameObject _characterImgObj;
   [SerializeField] private ExImage _slotBg;
    [SerializeField] private ExImage _charImg;
    [SerializeField] private ExButton _button;

    [SerializeField] private int _slotIndex;

    private Lobby_TableData _data;
    private string _lobbyTableId;
    private string _slotimage;
    private string _slottableid;
    //private LOBBY_INTERACTION_TYPE _slotype;
    private string _slotcharacter;
    private bool _slotisspine;

    // 기기 저장 할 key string
    
    public string SlotCharacter { get => _slotcharacter; }
    public ExButton Button { get => _button; }

    private void Awake()
    {
      
    }

    public void InitSlot(int index, Action<int> onClick)
    {
        _slotIndex = index;
        _slotNumber.ToTableText("str_lobby_slot_title_default", (_slotIndex+1));        // 커스텀로비{0}
        //_slotype = LOBBY_INTERACTION_TYPE.none;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick(index));
        }

        _data = new Lobby_TableData();
        //_lobbyTableId = GameInfoManager.Instance.GetCustomLobbyTable(_slotIndex);
        _lobbyTableId = CustomLobbyManager.Instance.GetTableTid(_slotIndex);

        if (!string.IsNullOrEmpty(_lobbyTableId))
        {
            //if (TableManager.Instance.Lobby_Table[_lobbyTableId] != null)
            //{
            //    _data = TableManager.Instance.Lobby_Table[_lobbyTableId];
            //    _lobbyTableId = _data.Group_Id;
            //}
            if (TableManager.Instance.Lobby_Table.GetData(_lobbyTableId) !=  null)
                _data = TableManager.Instance.Lobby_Table.GetData(_lobbyTableId);
        }
        else
            _data = null;

        UpdateSlotUI();
    }



    public void UpdateSlotUI()
    {
        if (_data == null || string.IsNullOrEmpty(_data.Tid))
        {
            _slotNonData.SetActive(true);
            _slotBg.gameObject.SetActive(false);
            _characterImgObj.SetActive(false);
            _charImg.gameObject.SetActive(false);

        }
        else
        {
            _slotNonData.SetActive(false);
            _slotBg.gameObject.SetActive(true);
            _characterImgObj.SetActive(true);
            _slotBg.SetSprite(_data.Lobby_Slot_Img);
            _slotcharacter = CustomLobbyManager.Instance.GetCharacterTid(_slotIndex);
            //_slotcharacter = GameInfoManager.Instance.GetCustomLobbyCharacter(_slotIndex);
            if (!string.IsNullOrEmpty(_slotcharacter))
            {
                _charImg.gameObject.SetActive(true);
                //_slotcharacter = GameInfoManager.Instance.GetCustomLobbyCharacter(_slotIndex);
                //_slotcharacter = GameInfoManager.Instance.LodaCustomLobbyCharacter(_slotIndex);
                var data = TableManager.Instance.Character_PC_Table[_slotcharacter];
                _charImg.SetSprite(data.Char_Icon_Square_01.ToCostumeKey(data.Tid));
                //_slotisspine = GameInfoManager.Instance.GetCustomLobbyIsSpine(_slotIndex);
                //_slotisspine = GameInfoManager.Instance.LodaCustomLobbyIsSpine(_slotIndex);
            }
            else
            {
                _charImg.gameObject.SetActive(false);
            }

        }
    }

    public void UpdateSlotImage()
    {
        if (_data == null)
        {
            _slotNonData.SetActive(true);
            _slotBg.gameObject.SetActive(false);
            _charImg.gameObject.SetActive(false);
        }
        else
        {
            _slotNonData.SetActive(false);
            _slotBg.gameObject.SetActive(true);
            _slotBg.SetSprite(_data.Lobby_Slot_Img);
        }
    }


    // 리셋 버튼
    public void OnClickReset()
    {
        ResetSlot();
    }

    public void ResetSlot()
    {
        if (!CustomLobbyManager.Instance.HasData(_slotIndex))
            return;

        //if (CustomLobbyManager.Instance.CustomLobbyDatas.Count == 1)
        if(!CustomLobbyManager.Instance.GetSavedData())
        {
            string returnmsg = LocalizeManager.Instance.GetString("str_lobby_delet_deny_01");       //최소 1개의 커스텀 로비가 존재해야 합니다.
            UIManager.Instance.ShowToastMessage(returnmsg);
            return;
        }

        if (CustomLobbyManager.Instance.HasCustomLobby())
            {
            _slotimage = null;
            _slotcharacter = null;
            _slotBg.gameObject.SetActive(false);
            _charImg.gameObject.SetActive(false);
            _slotNonData.SetActive(true);
            CustomLobbyManager.Instance.DeleteCustomLobbyData(_slotIndex);
        }
        else
        {
            string returnmsg = LocalizeManager.Instance.GetString("str_lobby_delet_deny_01");       //최소 1개의 커스텀 로비가 존재해야 합니다.
            UIManager.Instance.ShowToastMessage(returnmsg);
        }
    }
}
