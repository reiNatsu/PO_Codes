using Consts;
using DebuggingEssentials;
using ES3Types;
using LIFULSE.Manager;
using NiloToon.NiloToonURP;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TouchAreaController : MonoBehaviour
{

    public const string HeadTouchArea = "HeadTouchArea";
    public const string BreastTouchArea = "BreastTouchArea";
    public const string NormalTouchArea = "NormalTouchArea";

    [SerializeField] private Camera _camera;

    private bool _isTouchActive = true;

    public Camera Camera
    {
        get => _camera;
    }

    private HashSet<string> _loadeds = new HashSet<string>();

    private string _soundKey;
    private List<GraphicRaycaster> _canvasList;

    
    private PointerEventData _pointerEventData = new PointerEventData(null);

  
    private TouchAreaObject _touchAreaObject;

    public TouchAreaObject TouchAreaObject
    {
        get => _touchAreaObject;
        set => _touchAreaObject = value;
    }

#if UNITY_EDITOR


#endif


    public Vector3 GetTouchAreaPivot
    {
        get => _touchAreaObject.Pivot.transform.localPosition;
    }

    public bool IsTouchActive { get => _isTouchActive; set => _isTouchActive = value; }


    public void AllSoundOff()
    {
        if (!string.IsNullOrEmpty(_soundKey))
        {
            AudioManager.Instance.StopAudio(_soundKey);
            _soundKey = null;
        }
    }
 
    public void PlayLobbyCharacterAnimation(CharacterMainMenuType key)
    {
        TouchAreaObject.SetTrigger(key);
        var interactionData = GetInteractionData(true);
        if (interactionData != null)
        {
            CustomLobbyManager.Instance.ActiveTalkBoxUI(interactionData.Talk_Str, interactionData.Interaction_Time);
            if (!GameInfoManager.Instance.IsSkipInteractionSounde)
            {
                var type = GameInfoManager.Instance.GetSavedSoundLan();
                var audioClip = AudioManager.Instance.LocalizeAudioManager.GetCachedData(interactionData.Talk_Sound, type);
                _soundKey = audioClip;
                AudioManager.Instance.PoolPlayAudio(_soundKey);
                if(_loadeds.Contains(_soundKey))
                _loadeds.Add(_soundKey);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var loaded in _loadeds)
        {
            AudioManager.Instance.DisposeAudio(loaded);
        }
    }

    public void AnimatorRebind()
    {
        TouchAreaObject.AnimatorRebind();
    }

    void Start()
    {
        _canvasList = new List<GraphicRaycaster>();
        if (UIManager.Instance != null)
        {
            _canvasList.Add(UIManager.Instance.GetCanvas(UICanvasType.UIPopup).GetComponent<GraphicRaycaster>());
            _canvasList.Add(UIManager.Instance.GetCanvas(UICanvasType.UITop).GetComponent<GraphicRaycaster>());
            _canvasList.Add(UIManager.Instance.GetCanvas(UICanvasType.UILobby).GetComponent<GraphicRaycaster>());
            _canvasList.Add(UIManager.Instance.GetCanvas(UICanvasType.UI).GetComponent<GraphicRaycaster>());
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!_isTouchActive)
            return;

        if (Input.GetMouseButtonDown(0) || ControllerCombiner.GetCursorDown())
        {
            var uiSkinPage = UIManager.Instance.GetUI<UISkinPage>();
            if (uiSkinPage != null && uiSkinPage.gameObject.activeInHierarchy)
                return;

            var mousePosition = Input.mousePosition;

            if (InputManager.Instance.EventSystem.currentInputModule != null)
                mousePosition = InputManager.Instance.EventSystem.currentInputModule.input.mousePosition; // 패드일 경우 커서 포지션 받아옴

            Ray ray = _camera.ScreenPointToRay(mousePosition);
            //RaycastHit hit;
            RaycastHit[] hits = Physics.RaycastAll(ray, 100.0f);
            var sortedHits = hits.OrderBy(hit =>
            {
                if (hit.transform.name == HeadTouchArea) return 0; // 가장 높은 우선순위
                if (hit.transform.name == BreastTouchArea) return 1; // 두 번째 우선순위
                if (hit.transform.name == NormalTouchArea) return 2; // 세 번째 우선순위
                return 3; // 기타
            });

            foreach (var hit in sortedHits)
            {
                HandleTouchArea(hit);
                break; // 가장 높은 우선순위만 처리
            }

            if (UIManager.Instance.GetUI<UILobby>() != null && UIManager.Instance.GetUI<UILobby>().gameObject.activeInHierarchy)
            {
                var lobby = UIManager.Instance.GetUI<UILobby>();
                if (lobby.IsLobbyHide)
                    lobby.OnClickShowLobbyShowBtn();
            }
          
        }
        //레이를 여러번 쏴야함...
    }

    void HandleTouchArea(RaycastHit hit)
    {
        if (hit.transform.name == HeadTouchArea || hit.transform.name == BreastTouchArea  || hit.transform.name == NormalTouchArea)
        {
            var touchArea = hit.transform.GetComponent<TouchArea>();
            if (touchArea != null)
            {
                _pointerEventData.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                //touchArea.StopAudio();
                foreach (var canvas in _canvasList)
                {
                    canvas.Raycast(_pointerEventData, results);
                }
                if (results.Count == 0)
                {
                    //touchArea.PlayAnimation();
                    //touchArea.StopAudio();
                    //touchArea.PlayAudio();
                    // SetInteractionBox(hit.transform.name);          // 캐릭터 대사 
                    if (GetInteractionData(false, hit.transform.name) == null)
                    {
                        return;
                    }
                    else
                    {
                        //welcome sound 있으면 꺼주기
                        if (!string.IsNullOrEmpty(_soundKey))
                        {
                            AudioManager.Instance.StopAudio(_soundKey);
                            _soundKey = null;
                        }
                        var interactionData = GetInteractionData(false, hit.transform.name);
                        if (interactionData != null)
                        {
                            CustomLobbyManager.Instance.ActiveTalkBoxUI(interactionData.Talk_Str, interactionData.Interaction_Time);
                            _soundKey = interactionData.Talk_Sound;
                            touchArea.PlayAudio(interactionData.Talk_Sound);
                            _soundKey = touchArea.SoundKey;
                            CharacterMainMenuType touchtype = CharacterMainMenuType.none;
                            if (!string.IsNullOrEmpty(interactionData.Charactermainmenutype))
                                touchtype = (CharacterMainMenuType)Enum.Parse(typeof(CharacterMainMenuType), interactionData.Charactermainmenutype);
                            _touchAreaObject.SetTrigger(touchtype);
                        }
                        
                    }
                    if (hit.transform.name == NormalTouchArea)
                        AnimatorRebind();
                }
            }
          
        }
    }

    // 20241224 동연 - 캐릭터 인터렉션 대사창 노출
    public void SetInteractionBox(string touchtype)
    {
        var interactionData = GetInteractionData(false,touchtype);
         CustomLobbyManager.Instance.ActiveTalkBoxUI(interactionData.Talk_Str,interactionData.Interaction_Time);
    }

    public string GetCharacterId(string name)
    {
        string originalName = name;
        int index = originalName.IndexOf("(Clone)");

        if (index != -1)
            return originalName.Substring(0, index);
        else
            return originalName;
    }

    public Interaction_Data_TableData GetInteractionData(bool isWelcome, string touchtype = null)
    {
        Interaction_Data_TableData data = null;
        //string charactername = CustomLobbyManager.Instance.GetCurrentCharacter();

        string charactername = GetCharacterId(_touchAreaObject.CharacterName);
        //Debug.Log("<color=#ff00ff> 현재 로비 캐릭터 : "+charactername+" / 클릭 한 부위 : "+touchtype+"</color>");

        DosaInfo currentDosa = GameInfoManager.Instance.CharacterInfo.GetDosa(charactername);
        INTERACTION_PARENTS_TYPE currenttype = INTERACTION_PARENTS_TYPE.defalt;
        if (!isWelcome)
        {
            if (string.IsNullOrEmpty(touchtype))
                return null;

            INTERACTION_PARTS touchpart = INTERACTION_PARTS.none;
            switch (touchtype)
            {
                case "HeadTouchArea":
                    touchpart = INTERACTION_PARTS.head;
                    break;
                case "BreastTouchArea":
                    touchpart = INTERACTION_PARTS.boobs;
                    break;
                default:
                    touchpart = INTERACTION_PARTS.none;
                    break;
            }

            if (currentDosa != null && currentDosa.SpecialTouch == 1)
                currenttype= INTERACTION_PARENTS_TYPE.special;
            var interactions = TableManager.Instance.Interaction_Data_Table.GetTouchData(charactername, touchpart, currenttype);
            if (interactions.Count > 0)
            {
                var randomIndex = UnityEngine.Random.Range(0, interactions.Count);
                data = interactions[randomIndex];
            }
        }
        else
        {

            if (currentDosa != null && currentDosa.SpecialTouch == 1)
                currenttype= INTERACTION_PARENTS_TYPE.special;
            Debug.Log("<color=#ff00ff> 현재 로비 캐릭터 : "+charactername+" / 현재 터치 타입 : "+currenttype.ToString()+"</color>");
            var interactions = TableManager.Instance.Interaction_Data_Table.GetWelcomeData(charactername, currenttype);
            if (interactions.Count > 0)
            {
                var randomIndex = UnityEngine.Random.Range(0, interactions.Count);
                data = interactions[randomIndex];
            }
            else
                data = null;
        }

        return data;
    }
}
