using Consts;
using LIFULSE.Manager;
using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class ClanListCellData
{
    [SerializeField] public ClanData clanData;

    public ClanListCellData(ClanData data)
    {
        clanData = data;
    }
}


public class ClanListCell : ScrollCell
{
    [SerializeField] private ExImage _iconImage;
    [SerializeField] private ExTMPUI _nameTMP;
    [SerializeField] private ExTMPUI _levelTMP;
    [SerializeField] private ExTMPUI _joinTypeTMP;
    [SerializeField] private ExTMPUI _memberCountTMP;

    [SerializeField] private GameObject _infoBtn;
    [SerializeField] private GameObject _joinCancleBtn;

    private List<ClanListCellData> _cellDatas;
    private ClanData _data;
    private bool _isShowInfo = false;
    private string _clanId;

    private Action _onShowInfo = null;
    private Action _onCancleJoin = null;

    // RecycleScroll 사용 안하고 cell 가져와서 데이터 넣을 경우 사용
    public void InitData(ClanData clandata, bool isShowInfo, Action OnCancleJoin = null)
    {
        _data = clandata;
        _onCancleJoin =OnCancleJoin;
        SetChangeButtonState(isShowInfo);

        UpdateUI();
    }
    public void InitData(string clanId, bool isShowInfo, Action OnCancleJoin = null)
    {
        _clanId = clanId;
        _nameTMP.text = clanId;
        SetChangeButtonState(isShowInfo);
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;
        _data = _cellDatas[DataIndex].clanData;

        SetChangeButtonState(_isShowInfo);
        UpdateUI();
    }

    private void UpdateUI()
    {
        _nameTMP.text = _data.Name;
        _levelTMP.text = _data.Level.ToLevelText();

        if (string.IsNullOrEmpty(_data.Icon))
            _iconImage.SetSprite("IC_MN_dongjasam_elite_01_result");
        else
        {
            var icon = TableManager.Instance.Clan_Icon_Table[_data.Icon];

            if(icon != null)
                _iconImage.SetSprite(icon.Icon_Id);
        }

        SetClanMembersInfo();
        SetClanConditionInfo();
    }

    private void SetClanMembersInfo()
    {
        int maxMembers = _data.MaxMemberCount;
        int currentMembers = _data.CurrentMemberCount;       // 나중에 바꿔줘야 함. 
                                                             //StringBuilder sb = new StringBuilder();
                                                             //sb.Append(currentMembers);
                                                             //sb.Append("/");
                                                             //sb.Append(maxMembers);

        //_memberCountTMP.text = sb.ToString();
        _memberCountTMP.ToTableText("str_ui_collection_monster_cost", currentMembers, maxMembers);
    }

    private void SetClanConditionInfo()
    {
        string approveStr = GameInfoManager.Instance.GetClanApproveString(_data.JoinType);
        string conditionTypeStr = GameInfoManager.Instance.GetClanConditionString(_data.ConditionType, _data.ConditionValue);

        StringBuilder sb = new StringBuilder();
        sb.Append(LocalizeManager.Instance.GetString(approveStr));
        sb.AppendLine();
        sb.Append(conditionTypeStr);

        //_joinTypeTMP.text = sb.ToString();
        _joinTypeTMP.ToTableText(approveStr);
    }

    public void SetCellDatas(List<ClanListCellData> datas, bool isShowInfo)
    {
        _cellDatas = datas;
        _isShowInfo = isShowInfo;
    }

    private void SetChangeButtonState(bool isShowInfo)
    {
        _infoBtn.gameObject.SetActive(isShowInfo);
        _joinCancleBtn.gameObject.SetActive(!isShowInfo);
    }    

    public void OnClickInfo()
    {
        RestApiManager.Instance.RequestClanGetClanData(_data.Id, null, false, false,
            (res) =>
            {
                var clanData = new ClanData(res["result"]);
                UIManager.Instance.Show<UIPopupClanInfo>(Utils.GetUIOption(
                    UIOption.Data, clanData,
                    UIOption.Bool, false));
            });
    }

    public void OnClickJoinCancle()
    {
        // 가입 취소 api... 없나....?
        Debug.Log("<color=#9efc9e>가입취소 키 누름.</color>");

        RestApiManager.Instance.RequestClanJoinCancel(_data.Id, (response) =>
        //RestApiManager.Instance.RequestClanJoinCancel(_data.Id, (response) =>
        {
            RestApiManager.Instance.CheckIsEmptyResult(response, () => {
                string message = LocalizeManager.Instance.GetString("str_clan_join_cancel_msg");   // 클랜 가입이 취소되었습니다.
                UIManager.Instance.ShowToastMessage(message);
                GameInfoManager.Instance.CancleJoinClan(_data);
                _onCancleJoin?.Invoke();
                var uiclanjoinpopup = UIManager.Instance.GetUI<UIPopupClanJoinInfo>();
                if (uiclanjoinpopup != null && uiclanjoinpopup.gameObject.activeInHierarchy)
                    uiclanjoinpopup.UpdateJoinList(GameInfoManager.Instance.ClanInfo.UserClanData.JoinDatas);
            });
        });
    }
}
