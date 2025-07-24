using Consts;
using LIFULSE.Manager;
using System.Linq;
using UnityEngine;

public class UIQuestTab : MonoBehaviour
{
    [Header("[ UI ]")]
    [SerializeField] private ExImage _bg;
    [SerializeField] private ExTMPUI _title;
    [SerializeField] private ExTMPUI _title2;
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExButton _button;

    [Header("[ UI RedDot ]")]
    [SerializeField] private UIRedDot _uiReddDot;
    [Header("[ UI Etc ]")]
    [SerializeField] private UILayoutGroup _uiLayoutGroup;
    [Header("[ UI Etc ]")]
    [SerializeField] private QUEST_TYPE _questType;

    private UIQuest _uiQuest;
    private int _reddotIndex;

    public int GetRedDotKey()
    {
        return _uiReddDot.RdKey;
    }
    public string GetRedDotTid()
    {
        return _uiReddDot.RdTid;
    }

    public void Init(QUEST_TYPE questType, int index = 0, int parentKey =0 ,string parentTid = null)
    {
        _uiQuest = UIManager.Instance.GetUI<UIQuest>();
        _questType = questType;
        // 부모RD 인덱스, 부모RD Tid 가져오기
        if (_questType != QUEST_TYPE.all)
        {
            _reddotIndex = index;
            var rdtid = _uiReddDot.RdTid;
            var rddata = TableManager.Instance.RedDot_Table[rdtid];
            var parenttid = TableManager.Instance.RedDot_Table[rddata.Parent_Tids.FirstOrDefault()].Tid;
            _uiReddDot.UpdateInfo(_reddotIndex, parentTid, parentKey);
        }

        this.gameObject.name = "Q_tab_"+_questType.ToString()+"_"+_uiReddDot.RdKey;
        // SetTab();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // _title.text = _questType.ToString();
        _title.ToTableText(GameInfoManager.Instance.ReturnQuestTypeString(_questType));
        _title2.ToTableText(GameInfoManager.Instance.ReturnQuestTypeString(_questType));
        _bg.SetSprite("UI_Quest_TypeTap_Btn");
        // On :  UI_Quest_TypeTap_Btn_On   Off : UI_Quest_TypeTap_Btn
        ColorUtility.TryParseHtmlString("#575757", out Color icon_color);
        _title.color = icon_color;
        _icon.color = icon_color;
        SetTabIcon();
}

    public void OnEnableTab()
    {
        _bg.SetSprite("UI_Quest_TypeTap_Btn_On");
        _title.color = Color.white;
        _icon.color = Color.white;
        _button.interactable = false;
    }

    public void OnDisableTab()
    {
        _bg.SetSprite("UI_Quest_TypeTap_Btn");
        ColorUtility.TryParseHtmlString("#575757", out Color icon_color);
        _title.color = icon_color;
        _icon.color = icon_color;
        _button.interactable = true;
    }

    public void OnClickThisMenu()
    {
        OnEnableTab();
        _uiQuest.ChangeTab(_questType);
        //_uiStore.ChangeMainTab(_thisTab);
    }


    // 퀘스트 탭 아이콘 
    private void SetTabIcon()
    {
        if (!_icon.gameObject.activeInHierarchy)
        {
            _icon.gameObject.SetActive(true);
        }

        switch (_questType)
        {
            case QUEST_TYPE.all:
                _icon.SetSprite("UI_IC_all");
                break;
            case QUEST_TYPE.daily:
                _icon.SetSprite("UI_Quest_TapDayli_Icon");
                break;
            case QUEST_TYPE.week:
                _icon.SetSprite("UI_Quest_TapWeekly_Icon");
                break;
            case QUEST_TYPE.feat:
                _icon.SetSprite("UI_Quest_TapReward_Icon");
                break;
            case QUEST_TYPE.special:
                _icon.SetSprite("UI_Quest_TapSpecial_Icon");
                break;
        }
        _icon.rectTransform.sizeDelta = new Vector2(100, 100);
    }
   
}

//#575757
