using ES3Internal;
using LIFULSE.Manager;
using System.Collections.Generic;
using UnityEngine;

public class UIStoreScrollPackage : MonoBehaviour
{
    [SerializeField] private RecycleScroll _scroll;

    private Store_Option_TableData _soData;

    private List<StoreItemCellPackage> _storeCells;
    private List<StoreItemCellPackageData> _cellDatas = null;

    private List<Store_TableData> _curStoresData = new List<Store_TableData>();
    private string _storeTab;

    private Vector2 _currentAnchor = Vector2.zero;

    public RecycleScroll Scroll { get { return _scroll; } }

    public void Init()
    {
        _scroll.Init();
       
        _storeCells = _scroll.GetCellToList<StoreItemCellPackage>();
        _cellDatas = new List<StoreItemCellPackageData>();
    }

    public void InitData(string tab, int index, bool isRefresh)
    {
        _storeTab = tab;

        _soData = TableManager.Instance.Store_Option_Table.GetStoreOprionData(_storeTab);
        _currentAnchor = _scroll.ContentRect.anchoredPosition;
        if (isRefresh)
            _currentAnchor = Vector2.zero;
        UpdateData(index);
        //_scroll.FocusLine(0);
    }

    public void UpdateData(int index)
    {
        _cellDatas.Clear();
        _curStoresData =  GameInfoManager.Instance.CheckExpiredTime(_storeTab);
        //_scroll.FocusLine(0);
        for (int n = 0; n < _curStoresData.Count; n++)
        {
            _cellDatas.Add(new StoreItemCellPackageData(_curStoresData[n]));
        }
        

        for (int i = 0; i < _storeCells.Count; i++)
        {
            _storeCells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(_curStoresData.Count);
        // _scroll.FocusLine(index);
        _scroll.ContentRect.anchoredPosition = _currentAnchor;
        //var focusline = _scroll.SetScrollLineToItem(index);
        //_scroll.FocusLine(focusline);
        //_scroll.ActivateCells(_curStoresData.Count);
        //_scroll.FocusLine(index); 
    }
}
