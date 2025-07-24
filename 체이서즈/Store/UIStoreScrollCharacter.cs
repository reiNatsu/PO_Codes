using LIFULSE.Manager;
using System.Collections.Generic;
using UnityEngine;

public class UIStoreScrollCharacter : MonoBehaviour
{
    [SerializeField] private RecycleScroll _scroll;

    private List<StoreItemCharacter> _storeCells;
    private List<StoreItemCellCharacterData> _cellDatas = null;
    private List<Store_TableData> _curStoresData= new List<Store_TableData>();

    private Store_Option_TableData _soData;

    private string _storeTab;       // 보따리 상점, 패키지 상점, 유료상점등 구분

    public void Init()
    {
        _scroll.Init();
    }

    public void InitData( string tab, int index)
    {
        _storeTab = tab;

        _storeCells = _scroll.GetCellToList<StoreItemCharacter>();
        _cellDatas = new List<StoreItemCellCharacterData>();
        _soData = TableManager.Instance.Store_Option_Table.GetStoreOprionData(_storeTab);
        UpdateData(index);
    }

    public void UpdateData(int index)
    {
        _cellDatas.Clear();
        // _curStoresData =  GameInfoManager.Instance.GetSotreItemsList(_storeTab);
        _curStoresData =  GameInfoManager.Instance.CheckExpiredTime(_storeTab);
     
        for (int n = 0; n < _curStoresData.Count; n++)
        {
            _cellDatas.Add(new StoreItemCellCharacterData(_curStoresData[n]));
        }
        for (int i = 0; i < _storeCells.Count; i++)
        {
            _storeCells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(_curStoresData.Count);

        _scroll.FocusLine(index);
    }
}
