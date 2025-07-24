using Consts;
using LIFULSE.Manager;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public enum COSTUME_STATE
{ 
    hold,                    // 보유중
    nothold,               // 미보유
    wearing,              // 착용중
    wear,                   // 착용
}

public class UISkinPage : UIBase
{
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private ExTMPUI _nameTMP;
    [SerializeField] private ExTMPUI _descTMP;
    [SerializeField] private ExTMPUI _onBtnTMP;
    [SerializeField] private GameObject _detailBtnObj;
    //[SerializeField] private GameObject _detailObj;
    //[SerializeField] private ExImage _detailImg;
    [SerializeField] private GameObject _confirmBtn; 

    private UICharacterModelViewer _characterModelViewer;

    private List<SkinListCell> _cells;
    private List<SkinListCellData> _cellDatas = null;

    private string _characterTid;
    private DosaInfo _currentDosa = null;
    private Costume_TableData _selectData;
    private int _selectIndex;
    private bool _selectIsHold = false;
    private COSTUME_STATE _costumeState;

    private Costume_TableData _showData = null;
    private bool _isHideBtn = false;

    public override void Close(bool needCached = true)
    {
        _showData = null;
        LobbyController.Instance.SwithCamera(false);
        var uiSkinBG = UIManager.Instance.GetUI<UISkinPageBG>();
        if (uiSkinBG != null && uiSkinBG.gameObject.activeInHierarchy)
            uiSkinBG.Close();
        LobbyController.Instance.TouchAreaController.AllSoundOff();
        base.Close(needCached);
    }
    public override void Refresh()
    {
        base.Refresh();
    }
    public override void Init()
    {
        base.Init();

        _scroll.Init();
        _cells = _scroll.GetCellToList<SkinListCell>();
        _cellDatas = new List<SkinListCellData>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _isHideBtn = false;
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Tid, out var characterid))
                _characterTid = (string)characterid;

            if (optionDict.TryGetValue(UIOption.Data, out var costumeData))
            {
                _showData = (Costume_TableData)costumeData;
            }

            if(optionDict.TryGetValue(UIOption.Bool, out var isHide))
                _isHideBtn = (bool)isHide;
        }
      
        UIManager.Instance.Show<UISkinPageBG>(Utils.GetUIOption(
            UIOption.Tid, _characterTid));
        //LobbyController.Instance.SwithCamera(true);
        //LobbyController.Instance.SwithCamera(true);

        SetDosaInfo();
        SetCostumeLists();
        LobbyController.Instance.SwithCamera(true);
        _confirmBtn.SetActive(!_isHideBtn);
    }
     
    public void SetDosaInfo()
    {
        _currentDosa = GameInfoManager.Instance.CharacterInfo.GetDosa(_characterTid);
    }

    public void SetDetailUI(bool inShow)
    {
        _detailBtnObj.SetActive(inShow);
    }

    public void SetCostumeLists()
    {
        _cellDatas.Clear();
        var list = TableManager.Instance.Costume_Table.GetDatas(_characterTid);
        int currentindex = 0;

        Costume_TableData data = list.FirstOrDefault();

        for (int n = 0; n < list.Count; n++)
        {
            _cellDatas.Add(new SkinListCellData(list[n], (data, index, hold) => UpdateSkinInfo(data, index, hold)));
            if (_showData != null && _showData.Tid.Equals(list[n].Tid))
            {
                currentindex = n;
                data = _showData;
            }
            else
            {
                if (_currentDosa != null && !string.IsNullOrEmpty(_currentDosa.CostumeTid) && _currentDosa.CostumeTid.Equals(list[n].Tid))
                {
                    currentindex = n;
                    data = list[n];
                }
            }
        }

        for (int i = 0; i< _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(list.Count);

        // 현재 착용중인 코스튬으로
        UpdateSkinInfo(data, currentindex, IsWearing(data));
    }

    private bool IsWearing(Costume_TableData data)
    {
        bool isHold = false;
        switch (data.Cosutume_Grade)
        {
            case "cash":
                {
                    var itemid = data.Coustume_Item_Groupid;
                    var amount = 0;
                    if (!string.IsNullOrEmpty(itemid))
                    {
                        var itemData = TableManager.Instance.Item_Table[itemid];
                        amount= GameInfoManager.Instance.GetAmount(itemid);
                    }

                    isHold = amount >0;
                }
                break;
            case "default":
                isHold = GameInfoManager.Instance.CharacterInfo.HasDosa(data.Costume_Character);
                break;
        }
        return isHold;
    }

    public void UpdateSkinInfo(Costume_TableData data, int index, bool isHold)
    {
        _selectData = data;
        _selectIsHold = isHold;
        _selectIndex = index;

        if (string.IsNullOrEmpty(data.Costume_Name))
            _nameTMP.text = data.Tid + " 코스튬";
        else
            _nameTMP.ToTableText(data.Costume_Name);
        _descTMP.ToTableText(data.Costume_Desc);

        //Select 정보 변경
        for (int n = 0; n< _cellDatas.Count; n++)
        {
            _cellDatas[n].SetIsSelect(index == n);
        }
        for (int i = 0; i< _cells.Count; i++)
        {
            if (_cells[i].gameObject.activeInHierarchy)
                _cells[i].SetSelected();
        }
        SetDetailUI(!string.IsNullOrEmpty(data.Costume_Illust));
        UpdateSkinState();
        LobbyController.Instance.SetCharacter(data.Costume_Art_01, true);
        LobbyController.Instance.ActiveDissolve(true);
        // LobbyController.Instance.SetCameraSkinFOV();
        var uiSkinBG = UIManager.Instance.GetUI<UISkinPageBG>();
        if (uiSkinBG != null && uiSkinBG.gameObject.activeInHierarchy)
            uiSkinBG.SetCharacterImg(data.Costume_Icon);
    }

    public void UpdateSkinState()
    {
        //  1번 체크 : 착용중인지 아닌지
        //DosaInfo dosainfo = GameInfoManager.Instance.CharacterInfo.GetDosa(_characterTid);
        if (_currentDosa != null)
        {
            if (_selectIsHold)
            {
                _costumeState = COSTUME_STATE.hold;
                if (string.IsNullOrEmpty(_currentDosa.CostumeTid) && _selectData.Cosutume_Grade.Equals("default")
                    || !string.IsNullOrEmpty(_currentDosa.CostumeTid) &&_currentDosa.CostumeTid.Equals(_selectData.Tid))
                {
                    _costumeState = COSTUME_STATE.wearing;
                    _onBtnTMP.ToTableText("str_ui_costume_button_01"); //착용 중
                }
                else
                {         
                    _costumeState = COSTUME_STATE.wear;
                    _onBtnTMP.ToTableText("str_ui_costume_button_05"); //착용
                }
                //if (!_currentDosa.CostumeTid.Equals(_selectData.Tid))
                //{
                //    _costumeState = COSTUME_STATE.wear;
                //    _onBtnTMP.text = "착용";
                //}
                //else
                //{
                //    _costumeState = COSTUME_STATE.wearing;
                //    _onBtnTMP.ToTableText("str_ui_costume_button_01");      // 착용 중
                //}
            }
            else
            {
                _costumeState = COSTUME_STATE.nothold;
                _onBtnTMP.ToTableText("str_ui_costume_button_06"); //획득처 이동
            }
        }
    }

    public void OnClickShowDetail()
    {
        if (string.IsNullOrEmpty(_selectData.Costume_Illust))
            return;

        //_detailObj.SetActive(true);
        //_detailImg.SetSprite(_selectData.Costume_Illust);
        UIManager.Instance.Show<UIPopupSkinIllust>(Utils.GetUIOption(UIOption.Name, _selectData.Costume_Illust));
    }


    public void OnClickConfirm()
    {
        switch (_costumeState)
        {
            case COSTUME_STATE.hold:
               {
                    // 보유중은 버튼에 표기 안함
                }
                break;
            case COSTUME_STATE.nothold:
                {
                    // 획득처로 이동시켜야함
                    if (string.IsNullOrEmpty(_selectData.End_Time))
                        UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "cash", UIOption.Tid, "store_costume_01"));
                    else
                    { 
                      var expiredTime = GetItemRemainTime(_selectData.End_Time);
                       if(expiredTime.Ticks > 10)
                            UIManager.Instance.Show<UISeasonPass>();
                       else
                            UIManager.Instance.Show<UIStore>(Utils.GetUIOption(UIOption.Index, "cash", UIOption.Tid, "store_character_01"));
                    }
                }
                break;
            case COSTUME_STATE.wear:                // 착용 하기
                {
                    RestApiManager.Instance.RequestCharacterSetCostume(_selectData.Tid, () => {
                        UIManager.Instance.ShowToastMessage("str_ui_costume_errer_02");     //코스튬이 변경되었습니다.
                        SetDosaInfo();
                        UpdateSkinState();
                        for (int i = 0; i< _cells.Count; i++)
                        {
                            if (_cells[i].gameObject.activeInHierarchy)
                                _cells[i].SetHoldUI();
                        }
                    });
                }
                break;
            case COSTUME_STATE.wearing:             // 착용중일때
                {
                    UIManager.Instance.ShowToastMessage("str_ui_costume_errer_01");     // 이미 착용 중인 코스튬 입니다.
                }
                break;
        }
    }

    private TimeSpan GetItemRemainTime(string expiredTime)
    {
        var period = expiredTime;
        string format = "MM/dd/yyyy HH:mm";
        CultureInfo provider = CultureInfo.InvariantCulture;

        DateTime expiretime = DateTime.ParseExact(period, format, provider, DateTimeStyles.AssumeLocal);
        return expiretime.GetRemainTime();
    }
}
