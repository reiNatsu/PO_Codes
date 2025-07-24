using LIFULSE.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIStoreScrollSubscribe : MonoBehaviour
{
    [SerializeField] private UIStore _uiStore;
    [SerializeField] private List<StoreItemCellSubscribe> _items= new List<StoreItemCellSubscribe>();

    private List<StoreItemCellSubscribe> _storeCells;
    private List<StoreItemCellSubscribeData> _cellDatas = null;
    private List<Store_TableData> _curStoresData = new List<Store_TableData>();

    private Store_Option_TableData _soData;

    private string _storeTab;       // 보따리 상점, 패키지 상점, 유료상점등 구분
    private Vector2 _currentAnchor = Vector2.zero;
    public void Init()
    {
        //_scroll.Init();
    }

    public void InitData( string tab, int index, bool isRefresh)
    {
        _storeTab = tab;

        //_storeCells = _scroll.GetCellToList<StoreItemCellSubscribe>();
        //_cellDatas = new List<StoreItemCellSubscribeData>();
        _soData = TableManager.Instance.Store_Option_Table.GetStoreOprionData(_storeTab);

        //_currentAnchor = _scroll.ContentRect.anchoredPosition;
        //if (isRefresh)
        //    _currentAnchor = Vector2.zero;

        UpdateData(index);
    }

    public void UpdateData(int index)
    {
        _curStoresData= GameInfoManager.Instance.CheckExpiredTime(_storeTab);
        _curStoresData = _curStoresData.OrderBy(store => store.Order).ToList();
        var ops = TableManager.Instance.Store_Option_Table[_storeTab].Main_Option_Group_Tid;
        var mainOps = TableManager.Instance.Store_MainOption_Table[ops].Store_Classification;

        for (int n = 0; n < _curStoresData.Count; n++)
        {
            if (n < _items.Count)
            {
                _items[n].gameObject.SetActive(true);
                _items[n].UpdateInfo(_curStoresData[n]);
            }
            else
                _items[n].gameObject.SetActive(false);
            //_cellDatas.Add(new StoreItemCellSubscribeData(_curStoresData[n]));
        }
    }

    
}
