using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupClanJoinInfo : UIBase
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private VerticalLayoutGroup _layout;
    [SerializeField] private GameObject _clanObj;

    [SerializeField] private GameObject _noDataObj;

    private List<ClanListCell> _clanCells;

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }

    public override void Init()
    {
        base.Init();

        _clanCells = new List<ClanListCell>();
        _clanCells.Add(_clanObj.GetComponent<ClanListCell>());
        //Utils.SetResolutionUI(this.gameObject);
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {

        UpdateUI();
    }

    public void UpdateUI()
    {
        // 팀 리스트 설정
        // 데이터가 하나도 없을 경우 : 기존에 추가 해 둔 오브젝트 setactive(false) 시키고, 있으면 setactive(true). 한개 이상이면 추가.
        var myJoinList = GameInfoManager.Instance.ClanInfo.UserClanData.JoinDatas;
        //var myJoinList = GameInfoManager.Instance.ClanInfo.UserClanData.JoinList;
        //if (myJoinList == null || myJoinList.Count < 1)
        //{
        //    SetChangeIsDataUI(false);

        //}
        //else
        //    SetChangeIsDataUI(true);
        //for (int n = 0; n < _clanCells.Count; n++)
        //{
        //    _clanCells[n].gameObject.SetActive(false);
        //}

        if (myJoinList.Count > _clanCells.Count)
            CreateCell(myJoinList.Count - _clanCells.Count);

        UpdateJoinList(myJoinList);
    }

    private void SetChangeIsDataUI(bool isData)
    {
        _scrollRect.gameObject.SetActive(isData);
        _noDataObj.SetActive(!isData);
    }
     
    private void CreateCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_clanObj, _layout.transform);
            var clanCell = newObj.GetComponent<ClanListCell>();

            _clanCells.Add(clanCell);
        }
    }

    //TODOCLAN : List<string> => List<ClanData> 로 바뀌어야 함.
    //public void UpdateJoinList(List<string> clanCellData)
    public void UpdateJoinList(List<ClanData> clanCellData)
    {
        // 임시로 만들어둔는 함수
        Action onCancleJoin = () => {
            UpdateUI();
        };
        SetChangeIsDataUI(clanCellData != null &&clanCellData.Count > 0);

        if (clanCellData.Count > 3)
        {
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _layout.padding.right = 30;
        }
        else
        {
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _layout.padding.right = 0;
        }

        for (int i = 0; i < _clanCells.Count; i++)
        {
            if (i < clanCellData.Count)
            {
                _clanCells[i].InitData(clanCellData[i], false, onCancleJoin);
            }
            else
                _clanCells[i].gameObject.SetActive(false);
        }
    }
}
