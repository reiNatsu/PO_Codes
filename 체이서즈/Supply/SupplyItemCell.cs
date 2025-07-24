using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SupplyItemCell : MonoBehaviour
{
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExTMPUI _indexTMP;
    [SerializeField] private ExTMPUI _descTMP = null;
    // Start is called before the first frame update

    public void Init(string groupId, bool isLastitem, string iconName, string descid = null)
    {
        var rewardData = TableManager.Instance.Reward_Table.GetDatas(groupId).FirstOrDefault();
        var itemData = TableManager.Instance.Item_Table[rewardData.Item_Tid];
        if (!isLastitem)
            _icon.SetSprite(itemData.Icon_Id);
        else
            _icon.SetSprite(iconName);
        _indexTMP.ToTableText("str_supples_10minuts_value_01", rewardData.Item_Min);    //{0}/10M

        if (_descTMP != null)
        {
            if (!isLastitem)
                _descTMP.ToTableText(descid, rewardData.Item_Min);
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(LocalizeManager.Instance.GetString(descid, rewardData.Item_Min));
                sb.AppendLine();
                sb.AppendLine();
                sb.Append(LocalizeManager.Instance.GetString("str_supples_probability_up_04"));

                _descTMP.text = sb.ToString();  
            }
        }
    }
}
