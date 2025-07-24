using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UnityEngine;

public partial class RestApiManager : Singleton<RestApiManager> 
{
    public void CheckIsEmptyResult(JToken token,Action goEvent = null, Action nullEvent = null)
    {
        Action goNullEvent = null;
        // config 데이터 업데이트
        if (token["result"]["claninfo"] != null)
        {
            var infotoken = token["result"]["claninfo"];
            GameInfoManager.Instance.ClanInfo.UpdateUserClanInfo(infotoken);
        }
       
        if (token["result"]["state"] == null)
        {
            goEvent?.Invoke();
            return;
        }
        var state = token["result"]["state"];

        //if (token["result"].Type == JTokenType.Object)
        //{
        //    goEvent?.Invoke();
        //    return;
        //}
        ClanState tokenType = (ClanState)Enum.Parse(typeof(ClanState), state.ToString());
        // ClanState tokenType = (ClanState)Enum.Parse(typeof(ClanState), token["result"].ToString());
        switch (tokenType)
        {
            case ClanState.Kick:        // 추방 당했을때.
                goNullEvent = () =>
                {
                    UIManager.Instance.ShowToastMessage("str_clan_fire_msg_03");     // 클랜에서 추방당했습니다.
                    UIManager.Instance.CloseAllUI();
                    UIManager.Instance.Show<UILobby>();
                    //CustomLobbyManager.Instance.RefreshLobby();
                };
             break;
            case ClanState.NotFound:
                goNullEvent = () =>
                {
                    if (nullEvent == null)
                    {
                        GameInfoManager.Instance.ClanInfo.UserClanData.InitializeUserData();
                        //var config = GameInfoManager.Instance.ClanInfo.GetUserClanConfig();
                        JObject config = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.SetConfigChange();
                        GameInfoManager.Instance.HelperInfo.UpdateHelpersUserName();
                        var helperDatas = GameInfoManager.Instance.HelperInfo.Helpers;

                        if (config.Count > 0)
                        {
                            RequestClanSetConfig(config, helperDatas, () =>
                            {
                                Debug.Log("<color=#4cd311>22 UILobby > clanid == <b>"+GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId+"</b></color>");
                                RequestClanGetList(() =>
                                {
                                    UIManager.Instance.Show<UIClan>();
                                });
                            });
                        }
                        else
                        {
                            RequestClanGetList(() =>
                            {
                                UIManager.Instance.Show<UIClan>();
                            });
                        }
                    }
                    else
                        nullEvent.Invoke();
                };
                break;
            case ClanState.Overlapped:            // 이름 변경 시, 중복된 이름인 경우
                goNullEvent = () =>
                {
                    // 테스트 해 볼 필요 있음.
                    UIManager.Instance.ShowToastMessage("str_clan_create_deny_msg_03");     // 이미 사용중인 클랜명입니다.
                };
                break;
            case ClanState.Join:            // 클랜 가입 또는 가입 신청 하려고 할 때, 신청한 클랜에서 가입이 승인 되었을 때 
                goNullEvent = () =>
                {
                    // 테스트 해 볼 필요 있음.
                    UIManager.Instance.ShowToastMessage("str_clan_join_cancel_deny_msg_01");     // 해당 클랜에서 이미 가입을 승인하였습니다.
                    UIManager.Instance.CloseAllPopup();
                    UIManager.Instance.CloseAllUI();
                    UIManager.Instance.Show<UILobby>();
                    //CustomLobbyManager.Instance.RefreshLobby();
                };
                break;
            case ClanState.EmptyList:            // 클랜 신청 리스트가 없을때
                goNullEvent = () =>
                {
                    // 테스트 해 볼 필요 있음.
                    if(nullEvent == null)
                        UIManager.Instance.ShowToastMessage("str_clan_join_list_none");     //가입 신청한 유저가 없습니다.
                    else
                        nullEvent?.Invoke();
                    //UIManager.Instance.ShowToastMessage("아무도 없숴");

                };
                break;
            case ClanState.NoPermission:
                goNullEvent = () =>
                {
                    UIManager.Instance.ShowToastMessage("str_ui_clan_menu_access_impossible");          //해당 메뉴에 대한 권한이 없습니다.

                    if (UIManager.Instance.GetUI<UIClanInfo>() != null)
                    {
                        var uiClaninfo = UIManager.Instance.GetUI<UIClanInfo>();
                        uiClaninfo.UpdateUserClanInfo();
                        uiClaninfo.SetMemberGradeUI();
                    }
                };
                break;
            case ClanState.AlreadyJoin:                     // 이미 가입 된 클랜이 있을 경우
                goNullEvent = () =>
                {
                    RequestClanGetUserData((response) => {

                        Action onClickOk = () => {
                            GameInfoManager.Instance.IsCheckAttendance = false;
                            UIManager.Instance.CloseAllUI();
                            UIManager.Instance.Show<UILobby>();
                            GameInfoManager.Instance.CheckClanConfig();
                        };

                        var userclan = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig;
                       RequestClanGetClanData(userclan.ClanId,null, false, false, (response) =>
                        {
                            string title = LocalizeManager.Instance.GetString("str_clan_join_title");
                            string message = LocalizeManager.Instance.GetString("str_clan_join_msg_02", GameInfoManager.Instance.ClanInfo.MyClanData.Name);         //{0} 클랜에서 가입을 승인하였습니다.클랜 화면으로 이동됩니다.

                            UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK, title: title, message: message,
                           onClickOK: onClickOk, onClickCancel: onClickOk, onClickClosed: onClickOk, onClickExit: onClickOk);
                        });
                    });
                };
                break;
        }

        if (token["result"]["state"] == null)
        {
            goEvent?.Invoke();
        }
        else
            goNullEvent?.Invoke();
    }


    //클랜 내 유저 정보 가져오기 => ok
    [Button("RequestClanGetUserData")]
    public void RequestClanGetUserData(Action<JObject> callback = null)
    {
        RestApi("userclan/getdata", null, (state, response) =>
        {
            if (response["result"]["claninfo"] != null)
            {
                Debug.Log(response.ToString());
                UpdateInfo(response["result"]);
            }
            callback?.Invoke(response);
        });
    }

    //클랜 데이터 가져오기
    [Button("RequestClanGetClanData")]
    public void RequestClanGetClanData(string clanId, string targetPublicKey, bool isConfirm, bool isCheckKick, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        if(!string.IsNullOrEmpty(targetPublicKey))
            bodys["targetPublicKey"] = targetPublicKey;

        if(isCheckKick)
            bodys["isCheckKick"] = "1";

        bodys["clanId"] = clanId;
        bodys["isConfirm"] = isConfirm.ToString();

        RestApi("userclan/getclandata", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());

            if (response["result"]["claninfos"] != null)
            {
                var isCheckMy = false;
                if (response["result"]["clandata"]["M"]["ML"] != null)
                {
                    var members = response["result"]["clandata"]["M"]["ML"]["L"].ToArray();
                    for (int n = 0; n< members.Length; n++)
                    {
                        if (members[n].S_String().Equals(GetPublicKey()))
                            GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
                    }
                }
            }
            //if (!response["result"].ToString().Equals(ClanState.NotFound.ToString()))               //  나중에 수정 해야 함
            //    GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);

            callback?.Invoke(response);
        });
    }

    //클랜 생성 => state 변경 완료
    public void RequestClanCreate(string name, ClanConfig clanConfig, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["name"] = name;

        if (clanConfig != null)
            bodys["clanConfig"] = JsonConvert.SerializeObject(clanConfig);

        RestApi("userclan/create", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            //if (!response["result"].ToString().Equals(ClanState.Overlapped.ToString()))
            if (response["result"]["claninfos"] != null )
            {
                var claninfo = response["result"]["claninfos"]["L"].ToArray();
                UpdateInfo(response["result"]);
                UpdateUserClanInfo(claninfo[0]);
                GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
            }
            callback?.Invoke(response);
        });
    }

    //클랜 가입
    public void RequestClanJoin(string clanId, ClanConfig clanConfig, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        if (clanConfig != null)
            bodys["clanConfig"] = JsonConvert.SerializeObject(clanConfig);

        RestApi("userclan/join", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            //if (response["result"].Type == JTokenType.Object)
            if (response["result"]["claninfos"] != null)
            {
                var claninfo = response["result"]["claninfos"]["L"].ToArray();
                UpdateUserClanInfo(claninfo[0]);
                GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
            }
            //if (!response["result"].ToString().Equals(ClanState.Join.ToString()))
            //{
            //    var claninfo = response["result"]["claninfos"]["L"].ToArray();
            //    UpdateUserClanInfo(claninfo[0]);
            //    GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
            //}
            callback?.Invoke(response);
        });
    }

    //클랜 탈퇴
    public void RequestClanLeave(string clanId, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        RestApi("userclan/leave", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            if (response["result"]["claninfos"] != null)
            {
                var claninfo = response["result"]["claninfos"]["L"].ToArray();
                UpdateUserClanInfo(claninfo[0]);
            }
            callback?.Invoke(response);
        });
    }

    //클랜원 강퇴
    public void RequestClanOut(string clanId, string targetPublicKey, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;
        bodys["targetPublicKey"] = targetPublicKey;

        RestApi("userclan/out", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke(response);
        });
    }

    //스킬 강화
    public void RequestClanSkillUp(string skillTid, Action callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["skillTid"] = skillTid;

        RestApi("userclan/addskill", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke();
        });
    }

    //가입 조건 변경
    public void RequestClanSetCondition(int conditionType, int conditionValue, Action callback = null)
    {
        var bodys = new Dictionary<string, string>();

        RestApi("userclan/setcondition", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke();
        });
    }

    //클랜 검색
    public void RequestClanSearch(string clanName, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanName"] = clanName;

        RestApi("userclan/search", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke(response);
        });
    }

    //클랜원 등급 수정
    public void RequestClanSetGrade(string targetPublicKey, string clanId, int grade, Action<JToken> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["targetPublicKey"] = targetPublicKey;
        bodys["clanId"] = clanId;
        bodys["grade"] = grade.ToString();

        RestApi("userclan/setgrade", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke(response);
        });
    }

    //다른 유저들이 나를 봤을 때 정보 업데이트용
    public void RequestClanSetConfig(JObject config, Dictionary<string, HelperData> helperDatas, Action callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["config"] = JsonConvert.SerializeObject(config);

        if(helperDatas != null)
            bodys["helperDatas"] = JsonConvert.SerializeObject(helperDatas);

        RestApi("userclan/setconfig", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke();
        });
    }

    //클랜 정보 수정
    public void RequestClanSetSettings(string clanId, EditClanData settings, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;
        bodys["settings"] = JsonConvert.SerializeObject(settings);

        RestApi("userclan/setsettings", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            //if (!response["result"].ToString().Equals(ClanState.Overlapped.ToString()))
            if(response["result"]["claninfos"] != null || response["result"]["clandata"] != null)
            {
                GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
            }
            callback?.Invoke(response);
        });
    }

    public void UpdateUserClanInfo(JToken token)
    {
        if (token == null)
            return;

        GameInfoManager.Instance.ClanInfo.UpdateUserClanInfo(token);
        GameInfoManager.Instance.ClanInfo.SetMyClanData(token);
    }

    //출석
    public void RequestClanAttendance(Action callback = null, Action endCallback = null)
    {
        RestApi("userclan/attendance", null, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            UpdateReward(response, true, endCallback);
            callback?.Invoke();
        });
    }

    //선택한 클랜 정보 가져오기
    public void RequestClanGetInfos(List<string> ClanIds, Action callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanIds"] = JsonConvert.SerializeObject(ClanIds);

        RestApi("userclan/getinfo", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke();
        });
    }

    //클랜 UI에 출력되는 리스트 => ok
    [Button("GetClanList")]
    public void RequestClanGetList(Action callback = null)
    {
        RestApi("userclan/getlist", null, (state, response) =>
        {
            Debug.Log(response.ToString());
            GameInfoManager.Instance.ClanInfo.SetClanList(response);
            callback?.Invoke();
        });
    }

    //클랜 리스트 갱신
    public void RequestClanGetMemberJoinList(string clanId, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        RestApi("userclan/getmemberjoinlist", bodys, (state, response) =>
        {
           // if (response["result"]["claninfos"] != null)
            Debug.Log(response.ToString());           
            callback?.Invoke(response);
        });
    }

    //클랜 가입 신청 취소
    public void RequestClanJoinCancel(string clanId, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        RestApi("userclan/joincancel", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            if (response["result"]["claninfos"] != null)
            {
                var userClan = GameInfoManager.Instance.ClanInfo.UserClanData;
                GameInfoManager.Instance.ClanInfo.UserClanData.UpdateUserClan(userClan, response["result"]["claninfos"]["L"][0]["M"]);
                GameInfoManager.Instance.ClanInfo.UserClanData.UpdateJoinDatas(userClan, clanId);
            }
            callback?.Invoke(response);
        });
    }

    //클랜 가입 신청 승인
    public void RequestClanJoinAccept(string clanId, string targetPublicKey, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;
        bodys["targetPublicKey"] = targetPublicKey;

        RestApi("userclan/joinaccept", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke(response);
        });
    }

    //클랜 가입 신청 거절
    public void RequestClanJoinReject(string clanId, string targetPublicKey, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;
        bodys["targetPublicKey"] = targetPublicKey;

        RestApi("userclan/joinreject", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke(response);
        });
    }

    //클랜 가입 신청 리스트 정보
    public void RequestClanJoinDatas(Action<JObject> callback = null)
    {
        RestApi("userclan/getjoindatas", null, (state, response) =>
        {
            Debug.Log(response.ToString());
            GameInfoManager.Instance.ClanInfo.UserClanData.SetJoinList(response);
            callback?.Invoke(response);
        });
    }

    //클랜 삭제
    public void RequestClanDelete(string clanId, Action callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        RestApi("userclan/delete", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            var claninfo = response["result"]["claninfos"]["L"].ToArray();
            UpdateUserClanInfo(claninfo[0]);
            callback?.Invoke();
        });
    }


    //클랜 경험치 치트
    public void RequestClanAddExp(string clanId, int exp, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;
        bodys["exp"] = exp.ToString();

        RestApi("userclan/addexp", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            GameInfoManager.Instance.ClanInfo.MyClanData.Setup(response["result"]);
            callback?.Invoke(response);
        });
    }

    public void RequestClanRefreshWeekPoint(string clanId)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        RestApi("userclan/refreshweekpoint", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            GameInfoManager.Instance.ClanInfo.MyClanData.Setup(response["result"]);
        });
    }
}