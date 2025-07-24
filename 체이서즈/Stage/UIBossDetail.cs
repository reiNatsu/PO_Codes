using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossDetail : MonoBehaviour
{
    [SerializeField] private UIStatInfo _uiStatInfo;

    [SerializeField] private List<UILayoutGroup> _uiLayoutGroups = new List<UILayoutGroup>();

    public void SetBossStat(Stage_TableData data)
    {
        _uiStatInfo.Show(data);
        for (int n = 0; n< _uiLayoutGroups.Count; n++)
        {
            _uiLayoutGroups[n].UpdateLayoutGroup();
        }
    }

    public void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }
}
