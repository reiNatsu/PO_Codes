using LIFULSE.Manager;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UICharacterModelObject))]
public class TouchAreaObject : MonoBehaviour
{
    [SerializeField] private GameObject[] _hideObjs;
    //[SerializeField] private AudioSource _source;
    //[SerializeField] private AudioClip _wellcomeClip;
    [SerializeField] private UICharacterModelObject _characterModelObject;

    [SerializeField] private string _characterName;

    public UICharacterModelObject CharacterModelObject
    {
        get => _characterModelObject;
    }

    public string CharacterName { get => _characterName; }

    private string _neckPath1 =
        "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck";
    private string _neckPath2 =
        "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck";

    private string _headPath1 =
        "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head";
    private string _headPath2 =
        "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 Head";

    private string _spine1 = "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1";
    private string _spine2 = "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2";
    [SerializeField] private TouchArea _head;
    [SerializeField] private TouchArea _breast;
    [SerializeField] private TouchArea _normal;

    [SerializeField] private Transform _pivot;

    public Transform Pivot
    {
        get => _pivot;
    }
    [OnInspectorInit]
    void Init()
    {
        //_source = GetComponent<AudioSource>();
        if (_head == null)
        {
            var newObj = new GameObject(TouchAreaController.HeadTouchArea);
            var haedTran = transform.Find(_headPath1);
            if (haedTran == null)
            {
                haedTran = transform.Find(_headPath2);
            }
            newObj.transform.SetParent(haedTran);
            

            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localRotation = Quaternion.identity;
            newObj.transform.localScale = Vector3.one;
            _head = newObj.AddComponent<TouchArea>();
            var collider = _head.gameObject.AddComponent<SphereCollider>(); 
            collider.isTrigger = true;
            collider.center = new Vector3(-0.1f, 0, 0);
            collider.radius = 0.2f;
            
        }

        if (_breast == null)
        {
            var newObj = new GameObject(TouchAreaController.BreastTouchArea);
            newObj.transform.SetParent(transform.Find(_spine1));
            Transform spine1 = transform.Find(_spine1);
            Transform neck = transform.Find(_neckPath1);
            if (neck == null)
            {
                neck = transform.Find(_neckPath2);
            }

          
            newObj.transform.position = Vector3.Lerp(spine1.position, neck.position, .5f);
            newObj.transform.localRotation = Quaternion.identity;
            newObj.transform.localScale = Vector3.one;
            _breast = newObj.AddComponent<TouchArea>();
            var collider = _breast.gameObject.AddComponent<BoxCollider>(); 
            collider.isTrigger = true;
            collider.size = new Vector3(0.3f,0.4f,0.4f);
        }

        if (_normal == null)
        {
            var newObj = new GameObject(TouchAreaController.NormalTouchArea);
            Transform spine1 = transform.Find(_spine1);

            if (spine1 == null)
            {
                Debug.LogError($"Cannot find {_spine1} in the transform hierarchy.");
                return;
            }
            newObj.transform.SetParent(transform.Find(_spine1));

            newObj.transform.position = spine1.position;
            newObj.transform.localRotation = Quaternion.identity;
            newObj.transform.localScale = Vector3.one;
            _normal = newObj.AddComponent<TouchArea>();
            var collider = _normal.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            Renderer spineRenderer = spine1.GetComponent<Renderer>();
            if (spineRenderer != null)
            {
                collider.size = spineRenderer.bounds.size;
                collider.center = spineRenderer.bounds.center - spine1.position;
            }
            else
            {
                // 기본 크기 설정 (모델링 크기를 찾을 수 없을 경우)
                collider.size = new Vector3(1.6f, 0.4f, 0.4f);
                collider.center = new Vector3(0.25f, 0, 0);
            }
        }
        else
        {
            // 이미 있는 경우는 혹시 모르니 위치 수정
            var colider = _normal.GetComponent<BoxCollider>();
            //var targetSize = new Vector3(1.6f, 0.4f, 0.4f);
            //var targetCenter = new Vector3(0.25f, 0, 0);
            //if(colider.size != targetSize)
            //    colider.size = targetSize;
            //if(colider.center != targetCenter)
            //    colider.center = targetCenter;
        }

        if (_pivot == null)
        {
            var newObj = new GameObject("Pivot");
            newObj.transform.SetParent(transform);
            newObj.transform.position = _breast.transform.position;
            newObj.transform.localRotation = Quaternion.identity;
            newObj.transform.localScale = Vector3.one;
            _pivot = newObj.transform;
        }

        if (_characterModelObject == null)
        {
            _characterModelObject = GetComponent<UICharacterModelObject>();
        }

        if (_characterModelObject != null)
        {
            _characterModelObject.ModelPivot = _pivot;
        }

        // 캐릭터 tid 세팅
        string objectname = _characterModelObject.name;
        string prefixtoremove = "FAB_MODEL_";

        _characterName = GetCharacterTid(objectname, prefixtoremove);
    }

    private void Awake()
    {
        foreach (var hideObj in _hideObjs)
        {
            hideObj.SetActive(false);
        }
    }

    public void SetTrigger(CharacterMainMenuType key)
    {
        /*if (key == CharacterMainMenuType.Welcome)
        {
            if (_wellcomeClip != null && _source != null && !GameInfoManager.Instance.IsSkipInteractionSounde)
            {
                _source.clip = _wellcomeClip;
                _source.Play();
            }
        }*/
        SetTrigger(key.ToString());
    }
    public void SetTrigger(string key)
    {
        if(_characterModelObject!=null)
        _characterModelObject.SetTrigger(key);
    }
    public void AnimatorRebind()
    {
        _characterModelObject.Rebind();
    }


    // 20241226 동연 추가
    private string GetCharacterTid(string input, string prefix)
    {
        string original = input;
        if (input.StartsWith(prefix))
        {
            string re = input.Substring(prefix.Length);
            string[] parts = re.Split('_');
            original = string.Join("_", parts.Length > 3 ? parts[..3] : parts);
        }

        return original;
    }


    public void StopAllAudio()
    {
        _head.StopAudio();
        _breast.StopAudio();
        _normal.StopAudio();
    }
}
