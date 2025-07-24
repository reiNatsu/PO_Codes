using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//RestApiManagerUpdate
public partial class RestApiManager
{
    public void UpdateReward(JArray itemArray, JToken reward, bool isShowRewardPopup, Action rewardCallback = null)
    {
        if(itemArray != null)
        {
            for (int i = 0; i < itemArray.Count; i++)
            {
                var server = itemArray[i];
                var item = server["Item"];

                UpdateInfo(item);
            }
        }

        if (isShowRewardPopup && reward != null)
        {
            var list = Utils.ToRewardCellDatas(reward);
            UIManager.Instance.ShowRewardItem(list, rewardCallback);
        }
    }

    public void UpdateReward(JObject response, bool isShowRewardPopup, Action rewardCallback = null)
    {
        JArray serverArray = response["result"]["server"] as JArray;

        if (serverArray == null)
            return;

        for (int i = 0; i < serverArray.Count; i++)
        {
            var server = serverArray[i];
            var item = server["Item"];
            if(item ==null)
            {
                continue;
            }
            UpdateInfo(item);
        }

        var displayReward = response["result"]["reward"];
        
        if(isShowRewardPopup)
        {
            var list = Utils.ToRewardCellDatas(displayReward);

            if(list != null && list.Count > 0)
                UIManager.Instance.ShowRewardItem(list, rewardCallback);
        }

        var rewardInfo = response["result"]["rewardinfo"];

        if(rewardInfo != null)
        {
            GameInfoManager.Instance.RewardInfo.UpdateRewardData(rewardInfo);
        }

        // 조력자.
        var helperInfo = response["result"]["helperinfo"];
        if (helperInfo != null)
        {
            UpdateHelperInfo(helperInfo);
        }
    }

    public void UpdateInfo(JToken item,string argument = "")
    {
        var infos = item.Cast<JProperty>();

        foreach (var info in infos)
        {
            var infoType = info.Name;

            switch (infoType)
            {
                case "datainfo":
                    UpdateUserData(item["datainfo"],argument);
                    break;
                case "characterinfo":
                    UpdateUserCharacter(item["characterinfo"],argument);
                    break;
                case "monsterinfo":
                    UpdateUserMonster(item["monsterinfo"]);
                    break;
                case "inventoryinfo":
                    UpdateUserInventory(item["inventoryinfo"]);
                    break;
                case "equipmentinfo":
                    UpdateUserEquipment(item["equipmentinfo"]);
                    break;
                case "storeinfo":
                    UpdateUserStore(item["storeinfo"]);
                    break;
                case "eventinfo":
                    UpdateEventData(item["eventinfo"]);
                    break;
                case "storyinfo":
                    UpdateStoryInfo(item["storyinfo"]);
                    break;
                case "likeabilityinfo":
                    UpdateLikeAbilityInfo(item["likeabilityinfo"]);
                    break;
                case "claninfo":
                    UpdateUserClanInfo(item["claninfo"]);
                    break;
                case "helperinfo":
                    UpdateHelperInfo(item["helperinfo"]);
                    break;
                case "supplyinfo":
                    GameInfoManager.Instance.SupplyInfo.UpdateSupplyInfo(item["supplyinfo"]);
                    break;
                case "guideinfo":
                    GameInfoManager.Instance.GuideInfo.UpdateGuideInfo(item["guideinfo"]);
                    break;
                case "hotdealinfo":
                    GameInfoManager.Instance.HotdealInfo.UpdateHotdealInfo(item["hotdealinfo"]);
                    break;
                case "minigameinfo":
                    GameInfoManager.Instance.MiniGameInfo.UpdateInfo(item["minigameinfo"]);
                    break;
                case "postinfo":
                    GameInfoManager.Instance.PostInfo.UpdatePostData(item["postinfo"]["M"], false);
                    break;
            }
        }
    }

    public void UpdateLevelUPPopup(JToken item)
    {
        if (item == null)
            return;
        if (item["reward"] == null)
            return;
        if (item["reward"]["account"] == null)
            return;

        JArray rewards = item["reward"]["account"] as JArray;
        List<RewardCellData> rewardDatas = new List<RewardCellData>();
        Dictionary<string, RewardCellData> rewardInfos = new Dictionary<string, RewardCellData>();
        for (int n = 0; n < rewards.Count; n++)
        {
            var tid = rewards[n][0].ToString();
            var amount = (int)rewards[n][1];
            var itemtype = (ITEM_TYPE)Enum.Parse(typeof(ITEM_TYPE), rewards[n].Last().ToString());
            if (!itemtype.IsEquip())
            {
                if (!rewardInfos.ContainsKey(tid))
                {
                    RewardCellData cellData = new RewardCellData(tid, itemtype, amount);
                    rewardInfos.Add(tid, cellData);
                }
                else
                    rewardInfos[tid].AddAmount(amount);

                //RewardCellData cellData = new RewardCellData(tid, itemtype, amount);
                //rewardDatas.Add(cellData);
            }
            else
            {
                RewardCellData cellData = new RewardCellData(tid, itemtype, 1);
                rewardDatas.Add(cellData);
            }
        }

        // 데이터 저장 하고.
        UIManager.Instance.Show<UIPopupAccountLvUp>(
            Utils.GetUIOption(
                UIOption.Tid, rewardInfos.FirstOrDefault().Value.Tid,
                UIOption.Amount, rewardInfos.FirstOrDefault().Value.Amount,
                UIOption.EnumType, rewardInfos.FirstOrDefault().Value.ItemType
                ));
    }


    public void UpdateUserData(JToken userDataInfo,string argument = "")
    {
        var userData = userDataInfo.M_JToken();
        var account = userData["account"];
        if (account != null)
        {
            GameInfoManager.Instance.AccountInfo.UpdateAccount(account);
        }
        var currency = userData["currency"];
        if (currency != null)
        {
            GameInfoManager.Instance.CurrencyInfo.UpdateCurrency(currency);
            
        }
        var organization = userData["organization"];
        if (organization != null)
        {
            GameInfoManager.Instance.OrganizationInfo.UpdateOrganization(organization);
        }
        var stage = userData["stage"];
        if (stage != null)
        {
            GameInfoManager.Instance.StageInfo.UpdateStage(stage);
        }
        var food = userData["food"];
        if (food != null)
        {
            GameInfoManager.Instance.FoodInfo.UpdateFood(food);
        }

        var quest = userData["quest"];
        if (quest != null)
        {
            GameInfoManager.Instance.QuestBase.UpdateNewQuest(quest["M"]);
        }
        var ability = userData["ability"];
        if(ability !=null)
        {
            GameInfoManager.Instance.AbilityInfo.UpdateAbilityInfo(ability, argument);
        }

        var leaderboard = userData["leaderboard"];
        if (leaderboard !=null)
        {
            GameInfoManager.Instance.Leaderboards.SetPrevRankData(leaderboard);
        }

        var ticket = userData["ticket"];

        if(ticket != null)
            GameInfoManager.Instance.TicketInfo.SetTicketInfo(ticket["M"]);

        var liberation = userData["liberation"];
        if (liberation !=null)
            GameInfoManager.Instance.LiberationInfo.Setup(liberation["M"]);

        var ads = userData["ads"];
        if (ads != null)
            GameInfoManager.Instance.AccountInfo.UpdateAdsInfo(ads);

        var naviquest = userData["naviquest"];

        if (naviquest != null)
            GameInfoManager.Instance.NaviQuestInfo.UpdateNaviInfo(naviquest["M"]);

        GameInfoManager.Instance.SetContentsTIcketsRedDot();
    }

    public void UpdateUserInventory(JToken userInventoryInfo)
    {
        var inventoryInfo = userInventoryInfo["M"];
        var inventoryObject = new Dictionary<string, JObject>();
        var stack = inventoryInfo["stack"];
        if (stack != null)
        {
            inventoryObject = stack["M"].ToObject<Dictionary<string, JObject>>();
            foreach (var obj in inventoryObject)
            {
                GameInfoManager.Instance.AddItem(obj.Key, obj.Value["N"].Value<int>());
            }
        }
        var split = inventoryInfo["split"];
        if (split != null)
        {
            inventoryObject = stack["M"].ToObject<Dictionary<string, JObject>>();
            foreach (var obj in inventoryObject)
            {
                GameInfoManager.Instance.AddItem(obj.Key, obj.Value["N"].Value<int>());
            }
        }
        var onlyone = inventoryInfo["onlyone"];
        if (onlyone != null)
        {
            inventoryObject = onlyone["M"].ToObject<Dictionary<string, JObject>>();
            foreach (var obj in inventoryObject)
            {
                Debug.Log("onlyone != null"+obj.Key + " : " + obj.Value["N"].Value<int>());
                GameInfoManager.Instance.AddItem(obj.Key, obj.Value["N"].Value<int>());
            }
        }
        var coin = inventoryInfo["coin"];
        if (coin != null)
        {
            inventoryObject = coin["M"].ToObject<Dictionary<string, JObject>>();
            
            var itemSet = new HashSet<string>();

            foreach (var obj in inventoryObject)
            {
                var characterPiece = obj.Key;
                var itemData = TableManager.Instance.Item_Table[characterPiece];

                if(!string.IsNullOrEmpty(itemData.Item_Acquire_Effect_Value))
                    itemSet.Add(characterPiece);

                Debug.Log("coin != null"+obj.Key + " : " + obj.Value["N"].Value<int>());
                GameInfoManager.Instance.AddItem(characterPiece, obj.Value["N"].Value<int>() - GameInfoManager.Instance.GetAmount(characterPiece));
            }

            //캐릭터 코인 갯수 체크 => 체이서 레드닷
            GameInfoManager.Instance.CharacterInfo.UpdateCharacterRedDot(itemSet);
        }
    }

    public void UpdateUserCharacter(JToken userCharacterInfo,string argument = "")
    {
        var characterData = userCharacterInfo.M_JToken();
        GameInfoManager.Instance.CharacterInfo.UpdateCharacter(characterData,argument);
    }
    public void UpdateUserEquipment(JToken userEquipmentInfo)
    {
        var infos =  Utils.ToEquipInfos(userEquipmentInfo); 

        GameInfoManager.Instance.AddEquipments(infos);
    }

    public void UpdateUserMonster(JToken userMonsterInfo)
    {
        var monInfo = userMonsterInfo["M"]["S"]["M"] as JObject;
        foreach (JProperty prop in monInfo.Properties())
        {
            GameInfoManager.Instance.SetCollectionInfoDic(prop.Name, GameInfoManager.Instance.SetCollectionMonsterInfo(prop.Name, prop.Value.N_Int()));
        }
    }
    public void UpdateUserStore(JToken userStoreInfo)
    {
        if(userStoreInfo == null)
            return;
        var list = userStoreInfo["M"]["S"]["M"].Cast<JProperty>();
        GameInfoManager.Instance.UpdatePurchaseItemCount(userStoreInfo["M"]["S"]);
        //foreach (var item in list)
        //{
        //    GameInfoManager.Instance.UpdatePurchaseItemCount(item.Name, item.Value["M"]["C"].N_Int(), userStoreInfo["M"]["S"]["M"]);
        //}
        // 갱신 횟수 업데이트
        if (userStoreInfo["M"]["SO"] != null)
        {
            var info = userStoreInfo["M"]["SO"]["M"].Cast<JProperty>();
            foreach (var sinfo in info)
            {
                GameInfoManager.Instance.UpdateStoreInfo(sinfo.Name, sinfo.Value);
            }
        }
       GameInfoManager.Instance.UpdateStoreReddot();
    }
    public List<(string,int)> UpdateEventData(JToken eventDataInfo)
    {
        return GameInfoManager.Instance.EventInfo.UpdateEvent(eventDataInfo);
    }

    public void UpdateStoryInfo(JToken token)
    {
        //GameInfoManager.Instance.EventStoryInfo.SetBonusInfo(token);
        if (token["M"] == null)
            return;

        var eventstoryToken = token["M"].Cast<JProperty>();

        // 이벤트 별로 저장.
        foreach (var events in eventstoryToken)
        {
            var eventName = events.Name;
            // 보너스 데이터 
            if (events.Value["M"]["bonus"] != null)
            {
                GameInfoManager.Instance.EventStoryInfo.SetBonusInfo(events.Value["M"]["bonus"]);
            }
            // 클리어 스테이지 정보 
            if (events.Value["M"]["stage"] != null)
            {
                GameInfoManager.Instance.EventStoryInfo.UpdateCelarStatge(events.Value["M"]["stage"]);
            }

            GameInfoManager.Instance.EventStoryInfo.SetClearEventStages();
            // 여기서 이벤트 별로 reddot 업데이트
            GameInfoManager.Instance.EventStoryInfo.UpdateEventStoryReddot(eventName);
        }
    }
}
