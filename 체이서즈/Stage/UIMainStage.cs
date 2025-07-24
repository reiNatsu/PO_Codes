using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainStage : UIBase
{
    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }

    public override void Init()
    { 
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        
    }

    public void OnClickOpenStory()
    {
        //UIManager.Instance.Show<UIStoryMain>();
        UIManager.Instance.Show<UIStoryList>();
        this.Close();
    }

    public void OnClickOpenStage()
    {
        UIManager.Instance.Show<UIChapter>();
        this.Close();
    }
}
