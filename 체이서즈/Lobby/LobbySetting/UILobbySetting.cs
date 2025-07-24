using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public enum SELCET_CATEGORY
{ 
   ILLUST,
   CUSTOM
}

public class UILobbySetting : UIBase
{
    [Header("리스트 오브젝트")]
    [SerializeField] private GameObject _illustObj;     // 일러스트 보여주는 리스트 오브젝트
    [SerializeField] private GameObject _customObj;     // 캐릭터 & 배경 보여주는 리스트 오브젝트

    [SerializeField] private RecycleScroll _scroll;

    [SerializeField] private ExTMPUI _slotnumber;
    [SerializeField] private GameObject _closeDimd;
    [SerializeField] private GameObject _saveBTN;

    [SerializeField] private ExTMPUI _listTitle;

    [SerializeField] private UICharacterList _charList;          // 캐릭터 리스트
   // [SerializeField] private GameObject _illustList;                // 일러스트, 메모리얼, 배경 리스트 
    [SerializeField] private GameObject _rightObj;          // 왼쪽 리스트 탭
    [SerializeField] private GameObject _leftObj;            // 오른쪽 버튼 탭
    [SerializeField] private ExButton _showBtn;             //  UI 보이도록 하는 버튼(빈 공간 누르면 ui 보이도록)

    [SerializeField] private GameObject _illustTitleObj;        // 일러스트 타입 제목 Obj;

    [SerializeField] private Transform _characterPos;
    [SerializeField] private ExTMPUI _noDataTMP;

    [SerializeField] private Animator _rightAnim;
    [SerializeField] private Animator _leftAnim;
    [SerializeField] private List<UILobbyTypeTab> _selectTypeTabs;
    [SerializeField] private List<UILobbyTypeTab> _selectCustomTabs;


    private List<LobbyPreviewCell> _cells;
    private List<LobbyPreviewCellData> _cellDatas = null;
    private List<LobbyPreviewCell> _previewSlots;
    private List<CharacterCellData> _charDatas;
    protected DosaInfo[] _characterDataList = new DosaInfo[4];

    private Lobby_TableData _data;
    private string _selectedCharacter;
    private LOBBY_INTERACTION_TYPE _currenttype;

    private int _currentSlotIndex;
    private int _tabIndex;
    private int _customIndex;
    private bool _isListOn = false;
    
    private string _seltablegroupid;

    private int _previewcellIndex;

    public int CurrentSlotNumber{ get { return _currentSlotIndex; } }
    public int PreviewCellIndex{ get { return _previewcellIndex; } set { _previewcellIndex = value; } }

    private bool _isSave = false;
    private int _preSelectIndex;
    private bool _isDefault = false;
    public bool IsDefault { get { return _isDefault; } }
    public override void Refresh()
    {
        base.Refresh();
        CharacterListInit();
        UpdateUI();
      
        if (_isListOn)
        {
            _rightAnim.Play("Sidebar_On", -1, 1);
            _rightAnim.speed = 0;
        }
        else
        {
          
            _rightAnim.Play("Sidebar_Off", -1, 1);
            _rightAnim.speed = 0;
        }
    
    }

    public override void Hide()
    {
        if (LobbyController.Instance != null)
        {
            LobbyController.Instance.IllistrationObjOnOff(false);
        }

        //위치저장

        base.Hide();
    }

    public override void Close(bool needCached = true)
    {
        //RefreshSetting(true);
        _rightAnim.Play("Sidebar_Off", -1, 0);
        _rightAnim.speed = 0;

        if (_tabIndex > -1)
            _selectTypeTabs[_tabIndex].Refresh();

        if (_isListOn)
            SetListClose(false);

        _tabIndex = -1;
        base.Close(needCached);
        if (!_isSave)
        {
            CustomLobbyManager.Instance.ActiveLobbyData(_preSelectIndex);
        }
    }

    private void SetListClose(bool isOn)
    {
        _closeDimd.SetActive(isOn);
        _saveBTN.SetActive(isOn);
         
    }

    public override void Init()
    {
        base.Init();
        _scroll.Init();

        _cells = _scroll.GetCellToList<LobbyPreviewCell>();
        _cellDatas = new List<LobbyPreviewCellData>();
        //CharacterListInit();
        _charList.UpdateCellDataCallback = (x) => { UpdateCharacterCellUIState(x); };
    }

    public void CharacterListInit()
    {
        _charDatas = _charList.GetCellData();
        _charList.createScroll = UpdateCallbackCell;
        _charList.SetCellData();
        _charList.SetCharacterClickCallback(OnclickCharacterBtn, null);
    }
    private void UpdateCallbackCell()
    {
        for (int i = 0; i<_charDatas.Count; i++)
        {
            DosaInfo info = GameInfoManager.Instance.CharacterInfo.GetDosa(_charDatas[i].Tid);
            _charDatas[i].SetCallback((x) => OnclickCharacterBtn(info));
        }
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        CharacterListInit();
        _isSave = false;
        _preSelectIndex = CustomLobbyManager.Instance.SelectIndex;
        
        
        for (int n = 0; n< _selectTypeTabs.Count; n++)
        {
            int index = n;
            _selectTypeTabs[n].InitTab(index, (idx) => OnClickLeftTypeTab(idx));
        }
        for (int s = 0; s< _selectCustomTabs.Count; s++)
        {
            int index = s;
            _selectCustomTabs[s].InitTab(index, (idx) => OnClickSelectType(idx));
        }

        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Index, out var slotindex))
                _currentSlotIndex = (int)slotindex;
        }
        CustomLobbyManager.Instance.CreatTempData(_currentSlotIndex);
        CustomLobbyManager.Instance.ActiveLobbyData(-1);
        _previewcellIndex = -1;
        _tabIndex = -1;
        _customIndex = -1;

        _charList.InitFilter();
        _charList.SettingInit();
        _charList.SortTypeInit();
       
        _seltablegroupid = string.Empty;
        _selectedCharacter = string.Empty;
        _isListOn = false;

        _showBtn.gameObject.SetActive(false);
        UpdateUI();
    }

    protected virtual void UpdateCharacterCellUIState(List<CharacterCellData> characterCellData = null, bool useSort = true)
    {
        var cell = _charList.GetCell();
        if (characterCellData == null)
        {
            characterCellData = _charList.GetCellData();
        }

        var inPartyCharList = new List<DosaInfo>();
        for (int i = 0; i<_characterDataList.Length; i++)
        {
            if (_characterDataList[i] !=null)
            {
                if (!_characterDataList[i].Tid.IsNullOrEmpty())
                {
                    inPartyCharList.Add(_characterDataList[i]);
                }
            }
        }

        for (int i = 0; i<characterCellData.Count; i++)
        {
            characterCellData[i].IsAlreadySet = false;
            characterCellData[i].Selected = false;
            characterCellData[i].IncludePartyIndex = -1;
            for (int index = 0; index < inPartyCharList.Count; index++)
            {
                if (inPartyCharList[index].Tid == characterCellData[i].Tid)
                {
                    if (_tabIndex ==0)
                    {
                        if (inPartyCharList[index].HelperTargetKey.IsNullOrEmpty())
                        {
                            characterCellData[i].Selected = true;
                        }
                        else
                        {
                            characterCellData[i].SelectText = "str_team_formation_deny_txt_01";
                        }
                    }
                    else
                    {
                        if (inPartyCharList[index].HelperTargetKey == characterCellData[i].Tid)
                        {
                            characterCellData[i].SelectText = "str_ui_team_character_formation_001";
                        }
                        else
                        {
                            characterCellData[i].SelectText = "str_team_formation_deny_txt_01";
                        }
                    }

                    characterCellData[i].IncludePartyIndex = index;
                    break;
                }
            }
        }

        if (useSort)
        {
            characterCellData.ToSort(_charList.SortType, _charList.Content, _charList.IsAsc, false, out characterCellData);
        }

        for (int i = 0; i<cell.Count; i++)
        {
            cell[i].SetDataCells(characterCellData);
            cell[i].UpdateCellData(cell[i].DataIndex);
        }
    }

    private void UpdateUI()
    {
        SetListClose(false);
        _slotnumber.text = "NO."+_currentSlotIndex.ToString() + " Solt";

        //_isInfo = false;
        _currenttype = LOBBY_INTERACTION_TYPE.none;
        _data = new Lobby_TableData();
        _seltablegroupid = CustomLobbyManager.Instance.GetTableTid(_currentSlotIndex);
        if (!string.IsNullOrEmpty(_seltablegroupid))
        {
            if (TableManager.Instance.Lobby_Table.GetData(_seltablegroupid) != null)
            {
                _data = TableManager.Instance.Lobby_Table.GetData(_seltablegroupid);
                _currenttype = _data.LOBBY_INTERACTION_TYPE;
                // _isInfo = true;
            }
            else
                _data = null;
        }
        else
            _data = null;

        // 데이터가 없는 빈 슬롯일경우 첫번째 탭 열어주기
        if (_data == null)
        {
            _isDefault = true;
            if (_tabIndex == -1)
                OnClickLeftTypeTab(0);
        }
        else
        {
            _isDefault = false;
            _rightAnim.Play("Sidebar_Off", -1, 1);
            _rightAnim.speed = 0;
        }
        //_seltype= _currenttype;
        
        // 배경 업데이트   
        UpdateCustomBGUI();

        // 캐릭터 타입이면 캐릭터 보여주기
        // _selectedCharacter = GameInfoManager.Instance.GetCustomLobbyCharacter(_currentSlotIndex);
        if (_data != null)
            _selectedCharacter = CustomLobbyManager.Instance.GetCharacterTid(_currentSlotIndex);
        LobbyController.Instance.SetCharacter(_selectedCharacter);
    }

    public void UpdateCustomBGUI()
    {
        if (_data != null)
        {
            bool isSwitch = false;
           // _noDataTMP.gameObject.SetActive(false);
            switch (_data.Lobby_Bg_Type)
            {
                case "image":
                    LobbyController.Instance.SetBackground2D(_data.Lobby_Img);
                    LobbyController.Instance.SetBackground3D("");
                    LobbyController.Instance.SetIllustOptionLobby("");
                    break;
                case "mprefab":
                    LobbyController.Instance.SetBackground2D("");
                    LobbyController.Instance.SetBackground3D("");
                    LobbyController.Instance.SetIllustOptionLobby(_data.Lobby_Img);
                    break;
                case "lprefab":     //챕터 일러스트 배경만.
                    LobbyController.Instance.SetBackground2D("");
                    LobbyController.Instance.SetBackground3D(_data.Lobby_Img);
                    LobbyController.Instance.SetIllustOptionLobby("");
                    break;
            }
            LobbyController.Instance.LobbyOptionSwitch(true);

            if (_data.LOBBY_INTERACTION_TYPE == LOBBY_INTERACTION_TYPE.illust && !string.IsNullOrEmpty(_selectedCharacter))
                _selectedCharacter = null;

            SetCharacters();
        }
        else
        {
            //_noDataTMP.gameObject.SetActive(true);
            //_noDataTMP.ToTableText("str_lobby_select_01");
            var data = TableManager.Instance.Lobby_Table.GetLobbyDataList(LOBBY_INTERACTION_TYPE.defaultlobby).FirstOrDefault();
            _seltablegroupid = data.Group_Id;
            LobbyController.Instance.SetBackground2D("");
            LobbyController.Instance.SetBackground3D(data.Lobby_Img);
            LobbyController.Instance.SetIllustOptionLobby("");
        }
    }

    public void UpdateSelectImageData(Lobby_TableData data)
    {
        _data =data;
        _seltablegroupid = _data.Group_Id;
    }

    // illust, memorial, bg 리스트 테이터 업데이트.
    private void UpdateLists(LOBBY_INTERACTION_TYPE type)
    {
        _previewcellIndex = -1;
       _cellDatas.Clear();
        _previewSlots = new List<LobbyPreviewCell>();
        
        List<Lobby_TableData> list = new List<Lobby_TableData>();
        list = TableManager.Instance.Lobby_Table.GetLobbyDataList(type).ToList();
        if (type == LOBBY_INTERACTION_TYPE.bg && TableManager.Instance.Lobby_Table.GetLobbyDataList(LOBBY_INTERACTION_TYPE.defaultlobby) !=null)
            list.Insert(0, TableManager.Instance.Lobby_Table.GetDefaultLobbyData());

        if (list == null)
            return;

        for (int n = 0; n< list.Count; n++)
        {
            bool isSelect = false;
            if (_data == null && list[n].LOBBY_INTERACTION_TYPE == LOBBY_INTERACTION_TYPE.defaultlobby)
                isSelect = true;
            if(_data != null && list[n].Group_Id.Equals(_data.Group_Id))
                isSelect = true;
            _cellDatas.Add(new LobbyPreviewCellData(list[n], isSelect, OnClickPreviewCell));
        }
        for (int i = 0; i<_cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(list.Count);
    }

    public void ResetSelectData(LOBBY_INTERACTION_TYPE type)
    {
        _currenttype =  type;
    }
   
    public void SetCharacters()
    {
            LobbyController.Instance.SetCharacter(_selectedCharacter);
    }

    private void UpdatelistUI(bool isListOn)
    {
        if (_rightObj == null)
            return;

        if (_isListOn == isListOn)
            return;
        //_leftAnim.enabled = true;
        PlayRightAnim(isListOn);
        
        if (!isListOn)
        {
            _selectTypeTabs[_tabIndex].Off();
            //_closeDimd.SetActive(false);
            SetListClose(false);
            _tabIndex = -1;
        }
    }

    private void PlayLeftAnim(bool isOpen)
    {
        if (isOpen)
        {
            if (_leftAnim.speed == 0)
                _leftAnim.speed = 1;
            _leftAnim.Play("Left_Appear", -1, 0);
        }
        else
        {
            _leftAnim.Play("Left_Appear", 0, -1);
            _leftAnim.speed = 0;
        }
    }

    private void PlayRightAnim(bool isOpen)
    {
        if (isOpen)
        {
            if (_rightAnim.speed == 0)
                _rightAnim.speed = 1;
            _rightAnim.Play("Right_Appear", -1, 0);
        }
        else
        {
            _rightAnim.Play("Right_Appear", 0, -1);
            _rightAnim.speed = 0;
        }
        _isListOn = isOpen;
        if (!_isListOn)
        {
            for (int n = 0; n<  _selectTypeTabs.Count; n++)
            {
                _selectTypeTabs[n].Off();
            }
            _tabIndex = -1;
        }
    }

    #region
    // 왼쪽의 책갈피타입 탭 클릭.
    public void OnClickLeftTypeTab(int index)
    {
        if (index == -1)
        {
            _selectTypeTabs[_tabIndex].Off();
            return;
        }
          
        if (_tabIndex == index)
            return;
       
        var selectTab = (LOBBY_INTERACTION_TYPE)index;
        _customIndex = -1;
        if (_tabIndex != -1)
        {
            _selectTypeTabs[_tabIndex].Off();
            switch (selectTab)
            {
                case LOBBY_INTERACTION_TYPE.illust:
                case LOBBY_INTERACTION_TYPE.memorial:
                    _currenttype = LOBBY_INTERACTION_TYPE.illust;
                    break;
                default:
                    _currenttype = LOBBY_INTERACTION_TYPE.character;
                    break;
            }
        }

        _selectTypeTabs[index].On();

        SetSlelectTypeObject(selectTab == LOBBY_INTERACTION_TYPE.illust);
        
        if (selectTab == LOBBY_INTERACTION_TYPE.illust)
            UpdateLists(selectTab);
        else
        {
            for (int s = 0; s< _selectCustomTabs.Count; s++)
            {
                _selectCustomTabs[s].Refresh();
            }
            OnClickSelectType(0);
        }

        // _closeDimd.SetActive(true);
        SetListClose(true);
        _tabIndex = index;
        
        UpdatelistUI(true);
    }

    // 왼쪽 > 커스텀 버튼 > 캐릭터 or 배경 버튼 클릭
    public void OnClickSelectType(int index)
    {
        if (_customIndex == index)
            return;

        if (_customIndex != -1)
            _selectCustomTabs[_customIndex].Off();
                 
        _selectCustomTabs[index].On();
        _customIndex = index;

        if (_customIndex != 0)
        {
            _currenttype = LOBBY_INTERACTION_TYPE.bg;
            UpdateLists(LOBBY_INTERACTION_TYPE.bg);
        }
        else
            _currenttype = LOBBY_INTERACTION_TYPE.character;

        SetCustomList(index == 0);
    }
    private void SetSlelectTypeObject(bool isIllust)
    {
        _illustTitleObj.SetActive(isIllust);
        //_illustObj.SetActive(isIllust);
        _customObj.SetActive(!isIllust);
    }
    private void SetCustomList(bool isCharacter)
    {
        _charList.gameObject.SetActive(isCharacter);
        //_illustList.gameObject.SetActive(!isCharacter);
        _scroll.gameObject.SetActive(!isCharacter);
    }

    // 저장 버튼
    public void OnClickSave()
    {
        switch (_currenttype)
        {
            case LOBBY_INTERACTION_TYPE.illust:
            //case LOBBY_INTERACTION_TYPE.memorial:
                {
                    if (_data == null|| string.IsNullOrEmpty(_seltablegroupid))
                    {
                        string returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_03");        //일러스트를 선택하지 않아 저장할 수 없습니다.
                        UIManager.Instance.ShowToastMessage(returnmsg);
                        return;
                    }
                }
                break;
            case LOBBY_INTERACTION_TYPE.defaultlobby:
            case LOBBY_INTERACTION_TYPE.bg:
                {
                    string returnmsg = string.Empty;
                    if (_data == null && string.IsNullOrEmpty(_seltablegroupid))
                    {
                        if (!string.IsNullOrEmpty(_selectedCharacter))
                            returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_02");       //배경을 선택하지 않아 저장할 수 없습니다.
                        else
                            returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_01");       //체이서를 선택하지 않아 저장할 수 없습니다.

                        UIManager.Instance.ShowToastMessage(returnmsg);
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_selectedCharacter))
                        {
                            returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_01");       //체이서를 선택하지 않아 저장할 수 없습니다.
                            UIManager.Instance.ShowToastMessage(returnmsg);
                            return;
                        }
                    }

                    if (_data == null && !string.IsNullOrEmpty(_seltablegroupid))
                        _data = TableManager.Instance.Lobby_Table.GetLobbyDataList(LOBBY_INTERACTION_TYPE.defaultlobby).FirstOrDefault();
                }
                break;
            case LOBBY_INTERACTION_TYPE.character:
                {
                    string returnmsg = string.Empty;
                    if (string.IsNullOrEmpty(_selectedCharacter))
                    {
                        returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_01");       //체이서를 선택하지 않아 저장할 수 없습니다.
                        UIManager.Instance.ShowToastMessage(returnmsg);
                        return;
                    }
                    else
                    {// 캐릭터는 선택 했는데, 배경을 선택하지 않은 경우.
                        if (_data == null|| string.IsNullOrEmpty(_seltablegroupid))
                        {
                            returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_02");       //체이서를 선택하지 않아 저장할 수 없습니다.
                            UIManager.Instance.ShowToastMessage(returnmsg);
                            return;
                        }
                    }
                }
                break;
            default:
                if (_data == null)
                {
                    if (_data == null|| string.IsNullOrEmpty(_seltablegroupid))
                    {
                        string returnmsg = LocalizeManager.Instance.GetString("str_lobby_save_deny_03");        //일러스트를 선택하지 않아 저장할 수 없습니다.
                        UIManager.Instance.ShowToastMessage(returnmsg);
                        return;
                    }
                }
                    break;
        }
        CustomLobbyManager.Instance.SelectIndex = _currentSlotIndex;
        CustomLobbyManager.Instance.ActiveTableData(_data);
        CustomLobbyManager.Instance.ActiveCharacter(_selectedCharacter);
        //CustomLobbyManager.Instance.UpdateCustomLobbyData(_currentSlotIndex, _data, _selectedCharacter);
        CustomLobbyManager.Instance.SaveLobbyData();

        _isSave = true;
        

        // 토스트 팝업 띄우고
        string str = LocalizeManager.Instance.GetString("str_lobby_save_complete", _currentSlotIndex+1);
        UIManager.Instance.ShowToastMessage(str);
        // 동시에 닫기
        var uitopmenu = UIManager.Instance.GetUI<UITopMenu>();
        if (uitopmenu != null && uitopmenu.gameObject.activeInHierarchy)
            uitopmenu.OnClickBack();
        
    }
    #endregion

    // 캐릭터 리스트에서 캐릭터 선택
    public void OnclickCharacterBtn(DosaInfo dosaInfo)
    {
        _selectedCharacter = dosaInfo.Tid;

        string modelname = "FAB_MODEL_"+dosaInfo.Tid;

        //_selcharacter = tid;
        string image = string.Empty;
        if (_data != null)
        {
            switch (_data.LOBBY_INTERACTION_TYPE)
            {
                case LOBBY_INTERACTION_TYPE.bg:
                case LOBBY_INTERACTION_TYPE.defaultlobby:
                    {
                    }
                    break;
                default:
                    {
                        _data = null;
                        _seltablegroupid = string.Empty;
                    }
                    break;
            }
        }

        _currenttype = LOBBY_INTERACTION_TYPE.bg;
        UpdateCustomBGUI();
        SetCharacters();
    }

    // 캐릭터 포트레잇 ↔ 스파인 상태 체인지
    public void ChangeStateCharacterUI()
    {
        

        if (string.IsNullOrEmpty(_selectedCharacter))
            return;

        SetCharacters();
        
    }

    public void OnClickPreviewCell(int index)
    {
        if (_previewcellIndex == index)
            return;

        //if (!string.IsNullOrEmpty(_selectedCharacter))
        //{
        //    _selectedCharacter = null;
        //    SetCharacters();
        //}

        for (int n = 0; n< _cellDatas.Count; n++)
        {
            var dataindex = n;
            _cellDatas[n].SetIsSelect(dataindex == index);
        }

        for (int r = 0; r< _cells.Count;r++ )
        {
            if (_cells[r].gameObject.activeInHierarchy)
                _cells[r].SetIsSelected();
        }
    }
    // 왼쪽에서 열린 창 닫기.
    public void OnClickHideList()
    {
        PlayRightAnim(false);
    }

    public void OnClickShowUI()
    {
        _showBtn.gameObject.SetActive(false);
       //PlayRightAnim(true);
        if (_leftAnim.speed == 0)
            _leftAnim.speed = 1;
        _leftAnim.Play("Left_Appear", -1, 0);
    }

    public void OnClickHideAllUI()
    {
        PlayRightAnim(false);
        _leftAnim.Play("Left_Appear", 0, -1);
        _leftAnim.speed = 0;
        _showBtn.gameObject.SetActive(true);
    }
}
