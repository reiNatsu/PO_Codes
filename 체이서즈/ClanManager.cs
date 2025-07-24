using Consts;

using Newtonsoft.Json.Linq;

using System;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace LIFULSE.Manager
{
    public partial class GameInfoManager
    {
        private ClanConfig _clanConfig;
        private Dictionary<string, List<ClanConfig>> _clanConfigDic = new Dictionary<string, List<ClanConfig>>();
        //클랜 가입 제란 레벨 list
        private List<int> _restrictionlevels = new List<int>();
        private int _maxClanJoinLevel = 60;

        public ClanConfig ClanConfig { get => _clanConfig; }

        private List<ClanData> _joinClan = new List<ClanData>();
        public List<ClanData> JoinClans { get { return _joinClan; } }
        public List<int> RestrictionLevels { get => _restrictionlevels; }

        public int MaxHelperSlot { get; set; } = 0;         // 한 팀에 편성 가능한 조력자 수.
        public int MaxHelperDailyUse { get; set; } = 0;     // 조력자 일일 사용 가능 횟수.
        public int UseHelperNeedGold { get; set; } = 0;             // 조력자 사용시 드는 골드
        public bool IsCheckAttendance { get; set; } = false;
        public bool IsCheckConfirm { get; set; } = false;           // 클랜장 안내 팝업을 봤으면 true, 안봤으면 false

        public string PrevMasterName { get; set; } = null;
        // 임시 스킬 리스트
        private Dictionary<string, int> _skillPoint = new Dictionary<string, int> 
         {
            { "i_gold",1 },
            { "i_action_point",1 },
             { "i_cash",3},
         };

        public Dictionary<string, int> SkillList = new Dictionary<string, int>
        {
                { "i_gold", 8},
                {"i_action_point", 5 },
                {"i_cash", 2 }
            };


        // 스킬 버프 정보를 보너스 종류에 따라 string 넘겨주기.
        public string SetSkillBuffIndex(string bonustid, string indexTid)
        {
            var table = TableManager.Instance.Item_Bonus_Table[bonustid];
            var reward = TableManager.Instance.Reward_Table[table.Reward];
            string index = null;
            switch (table.Bonus_Type)
            {
                case "amount":
                    var bonusbuff = LocalizeManager.Instance.GetString(indexTid, GetSkillBuffAmount(bonustid));  //{0}%
                    index =  "<color=#C8912C> +" + bonusbuff+"</color>";
                    break;
                case "rate":
                    var ratebuff = LocalizeManager.Instance.GetString(indexTid, GetSkillBuffRate(bonustid));  //{0}%
                    index =  "<color=#C8912C> +" + ratebuff+"</color>";
                    break;
            }
            return index;
            //return reward.Item_Min;
        }
        public int GetSkillBuffAmount(string bonustid)
        {
            var table = TableManager.Instance.Item_Bonus_Table[bonustid];
            string buffstr = string.Empty;
            int buff = 0;
            buff = Mathf.FloorToInt(table.Bonus_Value * 100);       // 소수점 전부 버림
            return buff;
        }
        public float GetSkillBuffRate(string bonustid)
        {
            var table = TableManager.Instance.Item_Bonus_Table[bonustid];
            string buffstr = string.Empty;
            float buff = 0;
            buff = table.Bonus_Value;
            return buff;
        }

        public int GetClanSkillPoint(string name)
        {
            return _skillPoint[name];
        }
        public int GetClanSkillBuff(string name)
        {
            return SkillList[name];
        }

        public void SetHelperDefineInfo()
        {
            MaxHelperSlot = TableManager.Instance.Define_Table["ds_team_formation_helper"].Opt_01_Int;
            MaxHelperDailyUse = TableManager.Instance.Define_Table["ds_helper_use_max_count"].Opt_01_Int;
            UseHelperNeedGold = TableManager.Instance.Define_Table["ds_helper_default"].Opt_01_Int;
        }

        public void AddJoinClan(ClanData clandata)
        {
            if (_joinClan == null)
                _joinClan = new List<ClanData>();

            if (!_joinClan.Contains(clandata))
                _joinClan.Add(clandata);
        }
        public void CancleJoinClan(ClanData clandata)
        {
            if (_joinClan.Contains(clandata))
                _joinClan.Remove(clandata);
        }

        // 클랜  가입 조건 → 레벨 조건 list 추가. (5단위로. 0레벨이면 누구나 가입으로) :: TODO = 제한 MAX 레벨은 나중에 정의 필요. 
        public void SetClanRestrictionleveLists()
        {
            if (_restrictionlevels == null)
                _restrictionlevels = new List<int>();

            for (int n = 0; n < _maxClanJoinLevel+1; n+= 5)
            {
                var value = n;
                if (!_restrictionlevels.Contains(value))
                    _restrictionlevels.Add(value);
            }
        }

        // 클랜 가입 승인 타입 text 변화
        public string GetClanApproveString(int approveType)
        {
            string approveStr = string.Empty;
            switch (approveType)
            {
                case 0:
                    approveStr = "str_clan_join_type_01";  // 자동 가입
                    break;
                case 1:
                    approveStr = "str_clan_join_type_02";  // 가입 승인
                    break;
            }
            return approveStr;
        }

        public string SetMemberGrade(int grade)
        {
            string gradetext = string.Empty;
            switch (grade)
            {
                case 0:
                    gradetext = "str_clan_position_01";    // 마스터
                    break;
                case 1:
                    gradetext = "str_clan_position_02";     // 매니저
                    break;
                default:
                    gradetext = "str_clan_position_03";     // 맴버
                    break;
            }
            return gradetext;
        }

        public string GetClanConditionString(int conditionType, int conditionValue = 0)
        {
            string conditionStr = string.Empty;
            switch (conditionType)
            {
                case 0:
                    conditionStr = "누구나 가능";
                    break;
                case 1:
                    conditionStr = string.Format("Lv.{0} 이상", conditionValue);
                    break;
                default:
                    conditionStr = "누구나 가능";
                    break;
            }
            return conditionStr;
        }

        public string GetLastLoginTime(bool isMy, long lltIndex)
        {
            string lltText = string.Empty;

            if (isMy)
                lltText = LocalizeManager.Instance.GetString("str_clan_login_time_04");     //접속 중
            else
            {
                var lltValue = new DateTime(lltIndex, DateTimeKind.Utc);
                var remainTime = lltValue.GetLastLoginTime();

                if (remainTime.TotalSeconds <= CheckLoginPeriod)
                    lltText = LocalizeManager.Instance.GetString("str_clan_login_time_04");     //접속 중
                else
                {
                    if (lltValue == DateTime.MinValue)
                        lltText = LocalizeManager.Instance.GetString("str_clan_login_time_04");     //접속 중
                    else
                    {
                        if (remainTime.Days > 0)
                        {
                            lltText = LocalizeManager.Instance.GetString("str_clan_login_time_03", remainTime.Days);     // 일 전;
                        }
                        else
                        {
                            if (remainTime.Hours >0)
                                lltText = LocalizeManager.Instance.GetString("str_clan_login_time_02", remainTime.Hours);   // 시간 전;
                            else
                                lltText =  LocalizeManager.Instance.GetString("str_clan_login_time_01", remainTime.Minutes);  // {0}분 전
                        }
                    }
                    //lltText = lltValue.ToString();
                }
            }
            return lltText;
        }

        // 클랜 용 집무관 총 전투력 가지고 오기
        public int GetAllDosaTotalPower()
        {
            var allDosa = CharacterInfo.GetAllDosa();
            int power = 0;
            for (int n = 0; n< allDosa.Count; n++)
            {
                var dosa = CharacterInfo.GetDosa(allDosa[n].Tid);
                if (dosa != null)
                    power += dosa.GetTotalCombatPower();
            }

            Debug.Log("<color=#4cd311> ClanManager GetAllDosaTotalPower() : <b> " +power+ " </b></color>");
            return power;
        }

        public string GetApproveTypeStringId(int type)
        {
            string str = string.Empty;
           
            return str;
        }

        

        public bool CheckCanJoinClanLevel(ClanData data)
        {
            // 레벨 제한이 있는 경우 내 레벨과 비교
            var myLevel = GameInfoManager.Instance.AccountInfo.Level;
            if (data.ConditionType == 1 && myLevel < data.ConditionValue)
            {
                string str = "집무관 레벨이 조건 레벨보다 낮아서 가입신청이 불가능합니다!";
                UIManager.Instance.ShowToastMessage($"{str}");
                return false;
            }

            return true;
        }

        public bool CheckCanJoinClanCount(UserClan userClan)
        {
            // 내가 가입 신청한 클랜 리스트가 이미 5개인경우
            var maxCount = TableManager.Instance.Define_Table["ds_clan_max_join"].Opt_01_Int;
            if (userClan.JoinList.Count < maxCount)
                return true;

            string message = LocalizeManager.Instance.GetString("str_clan_join_deny_msg_01");           //클랜 가입 신청은 최대 5회까지 가능합니다.
            UIManager.Instance.ShowToastMessage(message);
            return false;
        }

        public bool CheckCanJoinClanTime(UserClan userClan, bool isCreate = false)
        {
            if (userClan == null || userClan.LimitTime == default(DateTime))
                return true;

            // 탈퇴 후 다음 가입 승인 시간이 안되었을 경우
            if (userClan.LimitTime >= DateTime.UtcNow.ToAddHours())
            {
                var str = SetRemainTime(userClan.LimitTime, isCreate);
                UIManager.Instance.ShowToastMessage(str);
                return false;
            }

            return true;
        }
        public bool CheckIsClanFullMember(ClanData data,bool isAccept = false)
        {
            var maxCount = TableManager.Instance.Define_Table["ds_clan_max_join_request"].Opt_01_Int;
            bool isEnableJoin = false;

            switch (data.JoinType)
            {
                case 0:
                    {
                        if (data.Configs.Count < data.MaxMemberCount)
                            isEnableJoin = true;
                        else
                        {
                            if (!isAccept)
                                UIManager.Instance.ShowToastMessage("str_clan_join_deny_msg_02");           //클랜 인원이 최대여서 신청이 불가합니다.
                            else
                                UIManager.Instance.ShowToastMessage("str_clan_agree_deny_msg_01");           //클랜원이 최대여서 더 이상 승인이 불가합니다.
                            //UIManager.Instance.ShowToastMessage("str_clan_join_deny_msg_02");           //클랜 인원이 최대여서 신청이 불가합니다.
                            isEnableJoin = false;
                        }
                    }
                    break;
                case 1:
                    {
                        if (data.JoinList.Count < maxCount && data.Configs.Count < data.MaxMemberCount)
                            isEnableJoin = true;
                        else
                        {
                            isEnableJoin = false;
                            if (data.Configs.Count >= data.MaxMemberCount)
                                UIManager.Instance.ShowToastMessage("str_clan_join_deny_msg_02");           //클랜 인원이 최대여서 신청이 불가합니다.;}
                            if (data.JoinList.Count >= maxCount)
                                UIManager.Instance.ShowToastMessage("str_clan_join_deny_msg_04");           //해당 클랜에 신청 가능 횟수가 최대입니다.

                        }
                
                    }
                    break;
            }

            return isEnableJoin;
        }

        public string SetRemainTime(DateTime goal, bool isCreate = false)
        {
            string message = null;
            DateTime now = DateTime.UtcNow.ToAddHours();
            TimeSpan timeDiff = goal - now;

            //message = string.Format("{0}시간 후 클랜 가입이 가능합니다.", timeDiff.Hours);
            if (!isCreate)       // 생성이 아니고 가입일 경우 String
            {
                message = LocalizeManager.Instance.GetString("str_clan_join_deny_msg_03", timeDiff.Hours);  //{0}시간 후 클랜 가입이 가능합니다.
                if (RestApiManager.Instance.ServerType ==ServerType.Test && timeDiff.Hours < 1)
                    message = LocalizeManager.Instance.GetString("str_clan_join_deny_msg_05", timeDiff.Minutes);  //{0}분 후 클랜 가입이 가능합니다.
            }
            else
            {
                message = LocalizeManager.Instance.GetString("str_clan_create_deny_msg_02", timeDiff.Hours);  //{0}시간 후 클랜 생성이 가능합니다.
                if (RestApiManager.Instance.ServerType ==ServerType.Test && timeDiff.Hours < 1)
                    message = LocalizeManager.Instance.GetString("str_clan_create_deny_msg_05", timeDiff.Hours);  //{0}분 후 클랜 생성이 가능합니다.
            }


            return message;
        }

        // 해당 메뉴 권한이 있는 지 체크
        public bool CheckEnableGrade(int grade)
        {
            //RestApiManager.Instance.RequestClanSetConfig(ClanInfo.GetUserClanConfig());
            var myConfig = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig;
            if (myConfig.Grade > grade)
            {
                if (grade == 0)
                    UIManager.Instance.ShowToastMessage("str_ui_clan_authority_error_01");          //클랜 마스터가 아니라 사용할 수 없습니다.
                else
                    UIManager.Instance.ShowToastMessage("str_clan_manager_right_deny_msg_01");  // 매니저 권한이 사라져 이용할 수 없습니다.
                return false;
            }
            return true;
        }


        // 클랜 정보 수정 바뀐게 있는지 체크
        public bool CheckIsNothingEdit(ClanData data, string icon, string name, string notice, int jointype, int conditiontype, int conditionvalue)
        {
            int changeCount = 0;
            // 아이콘 
            if (!string.IsNullOrEmpty(icon))
            {
                if (!data.Icon.Equals(icon))
                    changeCount++;
            }
            // 이름
            if (!string.IsNullOrEmpty(name))
            {
                if (!data.Name.Equals(name))
                    changeCount++;
            }
            // 공지
            if (!string.IsNullOrEmpty(notice))
            {
                if (!data.Notice.Equals(notice))
                    changeCount++;
            }
            // 가입 방식
            if (data.JoinType!=jointype)
                changeCount++;
            // 조건 타입
            if (data.ConditionType!=conditiontype)
                changeCount++;
            // 조건 값
            if (data.ConditionValue!=conditionvalue)
                changeCount++;

            return changeCount > 0;
        }

        // 클랜 정보 화면 오픈 이벤트 함수
        public void CheckClanConfig()
        {
            if (string.IsNullOrEmpty(ClanInfo.UserClanData.ClanConfig.ClanId))
                ShowClanList();
            else
            {
                // TODOCLAN : 나중에 수정 필요
                var clanid = ClanInfo.UserClanData.ClanConfig.ClanId;
                string targetPublicKey = null;
                var isConfirm = false;

                Debug.Log("<color=#4cd311>11 UILobby > clanid == <b>"+clanid+"</b></color>");
                //var str = GetChangeMasterTarget();
                // GameInfoManager.Instance.GetChangeMasterTarget();
                //클랜 위임 받을 사람 targetPublicKey에 초기화
                //targetPublicKey = GetChangeMasterTarget();
                if (ClanInfo.MyClanData != null && ClanInfo.NeedChangeMaster())
                {
                    // 클랜 자동 위임 멤버 리스트 > ClanManager에서 받아오기.
                    targetPublicKey = GetChangeMasterTarget();
                }

                if(ClanInfo.MyClanData.ConfirmMasterList != null)
                    isConfirm =  ClanInfo.MyClanData.ConfirmMasterList.Contains(RestApiManager.Instance.GetPublicKey());

                RestApiManager.Instance.RequestClanGetClanData(clanid, targetPublicKey, isConfirm, true, (response) =>
                {
                    // TODOCLAN : 이름 중복, 가입하려는 승인됨, 등급 변경
                    RestApiManager.Instance.CheckIsEmptyResult(response, () =>
                    {
                        var isCheck = !string.IsNullOrEmpty(targetPublicKey) || isConfirm;
                        // 여기서 kick인지 아닌지 확인하기.
                        ShowClan(response, isCheck);
                    });
                });
            }
        }

        public void ShowClanList()
        {
            // 가입된 클랜이 없을 경우 클랜 리스트 불러 온 뒤에 화면 전환
            RestApiManager.Instance.RequestClanGetList(() =>
            {
                UIManager.Instance.Show<UIClan>();
            });
        }

        public void ShowClan(JObject response, bool isConfirm)
        {
            ClanInfo.SetMyClanData(response["result"]);
            var clanData = ClanInfo.MyClanData;
            if (clanData == null)
                return;

            UIManager.Instance.Show<UIClanInfo>(Utils.GetUIOption(
                UIOption.Data, clanData,
                UIOption.Bool, GameInfoManager.Instance.IsCheckAttendance,
                UIOption.Bool2, isConfirm));
        }

        // ----- RequestClanSetConfig() 하기 전에 체크 해야 하는 사항.
        // 클랜 가입 신청 정보가 있는지 확인 
        public void CheckIsUpdateUserConfig(Action callback = null)
        {
            bool isNon = false;
            var userData = ClanInfo.UserClanData;
           
            if (userData != null)
            {
                if (!string.IsNullOrEmpty(userData.ClanConfig.ClanId))
                {
                    JObject config = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.SetConfigChange();
                    GameInfoManager.Instance.HelperInfo.UpdateHelpersUserName();
                    var helperDatas = GameInfoManager.Instance.HelperInfo.Helpers;

                    if (config.Count > 0)
                        RestApiManager.Instance.RequestClanSetConfig(config, helperDatas, callback);
                    else
                        callback?.Invoke();
                }
                else
                {
                    if (userData.JoinList.Count > 0)
                    {
                        RestApiManager.Instance.RequestClanGetUserData((response) =>
                        {
                            UserClan userClan = ClanInfo.UserClanData;
                            callback?.Invoke();
                        });
                    }
                    else
                        callback?.Invoke();
                }
            }
            else
                callback?.Invoke();
            //  callback?.Invoke();
        }

        // 이름 변경, 아이콘 변경, 프레인 변경 등 ClanMemberCell 에 보여지는 내용 중 변경된 내용이 있을 경우 호출
        public void RequestConfigChange(Action callback = null)
        {
            JObject config = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.SetConfigChange();
            GameInfoManager.Instance.HelperInfo.UpdateHelpersUserName();
            var helperDatas = GameInfoManager.Instance.HelperInfo.Helpers;

            if (config.Count > 0)
                RestApiManager.Instance.RequestClanSetConfig(config, helperDatas, callback);
        }
        public void TransferClanInfo(bool isAttendance)
        {
            ClanData myClanData = ClanInfo.MyClanData;
            UserClan userClanData = ClanInfo.UserClanData;
            UIManager.Instance.CloseAllUI();
            UIManager.Instance.Show<UILobby>();
            GameInfoManager.Instance.CheckClanConfig();
            CustomLobbyManager.Instance.RefreshLobby();
            //.Instance.Show<UIClanInfo>(Utils.GetUIOption(UIOption.Data, myClanData, UIOption.Data2, userClanData, UIOption.Bool, isAttendance));
        }

        public string GetDisableHelperString(DisableHelperState state)
        {
            var str = string.Empty;
            switch (state)
            {
                case DisableHelperState.Same:
                    str = "str_team_formation_deny_txt_01";         // 동일 캐릭터 편성 중
                    break;
                case DisableHelperState.AlreadyUse:
                    str = "str_team_formation_deny_txt_02";         // 이미 사용한 캐릭터 
                    break;
            }
            return str;
        }

        public string GetChangeMasterTarget()
        {
            if (ClanInfo.MyClanData.Configs == null)
                return null;

            List<ClanConfig> configs = new List<ClanConfig>();
            configs = ClanInfo.MyClanData.Configs.ToList();

            if (configs.Count == 1)
                return RestApiManager.Instance.GetPublicKey();
            
            configs.RemoveAt(0);
            if (!configs.Any())
                return null;
            
            configs = configs.OrderBy(c => c.Grade)
                .ThenByDescending(c => c.Point)
                .ThenByDescending(c => c.TotalPower)
                .ToList();

            string targetPublickey = null;
            double minTimeSpan = double.MaxValue;
            DateTime NowTime = DateTime.UtcNow;    // 현재 시간 설정

            for (int n = 0; n< configs.Count; n++)
            { 
                 var info = configs[n];
                DateTime lastLoginTime = new DateTime((long)info.LastLoginTime, DateTimeKind.Utc);
                var tiemSpan = NowTime.Subtract(lastLoginTime);
                var commissionTime = TableManager.Instance.Define_Table["ds_clan_auto_commission"].Opt_01_Int;
                if(info.LastLoginTime == 0 || tiemSpan.TotalSeconds < commissionTime)
                {
                    targetPublickey = info.PublicKey;
                    break;
                }

                if (tiemSpan.TotalSeconds < minTimeSpan)
                {
                    minTimeSpan = tiemSpan.TotalSeconds;
                    targetPublickey = info.PublicKey;
                }
            }

            //Debug.Log("<color=#ee4964>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            //for (int n = 0; n< ClanInfo.MyClanData.Configs.Count; n++)
            //{
            //    Debug.Log("<color=#4cd311>정렬 전 원본 Grade("+ClanInfo.MyClanData.Configs[n].Grade+")  Point("+ClanInfo.MyClanData.Configs[n].Point+")  TotalPower("+ClanInfo.MyClanData.Configs[n].TotalPower+")</color>");
            //}

            if (configs == null|| !configs.Any())
                return null;
            
            return targetPublickey;
        }

        // 조력자 사용 횟수 갱신 함수. => useCount가 0보다 클때만 호출
        public void SetHelperCountRefresh()
        {
            if (HelperInfo.UsedCount > 0)
            {
                RestApiManager.Instance.RequestHelperRefreshCount();
            }
        }

        // 클랜 이름 변경 가능 여부 확인0
        public bool EnableEditClanName()
        {
            //var nextEditName = ClanInfo.MyClanData.NameChangeTime.AddHours(24);
            float delaySeconds = TableManager.Instance.Define_Table["ds_clan_name_delay_time"].Opt_01_Int;
            var nextEditName = ClanInfo.MyClanData.NameChangeTime.AddSeconds(delaySeconds);
            var lastEditName = ClanInfo.MyClanData.NameChangeTime;
            if (lastEditName <= DateTime.Now && nextEditName >DateTime.Now)
            {
                DateTime now = DateTime.UtcNow.ToAddHours();
                TimeSpan timeDiff = nextEditName - now;
                 
                var str = LocalizeManager.Instance.GetString("str_clan_info_edit_deny_msg_01", timeDiff.Hours);         //{0}시간 후 클랜명을 수정할 수 있습니다.
                if (RestApiManager.Instance.ServerType ==ServerType.Test && timeDiff.Hours <= 0)
                    str =  LocalizeManager.Instance.GetString("str_clan_info_edit_deny_msg_02", timeDiff.Minutes);         //{0}분 후 클랜명을 수정할 수 있습니다.
                UIManager.Instance.ShowToastMessage(str);
                return false;
            }

            return true;
        }

        public ClanConfig GetMyClanCongfig(List<ClanConfig> configs)
        {
            ClanConfig config = new ClanConfig();
            var publickey = RestApiManager.Instance.GetPublicKey();
            for (int n = 0; n < configs.Count; n++)
            {
                if (publickey.Equals(configs[n].PublicKey))
                {
                    config = configs[n];
                    break;
                }
            }
            return config;
        }

        public ClanConfig GetMyClanCongfig()
        {
            ClanConfig config = new ClanConfig();
            if (ClanInfo.MyClanData.Configs != null)
            {
                var publickey = RestApiManager.Instance.GetPublicKey();
                for (int n = 0; n < ClanInfo.MyClanData.Configs.Count; n++)
                {
                    var data = ClanInfo.MyClanData.Configs[n];
                    if (publickey.Equals(data.PublicKey))
                    {
                        config = data;
                        break;
                    }
                }
            }
            else
                config = null;
           
            return config;
        }

        // 클랜 레벨이 Max 레벨 + Max Exp 인지 체크
        public bool IsMaxClanExp(int level, int exp)
        {
            var maxlevel = GetMaxClanLevel();
            var maxexp = TableManager.Instance.Clan_Level_Table.DataArray.LastOrDefault().Clan_Level_Exp;

            if (level == maxlevel && exp == maxexp)
                return true;

            return false;
        }

        public int GetMaxClanLevel()
        {
            var maxLevelData = TableManager.Instance.Clan_Level_Table.DataArray.LastOrDefault();
            var maxlevel = int.Parse(maxLevelData.Tid.Split("_").LastOrDefault());
            return maxlevel;
        }

        public bool IsKickedClan()
        {
            var configs = GameInfoManager.Instance.ClanInfo.MyClanData.Configs;
            var publickey = RestApiManager.Instance.GetPublicKey();
            int count = 0;
            
            if (configs.Count == 0)
                return true;

            for (int n = 0; n< configs.Count; n++)
            {
                if (configs[n].PublicKey.Equals(publickey))
                    count++;
            }
            return count ==0;
        }
    }
}
