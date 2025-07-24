using Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Video;
using static Cinemachine.DocumentationSortingAttribute;


namespace LIFULSE.Manager
{
    public partial class GameInfoManager
    {
        private Dictionary<string, bool> _contentIsOpenDic = new Dictionary<string, bool>();
        private Dictionary<string, bool> _contentLevelIsOpenDic = new Dictionary<string, bool>();
        private Dictionary<string, bool> _contentOpenStateDic = new Dictionary<string, bool>();
        private List<CheckOpenConditionsData> _unlockMessages = new List<CheckOpenConditionsData>();


        public Dictionary<string, bool> ContentIsOpenDic { get { return _contentIsOpenDic; } }
        public Dictionary<string, bool> ContentLevelIsOpenDic { get { return _contentLevelIsOpenDic; } }
        public Dictionary<string, bool> ContentOpenStateDic { get { return _contentOpenStateDic; } }

        public Action OpenContentUnlock = null;
        public int UnlockMsgIndex { get; set; } = 0;
        public string ContentOpenStateKey = "_ContentOpenStateKey";
        //public Action<string> OnClearStage { get; private set; } = null;
        // public Action<int> OnClearLevelUp { get; private set; } = null;

        //public void SetContentLockList(string tid, bool isOpen)
        //{
        //    if (!_contentIsOpenDic.ContainsKey(tid))
        //    {
        //        _contentIsOpenDic.Add(tid, isOpen);
        //    }
        //    else
        //    {
        //        _contentIsOpenDic[tid] = isOpen;
        //    }
        //}


        public List<int> GetLockedIndexs(string lockid)
        {
            List<int> list = new List<int>();
            if (TableManager.Instance.Lock_Table[lockid] == null)
                return list;
            var openValues = TableManager.Instance.Lock_Table.GetOpenConditions(lockid);

            if (openValues == null) 
            {
                Debug.LogError("lock table tid " + lockid);
                return list;
            }

            for (int i = 0; i < openValues.Count; i++)
            {
                var index = i;
                var info = openValues[i];
                switch (info.Type)
                {
                    case 0:
                        list.Add(0);
                        break;
                    case 1:             // 플레이어 레벨 체크 조건
                        {
                            if (AccountInfo != null && AccountInfo.Level < int.Parse(info.Value))
                                list.Add(index);
                        }
                        break;
                    case 2:             // 특정 스테이지 클리어 체크 조건
                        {
                            var stagetable = TableManager.Instance.Stage_Table[info.Value];
                            switch (stagetable.CONTENTS_TYPE_ID)
                            {
                                case CONTENTS_TYPE_ID.stage_main:
                                case CONTENTS_TYPE_ID.stage_sub:
                                case CONTENTS_TYPE_ID.story_main:
                                //case CONTENTS_TYPE_ID.event_main:
                                //case CONTENTS_TYPE_ID.story_event:
                                    if (!StageInfo.IsClear(info.Value))
                                        list.Add(index);
                                    break;
                                case CONTENTS_TYPE_ID.event_main:
                                case CONTENTS_TYPE_ID.story_event:
                                    if (!EventStoryInfo.IsClear(info.Value))
                                        list.Add(index);
                                    break;
                                default:
                                    if(!DungeonInfo.IsClear(stagetable.CONTENTS_TYPE_ID, info.Value))
                                        list.Add(index);
                                    break;
                            }
                        }
                        break;
                    case 3:

                        break;
                    default:
                        break;
                }
            }
            return list;
        }

        public void SetupStageClearEvent()
        {
            _stageInfo.OnClearStage  += StageCelarContent;
        }

        public void SetupLevelUpEvent()
        {
            // _accountInfo.OnClearLevelUp += UserLevelUpContent;
            _accountInfo.OnClearLevelUp += UserLevelUpContent;
        }

        public void StageEvent(string tid, bool isOpen)
        {
            if (!_contentIsOpenDic.ContainsKey(tid))
            {
                _contentIsOpenDic.Add(tid, isOpen);
            }
            else
            {
                _contentIsOpenDic[tid] = isOpen;
            }

            SetupStageClearEvent();
        }

        public void LevelUpEvent(string tid, bool isOpen)
        {
            if (!_contentIsOpenDic.ContainsKey(tid))
            {
                _contentIsOpenDic.Add(tid, isOpen);
              
            }
            else
            {
                _contentIsOpenDic[tid] = isOpen;
            }

            SetupLevelUpEvent();
        }

        //add,
        //remove
        
        public void StageCelarContent(string tid)
        {
            // var list = TableManager.Instance.Content_Table.GetContentTid(tid);
            var list = TableManager.Instance.Lock_Table.GetContentTid(tid);
            for (int n = 0; n < list.Count; n++)
            {
                if (!_contentIsOpenDic.ContainsKey(list[n]))
                {
                    _contentIsOpenDic.Add(list[n], true);
                    if (_stageInfo.OnClearStage != null)
                    {
                        _stageInfo.OnClearStage -= StageCelarContent;
                    }
                }
                else
                {
                    _contentIsOpenDic[list[n]] = true;
                }
            }
        }

        public void UserLevelUpContent(int level)
        {
            var curlevel = _accountInfo.Level;
            for (int i = curlevel; i < level+1; i++)
            {
                //if (TableManager.Instance.Content_Table.GetContentTid(i).Count > 0)
                if (TableManager.Instance.Lock_Table.GetContentTid(i).Count > 0)
                {
                    //var list = TableManager.Instance.Content_Table.GetContentTid(i);
                    var list = TableManager.Instance.Lock_Table.GetContentTid(i);
                    for (int n = 0; n < list.Count; n++)
                    {
                        if (!_contentIsOpenDic.ContainsKey(list[n]))
                        {
                            _contentIsOpenDic.Add(list[n], true);
                            if (_accountInfo.OnClearLevelUp != null)
                            {
                                _accountInfo.OnClearLevelUp -= UserLevelUpContent;
                            }
                        }
                        else
                        {
                            _contentIsOpenDic[list[n]] = true;
                        }
                    }
                }
            }
        }

        // 컨텐츠 해금 출력 체크용.
        public void InitializeOpenPopupData()
        {
            var publickkey = RestApiManager.Instance.GetPublicKey();
            var list = TableManager.Instance.Lock_Table.GetDatas();
            Dictionary<string, bool> data = new Dictionary<string, bool>();
            if (!ES3.KeyExists(publickkey+ContentOpenStateKey))
            {
                for (int n = 0; n< list.Count; n++)
                {
                    var info = list[n];
                    if (!_contentOpenStateDic.ContainsKey(info.Tid))
                        _contentOpenStateDic.Add(info.Tid, false);
                }

                ES3.Save(publickkey+ContentOpenStateKey, _contentOpenStateDic);
            }
            else
                _contentOpenStateDic = ES3.Load<Dictionary<string, bool>>(publickkey+ContentOpenStateKey);
        }

        // 레벨 조건 체크
        public void CheckOpenUI(string stageid = null)
        {
            _unlockMessages = new List<CheckOpenConditionsData>();
            UnlockMsgIndex = 0;
            if (_contentOpenStateDic.Count == 0)
                return;
            foreach (var info in _contentOpenStateDic)
            {
                var data = TableManager.Instance.Lock_Table.GetData(info.Key);
                if (data.Type == 2
                    && !string.IsNullOrEmpty(stageid)
                    && data.Value.Equals(stageid)
                    && !info.Value)
                {
                    if (StageInfo.IsClear(data.Value))
                        _unlockMessages.Add(data);
                }
            }

            if (_unlockMessages.Count > 0)
                OpenContentUnlock  = () => { ShowUnlockPopup(); };
        }

        public void ShowUnlockPopup()
        {
            if (UnlockMsgIndex >= _unlockMessages.Count)
            {
                OpenContentUnlock = null;
                return;
            }

            Action closePopup = () => {
                var currentkey = _unlockMessages[UnlockMsgIndex].Tid;
                foreach (var key in _contentOpenStateDic.Keys.ToList())
                {
                    if (TableManager.Instance.Lock_Table.GetData(key).Tid.Equals(currentkey))
                        _contentOpenStateDic[key] = true;
                }
                ES3.Save(RestApiManager.Instance.GetPublicKey()+ContentOpenStateKey, _contentOpenStateDic);
                if (UIManager.Instance.GetUI<UIUnlockPopup>() != null && UIManager.Instance.GetUI<UIUnlockPopup>().gameObject.activeInHierarchy)
                    UIManager.Instance.GetUI<UIUnlockPopup>().OnClickClose();
                UnlockMsgIndex++;
                ShowUnlockPopup();
            };

            var message = LocalizeManager.Instance.GetString(_unlockMessages[UnlockMsgIndex].Message);
            UIManager.Instance.Show<UIUnlockPopup>(
            Utils.GetUIOption(
                UIOption.Message, message,
                UIOption.Action, closePopup
                ));
        }
   
}
}