using Consts;
using LIFULSE.Manager;
using NiloToon.NiloToonURP;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;
    [SerializeField] private Vector3 _customLightBaseEuler;
    [SerializeField] private Transform _customLight;
    [SerializeField] private GameObject _light;
    [SerializeField] private GameObject _volume;
    [SerializeField] private Transform _skinCharacterPos;

    [Header("Camera")]
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Camera _uisubCam = null;
    [Header("Camera FOV Setting")]
    [SerializeField] private float _lobbyCamFOV;
    [SerializeField] private float _characterCamFOV;

    [Header("Interaction BackGround")]
    [SerializeField] private Transform _bgObjPosition;

    [SerializeField] private TouchAreaController _touchAreaController;

    [Header("SkinPage Camera Setting")]
    [SerializeField] private SkinCameraSetting _skinCamSetting;

    private Dictionary<string, GameObject> _lobby3DDic = new Dictionary<string, GameObject>();
    private string _illustBGkey;
    private GameObject _illistrationObj;
    
    private float _maincamFOV;
    private float _skinPageFOV = 91.5f;
    private bool _isShowModel = false;

    //명부 등 다른곳에서 쓰이는 캐릭터
    private GameObject _character;
    private float _characterPos = 2f;

    private Vector3 _skinpagePos = new Vector3(0.7f, -1.08f, 0.75f);
    private Vector3 _originalPos = Vector3.zero;
    public GameObject Character
    {
        get => _character;
    }
    public Camera MainCam { get { return _mainCam; } }
    public TouchAreaController TouchAreaController { get { return _touchAreaController; } }
    private NiloToonPerCharacterRenderController _niloToonPerCharacterRenderController;
    [SerializeField] private SpriteRenderer _background;
    
        private Coroutine _coroutine;

        private HashSet<string> _loadedCharacters = new HashSet<string>();
        private string _lastCharacterKey;
        private string _saveLobbyCharacterKey;
        private string _lastSpriteKey;
        private string _last3DObjKey;

    public Camera MainCamera
    {
        get => _mainCam;
        set
        {
            Debug.Log($"_mainCamera 값 변경: {value} (이전 값: {_mainCam})\n{StackTraceUtility.ExtractStackTrace()}");
            _mainCam = value;
        }
    }

    private void OnDestroy() 
    {
        
        LobbyController.Instance = null;
        foreach (var value in _lobby3DDic)
        {
         Destroy(value.Value);
         ResourceManager.Instance.ForceRelease(value.Key);
        }

        if (_character != null)
        {
            Destroy(_character);
        }

        foreach (var loadedCharacter in _loadedCharacters)
        {
            ResourceManager.Instance.ForceRelease(loadedCharacter);
        }
    }

   
    public void CameraView(bool b)
    {
        _mainCam.enabled = b;
        _originalPos = _mainCam.GetComponent<Transform>().position;
    }
    public void GachaView(bool b)
    {
        _customLight.gameObject.SetActive(!b);
        _light.SetActive(!b);
        CameraView(!b);
        _volume.SetActive(!b);
    }

    public void LobbyOptionSwitch(bool b)
    {
        if (!string.IsNullOrEmpty(_last3DObjKey))
        {
            _lobby3DDic[_last3DObjKey].GetComponent<Lobby3DBGObject>().SwitchObject(b);
            _customLight.gameObject.SetActive(!b);
            _light.SetActive(!b);
            CameraView(b);
            _volume.SetActive(!b);

            return;
        }
        _customLight.gameObject.SetActive(b);
        _light.SetActive(b);
        CameraView(b);
        _volume.SetActive(b);
    }

    public void SetIllustOptionLobby(string key)
    {
        CameraView(true);
        _customLight.gameObject.SetActive(false);
        _light.SetActive(false);
        _volume.SetActive(false);

        if (string.IsNullOrEmpty(key))
        {
            if (_illistrationObj != null)
                Destroy(_illistrationObj);

        }
        else
        {
            if (_illistrationObj != null)
            {
                if (_illistrationObj.name == key)
                    return;

                Destroy(_illistrationObj);
            }

            _illistrationObj = ResourceManager.Instance.Load<GameObject>(key, true, UIManager.Instance.GetCanvas(UICanvasType.UIIllust).transform);
            _illistrationObj.name = key;
            _illustBGkey = key;

            CharacterOnOff(false);
        }
    }

    public void Lobby3DOptionSwitch(bool b)
    {
        if (!string.IsNullOrEmpty(_last3DObjKey))
        {
            _lobby3DDic[_last3DObjKey].GetComponent<Lobby3DBGObject>().SwitchObject(b);
            CameraView(b);
            _background.gameObject.SetActive(false);
            
            return;
        }
    }

    public void AdllInteractionSoundOff()
    {
        _touchAreaController.AllSoundOff();
    }

    private void Awake()
    {
        Instance = this;
        _mainCam = Camera.main;
        _maincamFOV = _mainCam.fieldOfView;
        
        _uisubCam = UIManager.Instance.SkinCamera;
        //_originalScale = _background.GetComponent<Transform>().localScale;
        //기본데이터 인풋, 랜덤 선택 유무 필요

        if (!CustomLobbyManager.Instance.HasCustomLobby())
        {
            Debug.Log("<color=#a876ff>LobbyManager() 데이터가 0개이기 때문에 DEFAULT 데이터 넣어줌.</color>");
            if (TableManager.Instance.Lobby_Table.DataArray.Length > 0)
            {
                if (TableManager.Instance.Lobby_Table.GetLobbyDataList(LOBBY_INTERACTION_TYPE.defaultlobby) == null)
                    return;
                var data = TableManager.Instance.Lobby_Table.GetLobbyDataList(LOBBY_INTERACTION_TYPE.defaultlobby).FirstOrDefault();
                string defaultCharacter = TableManager.Instance.Define_Table["ds_default_dosa"].Opt_01_Str.Split(",").FirstOrDefault();
                int index = 0;

                CustomLobbyManager.Instance.IsRandomSelect = false;
                CustomLobbyManager.Instance.SelectIndex = index;

                CustomLobbyManager.Instance.ActiveTableData(data);
                CustomLobbyManager.Instance.ActiveCharacter(defaultCharacter);
                CustomLobbyManager.Instance.SaveLobbyData();
                //GameInfoManager.Instance.SetCustomLobbyData(index, defaultType, data.Group_Id, defaultCharacter, false);
            }
        }
        else
        {
            SetRandomLobby();
        }

        GameInfoManager.Instance.NeedWelcome = false;
    }

    void SetRandomLobby()
    {
        if (CustomLobbyManager.Instance.IsRandomSelect)
        {
            if (CustomLobbyManager.Instance.CurrentLobbyData == null)
                CustomLobbyManager.Instance.ActiveLobbyData(Random.Range(0, CustomLobbyManager.MaxCount));
            else
                CustomLobbyManager.Instance.RefreshLobby();
        }
        else
        {
            CustomLobbyManager.Instance.ActiveLobbyData(CustomLobbyManager.Instance.SelectIndex);
        }
    }

    private void Start()
    {
//        ShowCharacter("CH_jangsan_01");
    }

    public void Clear()
    {
        _lobby3DDic.Clear();
    }


    public void SetBackground3D(string prefabname)
    {
        if (_last3DObjKey == prefabname)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_last3DObjKey))
        {
            _lobby3DDic[_last3DObjKey].SetActive(false);
        }

        _last3DObjKey = prefabname;
        if (string.IsNullOrEmpty(prefabname))
            return;

        //SetIllustOptionLobby("");
        HideAllObject();
        if (!_lobby3DDic.ContainsKey(prefabname))
        {
            var bgobj = LIFULSE.Manager.ResourceManager.Instance.Load<GameObject>(prefabname, true, _bgObjPosition);

            _lobby3DDic.Add(prefabname, bgobj);
        }

        _lobby3DDic[prefabname].SetActive(true);

        Lobby3DBGObject lobbysetting = _lobby3DDic[prefabname].GetComponent<Lobby3DBGObject>();

        if(lobbysetting != null)
            SetMainCamera(lobbysetting.lobbyCameraSetting);
        
        LobbyOptionSwitch(true);
        //Set2DBGLight(false);
    }

  
    public void HideLobbyBG()
    {
        HideAllObject();
    }

    public void SetIllustBackground(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (_illustBGkey == key)
            return;


        if (!string.IsNullOrEmpty(_illustBGkey))
        {
            if (_background.sprite!=null)
                _background.sprite = null;
        }


        _illustBGkey = key;
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        _background.sprite = ResourceManager.Instance.Load<Sprite>(key);
        Backgound2DOnOff(true);
    }

    public void SetBackground2D(string key)
    {
        ShowBackground(key);
        //SetIllustOptionLobby("");
        //Set2DBGLight(true);
    }

    public void SetCharacter(string key, bool isSkin = false)
    {
        ShowCharacter(key,true, isSkin:isSkin);
    }

    public void StartTempCharacterMode()
    {
        _saveLobbyCharacterKey = _lastCharacterKey;
    }
    public void EndTempCharacterMode()
    {
        string key = null;
        if (_saveLobbyCharacterKey.StartsWith("FAB_MODEL_"))
            key = _saveLobbyCharacterKey.Substring("FAB_MODEL_".Length);
        else
            key = _saveLobbyCharacterKey;
        ShowCharacter(key, true);
        _saveLobbyCharacterKey = "";
    }

    public void CharacterOnOff(bool b)
    {
        if (Character != null)
        {
            Character.SetActive(b);   
        }
    }
    void HideAllObject()
    {
        IllistrationObjOnOff(false);
        Backgound2DOnOff(false);
        if (_bgObjPosition == null)
            return;

        for (int n = 0; n< _bgObjPosition.childCount; n++)
        {
            _bgObjPosition.GetChild(n).gameObject.SetActive(false);
        }
    }

    public void SetCameraLobbyFOV()
    {
        _mainCam.fieldOfView = _maincamFOV;
    }
    public void ReSettingCameraFOV()
    {
        _maincamFOV =_mainCam.fieldOfView;
    }
    public void SetCameraSkinFOV()
    {
        var mainCam = Camera.main;
        mainCam.fieldOfView = 91f;
        Debug.Log($"Main Camera FOV 변경됨: {_mainCam.fieldOfView}");
    }

    public void SetLobbyBGMKey(string bgmkey)
    {
        //_currentBGM = bgmkey;
    }
    #region[카메라 세팅 변경]

    public void SetMainCamera(lobbyCameraSetting settings)
    {
        var camTransform = _mainCam.GetComponent<Transform>();
        camTransform.position = settings.Position;
        camTransform.eulerAngles = settings.Roatation;
        var newAngle = _customLightBaseEuler;
        newAngle.y += settings.Roatation.y;
        //_customLight.localEulerAngles = newAngle;
        //camTransform.localScale = settings.Scale;

        _mainCam.orthographic = settings.IsOrthographic;
        if (settings.IsOrthographic)             // orthographic으로 설정 할 경우 값 변경
            _mainCam.orthographicSize = settings.OrthographicSize;
        
        _mainCam.fieldOfView = settings.FieldOfView;
        _mainCam.nearClipPlane = settings.Near;
        _mainCam.farClipPlane = settings.Far;
        
        _characterPos = settings.CharacterPos;
        if (_character != null)
        {
            var originPos = _character.transform.localPosition;
            _character.transform.localPosition = new Vector3(0, originPos.y, _characterPos);
        }
    }

    #endregion


    public void ShowCharacter(string modelKey, bool useDissolve, Action characterShowCallback = null, bool isSkin = false)
    {
        string convertkey = null;
        if (!isSkin)
        {
            if (!string.IsNullOrEmpty(modelKey))
                convertkey = modelKey.ToCostumeKey(modelKey);
            else
            {
                if (!string.IsNullOrEmpty(_lastCharacterKey))
                {
                    _touchAreaController.AllSoundOff();
                    if (_character!=null)
                        Destroy(_character.gameObject);
                    _lastCharacterKey = null;
                }
                return;
            }
        }
        else
        {
            // 테스트
            if (!string.IsNullOrEmpty(_lastCharacterKey))
            {
                _touchAreaController.AllSoundOff();
                if (_character!=null)
                    Destroy(_character.gameObject);
                _lastCharacterKey = null;
            }

            if (!string.IsNullOrEmpty(modelKey))
                convertkey = modelKey;
        }

        if (!string.IsNullOrEmpty(modelKey)&&!modelKey.Contains("MODEL"))
            modelKey = "FAB_MODEL_" + convertkey;
   
        if (modelKey == _lastCharacterKey)
        {
            if (!string.IsNullOrEmpty(_lastCharacterKey))
                _touchAreaController.AnimatorRebind();

            if (isSkin)
            {
                _character.transform.SetParent(_skinCharacterPos);
                _character.transform.localPosition = Vector3.zero;
                _character.transform.localRotation = Quaternion.AngleAxis(0, Vector3.zero);
            }
            else
            {
                var newVector = new Vector3(0, -_touchAreaController.GetTouchAreaPivot.y, _characterPos);
                _character.transform.SetParent(_mainCam.transform);
                _character.transform.localPosition = newVector;
                _character.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            }

            characterShowCallback?.Invoke();
            return;
        }

        ///Todo 신준호 => 아래 코드 앞부분에 ! 추가
        if (!string.IsNullOrEmpty(_lastCharacterKey))
        {
            if(_character!=null)
            Destroy(_character.gameObject);
        }
        
        _lastCharacterKey = modelKey;
        if(string.IsNullOrEmpty(modelKey))
            return;
        
        _character = ResourceManager.Instance.Load<GameObject>(modelKey,true);
        if (!_loadedCharacters.Contains(modelKey))
        {
            _loadedCharacters.Add(modelKey);
        }

        _isShowModel = true;
        //Todo 신준호 => 캐릭터 리소스 없을시 시온 모델로 출력하도록 예외처리
        if (_character == null)
        {
            if (UIManager.Instance.GetUI<UILobbySetting>() != null && UIManager.Instance.GetUI<UILobbySetting>().gameObject.activeInHierarchy)
                _isShowModel = false;
        }
        
        _touchAreaController.TouchAreaObject = _character.GetComponent<TouchAreaObject>();
        if (!isSkin)
        {
            var newVector = new Vector3(0, -_touchAreaController.GetTouchAreaPivot.y, _characterPos);
            _character.transform.SetParent(_mainCam.transform);
            _character.transform.localPosition = newVector;
            _character.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else
        {
            _character.transform.SetParent(_skinCharacterPos);
            _character.transform.localPosition = Vector3.zero;
            _character.transform.localRotation = Quaternion.AngleAxis(0, Vector3.zero);
        }
       
        
       // _character.transform.localPosition = newVector;
        //_character.transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
        
        _niloToonPerCharacterRenderController = _character.GetComponent<NiloToonPerCharacterRenderController>();
        _niloToonPerCharacterRenderController.dissolveAmount = useDissolve?1f:0f;
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        if (useDissolve)
        {
            _coroutine = StartCoroutine(ShowCharacterIEnumerator(GameInfoManager.Instance.NeedWelcome));
        }

        if (isSkin)
        {
            //if (_mainCam.fieldOfView >=91)
            //    _character.transform.localPosition = new Vector3(_skinpagePos.x, _skinpagePos.y, 0.75f);
            //else
            //    _character.transform.localPosition = new Vector3(_skinpagePos.x, _skinpagePos.y, 1.3f);
        }

        characterShowCallback?.Invoke();

        // 로비 세팅 > 미리 보기 화면에서 모델링이 없을경우는 안내 메세지 띄우기. 
        if (!_isShowModel&&
            UIManager.Instance.GetUI<UILobbySetting>() != null && 
            UIManager.Instance.GetUI<UILobbySetting>().gameObject.activeInHierarchy)
        {
            _character.gameObject.SetActive(false);
            var des = LocalizeManager.Instance.GetString("str_ui_server_error_dec_02");
            GameManager.Instance.ServerCanvas.QuitServerPopup(des, () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
            });
        }
    }


    //Todo 신준호 => 명부UI Close() 호출될때 캐릭터 포지션 초기화 목적으로 만듬
    public void InitCharacterPosition()
    {
        if(_character == null)
        {
            return;
        }
        _character.SetActive(true);
        _character.transform.SetParent(_mainCam.transform);
        var newVector = new Vector3(0, -_touchAreaController.GetTouchAreaPivot.y, 2);
        _character.transform.localPosition = newVector;
        _character.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
    }


    //커스텀 로비 세팅창에서 로비로 들어올 때
    public void ActiveDissolve(bool isWelcome = false)
    {  
        if(_character==null)
            return;
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _touchAreaController.AnimatorRebind();
        _niloToonPerCharacterRenderController.dissolveAmount = 1f;
        //var isWelcome = false;
        if (UIManager.Instance.GetUI<UILobby>() != null &&UIManager.Instance.GetUI<UILobby>().gameObject.activeInHierarchy)
            isWelcome = true;

      _coroutine = StartCoroutine(ShowCharacterIEnumerator(isWelcome, 0.5f));
    }

    IEnumerator ShowCharacterIEnumerator(bool useWelcome = true,float waitTime = 2.5f)
    {
        if (GameInfoManager.Instance.NeedWelcome)
        {
            yield return YieldWaitCache.WaitForSeconds(waitTime);
        }

        var localTime = 1f;
        while (localTime>0)
        {
            yield return null;
            localTime -= Time.deltaTime * 2f;
            _niloToonPerCharacterRenderController.dissolveAmount = localTime;
        }

        if (useWelcome && _isShowModel)
        {
            _touchAreaController.PlayLobbyCharacterAnimation(CharacterMainMenuType.Welcome);
        }

        _coroutine = null;
    }

    //Todo 신준호 => 명부용으로 추가함 
    public void ShowCharacaterToUICharacter(string modelKey, Action characterShowCallback = null)
    {
        ShowCharacter(modelKey,false,characterShowCallback);
        return;
        if (!string.IsNullOrEmpty(modelKey)&&!modelKey.Contains("MODEL"))
        {
            modelKey = "FAB_MODEL_" + modelKey;

        }
        //임시코드
        if (!string.IsNullOrEmpty(modelKey))
        {
            modelKey = "FAB_MODEL_CH_jangsan_01";
        }


        if(_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        //로비의 캐릭터와 같은 캐릭터를 선택한 경우 오브젝트 생성은 하지 않음
        if (modelKey == _lastCharacterKey)
        {
            //로비에서 동작하고 있는 애니메이션 초기화
            var anim = _character.GetComponent<Animator>();

            if (anim != null)
            {
                anim.Rebind();
            }
        }
        else if (!string.IsNullOrEmpty(_lastCharacterKey))
        {
            if (_character!=null)
                Destroy(_character.gameObject);

            _character = ResourceManager.Instance.Load<GameObject>(modelKey, true);
            _touchAreaController.TouchAreaObject = _character.GetComponent<TouchAreaObject>();
        }

        _lastCharacterKey = modelKey;
        if (string.IsNullOrEmpty(modelKey))
            return;

        _character.transform.SetParent(_mainCam.transform);
        var newVector = new Vector3(0, -_touchAreaController.GetTouchAreaPivot.y, 2);

        _character.transform.localPosition = newVector;
        _character.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);

        _niloToonPerCharacterRenderController = _character.GetComponent<NiloToonPerCharacterRenderController>();
        _niloToonPerCharacterRenderController.dissolveAmount = 0f;
        characterShowCallback?.Invoke();
    }

    public void Backgound2DOnOff(bool b)
    {
        _background.gameObject.SetActive(b);   
    }
    public void IllistrationObjOnOff(bool b)
    {
        if (CustomLobbyManager.Instance.IllistrationObj != null)
        {
            CustomLobbyManager.Instance.IllistrationObj.SetActive(b);
        }
    }

    [Button]
    void Fit()
    {
        var spriteSize = _background.sprite.rect.size*0.01f;

        var seta = _touchAreaController.Camera.fieldOfView*0.5f;
        
        var length = Mathf.Abs(_background.transform.localPosition.z);
        
        var halfHorizontal = Mathf.Tan(seta*Mathf.Deg2Rad);
        var resultHight = length * halfHorizontal;
        var newScale = resultHight/(spriteSize.y*0.5f);
        _background.transform.localScale = new Vector3(newScale, newScale, newScale);
        
        //y 사이즈 구해야함
    }

    [Button]
    void SetSkinCam()
    {
        _skinCamSetting.SetCamera();
    }

    public void ShowBackground(string backgroundKey)
    {
        
        if (_lastSpriteKey == backgroundKey)
            return;


        if (!string.IsNullOrEmpty(_lastSpriteKey))
        {
            if(_background.sprite!=null)
            _background.sprite = null;
        }


        _lastSpriteKey = backgroundKey;
        if (string.IsNullOrEmpty(backgroundKey))
        {
            return;
        }

        _background.sprite = ResourceManager.Instance.Load<Sprite>(backgroundKey);
        Backgound2DOnOff(true);
    }

    public void SetActiveCharacterTouch(bool isActive)
    {
        _touchAreaController.IsTouchActive = isActive;
    }

    public void SetActiveCharacter(bool isActive)
    {
        if (_character != null)
            _character.SetActive(isActive);
    }


    #region[카메라 세팅]

    public void SetSkinPageCamera(bool isSkinpage)
    {
        _mainCam.enabled = !isSkinpage;
        
    }


    public void SwithCamera(bool useuicam = false)
    {
        _uisubCam.enabled = useuicam;
        var mainCamData = _mainCam.GetUniversalAdditionalCameraData();
        var uiCamData = _uisubCam.GetUniversalAdditionalCameraData();
        _customLight.gameObject.SetActive(useuicam);
        _light.SetActive(useuicam);
        _volume.SetActive(useuicam);
        _originalPos = _mainCam.GetComponent<Transform>().localPosition;
        // 메인 카메라 태그 변경
        if (useuicam)
        {
            _uisubCam.tag = "MainCamera";
            _mainCam.tag = "Untagged";
           
            mainCamData.renderType = CameraRenderType.Overlay;
            UIManager.Instance.ChangeSkinPageUI(true, _mainCam);
            //uiCamData.renderType = CameraRenderType.Base;
            if (!uiCamData.cameraStack.Contains(_mainCam))
            {
                uiCamData.cameraStack.Add(_mainCam);
                Debug.Log("Main Camera가 UI Camera의 스택에 추가되었습니다.");
            }
            var vertialFOV = Camera.VerticalToHorizontalFieldOfView(_skinCamSetting.FieldOfView, _mainCam.aspect);
            _mainCam.fieldOfView = vertialFOV;
            var ypos = _character.GetComponent<UICharacterModelObject>().ModelPivot.localPosition.y;
            var yValue = ypos + _skinCamSetting.YValue;
            var tran = _mainCam.GetComponent<Transform>();
            tran.localPosition = new Vector3(_skinCamSetting.Pos.x, yValue, _skinCamSetting.Pos.z);
            _mainCam.fieldOfView = _skinCamSetting.FieldOfView;
            tran.localRotation = Quaternion.Euler(_skinCamSetting.Rot);
            _skinCamSetting.UpdateResultCmaPos(_mainCam);
            _uisubCam.fieldOfView = 91f;
            ToggleSpecificLayers(_mainCam, addLayers: false);
        }
        else
        {
            _mainCam.tag = "MainCamera";
            _uisubCam.tag = "Untagged";
            UIManager.Instance.ChangeSkinPageUI(false, _mainCam);
            //uiCamData.renderType = CameraRenderType.Overlay;
            mainCamData.renderType = CameraRenderType.Base;
            // _uiCam의 스택에서 _mainCam 제거
            if (uiCamData.cameraStack.Contains(_mainCam))
            {
                uiCamData.cameraStack.Remove(_mainCam);
                Debug.Log("Main Camera가 UI Camera의 스택에서 제거되었습니다.");
            }
            ToggleSpecificLayers(_mainCam, addLayers: true);
            _mainCam.fieldOfView = 60f;
            _uisubCam.fieldOfView = 60f;
        }
    }
    public void ToggleSpecificLayers(Camera camera, bool addLayers)
    {
        string[] layersToHandle = { "UI", "UITop", "UIPopUp" }; // 처리할 레이어 이름들

        foreach (var layerName in layersToHandle)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                if (addLayers)
                {
                    // 레이어를 추가할 때
                    camera.cullingMask |= (1 << layer); // 해당 레이어 추가
                    Debug.Log($"{layerName} 레이어가 추가되었습니다.");
                }
                else
                {
                    // 레이어를 제거할 때
                    camera.cullingMask &= ~(1 << layer); // 해당 레이어 제거
                    Debug.Log($"{layerName} 레이어가 제거되었습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"{layerName} 레이어가 존재하지 않습니다.");
            }
        }
    }
    #endregion



    public void SetMainCameraValues()
    {
        var vertialFOV = Camera.VerticalToHorizontalFieldOfView(_skinCamSetting.FieldOfView, _mainCam.aspect);
        _mainCam.fieldOfView = vertialFOV;

        _mainCam.GetComponent<Transform>().localPosition = new Vector3(_skinCamSetting.Pos.x, _character.GetComponent<UICharacterModelObject>().ModelPivot.localPosition.y, _skinCamSetting.Pos.z);
        _skinCamSetting.UpdateResultCmaPos(_mainCam);
        //var ypos = _character.GetComponent<UICharacterModelObject>().ModelPivot.localPosition.y;
        var ypos = _character.GetComponent<UICharacterModelObject>().ModelPivot.localPosition.y;
        var yValue = ypos + _skinCamSetting.YValue;
        //_mainCam.fieldOfView = _skinCamSetting.FieldOfView;
        _mainCam.GetComponent<Transform>().localPosition = new Vector3(_skinCamSetting.Pos.x, yValue, _skinCamSetting.Pos.z);
        _mainCam.fieldOfView = _skinCamSetting.FieldOfView;

        _skinCamSetting.UpdateResultCmaPos(_mainCam);
    }
}
