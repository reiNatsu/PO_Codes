using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomLobbyManager : Singleton<CustomLobbyManager>
{
    public const int MaxCount = 5;
    private Dictionary<int, CustomLobbyData> _customLobbyData = new Dictionary<int, CustomLobbyData>();
    //private CustomLobbyData _tempData;
    private CustomLobbyData _currentLobbyData;
    private int _selectIndex;
    //아마 기본값이 false
    private bool _isRandomSelect = false;

    private string _characterTid;

    public bool IsRandomSelect
    {
        get => _isRandomSelect;
        set
        {
            _isRandomSelect = value;
            ES3.Save(RestApiManager.Instance.GetPublicKey()+"CustomLobbyRandomSelect", _isRandomSelect);
        }
    }

    public int SelectIndex
    {
        get => _selectIndex;
        set
        {
            _selectIndex = value;
            if (_selectIndex != -1)
                ES3.Save(RestApiManager.Instance.GetPublicKey()+"CustomLobbySelectIndex", _selectIndex);
        }
    }

    public CustomLobbyData CurrentLobbyData { get { return _currentLobbyData; } }

    public Dictionary<int, CustomLobbyData> CustomLobbyDatas { get { return _customLobbyData; } }

    private bool _hideMode = false;
    /// <summary>
    /// 컨텐츠 재시도 등 커스텀로비가 보이면 안되는 시기가 있음...
    /// </summary>
    public bool HideMode
    {
        get => _hideMode;
        set => _hideMode = value;
    }

    //메모리 이슈로 캐싱 안함... 
    private GameObject _illistrationObj;

    public GameObject IllistrationObj
    {
        get => _illistrationObj;
    }


    public bool GetSavedData()
    {
        int count = 0;
        foreach (var info in _customLobbyData)
        {
            if (info.Key > -1)
                count++;
        }
        return count > 1;
    }

    public void CreatTempData(int copyTarget)
    {
        if (_customLobbyData.ContainsKey(copyTarget))
        {
            _customLobbyData[-1] = _customLobbyData[copyTarget].Clone();
        }
        else
        {
            _customLobbyData[-1] = new CustomLobbyData();
        }
    }

    public bool HasCustomLobby()
    {
        if (_customLobbyData.Count == 0)
            return false;

        foreach (var data in _customLobbyData)
        {
            if (data.Value == null)
                continue;
            if (data.Value.IsActive())
            {
                return true;
            }
        }
        return false;
    }


    void CustomLobbyDataCheck(int index)
    {
        if (!_customLobbyData.ContainsKey(index))
        {
            _customLobbyData[index] = new CustomLobbyData();
        }
    }

  

    void ShowCharacter(int index, string characterKey)
    {
        CustomLobbyDataCheck(index);

        _customLobbyData[index].CharacterTid = characterKey;

        LobbyController.Instance.SetCharacter(characterKey);
        LobbyController.Instance.LobbyOptionSwitch(true);
    }
    //
    void ShowBackground2D(int index, string tableKey, string background2DKey)
    {
        CustomLobbyDataCheck(index);
        _customLobbyData[index].TableTid = tableKey;
        _customLobbyData[index].Background2D = background2DKey;

        LobbyController.Instance.SetBackground2D(background2DKey);
        LobbyController.Instance.LobbyOptionSwitch(true);
    }

    void ShowBackground3D(int index, string tableKey, string background3DKey)
    {
        CustomLobbyDataCheck(index);
        _customLobbyData[index].TableTid = tableKey;
        _customLobbyData[index].Background3D = background3DKey;
        LobbyController.Instance.SetBackground3D(background3DKey);
        LobbyController.Instance.LobbyOptionSwitch(true);
    }
    void ShowIllustration(int index, string tableKey, string illustrationKey)
    {
        CustomLobbyDataCheck(index);
        _customLobbyData[index].TableTid = tableKey;
        _customLobbyData[index].Illustration = illustrationKey;

        LobbyController.Instance.SetIllustOptionLobby(illustrationKey);
        //LobbyController.Instance.SetBackground2D(illustrationKey);
        //LobbyController.Instance.LobbyOptionSwitch(true);
    }
    public string GetTableTid(int index)
    {

        if (_customLobbyData.ContainsKey(index))
        {
            return _customLobbyData[index].TableTid;
        }

        return "";
    }
    public string GetCharacterTid(int index)
    {
        if (_customLobbyData.ContainsKey(index))
        {
            return _customLobbyData[index].CharacterTid;
        }

        return "";
    }

    public void DeleteCustomLobbyData(int index)
    {
        if (_customLobbyData.ContainsKey(index))
        {
            _customLobbyData[index].ClearData();
            _customLobbyData.Remove(index);
            ES3.DeleteKey(RestApiManager.Instance.GetPublicKey() + "CustomLobby" + index);
            if (ES3.KeyExists(RestApiManager.Instance.GetPublicKey() + "CustomLobby" + index))
                Debug.Log(RestApiManager.Instance.GetPublicKey() + "CustomLobby" + index + " 키의 데이터가 여전히 존재합니다.");
        }
    }

    public void SaveLobbyData()
    {
        SaveLobbyData(_selectIndex);
    }

    public void SaveLobbyData(int index)
    {
        if (_customLobbyData.ContainsKey(index))
        {
            ES3.Save(RestApiManager.Instance.GetPublicKey()+"CustomLobby"+index, _customLobbyData[index]);
        }
    }

    public void LoadLobbyData()
    {
        for (int i = 0; i < MaxCount; i++)
        {
            var key = RestApiManager.Instance.GetPublicKey()+ "CustomLobby" + i;
            if (ES3.KeyExists(key))
            {
                var data = ES3.Load(key) as CustomLobbyData;
                if (data!=null)
                    _customLobbyData[i] = data;
            }
        }

        var selectIndexKey = RestApiManager.Instance.GetPublicKey() + "CustomLobbySelectIndex";
        if (ES3.KeyExists(selectIndexKey))
        {
            _selectIndex = ES3.Load<int>(selectIndexKey);
            if (_selectIndex == -1)
            {
                _selectIndex = 0;
            }
        }
        var isRandomSelect = RestApiManager.Instance.GetPublicKey() + "CustomLobbyRandomSelect";
        if (ES3.KeyExists(isRandomSelect))
        {
            _isRandomSelect = ES3.Load<bool>(isRandomSelect);
        }
    }

    public void ActiveTableData(Lobby_TableData data)
    {
        switch (data.Lobby_Bg_Type)
        {
            case "image":
                ShowBackground2D(_selectIndex, data.Group_Id, data.Lobby_Img);
                break;
            case "mprefab":
                ShowIllustration(_selectIndex, data.Group_Id, data.Lobby_Img);
                break;
            case "lprefab":
                ShowBackground3D(_selectIndex, data.Group_Id, data.Lobby_Img);
                break;
        }
    }
    public void ActiveCharacter(string key)
    {
        ShowCharacter(_selectIndex, key);

    }

    public bool HasData(int slotIndex)
    {
        if (_customLobbyData.ContainsKey(slotIndex))
            return true;

        return false;
    }

    public void PreLobbyData()
    {
        for (int i = 1; i < MaxCount; i++)
        {
            var newIndex = SelectIndex - i;
            if (newIndex <= -1)
            {
                newIndex += MaxCount;
            }

            if (_customLobbyData.ContainsKey(newIndex))
            {
                if (UIManager.Instance.GetUI<UILobby>() != null && UIManager.Instance.GetUI<UILobby>().gameObject.activeInHierarchy)
                    UIManager.Instance.GetUI<UILobby>().StopCharacterInteraction();
                ActiveLobbyData(newIndex);
                break;
            }
        }
    }
    public void NextLobbyData()
    {
        for (int i = 1; i < MaxCount; i++)
        {
            var newIndex = SelectIndex + i;
            if (newIndex >= MaxCount)
            {
                newIndex -= MaxCount;
            }

            if (_customLobbyData.ContainsKey(newIndex))
            {
                if (UIManager.Instance.GetUI<UILobby>() != null && UIManager.Instance.GetUI<UILobby>().gameObject.activeInHierarchy)
                    UIManager.Instance.GetUI<UILobby>().StopCharacterInteraction();
                ActiveLobbyData(newIndex);
                break;
            }
        }
    }

    public string GetCurrentCharacter()
    {
        return _characterTid;
    }

    public void SetExitLobby()
    {
        LobbyController.Instance.ReSettingCameraFOV();
        if (_currentLobbyData != null)
        {
            if (!string.IsNullOrEmpty(_currentLobbyData.Background2D))
                LobbyController.Instance.SetBackground2D("");
            if (!string.IsNullOrEmpty(_currentLobbyData.Background3D))
                LobbyController.Instance.SetBackground3D("");
            if(!string.IsNullOrEmpty(_currentLobbyData.Illustration))
                LobbyController.Instance.SetIllustOptionLobby("");
        }
    }
    public void RefreshLobby()
    {
        var index = SelectIndex;
        ActiveLobbyData(SelectIndex);
    }
    public void ActiveLobbyData(int index)
    {
        if (!_customLobbyData.ContainsKey(index))
        {
            var ranIndex = UnityEngine.Random.Range(0, _customLobbyData.Count);
            index = _customLobbyData.Keys.ToList()[ranIndex];
        }

        // (나중에 지울 수 있음) ; Dictionary에 데이터는 있으나, 안에 테이블tid가 없거나 2D/3D 배경 id 둘다 없으면 다른 데이터 받아오기
        if (string.IsNullOrEmpty(_customLobbyData[index].TableTid) ||
            (string.IsNullOrEmpty(_customLobbyData[index].Background2D) 
            && string.IsNullOrEmpty(_customLobbyData[index].Background3D)&& string.IsNullOrEmpty(_customLobbyData[index].Illustration)))
        {
            index = GetEnableCustomData(index);
        }

        SelectIndex = index;
        var data = _customLobbyData[index];
        if (index != -1)
            LobbyController.Instance.SetCharacter(data.CharacterTid);

        LobbyController.Instance.SetBackground2D(data.Background2D);
        LobbyController.Instance.SetBackground3D(data.Background3D);
        LobbyController.Instance.SetIllustOptionLobby(data.Illustration);
        if (string.IsNullOrEmpty(data.Illustration))
            LobbyController.Instance.LobbyOptionSwitch(true);
      
       
        var tabledata = TableManager.Instance.Lobby_Table[data.TableTid];
        _currentLobbyData = data;
        _characterTid = data.CharacterTid;
    }


    // _customLobbyDatakey에 인덱스 값은 있으나, 데이터가 없을 경우는 데이터가 있는 애로 다시 바꾸기
    private int GetEnableCustomData(int index)
    {
        bool isEnd = false;
        int enableIndex = 0;
        while (!isEnd)
        {
            var ranIndex = UnityEngine.Random.Range(0, _customLobbyData.Count);
            enableIndex = _customLobbyData.Keys.ToList()[ranIndex];
            if (!string.IsNullOrEmpty(_customLobbyData[enableIndex].TableTid))
            {
                isEnd = true;
                break;
            }
        }
        return enableIndex;
    }

    // 로비 인터렉련 TALKBOX 보여주기 위함
    public void ActiveTalkBoxUI(string message, float delaytime)
    {
        if (UIManager.Instance.GetUI<UILobby>() != null)
        {
            var lobby = UIManager.Instance.GetUI<UILobby>();
            if (lobby.gameObject.activeInHierarchy)
                lobby.ShowCharacterTalkBox(message, delaytime);
        }
    }


    [Serializable]
    public class CustomLobbyData
    {

        public bool IsActive()
        {

            if (!string.IsNullOrEmpty(_illustration))
            {
                return true;
            }
            if (!string.IsNullOrEmpty(_characterTid)&&(!string.IsNullOrEmpty(_background3D)||!string.IsNullOrEmpty(_background2D)))
            {
                return true;
            }
            return false;
        }
        [SerializeField]
        private string _tableTid;

        public string TableTid
        {
            get => _tableTid;
            set => _tableTid = value;
        }
        [SerializeField]
        private string _characterTid;
        public string CharacterTid
        {
            get => _characterTid;
            set
            {
                _characterTid = value;
                if (!string.IsNullOrEmpty(_characterTid))
                {
                    _illustration = "";
                }
            }
        }
        [SerializeField]
        private string _illustration;
        public string Illustration
        {
            get => _illustration;
            set
            {
                _illustration = value;
                if (!string.IsNullOrEmpty(_illustration))
                {
                    _characterTid = "";
                    _background2D = "";
                    _background3D = "";
                }
            }
        }

        [SerializeField]
        private string _background2D;
        public string Background2D
        {
            get => _background2D;
            set
            {
                _background2D = value;
                if (!string.IsNullOrEmpty(_background2D))
                {
                    _illustration = "";
                    _background3D = "";
                }
            }
        }

        [SerializeField]
        private string _background3D;
        public string Background3D
        {
            get => _background3D;
            set
            {
                _background3D = value;
                if (!string.IsNullOrEmpty(_background3D))
                {
                    _illustration = "";
                    _background2D = "";
                }
            }
        }

        public CustomLobbyData() { }

        public CustomLobbyData(string tabletid, string illustration, string bg2D, string bg3D, string characterid)
        {
            _tableTid = tabletid;
            _characterTid = characterid;
            _illustration = illustration;
            _background2D = bg2D;
            _background3D = bg3D;
        }

        public CustomLobbyData Clone()
        {
            var temp = new CustomLobbyData();
            temp._characterTid = _characterTid;
            temp._illustration = _illustration;
            temp._background2D = _background2D;
            temp._background3D = _background3D;

            return temp;
        }

        public void ClearData()
        {
            _characterTid = null;
            _illustration  = null;
            _background2D = null;
            _background3D = null;
            _tableTid = null;
        }
    }



    public override IEnumerator Initialize()
    {
        _instance = this;
        yield break;
    }

    public override IEnumerator Setting()
    {
        yield break;
    }

    public override void InitializeDirect()
    {
        _instance = this;
    }

    public override void SettingDirect()
    {
    }
}
