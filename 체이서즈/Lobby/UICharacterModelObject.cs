using DebuggingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterModelObject : MonoBehaviour
{
    [SerializeField]
    private UICharacterModelCameraSetting _cameraSettingDict;

    [SerializeField] private Transform _modelPivot;
    public Transform ModelPivot
    {
        set
        {
            _modelPivot = value;
        }
        get
        {
            
            return _modelPivot;
        }
    }

    private Animator _anim;
    private Animator Anim
    {
        get
        {
            if (_anim == null)
            {
                _anim = GetComponent<Animator>();
            }

            return _anim;
        }
    }

    public void Rebind()
    {
        //Anim.Rebind();
        var enums = System.Enum.GetNames(typeof(CharacterMainMenuType));
        for (int i = 0; i < enums.Length; i++)
        {
            Anim.ResetTrigger(enums[i]);
        }
        Anim.Play("Idle");
    }

    public void SetTrigger(string key)
    {
        Anim.SetTrigger(key);
    }

    public void PlayAnimationToTab(CharacterMainMenuType type)
    {
        switch(type)
        {
            case CharacterMainMenuType.Info:
            case CharacterMainMenuType.Skill:
            case CharacterMainMenuType.Equip:
            case CharacterMainMenuType.Limit:
                PlayAnimation("Idle");
                break;
            case CharacterMainMenuType.LikeAbility:
                PlayAnimation("LikeAbility");
                break;
        }
    }

    public void PlayAnimation(string animString)
    {
        Anim.Play(animString);
    }


    public void SetActive(bool isOn)
    {
        gameObject.SetActive(isOn);
    }

    public CameraSetting GetCameraDict(CharacterMainMenuType type)
    {
        if (_cameraSettingDict.ContainsKey(type))
        {
            return _cameraSettingDict[type];
        }
        else
        {
            return null;
        }
    }
}
