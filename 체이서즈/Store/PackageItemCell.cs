using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PackageItemCell : MonoBehaviour
{
    [SerializeField] private ITEM_TYPE _type;
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExTMPUI _count;

    private string _tableName;
    private Reward_TableData _rData;
    private string _packageGroupName;
    public string RewardGroupName { get { return _packageGroupName; } }

    public void InitData(string packageId,Reward_TableData data)
    {
        _rData = data;
        _packageGroupName = packageId+"_"+_rData.Item_Tid;
        this.gameObject.name = _packageGroupName;
        UpdateUI();
    }

    private void UpdateUI()
    {
        SetCellDataByType(_rData.ITEM_TYPE);
    }
   

    private void SetCellDataByType(ITEM_TYPE type)
    {
        string tid = _rData.Item_Tid;
        string iconStr = "";
        if (type == ITEM_TYPE.subscribe)
        {
            var subscribeData = TableManager.Instance.Attendance_Reward_Table.GetDatas(tid)[0];
            var rewardData = TableManager.Instance.Reward_Table.GetDatas(subscribeData.Attendance_Reward)[0];
            type = rewardData.ITEM_TYPE;
            tid = rewardData.Item_Tid;
        }
        iconStr = SetCellDataByType(type,tid);
        _icon.SetSprite(iconStr);
        string str = "X "+ _rData.Item_Min.ToString();
        _count.ToTableText(str);
    }

    private string SetCellDataByType(ITEM_TYPE type,string tid)
    {
        
        string iconStr = "";
        switch (type)
        {
            case ITEM_TYPE.equipment:// 호패 : equipment_table 참조 
                var data_e = TableManager.Instance.Equipment_Table;
                iconStr = data_e[tid].Equip_Icon;
                break;
            case ITEM_TYPE.character_coin:// 캐릭터 코인 character_pc_table 참조
                var data_c = TableManager.Instance.Character_PC_Table;
                iconStr = data_c[tid].Char_Icon_Circle_01;

                break;
            case ITEM_TYPE.soul:// monster_coin  collection_monster_table 참조
                var data_m = TableManager.Instance.Collection_Monster_Table[tid];
                //var monData = TableManager.Instance.Character_Monster_Table[data_m.Monster_Str];  //Collection_Monster_Table 컬럼 수정으로 주석 및 아이콘 데이터 수정
                iconStr = data_m.Item_Icon;
                break;
            case ITEM_TYPE.currency:    // 재화       item_table 참조(아래 전부)
            case ITEM_TYPE.consumables: // 소모품
            case ITEM_TYPE.stuff:       // 재료               
            case ITEM_TYPE.quest:       // 퀘스트용
            case ITEM_TYPE.box:         // 박스
                var data_i = TableManager.Instance.Item_Table;
                iconStr = data_i[tid].Icon_Id;
                break;
            // 나중에 특수 추가
        }

        return iconStr;
    }
}
