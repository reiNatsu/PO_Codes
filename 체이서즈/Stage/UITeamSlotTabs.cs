using System;
using System.Collections.Generic;
using UnityEngine;

public class UITeamSlotTabs : MonoBehaviour
{
    [SerializeField] private List<UITeamButton> _slotButtons = new List<UITeamButton>();

    private int _index = 0;
    Action<int> _updateSlots = null;

    public void Init(bool isUse, int index, Action<int> updateSlots)
    {
        this.gameObject.SetActive(isUse);
        _index = index;
        _updateSlots = updateSlots;
        if (isUse)
            UpdateUI();
    }

    private void UpdateUI()
    {
        for (int n = 0; n< _slotButtons.Count; n++)
        {
            _slotButtons[n].ActivateButton(_index == n);
        }

    }

    public void OnClicSlotButton(int index)
    {
        if (_index == index)
            return;

        _slotButtons[_index].ActivateButton(false);
        _slotButtons[index].ActivateButton(true);
        _index = index;


        _updateSlots?.Invoke(_index);
    }
}
