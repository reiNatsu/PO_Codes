using Consts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace LIFULSE.Manager
{
    public class StoreInfo
    {
        private string _storeType;
        private int _refreshCount;
        private List<Store_TableData> _itemList = new List<Store_TableData>();

        public string NowStore { get => _storeType; set => _storeType = value; }
        public int RefreshCount { get => _refreshCount; set => _refreshCount = value; }
        public List<Store_TableData> StoreItemList { get => _itemList; set => _itemList = value; }
        public StoreInfo()
        { }
    
        public StoreInfo(string tid, List<Store_TableData> list, int refreshCount = 0)
        {
            _storeType = tid;
            _itemList = list;
             _refreshCount = refreshCount;
        }
        // _itemList를 오름차순으로 정렬하는 메소드
        public List<Store_TableData> AscendingItemList()
        {
            // store_table → oreder 오름차순으로 정렬
            List<Store_TableData> sortedList = new List<Store_TableData>(_itemList); // _itemList의 복사본 생성
            //sortedList.Sort((x, y) => TableManager.Instance.Store_Table[x.Tid].Order.CompareTo(TableManager.Instance.Store_Table[y.Tid].Order));
            sortedList.Sort((x, y) =>
            {
                var xData = TableManager.Instance.Store_Table[x.Tid];
                var yData = TableManager.Instance.Store_Table[y.Tid];

                // purchase_type이 "free"이면 우선 정렬
                int purchaseTypeComparison = string.Compare(xData.Purchase_Type, yData.Purchase_Type, StringComparison.Ordinal);
                if (xData.Purchase_Type == "free" && yData.Purchase_Type != "free")
                    return -1;
                if (xData.Purchase_Type != "free" && yData.Purchase_Type == "free")
                    return 1;

                // Order 기준 오름차순 정렬
                return xData.Order.CompareTo(yData.Order);
            });

            return sortedList;
        }
    }


    public partial class GameInfoManager
    {
        private StoreInfo _storeInfo = new();

        private Dictionary<string, StoreInfo> _storeInfoDic = new Dictionary<string, StoreInfo>();
        private Dictionary<string,int> _itemPurchaseInfo = new Dictionary<string, int>();
        //private Dictionary<string,Dictionary<string, int>> _itemPurchaseInfo = new Dictionary<string, Dictionary<string, int>>();
        private Coroutine _storeTimer;
        private List<string> _contentStoreList = new List<string>();


        // TODO 동연 : 다음 리셋 시간, 이전 리셋 시간 정리 필요
        public long _rsetTime;
        private DateTime _nowTime;

        public Dictionary<string, StoreInfo> StoreInfoDict { get => _storeInfoDic; set => _storeInfoDic = value; }
        public Dictionary<string,int> ItemPurchaseInfo { get => _itemPurchaseInfo; set => _itemPurchaseInfo= value; }
        //public Dictionary<string, Dictionary<string, int>> ItemPurchaseInfo { get => _itemPurchaseInfo; set => _itemPurchaseInfo= value; }
        public DateTime NowResetTime { get; set; }
        public DateTime DailyResetTime { get; set; }
        public DateTime NowTime
        {
            get
            {
                if (_nowTime.Equals(default))
                    return DateTime.UtcNow.ToAddHours();
                else
                   return  _nowTime;
            }

            set
            {
                _nowTime = value;
            }
        }
        public DateTime WeeklyResetTime { get; set; }
        public DateTime MonthlyResetTime { get; set; }
        public DateTime LastResetDayTime { get; set; }
        public DateTime LastResetWeekTime { get; set; }
        public DateTime LastResetMonthTime { get; set; }
       
        public List<string> ContentStoreList { get => _contentStoreList; set => _contentStoreList = value; }


        public bool IsAddEvent { get; set; } = false;
        public Action<bool> OnStoreRefreshEvent { get; set; }
             public Action<bool> OnStoreRefreshWeeklyEvent { get; set; }
        public Action<bool> OnStoreRefreshMonthlyEvent { get; set; }

        private Coroutine _currentTimeRoutine;

        // 상점 리셋 시간 받아옴. => base/gettime
        public void SetRefreshDailyTimer()
        {
           SetupResetTimeEvent();    // Daily, weekly, Monthly reset time 이벤트 등록. 
            TimerManager.Instance.StartStopwatchEvent(TimerKey.RefreshDayTimer, DailyResetTime, () =>
            {
                InvokeResetTimeEvent(true);
            },20);

           //TimerManager.Instance.StartStopwatchEvent(TimerKey.RefreshWeekTimer, _weekResetTime, () => {
           //    InvokeResetTimeEventWeek(true);
           //});
            var time = Time.realtimeSinceStartup;
            Debug.Log("SetRefreshDailyTimer" + WeeklyResetTime);
        }

        // store/getdata , store/allrefreshstore 했을때 초기 데이터 세팅 
        //로그인 해 있을때 상점 갱신.
        public void SettingStoreInfo(StoreInfo info)
        {
            if (!_storeInfoDic.ContainsKey(info.NowStore))
            {
                _storeInfoDic.Add(info.NowStore, info);
            }
            else
            {
                // 기존에 있는 storeinfo 겍체
                StoreInfo exitStoreInfo = _storeInfoDic[info.NowStore];

                foreach (var newItem in info.StoreItemList)
                {
                    bool itemExit = false;
                    foreach (var existingItem in exitStoreInfo.StoreItemList)
                    {
                        if (existingItem == newItem)
                        {
                            itemExit = true;
                            break;
                        }
                    }
                    if (!itemExit)
                    {
                        // 새로운 데이터에만 존재하는 아이템 추가
                        exitStoreInfo.StoreItemList.Add(newItem);
                    }
                }

                //기존 에티어 아이템 중, 새로 받은 데이터에 없는 아이템 삭제
                exitStoreInfo.StoreItemList.RemoveAll(item => !info.StoreItemList.Contains(item));

                // 기존 데이터 없데이트
                _storeInfoDic[info.NowStore] = exitStoreInfo;
                _storeInfoDic[info.NowStore].RefreshCount = info.RefreshCount;
            }
            //UpdateStoreReddot();
        }
        // Store 아이템 전부 저장. (초기화 가능, 불가능 구분해서 Dic에 저장)
        public void SetAllStoreTypeItemList()
        {
            var option = TableManager.Instance.Store_Option_Table.DataArray;
            for (int n = 0; n < option.Length; n++)
            {
                var list = TableManager.Instance.Store_Table.GetStoreItemList(option[n].Tid);

                if (list == null)
                    continue;

                StoreInfo info = new StoreInfo(option[n].Tid, list);
                if (!_storeInfoDic.ContainsKey(option[n].Tid))
                {
                    SettingStoreInfo(info);
                }
            }
            //UpdateStoreReddot();
        }
        // Refresh 횟수 체크 

        //
        public int CheckRemainRefreshCount(string tid)
        {
            var data = TableManager.Instance.Store_Option_Table.GetStoreOprionData(tid);
            return data.Refresh_Limit - _storeInfoDic[tid].RefreshCount;
        }
        // Refresh → 데이터 다시 Get
        public void UpdateStoreInfo(string tid, JToken token)
        {
            // StoreInfo 적용(리프레시 가능한 상점 정보만 저장)
            _storeInfoDic[tid].RefreshCount = token["M"]["R"].N_Int();

            var list = token["M"]["IS"]["SS"];
            _storeInfoDic[tid].StoreItemList = new List<Store_TableData>();
            foreach (var iteminfo in list)
            {
                var info = TableManager.Instance.Store_Table[iteminfo.ToString()];
                _storeInfoDic[tid].StoreItemList.Add(info);
            }
        }

        // 정렬된 리스트 → expired_time 존재 → 기간 확인 후 만료된 아이템은 리스트에서 삭제.  
        public List<Store_TableData> CheckExpiredTime(string optionTid)
        {
            var tabData = TableManager.Instance.Store_Option_Table[optionTid];
            var subTablist = GetSotreItemsList(optionTid);
            List<Store_TableData> list = new List<Store_TableData>();
            for (int n= 0; n < subTablist.Count; n++)
            {
                if (subTablist[n] == null)
                    continue;

                if (string.IsNullOrEmpty(subTablist[n].Expired_Time))
                {
                    list.Add(subTablist[n]);
                }
                else
                {
                    string timeformat = "MM/dd/yyyy HH:mm";
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime expireTime = DateTime.ParseExact(subTablist[n].Expired_Time, timeformat, provider, DateTimeStyles.AssumeLocal);
                    if (expireTime  >= DateTime.UtcNow.ToAddHours())
                    {
                        list.Add(subTablist[n]);
                    }
                }
            }
            return list;
        }

        // 상점 Tab별 아이템 리스트 정렬해서 받아옴. (bundle, package, pay ....)
        public List<Store_TableData> GetSotreItemsList(string tid)
        {
            List<Store_TableData> soldOutItems = new List<Store_TableData>();

            if (!_storeInfoDic.ContainsKey(tid))
            {
                return new List<Store_TableData>();
            }
            //List<Store_TableData> storeItemsList = _storeInfoDic[tid].AscendingItemList();
            List<Store_TableData> storeItemsList = new List<Store_TableData>();
            storeItemsList = _storeInfoDic[tid].AscendingItemList();
            for (int n = 0; n < storeItemsList.Count; n++)
            {
                if (storeItemsList[n] == null)
                    continue;

                // 명패일 경우 Soldout 값 다시 체크
                bool soldOut = GetSoldOutItem(tid, storeItemsList[n].Tid);
                //Debug.Log("<color=#9efc9e>Item Tid <b>"+storeItemsList[n].Tid+"</b>, Is SoldOut?"+soldOut+"</color>");
                if (soldOut)
                {
                    soldOutItems.Add(storeItemsList[n]);
                }
            }
            foreach (var soldOutItem in soldOutItems)
            {
                storeItemsList.Remove(soldOutItem);
            }
            storeItemsList.AddRange(soldOutItems);

            return storeItemsList;
        }
     
        // Refresh  비용 계산
        public int GetRefreshCost(Store_Option_TableData data)
        {
            int cost = 0;
            //if (_storeInfoDic[data.Tid].RefreshCount == 0)
            //{
            //    cost = data.Refresh_Item_Value;
            //}
            //else
            //{
            //    if (data.Refresh_Reuse_Multiple == 0)
            //    { //재사용 시 비용 증가 배수 0 →재사용 시 비용 증가 절대치 계산 사용
            //        cost = (_storeInfoDic[data.Tid].RefreshCount * data.Refresh_Reuse_Increase_Count) + data.Refresh_Item_Value;
            //    }
            //    if (data.Refresh_Reuse_Increase_Count == 0)
            //    { //재사용 시 비용 증가 절대치 0 →재사용 시 비용 증가 배수 계산 사용
            //        cost = (int)(_storeInfoDic[data.Tid].RefreshCount * data.Refresh_Reuse_Multiple * data.Refresh_Item_Value);
            //    }
            //}
            if (data.Refresh_Reuse_Multiple == 0)
            { //재사용 시 비용 증가 배수 0 →재사용 시 비용 증가 절대치 계산 사용
                cost = ((_storeInfoDic[data.Tid].RefreshCount+1) * data.Refresh_Reuse_Increase_Count) + data.Refresh_Item_Value;
            }
            if (data.Refresh_Reuse_Increase_Count == 0)
            { //재사용 시 비용 증가 절대치 0 →재사용 시 비용 증가 배수 계산 사용
                cost = (int)((_storeInfoDic[data.Tid].RefreshCount+1) * data.Refresh_Reuse_Multiple * data.Refresh_Item_Value);
            }
            return cost;
        }
        //Store_Option_Table product_show_count 0=>리스트 전체 상품 노출, 1=>지정 수만큼
        public int GetShowListCount(string tid)
        {
            var data = TableManager.Instance.Store_Option_Table.GetStoreOprionData(tid);
            int showCnt = 0;
            if (data.Product_Show_Count == 0)
            {   // 해당 상점 탭에 등록되어있는 모든 상품 노출
                if (_storeInfoDic.ContainsKey(tid))
                {
                    showCnt = _storeInfoDic[tid].StoreItemList.Count;
                }
            }
            else
            {   // 해당 상점 탭에 등록되어있는 상품 중에서 해당 수치만큼만 상품 노출
                showCnt = data.Product_Show_Count;
            }
            return showCnt;
        }
        // SoldOut 체크
        public bool GetSoldOutItem(string storetype,string tid)
        {
            bool isShow = false;
            var storeData = TableManager.Instance.Store_Table.GetItemDataByTid(tid);
            var rewardData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(storeData.Product);

            if (rewardData == null)
                return false;

            if (rewardData.ITEM_TYPE == ITEM_TYPE.character_coin)
            {
                string characterid = GetCharacterId(tid);

                var isHold = CharacterInfo.HasDosa(characterid);
                //에러나서 바꿔두긴 했는대 코드 변경 필요....
                //최대 코인 체크 x 최대 단계인지 체크해야함
                if (!_itemPurchaseInfo.ContainsKey(tid))
                {
                    // 구매 이력이 없을때는 솔드아웃이 x
                    isShow = false;
                }
                else
                {
                    // 구매 이력이 있을때 soldout 인 경우
                    
                    var maxCoin = TableManager.Instance.Character_Breakthrough_Table.GetMaxLimit();
                    var pieceCount = 0;
                    if (isHold)
                        pieceCount = GetEnableCharacterCoin(rewardData.Item_Tid, rewardData.Item_Min);
                    else
                        pieceCount = maxCoin +1;

                    if (storeData.Use_Daily == 0 &&storeData.Use_Weekliy == 0 &&storeData.Use_Limit == 0 && storeData.Use_Month == 0)// 1. 구매 제한이 없을 경우 : 내가 구매 가능 한 코인 갯수가 0일때 soldout 처리 
                        isShow = pieceCount > 0;
                    else      // 2. 구매 제한이 있을 경우 : 구매 횟수 + 내가 보유한 수량이 max보다 큰 경우 sold
                    {
                        var limitCount = ItemBuyLimitCount(storeData);
                        if (GetAmount(storeData.Tid) + limitCount >= maxCoin)
                            isShow = false;
                        else
                            isShow = GetAbleBuyItemCount(storeData.Group_Id, storeData.Tid) > 0;
                    }


                    //if (ItemBuyLimitCount(storeData) -_itemPurchaseInfo[storetype][tid] == 0)
                    if (ItemBuyLimitCount(storeData) -_itemPurchaseInfo[tid] == 0)
                        isShow = true;
                    else
                        isShow = false;
                }

            }
            else
            {
                //if (!_itemPurchaseInfo.ContainsKey(storetype) || !_itemPurchaseInfo[storetype].ContainsKey(tid))
                if (!_itemPurchaseInfo.ContainsKey(tid))
                    isShow = false;
                else
                {
                    //if (ItemBuyLimitCount(storeData) -_itemPurchaseInfo[storetype][tid] == 0)
                    if (ItemBuyLimitCount(storeData) -_itemPurchaseInfo[tid] == 0)
                        isShow = true;
                    else
                        isShow = false;
                }
            }
          
            return isShow;
        }
        // 아이템 구매 후, 구매 가능한 갯수 업데이트
        public void UpdatePurchaseItemCount(JToken token)
        {
            if (token != null)
            {
                var buyToken = token["M"].Cast<JProperty>();
                foreach (var pitem in buyToken)
                {
                    //var value = token["S"]["M"][pitem.Name]["M"]["C"].N_Int();
                    var value = token["M"][pitem.Name]["M"]["C"].N_Int();
                    UpdatePurchaseItemCount(pitem.Name, value);
                }
            }
        }
        public void UpdatePurchaseItemCount(string tid, int count)
        {
            var option = TableManager.Instance.Store_Table[tid].Group_Id;

            // 구매 이력 정보 업데이트
            if (!_itemPurchaseInfo.ContainsKey(tid))
            {
                _itemPurchaseInfo.Add(tid, count);
            }
            else
            {
                _itemPurchaseInfo[tid] = count;
            }
     
        }


        // 갱신시간 리프레시 → 구매 이력이 사라져서 오기 때문에 수정 필요
        public void ReUpdatePurchaseItems(JToken pToken)
        {
            if (pToken == null)
            {
                _itemPurchaseInfo.Clear();
          
            }
            else
            {
                var list = pToken["M"].Cast<JProperty>();

                // 구매 이력이 없는 경우, 토큰에 없는 아이템들을 삭제
                foreach (var id in _itemPurchaseInfo.Keys.ToList())
                {
                    if (!list.Any(item => item.Name == id))
                    {
                        _itemPurchaseInfo.Remove(id);
                    }
                   
                }
            }

        }

        //아이템 별 구매 가능한 갯수 세팅
        public int GetAbleBuyItemCount(string type, string tid)
        {
            int count = 0;
            var data = _storeInfoDic[type].StoreItemList;

            if (_itemPurchaseInfo.ContainsKey(tid))
            {
                for (int n = 0; n < data.Count; n++)
                {
                    if (data[n].Tid == tid)
                    {
                        count = ItemBuyLimitCount(data[n]) -   _itemPurchaseInfo[tid];
                        //count = ItemBuyLimitCount(data[n]) -   _itemPurchaseInfo[type][tid];
                        break;
                    }
                }
            }
            else
            {
                for (int n = 0; n < data.Count; n++)
                {
                    if (data[n].Tid == tid)
                    {
                        count = ItemBuyLimitCount(data[n]);
                        break;
                    }
                }
            }
            return count;
        }
        // Store 아이템 별 최대 구매 갯수 세팅
        public int ItemBuyLimitCount(Store_TableData data)
        {
            int maxCount = 0;
            if (!data.Purchase_Type.Equals("ad"))
            {
                if (data.Use_Daily == 0 && data.Use_Weekliy == 0 &&data.Use_Limit == 0 && data.Use_Month == 0)
                {
                    maxCount = 0;
                }
                else
                {
                    if (data.Use_Daily != 0)
                    {
                        maxCount = data.Use_Daily;
                    }
                    if (data.Use_Weekliy != 0)
                    {
                        maxCount = data.Use_Weekliy;
                    }
                    if (data.Use_Limit != 0)
                    {
                        maxCount = data.Use_Limit;
                    }
                    if (data.Use_Month != 0)
                    {
                        maxCount = data.Use_Month;
                    }
                }
            }
            else
                maxCount = AccountInfo.AdsRewardCount[data.Ad_Goods_Tid];

            return maxCount;
        }
        public bool IsUnlimitedItem(Store_TableData data)
        {
            if (data.Use_Daily == 0 && data.Use_Weekliy == 0 &&data.Use_Limit == 0 && data.Use_Month == 0)
            {
                return true;
            }
            return false;
        }
        public void SetContentStoreRefresh()
        {
            var optionInfo = TableManager.Instance.Store_Option_Table.DataArray;
            for (int n = 0; n < optionInfo.Length; n++)
            {
                var data = optionInfo[n];
                if (!string.IsNullOrEmpty(data.Store_Refresh_Timer))
                {
                    var contentData = TableManager.Instance.Content_Season_Table[data.Store_Refresh_Timer];

                    if(contentData != null)
                    {
                        var contentTime = Leaderboards.GetSetting(contentData.Leaderboard_Id).NextOpenTime;

                        if (contentTime <= DateTime.UtcNow.ToAddHours())
                        {
                            if (!_contentStoreList.Contains(data.Tid))
                            {
                                _contentStoreList.Add(data.Tid);
                            }
                        }
                    }
                }
            }
        }

        public int CheckMainTabIsDisable(string maintab)
        {
            var subTabs = TableManager.Instance.Store_Option_Table.GetSortedOptions(maintab);
            int count = 0;

            if (subTabs == null || subTabs.Count == 0)
            {
                Debug.Log("subTabs 데이터 없음 mainOption과 연결된 subOption Data가 없음 " + maintab);
                return 0;
            }

            for (int s = 0; s < subTabs.Count; s++)
            {
                var items = CheckExpiredTime(subTabs[s]);
                if (items != null && items.Count > 0)
                    count++;
            }
            return count;
        }
        public List<string> CheckIsSubTabDisable(string mainop)
        {
            List<string> sublist = new List<string>();
            var subTabs = TableManager.Instance.Store_Option_Table.GetSortedOptions(mainop);
            for (int n = 0; n < subTabs.Count; n++)
            {
                if (CheckExpiredTime(subTabs[n]).Count > 0)
                {
                    if (!sublist.Contains(subTabs[n]))
                        sublist.Add(subTabs[n]);
                }
            }

            return sublist;
        }

        public string GetCostData(string tid, ITEM_TYPE type)
        {
            string iconStr = "";
            switch (type)
            {
                case ITEM_TYPE.equipment:// 호패 : equipment_table 참조 
                case ITEM_TYPE.equip_01:
                case ITEM_TYPE.equip_02:
                case ITEM_TYPE.equip_03:
                case ITEM_TYPE.equip_04:
                case ITEM_TYPE.equip_05:
                case ITEM_TYPE.equip_06:
                    {
                        var data_e = TableManager.Instance.Equipment_Table;
                        iconStr = data_e[tid].Equip_Icon;
                    }
                    break;
                case ITEM_TYPE.character_coin:// 캐릭터 코인 character_pc_table 참조
                    {
                        var data_c = TableManager.Instance.Character_PC_Table;
                        iconStr = data_c[tid].Char_Icon_Circle_01;
                    }
                    break;
                case ITEM_TYPE.soul:// monster_coin  collection_monster_table 참조
                    {
                        var data_m = TableManager.Instance.Collection_Monster_Table[tid];
                        //var monData = TableManager.Instance.Character_Monster_Table[data_m.Monster_Str]; //Collection_Monster_Table 컬럼 수정으로 주석 및 아이콘 데이터 수정
                        iconStr = data_m.Item_Icon;
                    }
                    break;
                case ITEM_TYPE.currency:    // 재화       item_table 참조(아래 전부)
                case ITEM_TYPE.consumables: // 소모품
                case ITEM_TYPE.stuff:       // 재료               
                case ITEM_TYPE.quest:       // 퀘스트용
                case ITEM_TYPE.box:         // 박스
                case ITEM_TYPE.etc:         // 박스
                    {
                        var data_i = TableManager.Instance.Item_Table;
                        iconStr = data_i[tid].Icon_Id;
                    }
                    break;

                    // 나중에 특수 추가
            }
            return iconStr;
        }


        public string GetCharacterId(string rewardId)
        {
            string prefix = "i_piece_";
            if (rewardId.StartsWith(prefix))
                return rewardId.Substring(prefix.Length);

            return rewardId;
        }

        // 캐릭터 코인 가능 갯수 가져오기
        public int GetEnableCharacterCoin(string rewardId, int rewardcount)
        {
            string characterid = GetCharacterId(rewardId);
          
           CharacterInfo.HasDosa(characterid);
            var maxCoin = TableManager.Instance.Character_Breakthrough_Table.GetMaxLimit();
            
             int currentcoin = 0;
            var dosaInfo = CharacterInfo.GetDosa(characterid);

            if (dosaInfo  != null)
            {
                if (dosaInfo.Breakthrough > 0)
                {
                    if (TableManager.Instance.Character_Breakthrough_Table.GetData(dosaInfo.Breakthrough  -1) == null)
                        currentcoin = 0;
                    else
                        currentcoin = TableManager.Instance.Character_Breakthrough_Table.GetData(dosaInfo.Breakthrough  -1).Piece_Cost;
                }
                else
                    currentcoin = 0;
            }

            var holdcoin = GameInfoManager.Instance.GetAmount(rewardId);

            var totalCoin = currentcoin + holdcoin;
            Debug.Log("["+rewardId+"] totalCoin("+totalCoin+")");
            var enableCoin = maxCoin - totalCoin;
            Debug.Log("["+rewardId+"] enableCoin("+enableCoin+")");
            var result = (int)Math.Ceiling((double)enableCoin / rewardcount);
            Debug.Log("["+rewardId+"] result("+enableCoin+")");
            if (result < 0)
                return 0;
            else
                return result;
        }

        #region[무료 아이템 레드닷]

        // 상점 레드닷 정보 초기화
        public void SetStoreReddotInfo()
        {
            //var storemainOps = TableManager.Instance.Store_MainOption_Table.GetStoreMainOptions("none");
            var storemainOps = TableManager.Instance.Store_MainOption_Table.SortedStoreMainOptions("none");
            for (int n = 0; n< storemainOps.Count; n++)
            {
                var mainindex = n;
                // var subOps = TableManager.Instance.Store_Option_Table.GetStoreOptionGroupByMainOption(storemainOps[n].Tid);
                var subOps = CheckIsSubTabDisable(storemainOps[n].Tid);
                //RedDotManager.Instance.UpdateRedDotDictionary("reddot_store_main_tab", storemainOps[n].Order, 6);
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_store_main_tab", mainindex, 6);
                for (int i = 0; i< subOps.Count; i++)
                {
                    var subIndex = TableManager.Instance.Store_Option_Table[subOps[i]].Order;

                    RedDotManager.Instance.UpdateRedDotDictionary("reddot_store_sub_tab", subIndex, mainindex);
                    // RedDotManager.Instance.UpdateRedDotDictionary("reddot_store_sub_tab", subOps[i].Order, storemainOps[n].Order);
                }
            }
        }
        public void SetCashStoreReddotInfo()
        {
            var cashmainOps = TableManager.Instance.Store_MainOption_Table.SortedStoreMainOptions("cash");
            for (int n = 0; n< cashmainOps.Count; n++)
            {
                var mainindex = n;
                var subOps = CheckIsSubTabDisable(cashmainOps[n].Tid);
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_cashstore_main_tab", mainindex, 5 );
                for (int i = 0; i< subOps.Count; i++)
                {
                    var subIndex = TableManager.Instance.Store_Option_Table[subOps[i]].Order;
                   
                    RedDotManager.Instance.UpdateRedDotDictionary("reddot_cashstore_sub_tab", subIndex, mainindex);
                }
            }
        }

        public int GetMainTabIndex(string classify,string tid)
        { 
            var datas = TableManager.Instance.Store_MainOption_Table.SortedStoreMainOptions(classify);
            int mainindex = 0;
            for (int n = 0; n< datas.Count; n++)
            {
                var index = n;
                if (datas[n].Tid.Equals(tid))
                    mainindex = n;
            }
            return mainindex;
        }

        // 레드닷 정보 업데이트
        public void UpdateStoreReddot()
        {
            int count = 0;
            foreach (var store in _storeInfoDic)
            {
                // 무료 아이템
                if (TableManager.Instance.Store_Table.IsFreeItem(store.Key) || TableManager.Instance.Store_Table.IsAdItem(store.Key))
                {
                    var ops = TableManager.Instance.Store_Option_Table[store.Key];
                    var mainOps = TableManager.Instance.Store_MainOption_Table.GetMainOpData(ops.Main_Option_Group_Tid);
                    if (mainOps == null)
                    {
                        Debug.LogError("Store_MainOption_Table has't " +ops.Main_Option_Group_Tid);
                        continue;
                    }
                    var list = store.Value.StoreItemList;
                    for (int n = 0; n <list.Count; n++)
                    {
                        string str = string.Empty;
                        switch (mainOps.Store_Classification)
                        {
                            case "none":
                                str = "reddot_store_sub_tab";
                                break;
                            case "cash":
                                str = "reddot_cashstore_sub_tab";
                                break;
                            case "emtpy":
                                continue;
                        }

                        if ((TableManager.Instance.Store_Table.GetFreeItems(store.Key) != null&&TableManager.Instance.Store_Table.GetFreeItems(store.Key).Contains(list[n].Tid))
                            ||(TableManager.Instance.Store_Table.GetADItems(store.Key) != null&&TableManager.Instance.Store_Table.GetADItems(store.Key).Contains(list[n].Tid)))
                        {
                            var item = TableManager.Instance.Store_Table[list[n].Tid];
                            var mainindex = GetMainTabIndex(mainOps.Store_Classification, mainOps.Tid);

                            RedDotManager.Instance.SetActiveRedDot(str, ops.Order, mainindex, GetAbleBuyItemCount(list[n].Group_Id, list[n].Tid) > 0);
                        }
                       
                    }
                }
            }
        }
        #endregion


        #region[상점 갱신 ]
        public void StoreRefresh(bool isActive)
        {
           SetContentStoreRefresh();

            OnStoreRefreshEvent -= StoreRefresh;
            OnStoreRefreshEvent = null;
            IsAddEvent = false;

          
            RestApiManager.Instance.RequestBaseGetTime(() =>
            {
                RestApiManager.Instance.ResponseStoreAllRefreshStore(_contentStoreList, isDaily:true, callBack:(res) =>
                {
                    // 주간, 월간 갱신 체크 
                    RestApiManager.Instance.CheckIsWeeklyRefresh();
                    RestApiManager.Instance.CheckIsMonthlyRefresh();
                    //callback?.Invoke();
                    if (isActive)           // 상점 UI 갱신 
                    {
                        RefreshStoreAction();
                    }
                });
                // RefreshStoreAction();
                SetRefreshDailyTimer();
                if (_contentStoreList.Count > 0)
                {
                    _contentStoreList.Clear();
                }
            });
 
        }

        public void RefreshStoreAction()
        {
            var uistore = UIManager.Instance.GetUI<UIStore>();
            if (uistore != null && uistore.gameObject.activeInHierarchy)
            {
                uistore.SetTimerCoroutine(false);
                uistore.UpdateScroll(uistore.SelectedTab);
                var option = TableManager.Instance.Store_Option_Table[uistore.SelectedTab];
                uistore.SetRefreshTimer(option);

                //uistore.SetIsEndTimer();
            }
        }
    }
    #endregion

}

