using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStageEnemyInfo : MonoBehaviour
{
    [SerializeField] private ScrollRect _enemyScroll;
    [SerializeField] private HorizontalLayoutGroup _layout;
    [SerializeField] private GameObject _enemySlotObj;

    [SerializeField] private GameObject _stageBuffButton;                   // 스테이지 버프 테이블 데이터 유무에 따라 노출.
    [SerializeField] private UIStageBuff _stageBuffInfoObj;                  // 스테이지 정보 임시 팝업

    private List<UITeamSlot> _enemyCells;
    private Stage_TableData _data;
    public void Show(Stage_TableData data)
    {
        if (_stageBuffInfoObj != null)
            _stageBuffInfoObj.gameObject.SetActive(false);

        //if(!string.IsNullOrEmpty(data.Stage_Buff))
        //    _stageBuffInfoObj.SetStageBuff(data.Stage_Buff);
        _data = data;

        if (_enemyCells == null)
            _enemyCells = new List<UITeamSlot>();
        else
        {
            for (int n = 0; n < _enemyCells.Count; n++)
            {
                _enemyCells[n].gameObject.SetActive(false);
            }
            _enemyCells.Clear();
        }
          
        _enemyCells.Add(_enemySlotObj.GetComponent<UITeamSlot>());

        SetStageBuff(data);
        SetMonsterInfo(data);
    }

    private void SetStageBuff(Stage_TableData data)
    {
        bool isShow = string.IsNullOrEmpty(data.Stage_Buff);
        _stageBuffButton.SetActive(!isShow);
    }

    public void SetMonsterInfo(Stage_TableData data)
    {
        var monsterList = TableManager.Instance.Stage_Table.GetEnemyTidList(data.Tid);
        if (monsterList.Count > _enemyCells.Count)
            CreateMonsterCell(monsterList.Count - _enemyCells.Count);

        UpdateMonsters(monsterList, data);
    }

    private void CreateMonsterCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_enemySlotObj, _layout.transform);
            var enemyCell = newObj.GetComponent<UITeamSlot>();

            _enemyCells.Add(enemyCell);
        }
    }

    private void UpdateMonsters(List<string> monsterCellDatas, Stage_TableData data)
    {
        if (monsterCellDatas.Count > 4)
        {
            _enemyScroll.movementType = ScrollRect.MovementType.Elastic;
            _layout.padding.right = 30;
        }
        else
        {
            _enemyScroll.movementType = ScrollRect.MovementType.Clamped;
            _layout.padding.right = 0;
        }

        for (int n = 0; n < _enemyCells.Count; n++)
        {
            if (n < monsterCellDatas.Count)
            {
                var isBoss = TableManager.Instance.Stage_Table.IsBoss(data, monsterCellDatas[n]);
                _enemyCells[n].gameObject.SetActive(true);
                _enemyCells[n].SetupMonster(monsterCellDatas[n], data.Stage_Attribute, isBoss:isBoss);
                _enemyCells[n].SetLevelTMP(false);
            }
            else
                _enemyCells[n].gameObject.SetActive(false);
        }
    }

    public void OnClickOpenTypeInfo()
    {
        UIManager.Instance.Show<UITypeInfoPopup>();
    }

    public void OnClickShowStageBuff()
    {
        if (_stageBuffInfoObj != null)
        {
            if (_stageBuffInfoObj.gameObject.activeInHierarchy)
                _stageBuffInfoObj.gameObject.SetActive(false);
            else
                _stageBuffInfoObj.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(_data.Stage_Buff))
                _stageBuffInfoObj.SetStageBuff(_data.Stage_Buff);
            _stageBuffInfoObj.UpdateBuffLayout();
        }
    }
    public void CloseInfoPopup()
    {
        if (_stageBuffInfoObj != null)
        {
            if (_stageBuffInfoObj.gameObject.activeInHierarchy)
                _stageBuffInfoObj.gameObject.SetActive(false);
        }
    }

}
