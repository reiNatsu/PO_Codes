using Consts;
using LIFULSE.Manager;
using System.Collections.Generic;
using UnityEngine;


public class StoreSubTabCellData
{
    [SerializeField] public string Tid;
    [SerializeField] public Store_Option_TableData Data; 

    public StoreSubTabCellData(string tid)
    {
        Tid = tid;
        Data = TableManager.Instance.Store_Option_Table[Tid];
    }
}

public class StoreSubTabCell : ScrollCell
{

    [SerializeField] private ExTMPUI _title;
    [SerializeField] private ExImage _bg;
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExButton _button;

    private string _thisMainTab;
    private UIStore _uiStore;
    private List<StoreSubTabCellData> _cellDatas;
    private Store_Option_TableData _tabData;

    protected override void Init()
    {
        base.Init();
        _uiStore = UIManager.Instance.GetUI<UIStore>();
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;
        _tabData = TableManager.Instance.Store_Option_Table[_cellDatas[DataIndex].Tid];

    }
    public void EnableTabUI()
    {
        _bg.enabled = true;
        _title.color = Color.white;
        _button.interactable = false;
    }
    public void DisableTabUI()
    {
        _button.interactable = true;
        _bg.enabled = false;
        _title.color = Color.black;
    }

    public void OnClickSubTab()
    {
        Debug.Log("OnClickSubTab() -> "+_cellDatas[DataIndex].Tid);
        EnableTabUI();
       // _uiStore.ChangeSubTab(_cellDatas[DataIndex].Tid);
    }

    public void SetCellDatas(List<StoreSubTabCellData> datas, string mainTab)
    {
        _cellDatas = datas;
        _thisMainTab = mainTab;
        this.gameObject.SetActive(false);
    }

    public void RefreshCellDatas()
    {
        _cellDatas = new List<StoreSubTabCellData>();
    }
}
