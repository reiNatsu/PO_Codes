using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPopupClanManagement : UIBase
{
    [Header("신청관리 UI")]
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private GameObject _noJoinListObj;
    [SerializeField] private ExTMPUI _joinListInfoTMP;

    [Header("신청관리 OBJECT")]
    [SerializeField] private GameObject _joinListObj;
    [Header("채팅 OBJECT")]
    [SerializeField] private GameObject _chatObj;
    [Header("조력자 OBJECT")]
    [SerializeField] private GameObject _helperObj;

    [SerializeField] private List<UILayoutGroup> _uiLayoutGroups;

    private CLAN_MANAGEMENT_TYPE _showType;

    // 멤버 리스트
    private List<ClanMemberCell> _cells;
    private List<ClanMemberCellData> _cellDatas;

    private List<ClanConfig> _configs;
    private string _clanId;
    private bool _isAcceptJoin = false;
    public override void Close(bool needCached = true)
    {
        //if (_showType == CLAN_MANAGEMENT_TYPE.joinlist && _isAcceptJoin)
        //{
        //    RestApiManager.Instance.RequestClanGetClanData(_clanId, (res) =>
        //    {
        //        var clanData = new ClanData(res["result"]);

        //        var uiclaninfo = UIManager.Instance.GetUI<UIPopupClanInfo>();
        //        if (uiclaninfo != null && uiclaninfo.gameObject.activeInHierarchy)
        //        {
        //            uiclaninfo.SetClanMember(clanData.Configs.Count);
        //            uiclaninfo.UpdateClanMemberList(clanData.Configs);
        //        }
        //    });
        //}
        base.Close(needCached);
    }

    public override void Init()
    {
        base.Init();
        _scroll.Init();
        _cells = _scroll.GetCellToList<ClanMemberCell>();
        _cellDatas = new List<ClanMemberCellData>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.List, out var joinList))
            {
                if (_configs != null)
                    _configs.Clear();
                else
                    _configs = new List<ClanConfig>();
              
                _configs = (List<ClanConfig>)joinList;
            }
            if (optionDict.TryGetValue(UIOption.EnumType, out var showType))
            {
                _showType = (CLAN_MANAGEMENT_TYPE)showType;
            }
            if (optionDict.TryGetValue(UIOption.Id, out var clanID))
            {
                _clanId = clanID.ToString();
            }
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        //_showType에 따라서 채팅, 신청관리, 조력자 object로 보여주기
        switch (_showType)
        {
            case CLAN_MANAGEMENT_TYPE.chat:
                {
                    _chatObj.SetActive(true);
                    _joinListObj.SetActive(false);
                    _helperObj.SetActive(false);
                }
                break;
            case CLAN_MANAGEMENT_TYPE.joinlist:
                {
                    _chatObj.SetActive(false);
                    _joinListObj.SetActive(true);
                    _helperObj.SetActive(false);
                    SetClanJoinList(_configs);
                }
                break;
            case CLAN_MANAGEMENT_TYPE.helper:
                {
                    _chatObj.SetActive(false);
                    _joinListObj.SetActive(false);
                    _helperObj.SetActive(true);
                }
                break;
            default:
                {
                    _chatObj.SetActive(true);
                    _joinListObj.SetActive(false);
                    _helperObj.SetActive(false);
                }
                break;
        }
        for (int n = 0; n< _uiLayoutGroups.Count; n++)
        {
            _uiLayoutGroups[n].UpdateLayoutGroup();
        }
    }
    
    private void SetClanJoinList(List<ClanConfig> configs)
    {
        if (configs == null || configs.Count == 0)
        {
            _noJoinListObj.SetActive(true);
            _scroll.gameObject.SetActive(false);
            _joinListInfoTMP.ToTableText("str_ui_collection_monster_cost", 0, 20);
            return;
        }
        _joinListInfoTMP.ToTableText("str_ui_collection_monster_cost", configs.Count, 20);
        _noJoinListObj.SetActive(false);
        if (_scroll != null && !_scroll.gameObject.activeInHierarchy)
            _scroll.gameObject.SetActive(true);

        _cellDatas.Clear();
        var myClanConfig = GameInfoManager.Instance.GetMyClanCongfig();
        for (int n = 0; n< configs.Count; n++)
        {
            var member = configs[n];
            _cellDatas.Add(new ClanMemberCellData(member, myClanConfig, false));
        }
        
        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas, UpdateJoinList);
        }
        _scroll.ActivateCells(configs.Count);
    }

    public void UpdateJoinList(bool isAccept ,JToken token, Action goAction = null)
    {
        _isAcceptJoin = isAccept;
        if (_isAcceptJoin)
        {
            var clan = GameInfoManager.Instance.ClanInfo.MyClanData;
            var uiclaninfo = UIManager.Instance.GetUI<UIClanInfo>();
            if (uiclaninfo != null && uiclaninfo.gameObject.activeInHierarchy)
            {
                uiclaninfo.UIClanInfoObject.SetClanMember(clan.Configs.Count);
                uiclaninfo.UpdateClanMemberList(clan.Configs);
            }
        }

        goAction?.Invoke();
        // 가입신청 리스트 업데이트 
        RestApiManager.Instance.RequestClanGetMemberJoinList(_clanId, (response) =>
        {

            RestApiManager.Instance.CheckIsEmptyResult(response, 
                () => {
                    // null 이벤트 아닌 경우
                    if (response["result"].ToString().Equals(ClanState.EmptyList.ToString()))
                    {
                        _noJoinListObj.SetActive(true);
                        _scroll.gameObject.SetActive(false);
                        return;
                    }

                    var clanInfosToken = response["result"]["claninfos"];

                    if (clanInfosToken == null)
                        return;

                    var configs = new List<ClanConfig>();
                    var infos = clanInfosToken["L"].ToArray();

                    for (int i = 0; i < infos.Length; i++)
                    {
                        var config = new ClanConfig(infos[i]["M"]["config"]["M"]);

                        configs.Add(config);
                    }

                    //goAction?.Invoke();
                    SetClanJoinList(configs);
                },  () => {
                    // null 이벤트인 경우
                    //goAction?.Invoke();
                    _noJoinListObj.SetActive(true);
                    _scroll.gameObject.SetActive(false);
                    _joinListInfoTMP.ToTableText("str_ui_collection_monster_cost",0, 20);
                });
            //아무도 신청 안했을 때
           
            
        });
    }
}
