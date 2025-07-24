using Consts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LIFULSE.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        private UILog _uiLog;
        private SpeechController _speechController;

        private Dictionary<string, Canvas> _canvasDict = null;
        private Dictionary<string, GraphicRaycaster> _raycasterDict = null;

        private Dictionary<string, UIBase> _activatedUIDict = new Dictionary<string, UIBase>();
        private Dictionary<string, UIBase> _cachedUIDict = new Dictionary<string, UIBase>();

        private List<UIBase> _notCachedUIList = new List<UIBase>();
        //
        private Stack<GameObject> _interactiveUIStack = new Stack<GameObject>();
        private Stack<UIBase> _fullUIStack = new Stack<UIBase>(); //화면에 가득차는 ui 뎁스 관리
        private Stack<UIStack> _uiStack = new Stack<UIStack>(); //popup ui stack
        private UIBase _defaultUi = null; //각 씬에서 디폴트로 존재하는 full ui (로비, 스테이즈 노드맵 등)

        public bool IsDefault { get { return _fullUIStack.Count == 0 && _uiStack.Count == 0; } }
        //오픈되어있는 팝업이 있는가?
        public bool IsOpenPopup { get { return _uiStack.Count != 0; } }

        private ToastMessageManager _toastMessageManager = null;
        private UIClickEffectCanvas _clickEffectCanvas = null;

        private UILoading _uiLoading = null;
        private UIBlock _uiBlock = null;
        private UIBlock_Store _uiBlockStore = null;

        private DimController _dimController = null;
        public FadeController FadeController { get; private set; } = null;
        public MaintenanceController MaintenanceController { get; private set; } = null;

        private float _baseFOV;
        private bool _isLockOverlay = false;
        private GameObject _uiCameras;
        //private Camera _mainCamera;
        //private Camera _uiMainCamera;
        private List<Coroutine> _speechRoutines = new List<Coroutine>();

        //public Camera MainCamera { get => _mainCamera; }
        //public Camera UIMainCamera { get => _uiMainCamera; }

        //스킨용 카메라
        private Camera _skinCamera;
        public Camera SkinCamera { get => _skinCamera; }

        //private EventSystem _eventSystem;
        //public EventSystem EventSystem { get => _eventSystem; }
        
        /*public bool MainCameraEnable
        {
            get => _mainCamera.enabled;
            set
            {
                Debug.Log("_mainCamera.enabled : " + value.ToString());
                _mainCamera.enabled = value;
            }
        }*/
        //private Dictionary<string, Camera> _cameraDict = new Dictionary<string, Camera>();
        private Dictionary<string, int> _activatedUICount = new Dictionary<string, int>();

        //public Dictionary<string, Camera> CameraDict => _cameraDict;

        private int _sortValue = 1000;
        private bool _isInit = false;
        private float _canvasPlaneDistance = 9;

        public UILoading UILoading
        {
            get
            {
                if (SceneManager.Instance.SceneState == SceneState.PvPScene || SceneManager.Instance.SceneState == SceneState.PvPLineScene)
                {
                    var pvpLoading = GetUI<UIPvpLoading>();
                    return pvpLoading;
                }
                else
                {
                    if (_uiLoading == null)
                    {
                        UIBase uiBase;

                        if (_activatedUIDict.TryGetValue(typeof(UILoading).Name, out uiBase))
                            _uiLoading = uiBase.GetComponent<UILoading>();
                        else
                        {
                            if (_cachedUIDict.TryGetValue(typeof(UILoading).Name, out uiBase))
                                _uiLoading = uiBase.GetComponent<UILoading>();
                        }
                    }
                }

                return _uiLoading;
            }
        }
        #region Temp

        private void AddActivatedUI(string key, UIBase value)
        {
            //Debug.Log("TempLog AddActivatedUI : " + key + " : " + value.name);
            _activatedUIDict.Add(key, value);
        }

        private void RemoveActivatedUI(string key)
        {
            //Debug.Log("TempLog RemoveActivatedUI : " + key );
            _activatedUIDict.Remove(key);
        }


        public void RemoveUIStack(UIStack uistack)
        {
            if (_uiStack.Count == 0)
                return;

            Stack<UIStack> tempStack = new Stack<UIStack>();

            while (_uiStack.Count > 0)
            {
                UIStack currentItem = _uiStack.Pop();

                if (currentItem == null || !currentItem.Equals(uistack))
                    tempStack.Push(currentItem);
            }

            while (tempStack.Count > 0)
            {
                _uiStack.Push(tempStack.Pop());
            }
        }

       

        

        private void StackPeekHide(UIBase _uiBase)
        {
            var peek = StackPeek();
            if (peek != _uiBase)
            {
                peek.Hide();
            }
        }

        public void PushUiStack(UIStack uiStack)
        {
            if (!_uiStack.Contains(uiStack))
                _uiStack.Push(uiStack);
            else
                MoveItemToTop(_uiStack, uiStack);
        }

        private UIStack PeekUiStack()
        {
            if (_uiStack == null || _uiStack.Count == 0)
                return null;

            return _uiStack.Peek();
        }

        private UIStack PopUiStack()
        {
            if (_uiStack == null || _uiStack.Count == 0)
                return null;

            return _uiStack.Pop();
        }

        private void PopUiStack(UIBase uiBase)
        {
            if (uiBase == null)
                return;

            if (_uiStack == null || _uiStack.Count == 0)
                return;

            Stack<UIStack> tempStack = new Stack<UIStack>();

            while (_uiStack.Count > 0)
            {
                var currentItem = _uiStack.Pop();

                if (currentItem.UIBase != null && !currentItem.UIBase.Equals(uiBase))
                    tempStack.Push(currentItem);
            }

            while (tempStack.Count > 0)
            {
                _uiStack.Push(tempStack.Pop());
            }
        }

        private void StackPush(UIBase uiBase)
        {

            if (!_fullUIStack.Contains(uiBase))
                _fullUIStack.Push(uiBase);
            else
                MoveItemToTop(_fullUIStack, uiBase);
        }

        //이미 존재하는 data 맨 위로 올리기
        private void MoveItemToTop<T>(Stack<T> stack, T item)
        {
            if (stack == null)
                return;

            if(stack.Contains(item) && !stack.Peek().Equals(item))
            {
                Stack<T> tempStack = new Stack<T>();

                while (!stack.Peek().Equals(item))
                {
                    tempStack.Push(stack.Pop());
                }

                stack.Pop();

                while (tempStack.Count > 0)
                {
                    stack.Push(tempStack.Pop());
                }

                stack.Push(item);
            }
        }

        private void RemoveStack<T>(Stack<T> stack, T item)
        {
            if (stack == null)
                return;

            Stack<T> tempStack = new Stack<T>();

            while (stack.Count > 0)
            {
                T currentItem = stack.Pop();
                if (!currentItem.Equals(item))
                {
                    tempStack.Push(currentItem);
                }
            }

            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }
        }

        private UIBase StackPop()
        {
            var pop = _fullUIStack.Pop();
            
            return pop;
        }
        private UIBase StackPeek()
        {
            if(_fullUIStack == null || _fullUIStack.Count == 0)
                return null;

            var peek = _fullUIStack.Peek();

            return peek;
        }
        #endregion


        private void Init(GameObject obj)
        {
            GameObject uiService = Instantiate(obj, this.transform);

            var canvasArrr = uiService.transform.GetComponentsInChildren<Canvas>();

            _canvasDict = new Dictionary<string, Canvas>();
            _raycasterDict = new Dictionary<string, GraphicRaycaster>();
            _activatedUICount = new Dictionary<string, int>();

            for (int i = 0; i < canvasArrr.Length; i++)
            {
                string key = canvasArrr[i].name.Replace("Canvas", "");

                _canvasDict.Add(key, canvasArrr[i]);
                _raycasterDict.Add(key, canvasArrr[i].GetComponent<GraphicRaycaster>());
                _activatedUICount.Add(key, 0);
            }

            SetupCanvasMatch();

            _toastMessageManager = _canvasDict[UICanvasType.UITop.ToString()].gameObject.AddComponent<ToastMessageManager>();

            _clickEffectCanvas = _canvasDict[UICanvasType.UIClickEffect.ToString()].GetComponent<UIClickEffectCanvas>();
            FadeController = _canvasDict[UICanvasType.UIFade.ToString()].GetComponent<FadeController>();
            FadeController.Init();
            MaintenanceController = _canvasDict[UICanvasType.UIFade.ToString()].GetComponent<MaintenanceController>();
            MaintenanceController.Init();

            _dimController = uiService.transform.Find("DimCanvas").GetComponent<DimController>();

            // 카메라
            //_uiMainCamera = uiService.transform.Find("UIMainCamera").GetComponent<Camera>();

            // 스킨용 카메라(일단 하위에 카메라 한개만 둘 것이므로, 제일 상단의 자식 오브젝트만 가지고 오기.)
            var skincamObj = uiService.transform.Find("UIModelBG");
            _skinCamera = skincamObj.GetChild(0).GetComponent<Camera>();
            _skinCamera.enabled = false;
            //var uiMainCamera = uiService.transform.Find("UIMainCamera");
            //var cameraChildCount = _uiMainCamera.transform.childCount;
            /*var cameraChildCount = uiMainCamera.childCount;
            for (int i = 0; i < cameraChildCount; ++i)
            {
                var childCamera = uiMainCamera.GetChild(i).GetComponent<Camera>();
                var childName = childCamera.name.Replace("Camera", string.Empty);

                _cameraDict.Add(childName, childCamera);
            }*/

            // 인트로 씬 이외에 처리
            if (SceneManager.Instance.SceneState != SceneState.IntroScene){
            
                
                if (ManagerLoader.Instance.ManagerLoadType == ManagerLoadType.Manual)
                {
                    DimData data = new DimData();
                    data.dimType = DimType.CenterToEnd;
                    PlayDim(data);
                }
            }

            _speechController = uiService.GetComponentInChildren<SpeechController>();
            _baseFOV = 30f;

            SetupLog();
        }

        //임시로 로그 보는 용 나중에 지워야함
        private void SetupLog()
        {
            _uiLog = gameObject.FindChild("UILog").GetComponent<UILog>();
        }

        //인게임 전용 튜토리얼
        public void StartTutorial(string id, Action endCallback = null)
        {
            //_checker.TutorialNum(id, endCallback);
            TutorialManager.Instance.StartTutorial(id, endCallback);
        }

        public void NextTutorial(bool isInput, Action endCallback = null)
        {
            //_checker.NextTutorial(isInput, endCallback);
            TutorialManager.Instance.Next();
        }

        public bool IsWaitInputTutorial()
        {
            return TutorialManager.Instance.IsWaitInput;
        }

        public override void InitializeDirect()
        {
            _instance = this;
            GameObject obj = ResourceManager.Instance.Load<GameObject>(typeof(UIManager).Name);
            Init(obj);
        }

        public override void SettingDirect()
        {
            _toastMessageManager.Setting();
            //_textMover.RefreshPosition();

            if (_clickEffectCanvas != null)
                _clickEffectCanvas.Setting();
        }
        public override IEnumerator Initialize()
        {
            _instance = this;
            var wait = false;
            ResourceManager.Instance.LoadAssetAsync<GameObject>(typeof(UIManager).Name, (r) =>
            {

                GameObject obj = r;
                Init(obj);
                wait = true;
            });
            yield return new WaitUntil(() => wait);
        }

        public override IEnumerator Setting()
        {
            SettingDirect();
            yield break;
        }

        #region CAMERA
        /*public void CameraSwitch(bool b) {
            foreach (var item in _cameraDict)
            {
                item.Value.enabled = b;
            }
        }*/

        public void SetBaseUIMainCamera()
        {
            /*OnChangeUICameraStack(_uiMainCamera);
            SetRenderType(_uiMainCamera, CameraRenderType.Base);*/
        }

        public void SetModelCamera(CameraSetting setting)
        {
            var camera = Camera.main;

            if (camera == null)
                return;

            var transform = camera.transform;

            transform.position = setting.Position;
            transform.rotation = setting.Rotation;

            camera.orthographic = setting.IsOrthographic;
            camera.orthographicSize = setting.OrthographicSize;

            camera.fieldOfView = setting.FieldOfView;

            camera.nearClipPlane = setting.NearClipPlane;
            camera.farClipPlane = setting.FarClipPlane;
        }
    

        public void AddCameraStack(Camera camera)
        {
            return;
            /*var cameraData = _mainCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(camera);*/
        }

        public void AddCameraStack(Camera camera, int index)
        {
            return;
            /*var cameraData = _mainCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Insert(index, camera);*/
        }

        public void RemoveCameraStack(Camera camera)
        {
            /*var cameraData = _mainCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(camera);*/
        }

        public void AddMainCameraStack(Camera camera)
        {
            /*var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(camera);*/
        }

        public void UpdateCanvasRenderCamera(Camera camera)
        {
            var keys = _canvasDict.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                _canvasDict[keys[i]].worldCamera = camera;
            }
        }

        
        public void UpdateClickEffectCanvasRenderCamera(Camera camera)
        {
            if (!_canvasDict.TryGetValue(UICanvasType.UIClickEffect.ToString(), out var canvas))
                return;
            canvas.worldCamera = camera;
        }

        private string GetCameraName(UICanvasType type)
        {
            // 정리할 필요가 있을 듯
            // 카메라 순서
            // UICamera
            // UIModelCamera
            // UILobbyCamera
            // UIPopupCamera
            // UITopCamera
            switch (type)
            {
                case UICanvasType.UITutorial:
                    return UICanvasType.UITutorial.ToString();                   
                case UICanvasType.UITop:                
                case UICanvasType.UIFade:
                case UICanvasType.UIClickEffect:
                    return UICanvasType.UITop.ToString();
                default:
                    return type.ToString();
            }
        }

        public Canvas GetCanvas(UICanvasType canvasType)
        {
            _canvasDict.TryGetValue(GetCameraName(canvasType), out var cavas);
            return cavas;
        }

        public Canvas GetCanvas(string canvasLayer)
        {
            _canvasDict.TryGetValue(canvasLayer, out var cavas);
            return cavas;
        }

     
        public GraphicRaycaster GetGraphicRaycaster(UICanvasType canvasType)
        {
            _raycasterDict.TryGetValue(canvasType.Str(), out var raycaster);
            return raycaster;
        }

        public Canvas GetUIFadeCanvas()
        {
            _canvasDict.TryGetValue(UICanvasType.UIFade.Str(), out var canvas);
            return canvas;
        }
   

        /*public Camera GetMainCamera()
        {
            if (_mainCamera == null)
                return _uiMainCamera;

            return _mainCamera;
        }*/

     

        public void UpdateCanvasPlaneDistance(float distance)
        {
            foreach (var canvas in _canvasDict)
            {
                canvas.Value.planeDistance = distance;
            }
        }
        #endregion

        //컨텐츠 UI 오픈 등에서 사용
        public void Show(string prefabName, Dictionary<UIOption, object> optionDict = null)
        {
            ShowUI(prefabName, optionDict);
        }

        public void Show<T>(Dictionary<UIOption, object> optionDict = null) where T : class
        {

            string key = typeof(T).Name;
            ShowUI(key, optionDict);
        }

        private void ShowUI(string key, Dictionary<UIOption, object> optionDict = null)
        {
            var layerName = "UI";

            if (_cachedUIDict.TryGetValue(key, out UIBase uiBase) || _activatedUIDict.TryGetValue(key, out uiBase))
            {
                if (!uiBase._needCached)
                    _notCachedUIList.Add(uiBase);
                else if (!_activatedUIDict.ContainsKey(key))
                {
                    AddActivatedUI(key, uiBase);
                }
                //activatedUIDict[key] = uiBase;

                 layerName = LayerMask.LayerToName(uiBase.gameObject.layer);

                
                UpdateActivatedUI(layerName, true);
                CheckDefaultUI(uiBase);
                uiBase.Show(optionDict, key);
            }
            else
            {
                var obj = ResourceManager.Instance.Load<GameObject>(key);

                if (obj == null)
                    Debug.LogError(key + "에 해당하는 Address 확인 필요");

                uiBase = obj.GetComponent<UIBase>();
                layerName = LayerMask.LayerToName(uiBase.gameObject.layer);
                

                Transform tr;
                if (_canvasDict.ContainsKey(layerName))
                    tr = _canvasDict[layerName].transform;
                else
                    tr = _canvasDict["UI"].transform;


                uiBase = Instantiate(obj, tr).GetComponent<UIBase>();

                if (uiBase != null)
                {
                    if (!uiBase._needCached)
                        _notCachedUIList.Add(uiBase);
                    else if (!_activatedUIDict.ContainsKey(key))
                    {
                        AddActivatedUI(key, uiBase);
                    }

                    uiBase.Init();
                    UpdateActivatedUI(layerName, true);
                    CheckDefaultUI(uiBase);
                    uiBase.Show(optionDict, key);
                }
            }


        }

        public void Close<T>(bool needCached = true) where T : UIBase
        {
            var ui = GetUI<T>();
            ui?.Close(needCached);
        }

        public void SetupCanvasMatch()
        {
            float defaultAspectRatio = 16.0f / 9.0f;
            float curAspectRatio = (float)Screen.width / (float)Screen.height;

            float matchValue = 0;

            if (curAspectRatio >= defaultAspectRatio)
                matchValue = 1;
            else
                matchValue = 0;

            var keys = _canvasDict.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].Equals("UIIllust"))
                    continue;

                _canvasDict[keys[i]].sortingOrder = i * _sortValue;
                _canvasDict[keys[i]].GetComponent<CanvasScaler>().matchWidthOrHeight = matchValue;
            }
        }

        public void SetCanvasMatch(float value)
        {
            var keys = _canvasDict.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                _canvasDict[keys[i]].GetComponent<CanvasScaler>().matchWidthOrHeight = value;
            }
        }

        public void AddLog(string message)
        {
            _uiLog.AddMessage(message);
        }

        public void ShowToastMessage(string stringTid, string imageId = null ,float displayTime = -1f, float positionY = 0f)
        {
            if (string.IsNullOrEmpty(imageId))
                _toastMessageManager.ShowToastMessage(stringTid, displayTime, positionY);
            else
                _toastMessageManager.ShowImageToastMessage(stringTid, imageId, displayTime, positionY);
        }


        // Localize 처리된 string 넣어줘야함
        public void ShowAlert(AlerType alerType, PopupButtonType buttonType,  string title = null, string message = null, string ok = null, string cancel = null,
            bool isExitHide = false,
            bool isUseDimdClose = true,
            Action onClickOK = null,
            Action onClickCancel = null,
            Action onClickClosed = null,
            Action onClickExit = null)
        {
            Dictionary<UIOption, object> optionDict = new Dictionary<UIOption, object>();

            optionDict.Add(UIOption.EnumType, buttonType);
            optionDict.Add(UIOption.EnumType2, alerType);
            optionDict.Add(UIOption.Bool, isExitHide);
            optionDict.Add(UIOption.Bool2, isUseDimdClose);

            if (!string.IsNullOrEmpty(title))
                optionDict.Add(UIOption.Title, title);
            
            if (!string.IsNullOrEmpty(message))
                optionDict.Add(UIOption.Message, message);

            if (!string.IsNullOrEmpty(ok))
                optionDict.Add(UIOption.OkButtonText, ok);

            if (!string.IsNullOrEmpty(cancel))
                optionDict.Add(UIOption.CancelButtonText, cancel);

            if (onClickOK != null)
                optionDict.Add(UIOption.OnClickOk, onClickOK);

            if (onClickCancel != null)
                optionDict.Add(UIOption.OnClickCancel, onClickCancel);

            if (onClickExit != null)
                optionDict.Add(UIOption.OnClickExit, onClickExit);

            if (onClickClosed != null)
                optionDict.Add(UIOption.OnClickClosed, onClickClosed);

            Show<UIPopupAlert>(optionDict);
        }

        // 롤링 텍스트 보이기
        public void ShowTextMove(string message)
        {

            MaintenanceController.ShowTextMover(message);

            //Show<UITextMover>(optionDict);
        }

        // middle size popup 커스텀으로 노출(점검 공지팝업등, 최상단에 보일때)
        public void ShowAlertPopup(PopupButtonType buttonType, string message, bool isShowExit, string title,
            Action onClikcOk = null, Action onClikcCancle = null, Action onClikcClose = null)
        {
            MaintenanceController.ShowAert(buttonType, message, isShowExit, title: title, onClikcOk:onClikcOk, onClikcCancle: onClikcCancle, onClickClose: onClikcClose);
        }



        private void CheckDefaultUI(UIBase uiBase)
        {
            if (uiBase != null)
            {
                //Full 아니면서 UIPopup 일 때
                switch (uiBase._uiType)
                {
                    case UIType.Common:
                    case UIType.Popup://
                    case UIType.Top:
                        if(uiBase.UseEscClose)
                            PushUiStack(new UIStack(uiBase));
                        break;
                    case UIType.Full:
                        {
                            if (_fullUIStack.Count > 0)
                            {
                                StackPeekHide(uiBase);
                            }
                            else
                            if (_defaultUi != null && !_defaultUi.IsHide)
                                _defaultUi.Hide();

                            if (_uiStack.Count > 0)
                            {
                                foreach (var ui in _uiStack)
                                {
                                    if (ui != null && ui.UIBase != null)
                                        ui.UIBase.Hide();
                                }
                            }

                            GameManager.Instance.EnableUseController(false);
                            StackPush(uiBase);
                            if (_fullUIStack.Count > 0)
                            {
                                uiBase.transform.SetAsLastSibling();
                            }
                        }
                        break;
                    case UIType.Default:
                        _defaultUi = uiBase;
                        break;
                    default:
                        break;
                }

            }
         }


        private void UpdateActivatedUI(string layerNAme, bool isAdd)
        {
            if (isAdd)
            {
                _activatedUICount[layerNAme]++;
            }
            else
            {
                _activatedUICount[layerNAme]--;
            }
        }

        public void Close(string name, bool needCached = true)
        {
            if (!_activatedUIDict.TryGetValue(name, out var activatedUI))
                return;

            if (activatedUI == null)
                return;

            string layerName = LayerMask.LayerToName(activatedUI.gameObject.layer);
            UpdateActivatedUI(layerName, false);

            // 팝업 스택에서 제거
            if (activatedUI.UseEscClose && activatedUI._uiType != UIType.Full)
                PopUiStack(activatedUI);

            // UI 캐싱 처리
            if (!_cachedUIDict.ContainsKey(name) && needCached)
            {
                _cachedUIDict.Add(name, activatedUI);
            }

            if (activatedUI._uiType == UIType.Default)
            {
                _defaultUi = null;
            }
            else if (activatedUI._uiType == UIType.Full)
            {
                GameManager.Instance.EnableUseController(true);

                // 풀 UI 스택에서 제거
                StackPop();

                // 남은 풀 UI가 있으면 표시
                if (_fullUIStack.Count > 0)
                {
                    var nextFullUI = StackPeek();
                    //nextFullUI.Display();
                    if (nextFullUI != null)
                    {
                        int fullIndex = nextFullUI.transform.GetSiblingIndex();         //  닫으려는 UI 이전에 열었던 full UI Index

                        if (_uiStack.Count > 0)
                        {
                            var tempstack = new Stack<UIStack>(_uiStack);
                            // 활성화된 팝업들을 확인하여 표시
                            foreach (var popup in tempstack)
                            {
                                if (popup != null && popup.UIBase != null &&(popup.UIBase._uiType == UIType.Popup || popup.UIBase._uiType == UIType.Common|| popup.UIBase._uiType == UIType.Top)
                                    && popup.UIBase.UseEscClose)
                                {
                                    //if (Mathf.Abs(popup.UIBase.transform.GetSiblingIndex() - fullIndex) == 1)
                                    if (popup.UIBase.transform.GetSiblingIndex() - fullIndex == 1)
                                    {
                                        nextFullUI.Display();
                                        if(popup.UIBase.IsHide)
                                            popup.UIBase.Display();
                                        else
                                            popup.UIBase.transform.SetAsLastSibling();
                      
                                    }
                                    else
                                    {
                                        nextFullUI.Display();
                                    }
                                    //popup.UIBase.transform.SetSiblingIndex(fullIndex + 1); // 풀 UI 위에 팝업 표시
                                }
                                else
                                    nextFullUI.Display();
                            }
                        }
                        else
                            nextFullUI.Display();
                    }
                    else
                    {
                        DisplayDefaultUI();
                    }
                }
                else
                {
                    DisplayDefaultUI();
                }
            }
            else if (activatedUI._uiType == UIType.Popup)
            {
                var topUi = _uiStack.Count > 0 ? _uiStack.Peek() : null;
                if (topUi != null && topUi.UIBase == activatedUI)
                {
                    activatedUI.gameObject.SetActive(false);
                    RemoveActivatedUI(name);

                    // 남은 풀 UI가 있으면 표시
                    if (_fullUIStack.Count > 0)
                    {
                        var nextFullUI = StackPeek();
                        nextFullUI.Display();
                    }
                }
            }

            // 활성화된 UI 제거 및 비활성화 처리
            activatedUI.gameObject.SetActive(false);
            RemoveActivatedUI(name);
        }
        public void CloseAllPopup()
        {
            var dataList = _activatedUIDict.ToArray();

            foreach (var uiBase in dataList)
            {
                if (uiBase.Value._isPopup)
                    uiBase.Value.Close();
            }
        }

        //public void CloseAllUI(string ignoreUIName = null)
        public void CloseAllUI(params string[] ignoreUINames)
        {
            // bool needIgnoreCheck = !string.IsNullOrEmpty(ignoreUIName);
            bool needIgnoreCheck = ignoreUINames != null && ignoreUINames.Length > 0;

            
            while (_fullUIStack.Count > 0)
            {
                StackPeek().Close();
            }
            
            var data = _activatedUIDict.Keys.ToArray();
            //if (data.Length > 0)
            //{
            //    if (LobbyManager.instance != null)
            //        LobbyManager.instance.HideLobbyBG();
            //}
            //if (LobbyController.Instance != null)
            //    LobbyController.Instance.HideLobbyBG();
            for (int i = 0; i < data.Length; i++)
            {
                //if (needIgnoreCheck && data[i].Equals(ignoreUIName))
                //    continue;

                //if (_activatedUIDict.TryGetValue(data[i], out var ui))
                //    ui.Close();
                if (needIgnoreCheck && ignoreUINames.Contains(data[i]))
                    continue;

                if (_activatedUIDict.TryGetValue(data[i], out var ui))
                {
                    ui.Close();
                }
                    
            }

            for (int i = 0; i < _notCachedUIList.Count; i++)
            {
                Destroy(_notCachedUIList[i].gameObject);
            }

            _notCachedUIList.Clear();
        }

        public void CleanseAllUI()
        {
            _activatedUIDict.Clear();
            _cachedUIDict.Clear();

            var keys = _canvasDict.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                int childCount = _canvasDict[keys[i]].transform.childCount;

                for (int j = 0; j < childCount; j++)
                {
                    Destroy(_canvasDict[keys[i]].transform.GetChild(0).gameObject);
                }
            }
        }

        public bool Loaded<T>() where T : UIBase
        {
            string name = typeof(T).Name;

            return _cachedUIDict.ContainsKey(name);
        }

        public T GetUI<T>() where T : UIBase
        {
            UIBase uiBase = null;

            string name = typeof(T).Name;

            if (_activatedUIDict.TryGetValue(name, out uiBase))
                return uiBase as T;

            return default;
        }
        public bool HasUI<T>() where T : UIBase
        {
            UIBase uiBase = null;

            string name = typeof(T).Name;

            if (_activatedUIDict.TryGetValue(name, out uiBase))
                return true;
            if (_cachedUIDict.TryGetValue(name, out uiBase))
                return true;
            return false;
        }

        public UIBase GetUIByName(string name)
        {
            UIBase uiBase = null;
            if (_activatedUIDict.TryGetValue(name, out uiBase))
                return uiBase;
            return default;
        }

        public void ClearStack()
        {
            int count = _fullUIStack.Count;

            for (int i = 0; i < count; i++)
            {
                var ui = StackPeek();

                if (ui != null)
                    ui.Close();
            }
        }

        public void Clear()
        {
            if (_fullUIStack != null && _fullUIStack.Count != 0)
            {
                foreach (var uiBase in _fullUIStack)
                {
                    Destroy(uiBase.gameObject);
                    ResourceManager.Instance.ForceRelease(uiBase.UIName);
                    
                }
                _fullUIStack.Clear();
            }
            if (_uiStack != null || _uiStack.Count != 0)
            {
                
                foreach (var uiBase in _uiStack)
                {
                    Destroy(uiBase.Obj);
                    ResourceManager.Instance.ForceRelease(uiBase.UIBase.UIName);
                }
            }
            _uiStack.Clear();

            List<string> removes = new List<string>();
            foreach (var cached in _cachedUIDict)
            {
                switch (cached.Key)
                {
                    case "UIBlock":
                    case "UILoading":
                    case "UIPvpLoading":
                        continue;
                        break;
                } 
                
                //Debug.LogError("흉진박멸"  + cached.Key);
                removes.Add(cached.Key);
                Destroy(cached.Value.gameObject);
                if (cached.Value != null)
                {
                    ResourceManager.Instance.ForceRelease(cached.Value.UIName);
                }
                
            }

            foreach (var remove in removes)
            {
                _cachedUIDict.Remove(remove);
                _activatedUIDict.Remove(remove);
            }
            removes.Clear();
            
        }

        public void GoBack()
        {
            var topMenu = GetUI<UITopMenu>();

            // 전투 씬이 아닌 경우 홈으로 돌아가는 처리
            if (SceneManager.Instance.SceneState != SceneState.CombatScene &&
                SceneManager.Instance.SceneState != SceneState.PvPScene &&
                SceneManager.Instance.SceneState != SceneState.PvPLineScene &&
                (_defaultUi == null || (_fullUIStack.Count == 0 && _uiStack.Count == 0)))
            {
                GoHome();
                return;
            }

            var fullUiStack = StackPeek();

            if (_uiStack.Count > 0)
            {
                var popup = PeekUiStack();

                if (popup == null || popup.UIBase == null)
                {
                    popup.Close();
                    return;
                }

                // 2024.11.26 동연 추가
                if (fullUiStack == null)
                {
                    popup.Close();
                    return;
                }


                int popupSiblingIndex = popup.UIBase.transform.GetSiblingIndex();
                int fullUiSiblingIndex = fullUiStack.transform.GetSiblingIndex();
                if (LayerMask.LayerToName(popup.UIBase.transform.gameObject.layer) == LayerMask.LayerToName(fullUiStack.transform.gameObject.layer))
                {
                    if (fullUiStack != null)
                    {
                        if (popup.GetSiblingIndex() > fullUiStack.transform.GetSiblingIndex())
                        {
                            if (popup != null)
                            {
                                popup.Close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (popup != null)
                        {
                            popup.Close();
                            return;
                        }
                    }
                }
                else
                {
                    if (popup != null)
                    {
                        popup.Close();
                        return;
                    }
                }

            }

            if(SceneManager.Instance.SceneState == SceneState.CombatScene)
            {
                fullUiStack = StackPeek();

                if (fullUiStack != null)
                    fullUiStack.GoBack();

                    return;
            }
            else
                StackPeek().GoBack();

            if (_fullUIStack != null && _fullUIStack.Count == 0)
            {
                GameInfoManager.Instance.IsSkipInteractionSounde = false;
                DisplayDefaultUI();
            }

            if (_fullUIStack != null && _fullUIStack.Count > 0)
            {
                StackPeek().Refresh();

                //if (topMenu != null)
                //{
                //    topMenu.transform.SetAsLastSibling();
                //    topMenu.AllTicketsHide();
                //    Dictionary<UIOption, object> options = Utils.GetUIOption(UIOption.List, StackPeek().TicketType, UIOption.EnumType, StackPeek().HelpLocationType);
                //    Show<UITopMenu>(options);
                //}
            }
            else
            {
                //_defaultUi.Refresh();

                if (topMenu != null && topMenu.gameObject.activeSelf)
                {
                    GoHome();
                }
            }
        }

        public void GoHome()
        {
            ClearStack();
            if (_uiStack.Count > 0)
            {
                for (int n = 0; n< _uiStack.Count; n++)
                {
                    var stack = PeekUiStack();
                    stack.Close();
                }
            }
            if (SceneManager.Instance.SceneState == SceneState.LobbyScene)
            {
                GameInfoManager.Instance.IsSkipInteractionSounde = false;
                DisplayDefaultUI();

                if(_defaultUi != null)
                    _defaultUi.Refresh();
            }
            else
                SceneManager.Instance.ChangeSceneState(SceneState.LobbyScene, true);

            //. UITopMenu 켜져 있으면 꺼 주기.
            var topMenu = GetUI<UITopMenu>();
            if (topMenu != null && topMenu.gameObject.activeSelf)
            {
                topMenu.Close();
            }
        }

        public void GoContent(string stageTid, Action callback = null,bool waitDim = false)
        {
            Action goContent = null;
            var stageData = TableManager.Instance.Stage_Table[stageTid];
            
            switch (stageData.CONTENTS_TYPE_ID)
            {
                case CONTENTS_TYPE_ID.stage_main:
                case CONTENTS_TYPE_ID.stage_sub:
                case CONTENTS_TYPE_ID.stage_character:
                    goContent = () =>
                    {
                       Show<UIChapter>();
                        var stageinfo = TableManager.Instance.Stage_Table[stageTid];
                        Show<UIStage>(
                            Utils.GetUIOption(
                                UIOption.Index, stageinfo.Theme_Id,
                                UIOption.Data, stageinfo
                             ));
               
                    };
                    break;
                case CONTENTS_TYPE_ID.manjang:
                    goContent = () =>
                        {
                            Stage_TableData current = null;
                            var total = TableManager.Instance.Stage_Table.GetDatas(stageData.CONTENTS_TYPE_ID);
                            var clearlist = GameInfoManager.Instance.DungeonInfo.GetClearList(stageData.CONTENTS_TYPE_ID);

                            if (clearlist.Count > 0)
                            {
                                // 클리어 리스트가 있는 경우. 
                                var lastClearInfo = TableManager.Instance.Stage_Table[clearlist.LastOrDefault()];
                                if (!lastClearInfo.Tid.Equals(total.LastOrDefault().Tid))
                                {
                                    // 클리어 스테이지 중 마지막 스테이지가, 해당 컨텐츠의 마지막 스테이지가 아닌 경우
                                    var info = TableManager.Instance.Stage_Table.GetData(stageData.CONTENTS_TYPE_ID, lastClearInfo.Stage_Id);
                                    current = info;
                                }
                                else
                                    current = lastClearInfo;
                            }
                            else
                                current = total.FirstOrDefault();

                            Show<UIContent>();
                            Show<UIDungeon>(Utils.GetUIOption(UIOption.EnumType, stageData.CONTENTS_TYPE_ID, UIOption.Data, current));
                        };
                    break;
                case CONTENTS_TYPE_ID.pvp:
                    goContent = () =>
                    {
                        Show<UIContent>();
                        if (TimerManager.Instance.IsEndTimer(TimerKey.PvpBattleTimer))
                            RestApiManager.Instance.RequestGetPvpList(0, () => { Show<UIPvP>(); });
                        else
                            Show<UIPvP>();
                    };
                    break;
                case CONTENTS_TYPE_ID.prologue:
                    break;
                case CONTENTS_TYPE_ID.elemental:
                    goContent = () =>
                    {
                        Show<UIContent>();
                        var themeIDs = TableManager.Instance.Stage_Table.GetThemeIds(stageData.CONTENTS_TYPE_ID);
                        int tabIndex = 0;

                        for (int i = 0; i < themeIDs.Count; i++)
                        {
                            if (themeIDs[i] == stageData.Theme_Id)
                            {
                                tabIndex = i;
                                break;
                            }    
                        }

                        Show<UIElementalDungeon>(Utils.GetUIOption(UIOption.Index, tabIndex));
                    };
                    break;
                case CONTENTS_TYPE_ID.exp:
                    goContent = () => {
                        Show<UIContent>();
                        Show<UIExpDungeon>(); 
                    };
                    break;
                case CONTENTS_TYPE_ID.gold:
                    goContent = () =>{
                        Show<UIContent>();
                        Show<UIGoldDungeon>(); 
                    };
                    break;
                case CONTENTS_TYPE_ID.total:
                    goContent = () =>
                    {
                        RestApiManager.Instance.RequestTotalGetData(LEADERBOARDID.Total, () =>
                        {
                        
                            //시간 만료 예외처리 
                            if (GameInfoManager.Instance.TotalInfo.IsExpired())
                                GameInfoManager.Instance.TotalInfo.GiveUp(() =>
                                {
                                    Show<UIContent>();
                                    Show<UITotal>();
                                });
                            else
                            {
                                Show<UIContent>();
                                Show<UITotal>();
                            }
                        });
                    };
                    break;
                case CONTENTS_TYPE_ID.challenge:
                case CONTENTS_TYPE_ID.trial:
                    goContent = () =>
                    {
                        Show<UIContent>();
                        FadeController.StartFadeRoutine(1, 0, 0, 0, () =>
                        {
                            RestApiManager.Instance.RequestTotalGetData(stageData.CONTENTS_TYPE_ID.ToLeaderboardId(), () =>
                            {
                                Show<UIChallenge>(Utils.GetUIOption(UIOption.EnumType, stageData.CONTENTS_TYPE_ID.ToLeaderboardId()));
                            });
                            FadeController.StartFadeRoutine(0, 0.8f, 1,0, () => { });
                        });

                    };
                    break;
                case CONTENTS_TYPE_ID.event_main:
                    goContent = () =>
                    {
                        var stageinfo = TableManager.Instance.Stage_Table[stageTid];
                        var eventinfo = TableManager.Instance.Event_Story_Table.GetEventData(stageinfo.Theme_Id);
                        Show<UIEventMain>(Utils.GetUIOption(UIOption.Tid, eventinfo.Event_Connect_Tid));
                        int index = 0;
                        if (stageinfo.LEVEL_DIFFICULTY == LEVEL_DIFFICULTY.normal  &&stageinfo.LEVEL_TYPE == LEVEL_TYPE.story)
                        {
                            index = 0;
                        }
                        else
                        {
                            index = (int)stageinfo.LEVEL_DIFFICULTY +1;
                        }
                        Show<UIEventStage>(
                            Utils.GetUIOption(
                                UIOption.Tid, eventinfo.Event_Stage,
                                UIOption.Index, index));
                    };
                    break;
                case CONTENTS_TYPE_ID.character_guide:
                    goContent = () =>
                    {
                        this.Show<UICharacterGuide>();
                    };
                    break;
                case CONTENTS_TYPE_ID.content_class:
                    goContent = () =>
                    {
                        Show<UIContent>();
                        var index = 1;
                        var splitTid = stageData.Tid.Split("_");

                        for(int i = 0; i < splitTid.Length; i++)
                        {
                            var tid = splitTid[i];

                            if (int.TryParse(tid, out index))
                                break;
                        }

                        this.Show<UIPositionDungeon>(Utils.GetUIOption(UIOption.Index, index - 1));
                    };
                    break;
                case CONTENTS_TYPE_ID.relay_boss:
                    goContent = () =>
                    {
                        Show<UIContent>();
                        var stageinfo = TableManager.Instance.Stage_Table[stageTid];

                        Show<UIBossDungeon>(Utils.GetUIOption(UIOption.Int, stageinfo.Theme_Id));
                    };
                    break;
                default:
                    break;
            }

            if (SceneManager.Instance.SceneState != SceneState.LobbyScene)
            {
                SetBaseUIMainCamera();
                callback?.Invoke();
                SceneManager.Instance.IsGoContent = true;
                SceneManager.Instance.AddEvent(SceneState.LobbyScene, SceneEventType.LoadCompleted, goContent, true);
                //SceneManager.Instance.ChangeSceneState(SceneState.LobbyScene,true,false,waitDim);
                SceneManager.Instance.ChangeSceneState(SceneState.LobbyScene, true, false, waitDim);
            }
            else
            {
                callback?.Invoke();
                
                goContent?.Invoke();
            }
        }

        public void ActivateCanvas(bool isActive)
        {
            var keys = _canvasDict.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                _canvasDict[keys[i]].gameObject.SetActive(isActive);
            }
        }

        private void DisplayDefaultUI()
        {
            if (_defaultUi != null && _defaultUi.IsHide)
            {
                _defaultUi.Display();
            }
        }

        public void PlayDim(DimData data)
        {
            _dimController.Dim(data);
        }

        public void EnableUseController()
        {

        }

        public void ShowToastNotImplement()
        {
            ShowToastMessage("str_content_update_lock_01"); //추후 업데이트 예정입니다.
        }

        //기본 메뉴들 Show
        public void ShowMenu()
        {
            //Show<UIHamburger>();
            //Show<UIQuestAcceptList>();
        }

        public void ShowSpeech(string receiveMessage)
        {
            if (!_speechController.gameObject.activeInHierarchy)
                _speechController.gameObject.SetActive(true);

            _speechController.StopRoutine();
            _speechRoutines.Clear();

            var receiveDatas = TableManager.Instance.Speech_Table.GetReceiveDatas(receiveMessage);

            for (int i = 0; i < receiveDatas.Count; i++)
            {
                List<Speech_TableData> nextDataList = new List<Speech_TableData>();

                nextDataList.Add(receiveDatas[i]);
                var nextDatas = TableManager.Instance.Speech_Table.GetNextSpeechDatas(receiveDatas[i].Tid);
                nextDataList.AddRange(nextDatas);

                _speechRoutines.Add(StartCoroutine(_speechController.SpeechRoutine(nextDataList)));
            }

        }

        public void ShowSpeechByTid(string tid)
        {
            if (!_speechController.gameObject.activeInHierarchy)
                _speechController.gameObject.SetActive(true);

            _speechController.StopRoutine();
            _speechRoutines.Clear();

            var datas = new List<Speech_TableData>();
            var data = TableManager.Instance.Speech_Table[tid];

            datas.Add(data);

            _speechRoutines.Add(StartCoroutine(_speechController.SpeechRoutine(datas)));
        }

        //결과창 뒤에 보이는 ui 끄기
        public void CloseSpeech()
        {
            for (int i = 0; i < _speechRoutines.Count; i++)
            {
                if(_speechRoutines[i]!=null)
                StopCoroutine(_speechRoutines[i]);
            }

            _speechRoutines.Clear();
            _speechController.StopRoutine();
        }

        public void ShowRewardItem(List<ItemCellData> datas, Action endCallback = null)
        {
            Show<UIRewardList>(Utils.GetUIOption(UIOption.List, datas, UIOption.Callback, endCallback));
        }

        //서버 요청 기다리는 동안 터치 막는 용
        public void ShowUIBlock()
        {
            if (_uiBlock != null && _uiBlock.gameObject.activeInHierarchy)
                return;

            if (SceneManager.Instance.SceneState != SceneState.CombatScene)
            {
                Show<UIBlock>();

                if (_uiBlock == null)
                {
                    var ui = GetUI<UIBlock>();

                    if (ui != null)
                        _uiBlock = ui;
                }
            }
        }

        public void CloseUIBlock()
        {
            if (RestApiManager.Instance.RestQueueCount() == 0)
                Close<UIBlock>();
        }
        public void ShowUIBlockStore()
        {
            if (_uiBlockStore != null && _uiBlockStore.gameObject.activeInHierarchy)
                return;

            if (SceneManager.Instance.SceneState != SceneState.CombatScene)
            {
                Show<UIBlock_Store>();

                if (_uiBlockStore == null)
                {
                    var ui = GetUI<UIBlock_Store>();

                    if (ui != null)
                        _uiBlockStore = ui;
                }
            }
        }
        public void CloseUIBlockStore()
        {
            Close<UIBlock_Store>();
        }

        public void ChangeSkinPageUI(bool isChange, Camera changeCam)
        {
            var illustCanvas = _canvasDict[UICanvasType.UIIllust.ToString()];
            if (isChange)
            {
                illustCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                illustCanvas.worldCamera =_skinCamera;
            }
            else
            { 
                var refreshData = _canvasDict[UICanvasType.UI.ToString()];
                illustCanvas.renderMode = refreshData.renderMode;
                illustCanvas.sortingOrder = -10;
            }
        }
        public void LobbyClear()
        {
            return;
         
            List<string> destroyKeyList = new List<string>();
            //_activatedUIDict
            foreach (var cacheObj in _cachedUIDict)
            {
                
                switch (cacheObj.Value)
                {
                    case UILobby uiLobby:
                    case UICharacter uiCharacter:
                    case UICharacterBg uiCharacterBg:
                    //case UIIllust uiIllust:
                    case UIQuest uiQuest:
                    case UITopMenu uiTopMenu:
                    case UIStore uiStore:
                    case UIDosaTalk uiDosaTalk:
                    case UIContent uiContent:
                    case UIChapter uiChapter:
                    case UISeasonPass uiDosaPass:
                    case UIEventMain uiEventMain:
                    case UIEventQuest uiEventQuest:
                    case UIEventStore uiEventStore:
                    case UIEventStage uiEventStage:
                    case UIEventBonusCharacter uiEventBonusCharacter:
                    case UIHelper uiHelper:
                    case UIMenu uiMenu:
                    case UIPost uiPost:
                    case UIEventPopup uiEventPopup:
                    case UICraftCookBg uiCraftCookBg:
                    case UIInventory uiInventory:
                    case UIPopupPurchaseGoods uiPopupPurchaseGoods:
                    case UILobbySettingPreview uiLobbySettingPreview:
                    case UIAttendance uiAttendance:
                    case UIStory uiStory:
                        destroyKeyList.Add(cacheObj.Key);
                   
                        Destroy(cacheObj.Value.gameObject);
                        ResourceManager.Instance.ForceRelease(cacheObj.Key);
                        break;

                }
            }
     
            foreach (var d in destroyKeyList)
            {
                _cachedUIDict.Remove(d);
            }
        }
    }
}
