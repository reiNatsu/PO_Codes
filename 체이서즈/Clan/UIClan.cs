
using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIClan : UIBase
{
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private GameObject _noDataObj;
    [SerializeField] private ExTMPUI _noDataTMP;
    [Header("Main Button UI")]
    [SerializeField] private ExTMPUI _searchTMP;

    [Header("Search Clan UI")]
    [SerializeField] private TMP_InputField _searchClanInput;
    //[SerializeField] private UISearchClan _uiSearchClan;

    [Header("Refresh List UI")]
    [SerializeField] private int _delayTime;            // 딜레이 타임. 나중에 어디선가 받아 올 필요 있음....
    [SerializeField] private ExTMPUI _delayTimeTMP;
    [SerializeField] private GameObject _delayDimd;
    [SerializeField] private UILayoutGroup _uiLayoutGroup;

    [SerializeField] private List<ClanListCell> _uiClanList;

    private string _searchLocaliseId = "str_clan_list_search";      // 클랜 검색

    private List<ClanListCell> _cells;
    private List<ClanListCellData> _cellData;

    private List<ClanData> _clanList = new List<ClanData>();

    private Coroutine _refreshCoroutine = null;

    public override void Close(bool needCached = true)
    {
        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }

        base.Close(needCached);
        //if (_uiSearchClan.gameObject.activeInHierarchy)
        //    _uiSearchClan.gameObject.SetActive(false);
    }

    public override void Refresh()
    {
        base.Refresh();
        SetResetSearchInput();
    }

    public override void Init()
    {
        base.Init();
        _scroll.Init();

        _cells = _scroll.GetCellToList<ClanListCell>();
        _cellData = new List<ClanListCellData>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        SetResetSearchInput();
        // claninfo 가져오기. 
        //if (GameInfoManager.Instance.ClanInfo.ShowClanList == null)
        //    GameInfoManager.Instance.ClanInfo.SetClanList();
        _clanList = GameInfoManager.Instance.ClanInfo.ClanDatas;
        //_clanList = GameInfoManager.Instance.ClanInfo.ShowClanList;
        //UpdateClanData();
        SetRefreshTimeTMP(5);

        UpdateUI();
    }

    private void UpdateUI()
    {
        //SetSearchClasnTMP();
        //SetSearchTMP();
        //SetClanSearchUI(false);
        SetRefreshDelayDimd(false);
        SetIsExistClan(_clanList.Count > 0);
        if (_clanList.Count > 0)
        {
            UpdateClanList(_clanList);
        }
        //else
        //    SetIsExistClan(false);
        _uiLayoutGroup.UpdateLayoutGroup();
    }

    private void SetIsExistClan(bool isExist, string message = null)
    {
        _scroll.gameObject.SetActive(isExist);
        _noDataObj.SetActive(!isExist);

        if (string.IsNullOrEmpty(message))
            _noDataTMP.ToTableText("str_clan_list_none_01");    // 생성된 클랜이 없습니다.
        else
            _noDataTMP.ToTableText(message);
    }

    public void UpdateClanData(string name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var data = GameInfoManager.Instance.ClanInfo.SetSearchClanList(name);
            UpdateClanList(data);
        }
        else
            UpdateClanList(_clanList);
    }
    public void UpdateClanList(List<ClanData> list)
    {
        _cellData.Clear();
        SetIsExistClan(list.Count >0);
        Debug.Log("<color=#4cd311> UpdateClanList("+list.Count+")</color>");
        for (int n = 0; n< list.Count; n++)
        {
            _cellData.Add(new ClanListCellData(list[n]));
        }

        for (int f = 0; f< _cells.Count; f++)
        {
            _cells[f].SetCellDatas(_cellData, true);
        }
        //_scroll.ActivateCells(list.Count);
        _scroll.ActivateCells(list.Count);
    }

    private void SearchClan(string clanName)
    {
        if (string.IsNullOrEmpty(clanName))
        {
            UIManager.Instance.ShowToastMessage("str_clan_search_deny_01");     // 클랜 이름에 입력한 내용이 없습니다.
            //UpdateClanList(_clanList);
            return;
        }

        RestApiManager.Instance.RequestClanSearch(clanName, (response) =>
        {
            //SetIsExistClan(response["result"].Type != JTokenType.String, "str_clan_list_none_02");      // 입력한 내용에 해당되는 클랜이 없습니다.
            //// 검색한 클랜이 없을경우
            //if (response["result"].Type == JTokenType.String)
            //{
            //    return;
            //}

            RestApiManager.Instance.CheckIsEmptyResult(response, () => {
                var clanData = new ClanData(response["result"]);
                List<ClanData> searchList = new List<ClanData>();
                searchList.Add(clanData);
                UpdateClanList(searchList);
            }, () => {
                SetIsExistClan(response["result"]["state"] == null, "str_clan_list_none_02");      // 입력한 내용에 해당되는 클랜이 없습니다.
            });

            //var clanData = new ClanData(response["result"]);
            //List<ClanData> searchList = new List<ClanData>();
            //searchList.Add(clanData);
            //UpdateClanList(searchList);
            return;
        });
    }

    // 검색 필드 초기화 함수.
    private void SetResetSearchInput()
    {
        _searchClanInput.text = "";
        _searchClanInput.placeholder.gameObject.SetActive(true);
    }

    public void OnClickRefreshSearch()
    {
         //사용 X
        UpdateClanList(_clanList);
    }

    // 검색 버튼 클릭
    public void OnClickClanSearch()
    {
        var name = _searchClanInput.text;
        SearchClan(name);
    }

    // 새로고침 버튼 클릭
    public void OnClickRefreshList()
    {
        SetResetSearchInput();
        SetRefreshDelayDimd(true);
      
        RestApiManager.Instance.RequestClanGetList(() =>
        {
            Debug.Log("<color=#4cd311><b> 클랜 리스트 새로고침 완료!("+GameInfoManager.Instance.ClanInfo.ClanDatas.Count+")</b></color>");
            UpdateClanList(GameInfoManager.Instance.ClanInfo.ClanDatas);
            if (_refreshCoroutine == null)
                _refreshCoroutine = StartCoroutine(SetDelayFreshTime());
        });
    }

    public void OnClickCreateClan()
    {
        UIManager.Instance.Show<UICreateClan>();
       // RestApiManager.Instance.RequestClanCreate("테스트길드");
    }

    public void OnClickShowJoinList()
    {
        Debug.Log("<color=#4cd311><b> OnClickShowJoinList() </b></color>");
        //ClanConfig clanconfig = GameInfoManager.Instance.ClanInfo.GetUserClanConfig();
        //RestApiManager.Instance.RequestClanJoin(GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId, 
        //    clanconfig, (response) => {

        //    RestApiManager.Instance.CheckIsEmptyResult(/*ClanState.Join,*/ response, () => {
        //        Action callback = () =>
        //        { 
        //        RestApiManager.Instance.RequestClanJoinDatas((response) => UIManager.Instance.Show<UIPopupClanJoinInfo>());
        //        };

        //       GameInfoManager.Instance.CheckIsUpdateUserConfig(callback);
        //    });
        //});


        Action callback = () =>
        {
            if (string.IsNullOrEmpty(GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId))
            {   // 아직 가입 된 클랜이 없을때
                RestApiManager.Instance.RequestClanJoinDatas((response) => UIManager.Instance.Show<UIPopupClanJoinInfo>());
            }
            else
            {   // 가입 된 클랜이 있을 때
                Action onclickok = () =>
                {
                    ClanConfig clanconfig = GameInfoManager.Instance.ClanInfo.GetUserClanConfig();
                    RestApiManager.Instance.RequestClanJoin(GameInfoManager.Instance.ClanInfo.MyClanData.Id, 
                        clanconfig, (response) =>
                    {
                        GameInfoManager.Instance.IsCheckAttendance = false;
                        UIManager.Instance.CloseAllUI();
                        UIManager.Instance.Show<UILobby>();
                        GameInfoManager.Instance.CheckClanConfig();
                    });
                };

                ClanData clanData = GameInfoManager.Instance.ClanInfo.MyClanData;
                string title = LocalizeManager.Instance.GetString("str_clan_join_title");
                string message = message = LocalizeManager.Instance.GetString("str_clan_join_msg_02", clanData.Name);
                UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK, title: title, message: message,
                 onClickOK: onclickok);
            }
        };
        GameInfoManager.Instance.CheckIsUpdateUserConfig(callback);
    }

    private IEnumerator SetDelayFreshTime()
    {
        while (_delayTime > 0)
        {
            yield return new WaitForSeconds(1);
            _delayTime--;
            SetRefreshTimeTMP(_delayTime);
            if (_delayTime == 0)
            {
                if (_refreshCoroutine != null)
                {
                    StopCoroutine(_refreshCoroutine);
                    _refreshCoroutine = null;
                    _delayTime = 5;
                    SetRefreshTimeTMP(_delayTime);
                    SetRefreshDelayDimd(false);
                }
            }
        }
    }

    private void SetSearchClasnTMP(string title = null)
    {
        //_searchLocaliseId
        if (string.IsNullOrEmpty(title))
            _searchTMP.ToTableText(_searchLocaliseId);
        else
            _searchTMP.text = title;
    }

    private void SetRefreshTimeTMP(int time)
    {
        //_delayTimeTMP.text = time+"S";
        _delayTimeTMP.ToTableText("str_ui_skill_second", time);     // N초
    }
    private void SetRefreshDelayDimd(bool isOn)
    {
        if (_delayDimd != null && _delayDimd.activeInHierarchy == !isOn)
            _delayDimd.SetActive(isOn);
    }

    public void SetSearchTMP(string index = null)
    {
        if (string.IsNullOrEmpty(index))
            _searchTMP.ToTableText(_searchLocaliseId);
        else
            _searchTMP.text = index;
    }

    public void OnSearchSelected(bool isHide)
    {
        if (!isHide && !string.IsNullOrEmpty(_searchClanInput.text))
            _searchClanInput.placeholder.gameObject.SetActive(false);

        _searchClanInput.placeholder.gameObject.SetActive(!isHide);
       
    }
}
 