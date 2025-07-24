using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkinPageBG : UIBase
{

    [SerializeField] private ExImage _characterImg;
    [SerializeField] private ExTMPUI _characterName;

    private string _characterTid;
    private string _characterSprite;
    private Character_PC_TableData _characterData = null;

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    public override void Refresh()
    {
        base.Refresh();
    }
    public override void Init()
    {
        base.Init();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Tid, out var charTid))
            {
                _characterTid = (string)charTid;
                _characterData = TableManager.Instance.Character_PC_Table[_characterTid];
            }
        }

        SetCharacterName();
    }

    private void SetCharacterName()
    {
        _characterName.ToTableText(_characterData.Str);
        //_characterImg.SetSprite(_characterSprite);
    }

    public void SetCharacterImg(string sprite)
    {
        _characterImg.SetSprite(sprite);
    }
}
