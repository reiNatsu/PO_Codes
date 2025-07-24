using Consts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LIFULSE.Manager
{
    public partial class GameInfoManager
    {
        private Dictionary<CONTENTS_TYPE_ID, Coroutine> _supplyCoroutines = new Dictionary<CONTENTS_TYPE_ID, Coroutine>();

        private int _supplyLevel;
        public int SupplyLevel { get { return _supplyLevel; } }

        // 10분마다 타이머가 돌 때, RequestSupplyUpdate 한 이후에 실행 할 이벤트 함수
        public void UpdateSupplyUIEvent()
        {
            var uipopupSupply = UIManager.Instance.GetUI<UIPopupSupply>();
            if (uipopupSupply != null && uipopupSupply.gameObject.activeInHierarchy)
            {
                uipopupSupply.SetSupplyItemsUI();
            }
        }

        // Supplies_table의 데이터 돌면서 현재 클리어한 스테이지 리스트에서 가장 마지막 레벨을 가지고 온다. 
        public int GetSupplyLevel()
        {
            List<Supplies_TableData> list = new List<Supplies_TableData>();
            var supplies = SupplyInfo.SupplyDatas;
            foreach (var info in supplies)
            {
                switch (info.Key)
                {
                    case CONTENTS_TYPE_ID.manjang:
                        var supplyTable = TableManager.Instance.Supplies_Table.DataArray;
                        for (int n = 0; n< supplyTable.Length; n++)
                        {
                            var data = supplyTable[n];
                            if (DungeonInfo.IsClear(info.Key, data.Supplies_Lv_Condition))
                                list.Add(data);
                        }

                        break;
                    default:
                        break;
                }
            }

            if (list.LastOrDefault() == null || string.IsNullOrEmpty(list.LastOrDefault().Supplies_Lv_Condition))
                return 0;

            return list.LastOrDefault().Supplies_Lv;
        }


        // supplies_table의 가작 첫번째 항목의 supplies_lv_condition 조건이 충족 되어 있는지 확인 
        public bool IsOpendSupplies()
        {
            var data = TableManager.Instance.Supplies_Table.DataArray.FirstOrDefault();
            var key = GameInfoManager.Instance.SupplyInfo.SupplyDatas.FirstOrDefault().Key;
            return DungeonInfo.IsClear(key, data.Supplies_Lv_Condition);
        }

        public int GetNextRefreshValue()
        {
            var data = TableManager.Instance.Define_Table["ds_supplies_time"].Opt_01_Int;
            return data;
        }

        public void IsSuppliesLevelUP(string dungeonTId)
        {
            var curData = TableManager.Instance.Supplies_Table.GetData(dungeonTId);
            if (string.IsNullOrEmpty(curData.Tid))
                return;
            var prevData = new Supplies_TableData();
            if (!curData.Tid.Equals(TableManager.Instance.Supplies_Table.DataArray.FirstOrDefault().Tid))
                prevData = TableManager.Instance.Supplies_Table.GetData(curData.Supplies_Lv-1);

            var supplies = TableManager.Instance.Supplies_Table.DataArray;
            for (int n = 0; n < supplies.Length; n++)
            {
                var data = supplies[n];
                if (data.Supplies_Lv_Condition.Equals(dungeonTId) && _supplyLevel != curData.Supplies_Lv)
                {
                    Debug.Log("<color=#4cd311>("+dungeonTId+") 방치보상 레벨 업데이트! LEVEL == "+data.Supplies_Lv+"</color>");
                    // 임시로 토스트 메세지 출력
                    if (data != TableManager.Instance.Supplies_Table.DataArray.FirstOrDefault())            // 최초 오픈일경우
                    {
                        _supplyLevel = curData.Supplies_Lv;
                        //string str = LocalizeManager.Instance.GetString("str_supples_lv_up_01", prevData.Supplies_Lv, curData.Supplies_Lv);
                        //UIManager.Instance.ShowToastMessage(str, "IC_random_box_gold_01");
                        UIManager.Instance.Show<UIPopupSupplyLvUp>(Utils.GetUIOption
                            (UIOption.Bool, false,
                            UIOption.Int, prevData.Supplies_Lv,
                            UIOption.Int2, curData.Supplies_Lv));

                        // UIManager.Instance.ShowToastMessage("보급품이 개방되었습니다!");
                        //UIManager.Instance.Show<UIPopupSupplyLvUp>(Utils.GetUIOption(
                        //    UIOption.Bool, true));  
                    }
                    //else
                    //{
                    //    //_supplyLevel = curData.Supplies_Lv;
                    //    ////string str = LocalizeManager.Instance.GetString("str_supples_lv_up_01", prevData.Supplies_Lv, curData.Supplies_Lv);
                    //    ////UIManager.Instance.ShowToastMessage(str, "IC_random_box_gold_01");
                    //    //UIManager.Instance.Show<UIPopupSupplyLvUp>(Utils.GetUIOption
                    //    //    (UIOption.Bool, false,
                    //    //    UIOption.Int, prevData.Supplies_Lv,
                    //    //    UIOption.Int2, curData.Supplies_Lv));
                    //}
                    break;
                }
            }
        }

        public void SetCurrentSupplyLevel(int level)
        {
            _supplyLevel = level;
        }
    }
}
