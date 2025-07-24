using Consts;
using DebuggingEssentials;
using LIFULSE.CharacterController;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class UIPopupSupply : UIBase
{
    [Header("왼쪽")]
    [SerializeField] private ExTMPUI _levelTMP;
    [SerializeField] private Slider _accureSlider;        // 왼쪽 하단의 누적도 슬라이드
    [SerializeField] private ExTMPUI _accurePercentTMP;

    [SerializeField] private ExTMPUI _nextRefreshTMP;               // 10분당 보급품 획득량 TMP
    [SerializeField] private Slider _nextRefreshSlider;       // 다음 보급품 업데이트시간 게이지(10분)

    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private GridLayoutGroup _layout;
    [SerializeField] private GameObject _supplyItemObj;
    [SerializeField] private ExTMPUI _timeTMP;

    [Header("오른쪽")]
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private GameObject _confirmDimdObj;

    [Header("안내 팝업")]
    [SerializeField] private GameObject _infoObj;
    [SerializeField] private ScrollRect _infoScroll;
    [SerializeField] private List<UILayoutGroup> _uiLayoutGroups = new List<UILayoutGroup>();
    

    [Header("안내 팝업 보급풉 UI")]
    [SerializeField] private List<SupplyItemCell> _supplyInfoCells = new List<SupplyItemCell>();
    //[SerializeField] private List<ExTMPUI> _supplyEffectTMP = new List<ExTMPUI>();        // 클랜 스킬 효과 {0}% 표기들
    //[SerializeField] private ExTMPUI _randomBoxRateTMP;
    //[SerializeField] private ExTMPUI _choiceBoxRateTMP;

    private List<SupplyItemCell> _supplyCells;
    private List<ItemCell> _cells;
    private List<ItemCellData> _cellDatas = null;

    private Supplies_TableData _data;
    private CONTENTS_TYPE_ID _typeID;
    private int _timeVal = 0;
    private int _supplyLevel = 0;

   // private Coroutine _coroutine = null;
    private Coroutine _nextCoroutine = null;            // 10분당 보급 획득 타이머 코루틴
    private Coroutine _coroutine = null;                    // 10분마다 서버에 supply/update api 보내기 위한 코루틴
    private Coroutine _waitToast = null;

    //private bool _isTotal = false;
    private bool _isNext = false;
    private bool _isSendUpdate = false;
    private bool _isInfoOpend = false;

    public override void Close(bool needCached = true)
    {
        SetAllCoroutineStop();

        base.Close(needCached);
    }

    private void SetAllCoroutineStop()
    {
        _isNext = false;
      
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        if (_nextCoroutine != null)
        {
            StopCoroutine(_nextCoroutine);
            _nextCoroutine = null;
        }

        if(_waitToast != null)
        {
            StopCoroutine(_waitToast);
            _waitToast = null;
        }
    }

    public override void Init()
    {
        base.Init();

        _scroll.Init();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.EnumType, out var type))
                _typeID = (CONTENTS_TYPE_ID)type;
        }
        _infoObj.gameObject.SetActive(false);
        GetSupplyLevel();
        // 오른쪽 보급품 아이템 업데이트. 
        _cells = _scroll.GetCellToList<ItemCell>();
        _cellDatas = new List<ItemCellData>();
        _confirmDimdObj.SetActive(!GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].IsRecieve());

        _isInfoOpend = false;
        UpdateUI();
    }
    public void GetSupplyLevel()
    {
        _supplyLevel = GameInfoManager.Instance.GetSupplyLevel();
        _data =TableManager.Instance.Supplies_Table.GetData(_supplyLevel);
        _timeVal = TableManager.Instance.Supplies_Table.GetTimeValue(_supplyLevel);
    }

    public void UpdateUI()
    {
        int refreshTime = GameInfoManager.Instance.GetNextRefreshValue() / 60;
        _nextRefreshTMP.ToTableText("str_supples_10minuts_reward_01", refreshTime);

        SetTitleLevelUI();
        SetItemListUI();            // 왼쪽에 보상 받을 수 있는 아이템 목록(행동력, 골드, 랜덤박스 3개 보여짐)

        UpdateAccureSlider();
        _isNext = _timeVal > GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount;
        
        if (_isNext)
        {
            if (_nextCoroutine == null)
                _nextCoroutine = StartCoroutine(SetNextRefreshTimer());

            if (_coroutine == null)
                _coroutine = StartCoroutine(RequestUpdateSupply());
        }
        else
            ResetNextRefreshTimer();

        SetSupplyItemsUI();
        
    }
    private void SetTitleLevelUI()
    {
        //var index = LocalizeManager.Instance.GetString("str_ui_char_level_001", _supplyLevel);            //보급품 지급 레벨
        _levelTMP.ToTableText("str_ui_char_level_001", _supplyLevel);
        var time = _timeVal * GameInfoManager.Instance.GetNextRefreshValue();
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        int displayHours = (int)timeSpan.TotalHours; // 전체 시간을 정수로 변환
        _timeTMP.text = string.Format("{0:D2}:{1:D2}:{2:D2}", displayHours, timeSpan.Minutes, timeSpan.Seconds);
    }

    private void SetItemListUI()
    {
        var supplyItems = TableManager.Instance.Supplies_Table.GetItemList(_supplyLevel);
        if (_supplyCells == null)
        {
            _supplyCells = new List<SupplyItemCell>();
            _supplyCells.Add(_supplyItemObj.GetComponent<SupplyItemCell>());
        }

        if (supplyItems.Count > _supplyCells.Count)
            CreateCell(supplyItems.Count - _supplyCells.Count);

        UpdateSkillList(supplyItems);
    }

    private void CreateCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_supplyItemObj, _layout.transform);
            var supplyItemCell = newObj.GetComponent<SupplyItemCell>();

            _supplyCells.Add(supplyItemCell);
        }
    }

    public void UpdateSkillList(List<string> skillData)
    {
        if (skillData.Count > 3)
        {
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _layout.padding.right = 30;
        }
        else
        {
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _layout.padding.right = 0;
        }

        //List<string> keys = new List<string>(skillData.Keys);
        //List<int> values = new List<int>(skillData.Values);

        for (int i = 0; i < _supplyCells.Count; i++)
        {
            if (i < skillData.Count)
            {
                var index = i+1;
                _supplyCells[i].gameObject.SetActive(true);
                _supplyCells[i].Init(skillData[i], i == skillData.Count-1, _data.Supplies_Box_Icon);
                var desc = "str_supples_probability_up_"+index.ToString("D2");
                _supplyInfoCells[i].Init(skillData[i], i == skillData.Count-1, _data.Supplies_Box_Icon, desc);
            }
            else
                _supplyCells[i].gameObject.SetActive(false);
        }
    }

    private void ResetNextRefreshTimer()
    {
        SetAllCoroutineStop();
        _nextRefreshSlider.value = 0;
    }

    public IEnumerator SetNextRefreshTimer()
    {
        
        while (_isNext)
        {
            var nextTime = GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].GetNextSuppyRefreshTime();
            float minValue = 0;
            float maxValue = GameInfoManager.Instance.GetNextRefreshValue();             // defineTable값으로 변경 예정
            TimeSpan current = GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].SupplyRefreshTime - DateTime.UtcNow;
            float value = maxValue - (int)current.TotalSeconds;
            float currentvalue = maxValue - value;
            _nextRefreshSlider.minValue = minValue;
            _nextRefreshSlider.maxValue = maxValue;
            _nextRefreshSlider.value =value;

            // 나중에 reciveCount 추가 해서, 해당 값이 supplies_table의 supplies_time_value와 같으면 더이상 update함수 안보내도록 추가 
            if (value > maxValue)
            {
                if (_nextCoroutine != null)
                {
                    StopCoroutine(_nextCoroutine);
                    _nextCoroutine = null;
                }
            }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    IEnumerator RequestUpdateSupply()
    {
        //while (_isNext)
        //{
            float maxValue = GameInfoManager.Instance.GetNextRefreshValue();
            TimeSpan current = GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].SupplyRefreshTime - DateTime.UtcNow;
            int value = (int)Math.Ceiling(current.TotalSeconds);
            if (value < 0)
                value = 0;
            yield return new WaitForSecondsRealtime(value);

        if (GameInfoManager.Instance.SupplyInfo.IsRequestUpdate(_typeID))
        {
            var clanid = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
            yield return new WaitForSecondsRealtime(1.0f);
            if (GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].EnableUpdate())
            {
                RestApiManager.Instance.RequestSupplyUpdate(clanid, (res) =>
                {
                    GetSupplyLevel();
                    UpdateAccureSlider();
                    SetSupplyItemsUI();
                    UpdateLobbyPercent(GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount);
                    _confirmDimdObj.SetActive(!GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].IsRecieve());
                    // Update를 다 하면 다시 코루틴 시작.
                    if (_timeVal == GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount)
                    {   // max 카운트 만큼 보상이 쌓였을 때
                        _isNext = false;
                        ResetNextRefreshTimer();
                    }
                    else
                    {
                        _isNext = _timeVal > GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount;
                        if (_nextCoroutine == null)
                            _nextCoroutine = StartCoroutine(SetNextRefreshTimer());

                        if (_coroutine != null)
                        {
                            StopCoroutine(_coroutine);
                            _coroutine = null;
                        }
                        if (_coroutine == null)
                            _coroutine = StartCoroutine(RequestUpdateSupply());
                    }
                });
            }
        }
        else
        {
            _isNext = _timeVal > GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            if (_coroutine == null)
                _coroutine = StartCoroutine(RequestUpdateSupply());
        }

        //}
    }

    private void UpdateAccureSlider()
    {
         int value = GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount;
        _accureSlider.minValue = 0;
        _accureSlider.maxValue = _timeVal;
        if (_timeVal == value)
            value = _timeVal;

        _accureSlider.value = value;

        float percent = ((float)value / _timeVal) *100f;
        percent = Mathf.Round(percent);
        _accurePercentTMP.text = percent+"%";

        //UpdateLobbyPercent(percent);
    }

    // 오른쪽에 보상 리스트
    public void SetSupplyItemsUI()
    {
        _cellDatas.Clear();
        var supplyItems = GameInfoManager.Instance.SupplyInfo.GetSupplyDatas(_typeID);

        if (supplyItems != null)
        {
            foreach (var supplyinfo in supplyItems)
            {
                var itemtable = TableManager.Instance.Item_Table[supplyinfo.Key];
                _cellDatas.Add(new ItemCellData(new RewardCellData(supplyinfo.Key, supplyinfo.Value, itemtable.ITEM_TYPE), ItemCustomValueType.RewardAmount));
            }

            for (int n = 0; n < _cells.Count; n++)
            {
                _cells[n].SetDataCells(_cellDatas);
                _cells[n].EnableUnSelected(true);
            }

            _scroll.ActivateCells(_cellDatas.Count);
        }
    }

    private void UpdateLobbyPercent(int value)
    {
        var uilobby = UIManager.Instance.GetUI<UILobby>();
        if (uilobby != null && uilobby.gameObject.activeInHierarchy)
            uilobby.UISuppliesButtonObj.UpdateSuppliesGaugeUI(value);
    }

    public void OnClickRewardSupply()
    {
        if (GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].IsRecieve())
        {
            var clanId = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
            RestApiManager.Instance.RequestSupplyGetReward(clanId, (res) =>
            {
                SetAllCoroutineStop();
                GetSupplyLevel();
                UpdateAccureSlider();
                UpdateLobbyPercent(GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount);
                _isNext = true;
              
                if (_nextCoroutine == null)
                    _nextCoroutine = StartCoroutine(SetNextRefreshTimer());

                if (_coroutine == null)
                    _coroutine = StartCoroutine(RequestUpdateSupply());

                SetSupplyItemsUI();
                _confirmDimdObj.SetActive(!GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].IsRecieve());
                
                if (res["result"]["postinfo"] != null)
                    _waitToast = StartCoroutine(WaitShowToastMessage());
            });
        }
        else
            UIManager.Instance.ShowToastMessage("str_supples_reward_deny_01");          //획득 가능한 보상이 없습니다.
    }

    //행동력 Max치 넘어간건 우편으로 지급
    private IEnumerator WaitShowToastMessage()
    {
        var ui = UIManager.Instance.GetUI<UIRewardList>();

        yield return new WaitUntil(()=>!ui.gameObject.activeInHierarchy);

        //획득 가능 수량을 초과한 아이템은 우편으로 지급 되었습니다. : str_ui_supplies_error_01
        UIManager.Instance.ShowToastMessage("str_ui_supplies_error_01");
    }

    public void OnClickInfoObjectOpen()
    {
        if (_isInfoOpend)
            _isInfoOpend = false;
        else
            _isInfoOpend = true;
        _infoScroll.verticalNormalizedPosition = 1f;
        _infoObj.gameObject.SetActive(_isInfoOpend);
        if (_isInfoOpend)
        {
            for (int n = 0; n< _uiLayoutGroups.Count; n++)
            {
                _uiLayoutGroups[n].UpdateLayoutGroup();
            }
        }
    }

    public void OnClickInfoObjectClose()
    {
        if (_isInfoOpend)
            _isInfoOpend = false;
        
        _infoObj.gameObject.SetActive(_isInfoOpend);
    }

    public void OnClickConfirmDimd()
    {
        UIManager.Instance.ShowToastMessage("str_supples_reward_deny_01");          //획득 가능한 보상이 없습니다.
    }

}
