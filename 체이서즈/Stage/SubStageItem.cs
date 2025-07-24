using Consts;
using LIFULSE.Manager;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

public class SubStageItem : MonoBehaviour
{
    [SerializeField] private GameObject _dimd;
    [SerializeField] private ExImage  _star;
    [SerializeField] private ExTMPUI _name;

    private Stage_TableData _data;

    private Animator _animator;

    private int _mainstageno = 0; 

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        this.gameObject.SetActive(true);
        SetSubStageDimd(true);
    }

    public void InitData(Stage_TableData data, int mainstagemo)
    {
        _data = data;
        _star.enabled = false;
        _mainstageno = mainstagemo;
        //SetSubStageDimd(true);
        UpdateUI();
    }

    public void UpdateUI()
    {
        // 별 체크
        var stars = GameInfoManager.Instance.ClearStarCount(_data.Tid);
        if (!stars.IsNullOrEmpty())
        {
            if (stars[0] == 1)
            {
                _star.enabled = true;
            }
            else
            {
                _star.enabled = false;
            }
        }

        //_name.text = "EX_"+_data.Extra_Id;
        _name.ToTableText("str_stage_sub_number_default", _data.Extra_Id);          // Sub {0}
    }

    public void PlayAnimation()
    {
        _animator.Rebind();
        _animator.enabled = true;
    }

    public void SetSubStageDimd(bool isOn)
    {
        _dimd.SetActive(isOn);
    }

    public void OnClickStage()
    {
        //var uiStage = UIManager.Instance.GetUI<UIStage_old>();
        //if (uiStage != null && uiStage.gameObject.activeInHierarchy)
        //{
        //    //uiStage.EnterdStageNo = _data.Stage_Id -1;
        //    //uiStage.EnterdStageData[_data.LEVEL_DIFFICULTY] = _data.Stage_Id;
        //    uiStage.SetEnteredStageLevel(_data.LEVEL_DIFFICULTY, _data.Stage_Id);
        //}
        // SceneManager.Instance.LoadCombatScene(_data.Stage_Scene);
        // 정보 창 열기
        Dictionary<UIOption, object> optionDict = Utils.GetUIOption(
           UIOption.Tid, _data.Tid
           );

        UIManager.Instance.Show<UIStageInfo>(optionDict);
    }

    public void OnClickDimdStage()
    {
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK, message: SetIsNotOpenIndex());
    }

    private string SetIsNotOpenIndex()
    {
        var mainstageIndex = LocalizeManager.Instance.GetString("str_stage_main_number_default" , _mainstageno);
        var index = LocalizeManager.Instance.GetString("str_stage_sub_number_default", _data.Extra_Id-1);

        StringBuilder sb = new StringBuilder();
        sb.Append(mainstageIndex);
        sb.Append(" ");
        sb.Append(index);
        sb.Append(" ");
        sb.Append("str_content_total_difficulty_guide_02".ToTableText()); //클리어 시 해금

        return sb.ToString();
    }
}
