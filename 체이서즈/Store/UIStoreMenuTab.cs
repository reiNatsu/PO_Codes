using LIFULSE.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum StoreTabType
{
    MainOption              =0,
    SubOption
}

public class UIStoreMenuTab : MonoBehaviour
{
    [SerializeField] private string _thisTab;
    [SerializeField] private ExTMPUI _title;
    [SerializeField] private ExImage _bg;
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExButton _button;

    private UIStore _uiStore;
    private Dictionary<string, int> _subTabs = new Dictionary<string, int>();
    private StoreTabType _tabType;

     public ExButton Button { get { return _button; } }

    public void InitData(string menuType, string title, StoreTabType type)
    {
        _uiStore = UIManager.Instance.GetUI<UIStore>();
        _thisTab = menuType;
        _title.ToTableText(title);
        _tabType = type;
        UpdateUI();
    }

    private void UpdateUI()
    {

        _bg.enabled = false;
        if (_icon != null)
        {
            _icon.SetSprite("UI_Store_TatBtn_Off");
            _icon.rectTransform.sizeDelta = new Vector2(40, 40);
        }

        ColorUtility.TryParseHtmlString("#575757", out Color icon_color);

        _title.color = icon_color;

    }

    public void OnSetSubMenus(string mainTab, bool isOn)
    {
        var subMenuDic = _uiStore._storeSubMenuDic[mainTab];
        foreach (var subs in subMenuDic)
        {
            subMenuDic[subs.Key].gameObject.SetActive(isOn);
            subMenuDic[subs.Key].DisableSubMenuButton();
        }
    }
    public void EnableSubMenuButton()
    {
        _bg.enabled = true;
        _title.color = Color.white;
        _button.interactable = false;
    }
    public void DisableSubMenuButton()
    {
        _button.interactable = true;
        _bg.enabled = false;
        _title.color = Color.black;
    }

    //public void OnClickSubTab()
    //{
    //    Debug.Log("OnClickSubTab("+_thisTab+")");
    //    EnableSubMenuButton();
    //    _uiStore.ChangeSubTab(_thisTab);
    //}

}
