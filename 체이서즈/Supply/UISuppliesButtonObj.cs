using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISuppliesButtonObj : MonoBehaviour
{
    [SerializeField] private GameObject _normalObj;
    [SerializeField] private GameObject _maxObj;                // 게이지 max일 때 보여주는 오브젝트
    //[SerializeField] private Slider _expSlider;
    [SerializeField] private ExTMPUI _percentageTMP;

    private int _timeVal = 0;
    private Coroutine _coroutine = null;
    private bool _isGoTime = false;
    private CONTENTS_TYPE_ID _typeID;
  //  private int _maxCount = 0;

    public void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
    public void Init()
    {
        _typeID = GameInfoManager.Instance.SupplyInfo.SupplyDatas.FirstOrDefault().Key;
        var supplyLevel = GameInfoManager.Instance.GetSupplyLevel();
        if (GameInfoManager.Instance.IsOpendSupplies())             // 방치 보상 받을 수 있음. 
        {
            //_expSlider.gameObject.SetActive(true);
            if (!GameInfoManager.Instance.SupplyInfo.IsRequestUpdate(_typeID))
            {
                GameInfoManager.Instance.SetCurrentSupplyLevel(supplyLevel);
                _timeVal = GetSupplyLevelCount();

                if (_coroutine == null)
                    _coroutine = StartCoroutine(SetRecieveGauage());

                UpdateUI();
                UpdateSuppliesGaugeUI(GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount);
            }
            else
            {
                if(GameInfoManager.Instance.SupplyInfo.IsRequestUpdate(_typeID))
                {
                    var clanid = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
                    if (GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].EnableUpdate())
                    {
                        RestApiManager.Instance.RequestSupplyUpdate(clanid, (res) =>
                        {
                            var supplyLevel = GameInfoManager.Instance.GetSupplyLevel();
                            GameInfoManager.Instance.SetCurrentSupplyLevel(supplyLevel);
                            _timeVal = GetSupplyLevelCount();

                            if (_coroutine == null)
                                _coroutine = StartCoroutine(SetRecieveGauage());

                            UpdateSuppliesGaugeUI(GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount);
                        });
                    }
                }
                else
                {
                    if (_coroutine == null)
                        _coroutine = StartCoroutine(SetRecieveGauage());
                }
            }
        }
        else         // 방치 보상 받을 수 없음. 
        {
            UpdateUI(0);
        }
    }

    //보상 수령 가능 상태에 따라 obj, percent 반영
    private void UpdateUI(int percent = 0)
    {
        var isMax = IsMaxRewardCount();

        _maxObj.SetActive(isMax);
        _normalObj.SetActive(!isMax);

        if(isMax)
            SetPercent(100);
        else
            SetPercent(percent);
    }

    public void UpdateSuppliesGaugeUI(int value)
    {
        //if (_timeVal == 0)
        //    UpdateUI(0);
        //else
        //{
        //    int percent = 0;

        //    if (value > 0)
        //    {
        //        percent = (int)((value / _timeVal) *100f);

        //        if (percent >= 100)
        //            percent = 100;
        //    }

        if (_timeVal == value)
            value = _timeVal;

        float percent = ((float)value / _timeVal) *100f;
        percent = Mathf.Round(percent);

        UpdateUI((int)percent);
    }

    public void SetPercent(int percent)
    {
        _percentageTMP.text = percent + "%";
    }


    IEnumerator SetRecieveGauage()
    {
        float maxValue = GameInfoManager.Instance.GetNextRefreshValue();
        TimeSpan current = GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].SupplyRefreshTime - DateTime.UtcNow;
        int value = (int)Math.Ceiling(current.TotalSeconds);
        if (value < 0)
            value = 0;
        yield return new WaitForSecondsRealtime(value);

        var uiPopupSupply = UIManager.Instance.GetUI<UIPopupSupply>();
        if (uiPopupSupply != null && uiPopupSupply.gameObject.activeInHierarchy)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            // Update를 다 하면 다시 코루틴 시작.
            if (_coroutine == null)
                _coroutine = StartCoroutine(SetRecieveGauage());
        }
        else
        {
            if (GameInfoManager.Instance.SupplyInfo.IsRequestUpdate(_typeID))
            {
                yield return new WaitForSecondsRealtime(0.2f);

                var clanid = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
                if (GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].EnableUpdate())
                {
                    RestApiManager.Instance.RequestSupplyUpdate(clanid, (res) =>
                    {
                        if (_coroutine != null)
                        {
                            StopCoroutine(_coroutine);
                            _coroutine = null;
                        }
                        // Update를 다 하면 다시 코루틴 시작.
                        int value = GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount;
                        UpdateSuppliesGaugeUI(value);
                        // 일단 코루틴 멈추고,

                        // Update를 다 하면 다시 코루틴 시작.
                        if (_coroutine == null)
                            _coroutine = StartCoroutine(SetRecieveGauage());
                    });
                }
                   
            }
            else
            {
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    _coroutine = null;
                }
                if (_coroutine == null)
                    _coroutine = StartCoroutine(SetRecieveGauage());
            }
        }
    }

    private int GetSupplyLevelCount()
    {
        var level = GameInfoManager.Instance.GetSupplyLevel();
        var count = TableManager.Instance.Supplies_Table.GetTimeValue(level);
        return count;
    }

    //보상 맥스까지 수령 가능한 지
    private bool IsMaxRewardCount()
    {
        if (_timeVal == 0)
            return false;

        return _timeVal <= GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].RewardCount;
    }

    public void OnClickSupply()
    {
        if (!GameInfoManager.Instance.IsOpendSupplies())
            return;

        Action onClick = () => {
            UIManager.Instance.Show<UIPopupSupply>(Utils.GetUIOption(UIOption.EnumType, _typeID));
        };
        // 저장되 SRT+10이 지금 시간보다 작으면 RequestSupplyUpdate 호출x
        var clanId = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
        if (GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].EnableUpdate())
        {
            RestApiManager.Instance.RequestSupplyUpdate(clanId, (res) =>
            {
                onClick.Invoke();
            });
        }
        else
            onClick.Invoke();
        //if (!GameInfoManager.Instance.SupplyInfo.IsRequestUpdate(_typeID))
        //{
        //    onClick.Invoke();
        //}
        //else
        //{
        //    var clanId = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
        //    if (GameInfoManager.Instance.SupplyInfo.SupplyDatas[_typeID].EnableUpdate())
        //    {
        //        RestApiManager.Instance.RequestSupplyUpdate(clanId, (res) =>
        //        {
        //            onClick.Invoke();
        //        });
        //    }
        //    else
        //        onClick.Invoke();


        //}
    }

    public void OnClickSupplyDimd()
    {
        UIManager.Instance.ShowToastMessage("str_ui_supply_con_open_01"); // 챌린저 로드 루트 1 클리어 시 해방됩니다.
    }
}
