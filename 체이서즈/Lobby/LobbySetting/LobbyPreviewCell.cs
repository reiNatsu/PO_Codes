using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPreviewCellData
{
    [SerializeField] public Lobby_TableData Data;
    [SerializeField] public string Tid;
    [SerializeField] public Action<int> OnClickSelect;

    [SerializeField] public bool IsSelect = false;

    public LobbyPreviewCellData(Lobby_TableData data, bool isSelect, Action<int> onClickSelect)
    {
        Data = data;
        Tid = data.Tid;
        IsSelect = isSelect;
        OnClickSelect = onClickSelect;
    }

    public void SetIsSelect(bool isSelect)
    {
        IsSelect = isSelect;
    }
}

public class LobbyPreviewCell : ScrollCell
{
    [SerializeField] private ExImage _illust;
    [SerializeField] private ExTMPUI _number;
    [SerializeField] private GameObject _dimd;
    [SerializeField] private ExButton _button;
    [SerializeField] private GameObject _selectObj;
    [SerializeField] private Animator _anim;

    private List<LobbyPreviewCellData> _cellDatas;
    private Lobby_TableData _data;


    protected override void Init()
    {
        base.Init();
    }

    public override void UpdateCellData(int dataIndex)
    {
        //DeSelect();
        DataIndex = dataIndex;
        _data = _cellDatas[DataIndex].Data;

        var uilobbysetting = UIManager.Instance.GetUI<UILobbySetting>();
        
        if (_cellDatas[DataIndex].IsSelect)
            Select();
        else
            DeSelect();
        //if (uilobbysetting != null && uilobbysetting.gameObject.activeInHierarchy)
        //{
        //    var tableData = CustomLobbyManager.Instance.GetTableTid(uilobbysetting.CurrentSlotNumber);
        //    if (uilobbysetting.IsDefault)
        //    {
        //        if(_data.LOBBY_INTERACTION_TYPE == LOBBY_INTERACTION_TYPE.defaultlobby)
        //            Select();
        //    }
        //    else
        //    {
        //        if (_data.Group_Id.Equals(tableData))
        //        {
        //            Select();
        //            uilobbysetting.PreviewCellIndex = DataIndex;
        //        }
        //        else
        //            DeSelect();
        //    }
        //}
        //else
        //    DeSelect();

        _illust.SetSprite(_data.Lobby_Preview_Img);
        _number.text = DataIndex.ToString();

        if (_dimd.activeInHierarchy)
            _dimd.SetActive(false);

        _button.onClick.AddListener(() => _cellDatas[DataIndex].OnClickSelect(DataIndex));


        SetItemIsOpen();
    }

    public void SetCellDatas(List<LobbyPreviewCellData> datas)
    {
        _cellDatas = datas;
        _data = new Lobby_TableData();
    }

    public void SetIsSelected()
    {
        if (_cellDatas[DataIndex].IsSelect)
            Select();
        else
            DeSelect();
    }

    public void OnClickCell()
    {
        _cellDatas[DataIndex].OnClickSelect?.Invoke(DataIndex);
        var uilobbysetting = UIManager.Instance.GetUI<UILobbySetting>();
        
        if (uilobbysetting != null && uilobbysetting.gameObject.activeInHierarchy)
        {
            uilobbysetting.ResetSelectData(_cellDatas[DataIndex].Data.LOBBY_INTERACTION_TYPE);
            uilobbysetting.UpdateSelectImageData(_cellDatas[DataIndex].Data);
            uilobbysetting.UpdateCustomBGUI();
        }
    }

    private void SetItemIsOpen()
    {
        switch (_data.Lobby_Open_Type)
        {
            case "clear":
                {
                    var openkey = _data.Lobby_Open;

                    if (string.IsNullOrEmpty(openkey) || TableManager.Instance.Stage_Table[openkey] == null)
                    {
                        _dimd.SetActive(false);
                        //_dimd.SetActive(true);
                        return;
                    }
                    var stagedata = TableManager.Instance.Stage_Table[openkey];
                    bool isClear = false;

                    switch (stagedata.CONTENTS_TYPE_ID)
                    {
                        case CONTENTS_TYPE_ID.stage_main:
                        case CONTENTS_TYPE_ID.story_main:
                            {
                                isClear = GameInfoManager.Instance.StageInfo.IsClear(_data.Lobby_Open);
                            }
                            break;
                        case CONTENTS_TYPE_ID.story_event:
                        case CONTENTS_TYPE_ID.event_main:
                            isClear = GameInfoManager.Instance.EventStoryInfo.IsClear(_data.Lobby_Open);
                            break;
                    }

                    _dimd.SetActive(!isClear);
                }
                break;
            default:
                break;
        }
    }


    public void Select()
    {
        _selectObj.SetActive(true);
    }

    public void DeSelect()
    {
        _selectObj.SetActive(false);
    }

 

    public void OnClickDimd()
    {
        string str = LocalizeManager.Instance.GetString(_data.Lock_Massage);
        UIManager.Instance.ShowToastMessage(str);
    }
}
