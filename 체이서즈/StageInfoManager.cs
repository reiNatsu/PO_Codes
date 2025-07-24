using Consts;
using LIFULSE.CharacterController;
using Pathfinding.ECS.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor.Tilemaps;
#endif
using UnityEngine;

namespace LIFULSE.Manager
{
    public enum EnterStatus
    {
        Success,
        Failed,
        Wait
    }

    public partial class GameInfoManager
    {
        private int _currentChapter;
        public int CurrentChapter { get => _currentChapter; set => _currentChapter = value; }
        private bool _storyMode;

        public bool StoryMode
        {
            get => _storyMode;
            set
            {
                _storyMode = value;
            }
        }

        private string _enterStageTid;

        public string EnterStageTid
        {
            get
            {
                return _enterStageTid;
            }
            set
            {
                _enterStageTid = value;
                
                
            }
        }

        public int KkaebiDungeonPoint { get; set; }

        //기믹 관련 데이터들
        private Dictionary<string, int> _localGimmickValue = new Dictionary<string, int>();
        private HashSet<string> _localGimmickStatus = new HashSet<string>();

        private Dictionary<string, Action> _completedLocalGimmickEvents = new Dictionary<string, Action>();
        private Dictionary<string, Action> _failLocalGimmickEvents = new Dictionary<string, Action>();

        private LEVEL_DIFFICULTY _stageDifficulty = LEVEL_DIFFICULTY.normal;

        public LEVEL_DIFFICULTY StageDifficulty
        {
            get => _stageDifficulty;
            set => _stageDifficulty = value;
        }

        public Dictionary<string, int> ConditionLocalGimmickValue { get => _localGimmickValue; }
        public EnterStatus EnterStatus { get; set; } = EnterStatus.Wait;

        public string GetSceneName()
        {
            string name = null;

            switch (SceneManager.Instance.SceneState)
            {
                case SceneState.PrologueSequence:
                case SceneState.LobbyScene:
                case SceneState.CombatScene:

                case SceneState.MapScene:
                    name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    break;
                default:
                    break;
            }

            return name;
        }

        public void SetupStageInfo()
        {

        }

        //스테이지 클리어 분기
        public void Result(ResultData resultData, List<string> deadList)
        {
            AudioManager.Instance.StopAudio("sfx_ui_stage_clear");
            var stageData = TableManager.Instance.Stage_Table[resultData.StageTid];

            CONTENTS_TYPE_ID contentId = stageData.CONTENTS_TYPE_ID;

            switch (contentId)
            {
                case CONTENTS_TYPE_ID.challenge:
                case CONTENTS_TYPE_ID.trial:
                    if (resultData.ResultType == ResultType.Victory)
                        ResultChallenge(resultData, stageData);
                    else
                    {
                        UIManager.Instance.Show<UIResultDefeat>(Utils.GetUIOption(
                            UIOption.Message, "str_ui_stage_failed_info_01",
                            UIOption.EnumType, contentId,
                            UIOption.Tid, resultData.StageTid));
                    }
                    break;
                case CONTENTS_TYPE_ID.total:
                    ResultTotal(resultData, stageData);
                    break;
                default:
                    {
                        if (resultData.ResultType == ResultType.Victory)
                        {
                            switch (contentId)
                            {
                                case CONTENTS_TYPE_ID.stage_main:
                                case CONTENTS_TYPE_ID.stage_sub:
                                case CONTENTS_TYPE_ID.stage_character:
                                    ResultStage(resultData, deadList, stageData.LEVEL_TYPE == LEVEL_TYPE.story);
                                    break;
                                case CONTENTS_TYPE_ID.event_main:
                                    ResultEventStory(resultData, deadList);
                                    break;
                                case CONTENTS_TYPE_ID.manjang:
                                case CONTENTS_TYPE_ID.gold:
                                case CONTENTS_TYPE_ID.elemental:
                                case CONTENTS_TYPE_ID.challenge:
                                case CONTENTS_TYPE_ID.exp:
                                case CONTENTS_TYPE_ID.trial:
                                case CONTENTS_TYPE_ID.content_class:
                                case CONTENTS_TYPE_ID.relay_boss:
                                    ResultDungeon(resultData, deadList);
                                    break;
                                case CONTENTS_TYPE_ID.prologue:
                                    /*if (stageData.Tid == "prologue_01")
                                    {
                                        AccountInfo.PrograssIndex = AccountInfo.PrologueStep.PrologueCombat2;
                                        GameInfoManager.Instance.EnterStageTid = "prologue_02";
                                        SceneManager.Instance.LoadCombatScene("r_prologue_02");
                                    }else*/ 
                                    if (stageData.Tid ==  "prologue_01") {
                                        
                                        AccountInfo.PrograssIndex = AccountInfo.PrologueStep.PrologueSequnece2;
                                        RestApiManager.Instance.SetPrograssIndex(
                                            (result) =>
                                            {
                                                GameManager.Instance.CacheStoryName = "S01_00_P02";
                                                SceneManager.Instance.ChangeSceneState(SceneState.PrologueSequence);
                                            });
                                    }
                                    else
                                    {
                                        AccountInfo.PrograssIndex = AccountInfo.PrologueStep.PrologueSequnece4;
                                        RestApiManager.Instance.SetPrograssIndex(
                                            (result) =>
                                            {
                                                GameManager.Instance.CacheStoryName = "S01_00_P04";
                                                SceneManager.Instance.ChangeSceneState(SceneState.PrologueSequence);
                                            });
                                    }

                                    break;
                                case CONTENTS_TYPE_ID.character_guide:
                                    ResultCharacterGuide(resultData);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                            UIManager.Instance.Show<UIResultDefeat>(Utils.GetUIOption(
                                UIOption.Message, "str_ui_stage_failed_info_01",
                                  UIOption.EnumType, contentId,
                                UIOption.Tid, resultData.StageTid));
                    }
                    break;
            }

            // 여기서....??
        }

        private void ResultDungeon(ResultData resultData, List<string> deadList)
        {
            var team = resultData.Team;
            var teamUsetType = resultData.TeamUseType;
            int[] starValues = null;
            var stageData = TableManager.Instance.Stage_Table[resultData.StageTid];

            //if (stageData.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.manjang)
            //    starValues = resultData.GetAchieveStarCount(deadList.Count);

            StageResutlData stageResultData = new StageResutlData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid, resultData.RemainTime, resultData.ResultType, 1, resultData.CombatPower, starValues);
            var isClear = DungeonInfo.IsClear(stageData.CONTENTS_TYPE_ID, stageData.Tid);
            var questData = RestApiManager.Instance.GetStageQuestDatas(resultData.StageTid, false);

            RestApiManager.Instance.RequestDungeonClear(resultData.ResultType, resultData.StageTid, resultData.Team, questData, 1, isClear, starValues,
                (response) =>
                {
                    if (stageData.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.manjang)
                        DungeonInfo.UpdateStarRewardStates(CONTENTS_TYPE_ID.manjang);

                    var list = Utils.ToRewardCellDatas(response["result"]["reward"]);
                    UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                        UIOption.Data, stageResultData,
                        UIOption.List, list));

                    //RestApiManager.Instance.UpdateLevelUPPopup(response["result"]);
                });
        }

        private void ResultStage(ResultData resultData, List<string> deadList, bool isStory)
        {
            var team = resultData.Team;
            var teamUsetType = resultData.TeamUseType;
            var levels = OrganizationInfo.GetTeam().GetLevels();
            var stars = resultData.GetAchieveStarCount(deadList.Count);
            var teams = OrganizationInfo.GetTeam();
            // 기기에 마지막 클리어한 스테이지 정보 저장
            var stagedata = TableManager.Instance.Stage_Table[resultData.StageTid];
            SaveLastEntered(stagedata);
            RestApiManager.Instance.RequestStageClear(resultData.ResultType, stars[0], stars[1], stars[2], 1, resultData.StageTid, isStory,
               teams.GetHelperKey(), teams.GetHelperSlot(), (response) =>
            {

                var list = Utils.ToRewardCellDatas(response["result"]["reward"]);
                StageResutlData stageResultData = new StageResutlData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid
                    , resultData.RemainTime, resultData.ResultType, 1, resultData.CombatPower, stars);

                UpdateTeamPreset(resultData.StageTid, teams);

                UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                   UIOption.Data, stageResultData,
                   UIOption.List, list));

                _stageInfo.OnClearStage?.Invoke(resultData.StageTid);
                RestApiManager.Instance.UpdateLevelUPPopup(response["result"]);
                CheckOpenUI(resultData.StageTid);
            });
        }

        private void ResultCharacterGuide(ResultData resultData)
        {
            var team = resultData.Team;
            var teamUsetType = resultData.TeamUseType;
            var levels = OrganizationInfo.GetTeam().GetLevels();
            var teams = OrganizationInfo.GetTeam();

            if (!this.GuideInfo.IsClear(resultData.StageTid))
            {
                RestApiManager.Instance.RequestGuideClear(resultData.StageTid, (response) =>
                {
                    var result = response["result"];

                    if (result != null)
                    {
                        var reward = result["reward"];

                        if (reward != null)
                        {
                            var rewardList = Utils.ToRewardCellDatas(reward);

                            var stageResultData = new StageResutlData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid
                                , resultData.RemainTime, resultData.ResultType, 1, resultData.CombatPower, null);
                            UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                                UIOption.Data, stageResultData,
                                UIOption.List, rewardList
                                ));

                            return;
                        }
                    }

                    this.GuideInfo.UpdateReddotDatas();
                });

                return;
            }

            var stageResultData = new StageResutlData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid
                , resultData.RemainTime, resultData.ResultType, 1, resultData.CombatPower, null);
            UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                UIOption.Data, stageResultData
                ));
        }

        // 저장된 프리셋에서 조력자 타입은 지우고 업데이트 해 줄 필요가 있음. > 작업 해야함.
        public void UpdateTeamPreset(string stageId, DosaTeamData dosaTeamData)
        {
            var table = TableManager.Instance.Stage_Table[stageId];
            CONTENTS_TYPE_ID typeId = table.CONTENTS_TYPE_ID;
            var presetIndex = OrganizationInfo.GetPresetIndex(typeId);
            DosaTeamData holdDosas = new DosaTeamData();
            for (int n = 0; n< dosaTeamData.DosaInfos.Length; n++)
            {
                var dosa = dosaTeamData.DosaInfos[n];
                if (dosa != null && dosa.CharacterUseType != CHARACTER_USE_TYPE.Helper)
                    holdDosas.DosaInfos[n] = dosa;
            }
            OrganizationInfo.SetTeam(typeId, presetIndex, holdDosas);
            OrganizationInfo.SavePreset(typeId, (obj) =>
            {
                OrganizationInfo.GetContentsLastTeam(typeId, out holdDosas);
            });
            OrganizationInfo.SetLastPreset(typeId, presetIndex);

            Debug.Log("<color=#0099ff>presetIndex("+presetIndex+")</color>");
        }

        private void ResultEventStory(ResultData resultData, List<string> deadList)
        {
            Debug.Log("<color=#ffe734>ResultEventStory</color>");
            var team = resultData.Team;
            var teamUsetType = resultData.TeamUseType;
            var levels = OrganizationInfo.GetTeam().GetLevels();
            var stars = resultData.GetAchieveStarCount(deadList.Count);

            for (int n = 0; n< stars.Length; n++)
            {
                Debug.Log("<color=#ffe734>ResultEventStory<b> STARS["+n+"] </b></color>");
            }

            var stageid = TableManager.Instance.Stage_Table[resultData.StageTid].Theme_Id;
            var eventname = TableManager.Instance.Event_Story_Table.GetEventData(stageid).Tid;
            Debug.Log("<color=#ffe734>ResultEventStory<b> eventname["+eventname+"] </b></color>");
            RestApiManager.Instance.RequestEventStoryClear(resultData.ResultType, stars[0], stars[1], stars[2], 1, resultData.StageTid
                , eventname, team, false,(response) =>
            {
                var list = Utils.ToRewardCellDatas(response["result"]["reward"]);
                //StageResutlData stageResultData = new StageResutlData(team, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid
                //    , resultData.RemainTime, resultData.ResultType, 1, stars);

                EventStageResultData eventstageResultData = new EventStageResultData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid
                    , resultData.RemainTime, resultData.ResultType, 1, stars, eventname, resultData.CombatPower);

                UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                   UIOption.Data, eventstageResultData,
                   UIOption.List, list));

                _stageInfo.OnClearStage?.Invoke(resultData.StageTid);
                //RestApiManager.Instance.UpdateLevelUPPopup(response["result"]);
            });
        }

        private void ResultChallenge(ResultData resultData, Stage_TableData stageData)
        {
            LEADERBOARDID Id = stageData.CONTENTS_TYPE_ID.ToLeaderboardId();
            string totalTid = TableManager.Instance.Challenge_Table.GetTid(stageData.Tid);

            switch (Id)
            {
                case LEADERBOARDID.Challenge:
                    totalTid = TableManager.Instance.Challenge_Table.GetTid(stageData.Tid);
                    break;
                default:
                    totalTid = TableManager.Instance.Trial_Table.GetTid(stageData.Tid);
                    break;
            }

            var monsterData = TableManager.Instance.Character_Monster_Table[stageData.Boss_Info];
            var monsters = GameManager.Instance.CombatController.Monsters;
            int damage = 0;
            var team = resultData.Team;
            var teamUsetType = resultData.TeamUseType;
            var levels = OrganizationInfo.GetTeam().GetLevels();
            float maxHp = monsterData.Hp;
            int dp = monsterData.Dp_Max;

            foreach (var monster in monsters)
            {
                if (monster.Key.TableKey.Equals(monsterData.Tid))
                {
                    dp = (int)(monster.Key.RuntimeStat.CurrentDp);
                    break;
                }
            }

            if (resultData.ResultType == ResultType.Victory)
            {
                if (stageData.Apply_Level > 0)
                {
                    maxHp += (monsterData.Hp_Growth * stageData.Apply_Level);

                    if (stageData.S_Hp > 0)
                        maxHp *= stageData.S_Hp;
                }

                foreach (var monster in monsters)
                {
                    if (monster.Key.TableKey.Equals(monsterData.Tid))
                    {
                        damage = (int)(monster.Key.RuntimeStat.Max_Hp - monster.Key.RuntimeStat.CurrentHp);
                        break;
                    }
                }

                //0.1초 단위까지만 점수에 반영
                int clearTime = (int)(stageData.Limit_Time * 0.01f) - (int)(resultData.RemainTime * 10);
                var isClear = DungeonInfo.IsClear(stageData.CONTENTS_TYPE_ID, stageData.Tid);
                var questData = RestApiManager.Instance.GetStageQuestDatas(resultData.StageTid, false);

                RestApiManager.Instance.RequestChallengeResult(
                    Id,
                    isClear,
                    totalTid,
                    AccountInfo.IconTid,
                    AccountInfo.LineTid,
                    (int)maxHp, clearTime,
                    dp,
                    resultData.ResultType,
                    1,
                    damage,
                    team,
                    resultData.TopDamage,
                    levels,
                    AccountInfo.Name,
                    GetMainCharacterInfo().Tid,
                    resultData.CombatPower,
                    questData,
                    (response) =>
                    {
                        SetActiveContentReddot();
                        if (resultData.ResultType == ResultType.Victory)
                        {
                            var list = Utils.ToRewardCellDatas(response["result"]["reward"]);
                            TotalResultData totalResultData = new TotalResultData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid, resultData.RemainTime, resultData.ResultType, 1, resultData.CombatPower, totalTid, GetChallengeInfo(Id).Score);
                            UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                                UIOption.Data, totalResultData,
                                UIOption.List, list));

                            //RestApiManager.Instance.UpdateLevelUPPopup(response["result"]);
                        }
                        else
                        {
                            var rTime = (int)(stageData.Limit_Time * 0.001f) -  resultData.RemainTime;
                            UIManager.Instance.Show<UIResultDefeat>(Utils.GetUIOption(
                                UIOption.Time, rTime,
                                UIOption.EnumType, stageData.CONTENTS_TYPE_ID.ToLeaderboardId(),
                                UIOption.Tid, resultData.StageTid));

                        }
                    });
            }
        }

        //섬멸전 결과 처리
        private void ResultTotal(ResultData resultData, Stage_TableData stageData)
        {
            var totalTid = TableManager.Instance.Total_Table.GetTid(stageData.Tid);
            var monsterData = TableManager.Instance.Character_Monster_Table[stageData.Boss_Info];
            var monsters = GameManager.Instance.CombatController.Monsters;
            int damage = 0;
            int dp = monsterData.Dp_Max;
            var team = resultData.Team;
            var teamUsetType = resultData.TeamUseType;
            var levels = OrganizationInfo.GetTeam().GetLevels();
            float maxHp = stageData.GetBossMaxHp();

            foreach (var monster in monsters)
            {
                if (monster.Key.TableKey.Equals(monsterData.Tid))
                {
                    damage = (int)(maxHp - monster.Key.RuntimeStat.CurrentHp - TotalInfo.Damage);
                    dp = (int)(monster.Key.RuntimeStat.CurrentDp);
                    break;
                }
            }

            LEADERBOARDID id;

            if (stageData.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.total)
                id = LEADERBOARDID.Total;
            else
                id = LEADERBOARDID.Challenge;

            //0.1초 단위까지만 점수에 반영
            int playTime = (int)(stageData.Limit_Time * 0.01f) - (int)(resultData.RemainTime * 10);

            RestApiManager.Instance.RequestTotalResult(id, 
                totalTid,
                AccountInfo.IconTid,
                AccountInfo.LineTid,
                (int)maxHp, playTime,
                resultData.ResultType,
                1,
                damage,
                team,
                levels,
                AccountInfo.Name,
                GetMainCharacterInfo().Tid,
                dp,
                (response) =>
                {
                    SetActiveContentReddot();
                    if (resultData.ResultType == ResultType.Victory)
                    {
                        var list = Utils.ToRewardCellDatas(response["result"]["reward"]);
                        var bossInfo = GetBossInfo(stageData.CONTENTS_TYPE_ID);

                        TotalResultData totalResultData = new TotalResultData(team, teamUsetType, resultData.TopDamage, resultData.SelectedDosa, resultData.StageTid, resultData.RemainTime, resultData.ResultType, 1, resultData.CombatPower, totalTid, TotalInfo.Score);
                        UIManager.Instance.Show<UIResult>(Utils.GetUIOption(
                            UIOption.Data, totalResultData,
                            UIOption.List, list));

                        //RestApiManager.Instance.UpdateLevelUPPopup(response["result"]);
                    }
                    else
                    {
                        var rTime = (int)(stageData.Limit_Time * 0.001f) -  resultData.RemainTime;
                        UIManager.Instance.Show<UIResultDefeat>(Utils.GetUIOption(
                            UIOption.Time, rTime,
                            UIOption.EnumType, CONTENTS_TYPE_ID.total,
                            UIOption.Tid, resultData.StageTid));
                    }
                });
        }

        //캐릭터 체험 결과
        private void ResultGuide(ResultData resultData, Stage_TableData stageData)
        {

        }

        //보너스 효과에 특정 조건이 있는 경우 별도로 세팅을 하고 isApplyBuff false로
        public void ApplyDungeonBonus(string bonusGroupId, bool isApplyBuff = true)
        {
            if (isApplyBuff)
                UpdateStageBonus(bonusGroupId);
        }

        //스테이지 종료 후 일시적으로 증가된 스탯 효과 초기화

        public void ClearStageBonus()
        {
            _genAddBuffs.Clear();
            _playerAddBuffs.Clear();
            _playerAddStats.Clear();
        }

        private void UpdateStageBonus(string bonusGroupId)
        {
            var tids = OrganizationInfo.GetTeam().GetTids();
            var buffData = GetStageBuff(bonusGroupId, tids);

            foreach (var buff in buffData)
            {
                for (int i = 0; i< buff.Value.Count; i++)
                {
                    AddDungeonPlayerBuff(buff.Key, buff.Value[i]);
                }
            }
        }

        public Dictionary<string, List<string>> GetStageBuff(string bonusGroupId, List<string> dosaTids)
        {
            var buffTable = TableManager.Instance.Stage_Buff_Table;
            var dataList = buffTable.GetGroupData(bonusGroupId);

            var buffDic = new Dictionary<string, List<string>>();

            for (int i = 0; i<dataList.Count; i++)
            {
                var data = dataList[i];
                var attributeAnd = data.Condition_Attribute_Apply<1;
                var gradeAnd = data.Condition_Grade_Apply<1;
                var charAnd = data.Condition_Character_Apply<1;

                for (int j = 0; j<dosaTids.Count; j++)
                {
                    var attSussecc = false;
                    var gradeSussecc = false;
                    var charSussecc = false;

                    if (string.IsNullOrEmpty(dosaTids[j]))
                    {
                        continue;
                    }

                    var pcData = TableManager.Instance.Character_PC_Table[dosaTids[j]];
                    switch (data.Select_Type)
                    {
                        case 0:
                            attSussecc = true;
                            break;
                        case 1:
                            attSussecc = data.ATTRIBUTE.Equals(ATTRIBUTE.none) || data.ATTRIBUTE.Equals(pcData.ATTRIBUTE);
                            break;
                        case 2:
                            
                            attSussecc = data.CHAR_SUBTYPE.Equals(CHAR_SUBTYPE.none) || pcData.CHAR_SUBTYPE.Contains(data.CHAR_SUBTYPE);
                            
                            /*if (data.CHAR_SUBTYPE == null||data.CHAR_SUBTYPE.Length==0)
                            {
                                attSussecc = true;
                                break;
                            }

                            for (int a = 0; a < data.CHAR_SUBTYPE.Length; a++)
                            {
                                var subType = data.CHAR_SUBTYPE[a];
                                for(int b = 0;b< pcData.CHAR_SUBTYPE.Length; b++)
                                {
                                    if (subType.Equals(pcData.CHAR_SUBTYPE[b]))
                                    {
                                        attSussecc = true;
                                        break;
                                    }
                                }
                             
                            }*/
                            break;
                        case 3:
                            attSussecc = data.CLASS_TYPE.Equals(CLASS_TYPE.none) || data.CLASS_TYPE.Equals(pcData.CLASS_TYPE);
                            break;
                    }
                    
                    gradeSussecc = data.Condition_Grade.Equals(0) || data.Condition_Grade.Equals(pcData.Tier_Grade);
                    charSussecc = data.Condition_Character.IsNullOrEmpty() || data.Condition_Character.Equals(pcData.Tid);

                    if (!attSussecc && attributeAnd)
                    {
                        continue;
                    }

                    if (!gradeSussecc && gradeAnd)
                    {
                        continue;
                    }

                    if (!charSussecc && charAnd)
                    {
                        continue;
                    }

                    if (attSussecc || gradeSussecc || charSussecc)
                    {
                        var buffList = buffTable.GetBuffList(data.Tid);
                        for (int k = 0; k <buffList.Count; k++)
                        {
                            if (buffDic.ContainsKey(pcData.Tid))
                            {

                                buffDic[pcData.Tid].Add(buffList[k]);
                            }
                            else
                            {
                                var list = new List<string> { buffList[k] };
                                buffDic.Add(pcData.Tid, list);
                            }
                        }

                        AddDungeonPlayerBuff(dosaTids[j], buffTable.GetBuffList(dataList[i].Tid));
                    }
                }
            }

            return buffDic;
        }

        public Stage_TableData GetCurrentStageData()
        {
            return TableManager.Instance.Stage_Table[EnterStageTid];
        }

        public void OnEnterDungeon(CONTENTS_TYPE_ID type, string stageTid, Action enterCallback = null, bool isApplyBuff = true)
        {
            var stageData = TableManager.Instance.Stage_Table[stageTid];
            EnterStatus = EnterStatus.Wait;

            //컨디션 체크
            if (!IsVerifyEnterStage(stageData))
                return;

            enterCallback?.Invoke();

            switch (stageData.CONTENTS_TYPE_ID)
            {
                //StageEnter나 TotalEnter처럼 씬 이동 전 서버 요청을 대기해야하는 경우 코루틴으로 대기 후 전투 씬 입장
                //서버 요청의 경우 enterCallback에 넣어준 후 응답 성공 여부에 따라 EnterStatus를 지정 해주어야함
                case CONTENTS_TYPE_ID.stage_main:
                case CONTENTS_TYPE_ID.stage_sub:
                case CONTENTS_TYPE_ID.stage_character:
                case CONTENTS_TYPE_ID.total:
                case CONTENTS_TYPE_ID.challenge:
                case CONTENTS_TYPE_ID.trial:
                    StartCoroutine(WaitEnter(stageData));
                    break;
                default:
                    LoadStageScene(stageData);
                    break;
            }
        }

        private IEnumerator WaitEnter(Stage_TableData stageData)
        {
            yield return new WaitUntil(() => EnterStatus != EnterStatus.Wait);

            if (EnterStatus == EnterStatus.Success)
                LoadStageScene(stageData);
        }

        private void LoadStageScene(Stage_TableData stageData)
        {
            if (!string.IsNullOrEmpty(stageData.Stage_Scene_Opt))
            {
                var stageOptionData = TableManager.Instance.Stage_Option_Table[stageData.Stage_Scene_Opt];

                if (!string.IsNullOrEmpty(stageOptionData.Bonus_Group_Id))
                    ApplyDungeonBonus(stageOptionData.Bonus_Group_Id);
            }

            if (!stageData.Stage_Buff.IsNullOrEmpty())
            {
                ApplyDungeonBonus(stageData.Stage_Buff);
            }

            var team = OrganizationInfo.GetTeam();

            UIManager.Instance.CloseAllUI();

            switch (stageData.CONTENTS_TYPE_ID)
            {
                case CONTENTS_TYPE_ID.pvp:
                    SceneManager.Instance.ChangeSceneState(SceneState.PvPScene);
                    break;
                //case CONTENTS_TYPE_ID.pvpline:
                //    SceneManager.Instance.ChangeSceneState(SceneState.PvPLineScene);
                //    break;
                default:
                    {
                        EnterStageTid = stageData.Tid;
                        SceneManager.Instance.LoadCombatScene(stageData.Stage_Scene);
                    }
                    break;
            }
        }

        //스테이지 입장 조건 체크
        public bool IsVerifyEnterStage(Stage_TableData stageData)
        {
            if (stageData.CONTENTS_TYPE_ID.Equals(CONTENTS_TYPE_ID.character_guide))
                return true;

            DosaTeamData team = OrganizationInfo.GetTeam();
            if (stageData.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.story_main ||stageData.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.story_event)
            {
                DosaTeamData storyTeam = new DosaTeamData();
                storyTeam.SetStoryTeam(stageData.Stage_Team_Apply);
                OrganizationInfo.SelectedTeam = storyTeam;

                team = storyTeam;
            }
          

            string optionTid = stageData.ToOptionTid(_stageDifficulty);

            if (!string.IsNullOrEmpty(optionTid))
            {
                var stageOptionInfo = TableManager.Instance.Stage_Option_Table.GetStageOptionInfo(optionTid);

                if (stageOptionInfo != null)
                {
                    for (int i = 0; i < stageOptionInfo.NeedList.Count; i++)
                    {
                        var needTid = stageOptionInfo.NeedList[i];

                        if (!team.HasDosa(needTid))
                        {
                            ShowEnterStageAlert(StageCondition.NeedOption);
                            return false;
                        }
                    }
                }
            }

            bool isAllEmptySlot = true;

            if (team != null)
            {
                foreach (var data in team.DosaInfos)
                {
                    if (data!=null && !string.IsNullOrEmpty(data.Tid))
                    {
                        isAllEmptySlot = false;
                        break;
                    }
                }
            }

            if (isAllEmptySlot && stageData.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.pvp)
            {
                UIManager.Instance.ShowToastMessage("str_ui_part_error_001");
                ShowEnterStageAlert(StageCondition.AllEmpty);
                return false;
            }

            //Todo 신준호 => 음식 관련 스텟 추가하는 부분이 없어 해당 부분 추가

            var infoManager = GameInfoManager.Instance;
            infoManager.FoodInfo.FoodDict.TryGetValue(stageData.CONTENTS_TYPE_ID.ToContentString(), out var foodTid);
            var selectFood = "";
            if (!foodTid.IsNullOrEmpty())
            {
                var item = infoManager.GetItemInfo(foodTid);
                if (item !=null && item.Amount>0)
                {
                    selectFood = foodTid;
                }
            }

            infoManager.AccountInfo.SelectedCookItemTid =selectFood;// foodTid.IsNullOrEmpty() ? "" : foodTid;        else
            if (infoManager.AccountInfo.SelectedCookItemTid.IsNullOrEmpty() || GameInfoManager.Instance.GetAmount(infoManager.AccountInfo.SelectedCookItemTid) == 0)
            {
                RestApiManager.Instance.SetFood(stageData.CONTENTS_TYPE_ID.ToString(), "");
            }

            //여기까지 요리

            //입장 재화 수량 체크
            var curCostAmount = GetAmount(stageData.Need_Cost_Type);
            bool needCostCheck = true;
            LEADERBOARDID id = stageData.CONTENTS_TYPE_ID.ToLeaderboardId();

            switch (id)
            {
                case LEADERBOARDID.Total:
                case LEADERBOARDID.Challenge:
                case LEADERBOARDID.Trial:
                    needCostCheck = false;
                    break;
                default:
                    break;
            }

            if (needCostCheck && stageData.Need_Cost_Value > curCostAmount)
            {
                var result = Enum.Parse(typeof(TICKET_TYPE), stageData.Need_Cost_Type);
                switch (result)
                {
                    case TICKET_TYPE.i_action_point:
                    //case TICKET_TYPE.i_elemental_key:
                    //case TICKET_TYPE.i_exp_key:
                    //case TICKET_TYPE.i_gold_key:
                    //case TICKET_TYPE.i_pvp_key:
                    case TICKET_TYPE.i_pvp_ticket:
                    case TICKET_TYPE.i_resource_ticket:
                    case TICKET_TYPE.i_boss_ticket:
                        UIManager.Instance.Show<UIPopupPurchaseGoods>(Utils.GetUIOption(UIOption.Tid, result));
                        return false;
                }

                //if (Enum.TryParse(typeof(TICKET_TYPE), stageData.Need_Cost_Type, out var result))
                //{

                //}
                string message;
                var itemData = TableManager.Instance.Item_Table[stageData.Need_Cost_Type];
                message = "str_ui_ticketplz_msg".ToTableArgs(itemData.Item_Name_Text_Id.ToTableText());
                ShowEnterStageAlert(StageCondition.NotEnoughTicket, message);
                return false;
            }

            var stageCondition = StageCondition.Pass;

            if (IsFullSlot(ITEM_TYPE.equipment))
                stageCondition = StageCondition.FullEquip;

            var rewardGroupID = TableManager.Instance.Stage_Table[stageData.Tid].Reward_01_Info;

            if (CheckIsInventoryFull(rewardGroupID)) 
            {
                string str = LocalizeManager.Instance.GetString("str_stage_play_deny_01"); // 보유 호패가 최대여서 스테이지 진행이 불가합니다.
                UIManager.Instance.ShowToastMessage($"{str}");
                return false;
            }
            //if (IsFullSlot(ITEM_TYPE.consumables))
            //    stageCondition = (StageCondition)((int)stageCondition + (int)StageCondition.FullConsum);
            //if (IsFullSlot(ITEM_TYPE.stuff))
            //    stageCondition = (StageCondition)((int)stageCondition + (int)StageCondition.FullStuff);

            if (stageCondition != StageCondition.Pass)
            {
                ShowEnterStageAlert(stageCondition);
                return false;
            }

            return true;
        }

        private void ShowEnterStageAlert(StageCondition stageCondition, string message = null)
        {
            switch (stageCondition)
            {
                case StageCondition.AllEmpty:
                    UIManager.Instance.ShowToastMessage("str_ui_part_error_001"); //1명 이상의 도사가 편성되어야 플레이가 가능합니다.
                    break;
                case StageCondition.NeedOption:
                    UIManager.Instance.ShowToastMessage("str_stage_play_deny_02");  // 필수 도사를 편성 하세요.
                    break;
                case StageCondition.NotEnoughTicket:
                    UIManager.Instance.ShowToastMessage(message);
                    break;
                case StageCondition.FullEquip:
                    UIManager.Instance.ShowToastMessage("str_ui_hopae_full_error_msg_01");
                    break;
                case StageCondition.FullConsum:
                    UIManager.Instance.ShowToastMessage("소비창 FULL!!!");
                    break;
                case StageCondition.FullStuff:
                    UIManager.Instance.ShowToastMessage("재료창 FULL!!!");
                    break;
                case StageCondition.FullEquip_Consum:
                    UIManager.Instance.ShowToastMessage("장비 + 소비창 FULL!!!");
                    break;
                case StageCondition.FullEquip_Stuff:
                    UIManager.Instance.ShowToastMessage("장비 + 재화창 FULL!!!");
                    break;
                case StageCondition.FullConsum_Stuff:
                    UIManager.Instance.ShowToastMessage("소비 + 재화창 FULL!!!");
                    break;
                case StageCondition.FullInventory:
                    UIManager.Instance.ShowToastMessage("인벤토리 부족!!!");
                    break;
                case StageCondition.Expired:
                    UIManager.Instance.ShowToastMessage("str_ui_toast_message_enter_deny_01");
                    break;
            }
        }

        //이벤트 컨트롤러에서 ec_value_01에 해당하는 module에 버프 추가 (깨비공 수련장 버프 추가 등...)
        private Dictionary<string, List<string>> _genAddBuffs = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _playerAddBuffs = new Dictionary<string, List<string>>();
        private Dictionary<string, List<RuntimeStatInfo>> _playerAddStats = new Dictionary<string, List<RuntimeStatInfo>>(); 

        public void OnAddBuffGenObject(AIModule module)
        {
            if (_genAddBuffs == null)
                return;

            if (_genAddBuffs.TryGetValue(module.TableKey, out var buffs))
            {
                for (int i = 0; i < buffs.Count; i++)
                {
                    Debug.Log(module.TableKey + " Add Buff "+ buffs[i]);
                    var buffTableData = TableManager.Instance.Buff_Table[buffs[i]];
                    //if (string.IsNullOrEmpty(buffTableData.Buff_group))
                    {
                        module.AddBuff(buffTableData, null, module, APPLY_TARGET.tg_me);   
                    }
                    /*
                    else
                    {
                        Buff_Group groupData;
                        if (TableManager.Instance.Buff_Table.Buff_GroupDic.TryGetValue(buffTableData.Buff_group, out groupData))
                        {
                            for (int a = 0; a < groupData.BuffTableDatas.Count; a++)
                            {
                                module.AddBuff(groupData.BuffTableDatas[a], null, module, APPLY_TARGET.tg_me);   
                            }
                        }
                    }
                    */
                }
            }
        }

        public void AddDungeonGenBuff(string pcTid, List<string> buffTids)
        {
            if (string.IsNullOrEmpty(pcTid) || buffTids == null || buffTids.Count == 0)
                return;

            if (!_genAddBuffs.ContainsKey(pcTid))
                _genAddBuffs.Add(pcTid, new List<string>());

            _genAddBuffs[pcTid].AddRange(buffTids);
        }


        //stage buff, stat 적용
        public void OnAddBuffPlayer(AIModule module)
        {
            if (_playerAddBuffs == null || _playerAddBuffs.Count == 0)
                return;

            if (_playerAddBuffs.TryGetValue(module.TableKey, out var buffs))
            {
                for (int i = 0; i < buffs.Count; i++)
                {
                    Debug.Log(module.TableKey + " Add Buff "+ buffs[i]);

                    var buffData = TableManager.Instance.Buff_Table[buffs[i]];
                    module.AddBuff(buffData, null, module, APPLY_TARGET.tg_me);
                }
            }

            if (_playerAddStats == null || _playerAddStats.Count == 0)
                return;

            if (_playerAddStats.TryGetValue(module.TableKey, out var stats))
            {
                for (int i = 0; i < stats.Count; i++)
                {
                    Debug.Log(module.TableKey + " Add stat "+ stats[i]);

                    var statType = stats[i].OptType.ToOptionType();
                    var statApply = stats[i].StatApply;
                    var key = stats[i].Key;
                    var value = stats[i].Value;

                    switch (statType)
                    {
                        case StatType.Mul:
                            module.RuntimeStat.GetAddtiveStat(statApply)?.AddMultiply(key, value * 0.01f);
                            break;
                        case StatType.Plus:
                            module.RuntimeStat.GetAddtiveStat(statApply)?.AddPlus(key, value);
                            break;
                        case StatType.SumPlus:
                            module.RuntimeStat.GetAddtiveStat(statApply)?.AddSumPlus(key, value);
                            break;
                    }
                }
            }
        }

        public void AddDungeonPlayerBuff(string pcTid, List<string> buffTids)
        {
            if (string.IsNullOrEmpty(pcTid) || buffTids == null || buffTids.Count == 0)
                return;

            if (!_playerAddBuffs.ContainsKey(pcTid))
                _playerAddBuffs.Add(pcTid, new List<string>());

            _playerAddBuffs[pcTid].AddRange(buffTids);
        }

        public void AddDungeonPlayerBuff(string pcTid, string buffTid)
        {
            if (string.IsNullOrEmpty(pcTid) || string.IsNullOrEmpty(buffTid))
                return;

            if (!_playerAddBuffs.ContainsKey(pcTid))
                _playerAddBuffs.Add(pcTid, new List<string>());

            _playerAddBuffs[pcTid].Add(buffTid);
        }

        public void AddDungeonPlayerStat(string pcTid, string key, float value, STAT_APPLY statApply, OPT_TYPE type)
        {
            if (string.IsNullOrEmpty(key) || value <= 0)
                return;

            if (!_playerAddStats.ContainsKey(pcTid))
                _playerAddStats.Add(pcTid, new List<RuntimeStatInfo>());

            _playerAddStats[pcTid].Add(new RuntimeStatInfo(key, value, statApply, type));
        }

        // CONTENTS_TYPE_ID 받아서 int index로 변환 -> contentcell의 index 번호를 알기 위함(부모 순서)
        public int SetContentReddotIndex(CONTENTS_TYPE_ID type)
        {
            switch (type)
            {
                case CONTENTS_TYPE_ID.challenge:
                    return 0;
                case CONTENTS_TYPE_ID.trial:
                    return 1;
                default:
                    return 2;
            }
        }
       
        // 경쟁 컨텐츠 보상 관련 레드닷 초기화
        public void SetBattlefieldReddot()
        {
            // CONTENTS_TAB_TYPE.rank 레드닷 업데이트
            var rankData = TableManager.Instance.Content_Table.GetDataByContentTab(CONTENTS_TAB_TYPE.rank);
            var rankparenttid = TableManager.Instance.RedDot_Table["reddot_content_rank_item"].Parent_Tids.FirstOrDefault();
            var rankparenindex = TableManager.Instance.RedDot_Table[rankparenttid].Rd_Index;
            for (int r=0; r< rankData.Count; r++)
            {
                var myIndex = r;
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_content_rank_item", myIndex, rankparenttid, rankparenindex);
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_content_battle_reward", myIndex, "reddot_content_rank_item", myIndex);
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_content_battle_reward_tab", myIndex, "reddot_content_battle_reward", myIndex);
                int rewardIndex = r+1;
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_content_rank_reward_receivable", myIndex, "reddot_content_rank_item", myIndex);
            }

            // CONTENTS_TAB_TYPE.pvp 레드닷 업데이트
            var pvpData = TableManager.Instance.Content_Table.GetDataByContentTab(CONTENTS_TAB_TYPE.pvp);
            var pvpparenttid = TableManager.Instance.RedDot_Table["reddot_content_pvp_item"].Parent_Tids.FirstOrDefault();
            var pvpparenindex = TableManager.Instance.RedDot_Table[pvpparenttid].Rd_Index;

            for (int p = 0; p< rankData.Count; p++)
            {
                var myIndex = p;
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_content_pvp_item", myIndex, pvpparenttid, pvpparenindex);
            }

            //SetEnableRewardContent();       // 컨텐츠 보상 수령 가능 레드닷
        }

        // 레드닷 활성화
        //public void SetActiveRedDot(string tid, int index, int parentIndex, bool isActive)
        public void SetActiveContentReddot()
        {
            // 경쟁 컨텐츠 데이터 
            var rankData = TableManager.Instance.Content_Table.GetDataByContentTab(CONTENTS_TAB_TYPE.rank);
            for (int n = 0; n< rankData.Count; n++)
            {
                var myIndex = n;
                var leaderboardid = rankData[n].CONTENTS_TYPE_ID.ToLeaderboardId();
                var leaderboardData = Leaderboards.GetSetting(leaderboardid);
                //var parentindex = SetContentReddotIndex(rankData[n].CONTENTS_TYPE_ID);
                var rankparenttid = TableManager.Instance.RedDot_Table["reddot_content_rank_item"].Parent_Tids.FirstOrDefault();
                var rankparenindex = TableManager.Instance.RedDot_Table[rankparenttid].Rd_Index;
                if (leaderboardData == null)
                    continue;
                if (IsContetnOpend(rankData[n].CONTENTS_TAB_TYPE))
                {
                    if (leaderboardData != null)
                    {
                        bool isEnable = EnableGetRankReward(leaderboardid);
                        int rewardIndex = n + 10;
                        //RedDotManager.Instance.SetActiveRedDot("reddot_content_rank_reward_receivable", myIndex, myIndex, isEnable);
                        var rewardType = GetRewardType(leaderboardid);

                        if (leaderboardData.OpenTime <= NowTime && leaderboardData.CloseTime >= NowTime)
                            RedDotManager.Instance.SetActiveRedDot("reddot_content_rank_reward_receivable", myIndex, myIndex, isEnable);

                        // 검증 기간 ~ 다음 시즌 오픈 전(VerifyTime ~ NextOpenTime)   : 랭킹 보상 
                        if (leaderboardData.VerifyTime <= NowTime && leaderboardData.NextOpenTime  > NowTime)
                        {
                            bool isReward = GetRankRewardKeys(rewardType, leaderboardid);
                        }
                        else
                            RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_tab", myIndex, myIndex, false);
                    }
                }
            }
            var pvpData = TableManager.Instance.Content_Table.GetDataByContentTab(CONTENTS_TAB_TYPE.pvp);
            //var contentdata = TableManager.Instance.Content_Table.GetDataByContentTab(CONTENTS_TAB_TYPE.pvp, CONTENTS_TAB_TYPE.rank);
            //for (int n = 0; n< contentdata.Count; n++)
            //{
            //    var leaderboardid = contentdata[n].CONTENTS_TYPE_ID.ToLeaderboardId();
            //    var leaderboardData = Leaderboards.GetSetting(leaderboardid);
            //    var parentindex = SetContentReddotIndex(contentdata[n].CONTENTS_TYPE_ID);
            //    if (leaderboardData == null)
            //    {
            //        //RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_accrue", 1, parentindex, false);
            //        //RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_rank", 0, parentindex, false);
            //        continue;
            //    }

            //    if (CheckIsLocked(contentdata[n].CONTENTS_TYPE_ID.ToString()) && contentdata[n].CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.pvp)
            //    {
            //        //if (leaderboardData != null)
            //        //{
            //        var rewardType = GetRewardType(leaderboardid);
            //        // 시즌 오픈 ~ 다음 시즌 오픈 전(OpenTime ~ NextOpenTime)   : 누적 보상 
            //        //if (leaderboardData.OpenTime <= NowTime && NowTime <= leaderboardData.NextOpenTime)
            //        //{
            //        //    bool isReward = GetTotalRewardKeys(rewardType, leaderboardid);
            //        //    RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_accrue", 1, parentindex, isReward);
            //        //}
            //        //else
            //        //    RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_accrue", 1, parentindex, false);

            //        // 검증 기간 ~ 다음 시즌 오픈 전(VerifyTime ~ NextOpenTime)   : 랭킹 보상 
            //        if (leaderboardData.VerifyTime <= NowTime && leaderboardData.NextOpenTime  > NowTime)
            //        {
            //            bool isReward = GetRankRewardKeys(rewardType, leaderboardid);
            //            RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_tab", 0, parentindex, isReward);
            //        }
            //        else
            //            RedDotManager.Instance.SetActiveRedDot("reddot_content_battle_reward_tab", 0, parentindex, false);
            //    }
            //}
        }

        public bool EnableGetRankReward(LEADERBOARDID leaderboardId)
        {
            int curCount = 0;
            int maxCount = 0;
            bool isEnable = false;
            switch (leaderboardId)
            {
                case LEADERBOARDID.Challenge:
                    {
                        maxCount = TableManager.Instance.Define_Table["ds_raid_reward_max_count"].Opt_01_Int;
                        curCount = maxCount - ChallengeInfo.RewardedCount;
                        //RedDotManager.Instance.SetActiveRedDot("reddot_content_challange_reward", curCount > 0);
                        isEnable = curCount > 0;
                    }
                    break;
                case LEADERBOARDID.Trial:
                    {
                        maxCount = TableManager.Instance.Define_Table["ds_raid_reward_max_count"].Opt_01_Int;
                        curCount = maxCount - TrialInfo.RewardedCount;
                        //RedDotManager.Instance.SetActiveRedDot("reddot_content_trial_reward", curCount > 0);
                        isEnable = curCount > 0;
                    }
                    break;
                default:
                    break;
            }
            return isEnable;
        }

        public RewardType GetRewardType(LEADERBOARDID leaderboardId)
        {
            switch (leaderboardId)
            {
                case LEADERBOARDID.Total:
                    return RewardType.totalcontent;
                case LEADERBOARDID.Challenge:
                    return RewardType.challenge;
                default:
                    return RewardType.trial;
            }
        }

        // 누적 보상 
        public bool GetTotalRewardKeys(RewardType rewardType,LEADERBOARDID leaderboardId)
        {
            bool isReward = false;
            // RewardInfo.IsReceivable(rewardType, keys);
            List<string> keys = new List<string>();
            switch (leaderboardId)
            {
                case LEADERBOARDID.Total:
                    {
                        var datas = TableManager.Instance.Total_Reward_Table.DataArray;
                        var rankData = TotalInfo.RankData;
                        if (rankData == null || rankData.Rank == 0)
                            isReward = false;
                        else
                        {
                            for (int i = 0; i < datas.Length; i++)
                            {
                                if (datas[i].Point > TotalInfo.TotalScore)
                                    break;

                                else
                                    keys.Add(datas[i].Tid);
                            }

                            isReward = RewardInfo.IsReceivable(rewardType, keys);
                        }
                    }
                    break;
                case LEADERBOARDID.Challenge:
                    {
                        var datas = TableManager.Instance.Challenge_Reward_Table.DataArray;
                        var rankData = GetChallengeInfo(leaderboardId).RankData;
                        if (rankData == null || rankData.Rank == 0)
                            isReward = false;
                        else
                        {
                            for (int i = 0; i < datas.Length; i++)
                            {
                                if (datas[i].Point > ChallengeInfo.TotalScore)
                                    break;
                                else
                                    keys.Add(datas[i].Tid);
                            }

                            isReward = RewardInfo.IsReceivable(rewardType, keys);
                        }
                    }
                    break;
                case LEADERBOARDID.Trial:
                    {
                        var datas = TableManager.Instance.Trial_Reward_Table.DataArray;
                        var rankData = GetChallengeInfo(leaderboardId).RankData;
                        if (rankData == null || rankData.Rank == 0)
                            isReward = false;
                        else
                        {
                            for (int i = 0; i < datas.Length; i++)
                            {
                                if (datas[i].Point > TrialInfo.TotalScore)
                                    break;
                                else
                                    keys.Add(datas[i].Tid);
                            }

                            isReward = RewardInfo.IsReceivable(rewardType, keys);
                        }
                    }
                    break;
                default:
                    break;
            }
            // return keys;
            return isReward;
        }

        // 랭킹 보상
        public bool GetRankRewardKeys(RewardType rewardType ,LEADERBOARDID leaderboardId)
        {
            List<string> keys = new List<string>();
            bool isReward = false;
            switch (leaderboardId)
            {
                case LEADERBOARDID.Total:
                    {
                        var datas = TableManager.Instance.Total_Tier_Table.DataArray;
                        var rankData = TotalInfo.RankData;
                        if (rankData == null || rankData.Rank == 0)
                            isReward = false;
                        else
                        {
                            for (int i = datas.Length - 1; i > 0; i--)
                            {
                                if (datas[i].Ranking_Point == 0 ||  datas[i].Ranking_Point >= rankData.Rank)
                                {
                                    keys.Add(datas[i].Tid);
                                    break;
                                }
                            }

                            isReward = RewardInfo.IsReceivable(rewardType, keys);
                        }
                    }
                    break;
                case LEADERBOARDID.Challenge:
                    {
                        var datas = TableManager.Instance.Challenge_Tier_Table.DataArray;
                        var rankData = GetChallengeInfo(leaderboardId).RankData;
                        if (rankData == null || rankData.Rank == 0)
                            isReward = false;
                        else
                        {
                            for (int i = datas.Length - 1; i > -1; i--)
                            {
                                //rankData.Rank
                                if (datas[i].Ranking_Point == 0 ||  datas[i].Ranking_Point >= rankData.Rank)
                                {
                                    keys.Add(datas[i].Tid);
                                    break;
                                }
                            }

                            isReward = RewardInfo.IsReceivable(rewardType, keys);
                        }
                    }
                    break;
                case LEADERBOARDID.Trial:
                    {
                       var datas = TableManager.Instance.Trial_Tier_Table.DataArray;
                        var rankData = GetChallengeInfo(leaderboardId).RankData;
                        if (rankData == null || rankData.Rank == 0)
                            isReward = false;
                        else
                        {
                            for (int i = datas.Length - 1; i > -1; i--)
                            {
                                //rankData.Rank
                                if (datas[i].Ranking_Point == 0 ||  datas[i].Ranking_Point >=  rankData.Rank)
                                {
                                    keys.Add(datas[i].Tid);
                                    break;
                                }
                            }

                            isReward = RewardInfo.IsReceivable(rewardType, keys);
                        }
                    }
                    break;
                default:
                    break;
            }
            return isReward;
        }


        // 챌린지 로드 현재 진입 가능 스테이지 체크
        public bool GetEnableEnterdDungeon(CONTENTS_TYPE_ID typeid, string tid)
        {
            bool isCurrent = false;
            var list = TableManager.Instance.Stage_Table.GetDatas(typeid);
            var clearlist = DungeonInfo.GetClearList(typeid);
            if (clearlist.Count > 0)
            {
                // 클리어 리스트가 있는 경우. 
                var lastClearInfo = TableManager.Instance.Stage_Table[clearlist.LastOrDefault()];
                if (!lastClearInfo.Tid.Equals(list.LastOrDefault().Tid))
                {
                    // 클리어 스테이지 중 마지막 스테이지가, 해당 컨텐츠의 마지막 스테이지가 아닌 경우
                    var info = TableManager.Instance.Stage_Table.GetData(typeid, lastClearInfo.Stage_Id);
                    isCurrent = info.Tid.Equals(tid);
                }
            }
            else
            {
                isCurrent = tid.Equals(list.FirstOrDefault().Tid);
            }

            return isCurrent;
        }
        public bool GetAllCelarDungeon(CONTENTS_TYPE_ID typeid)
        {
            bool isAllClear = false;
            var list = TableManager.Instance.Stage_Table.GetDatas(typeid);
            var clearlist = DungeonInfo.GetClearList(typeid);
            if (clearlist.Count > 0)
            {
                // 클리어 리스트가 있는 경우. 
                var lastClearInfo = TableManager.Instance.Stage_Table[clearlist.LastOrDefault()];
                isAllClear = lastClearInfo.Tid.Equals(list.LastOrDefault().Tid);
            }
            else
            {
                isAllClear = false;
            }

            return isAllClear;
        }

        public string GetContentTabStr(CONTENTS_TAB_TYPE type)
        {
            string name = null;
            switch (type)
            {
                case CONTENTS_TAB_TYPE.boss:                // 보스전
                    name = "str_ui_content_tab_04";
                    break;
                case CONTENTS_TAB_TYPE.resource:            // 보급
                    name = "str_ui_content_tab_01";
                    break;
                case CONTENTS_TAB_TYPE.pvp:                   // 아레나
                    name = "str_stage_pvp_popup_name_01";
                    break;
                case CONTENTS_TAB_TYPE.tower:                   // 타워
                    name = "str_ui_content_tab_03";
                    break;
                case CONTENTS_TAB_TYPE.rank:                   // 시험대
                    name = "str_ui_content_tab_05";
                    break;
                default:
                    break;
            }
            return name;
        }

    }

    public enum StageCondition
    {
        Pass = 0, //조건 달성
        AllEmpty = 1, //도사 미배치
        NeedOption = 2, //필수 캐릭터 미배치
        NotEnoughTicket = 4, //입장 티켓 부족

        FullEquip = 8, //장비창 가득참
        FullConsum = 16, //소비창 가득참
        FullStuff = 32, //재화창 가득참

        FullEquip_Consum = 24, //장비 + 소비창 가득참
        FullEquip_Stuff = 40, //장비 + 재화창 가득참
        FullConsum_Stuff = 48, //소비 + 재화창 가득참

        FullInventory = 56, //전부 꽉창

        Expired = 64 // 시즌 기간 지남
    }
}
