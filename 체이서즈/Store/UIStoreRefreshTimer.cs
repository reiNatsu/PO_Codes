using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStoreRefreshTimer : MonoBehaviour
{
    [SerializeField] private GameObject _refreshBtn;
    [SerializeField] private ExTMPUI _resetTimer;
    //[SerializeField] private UILayoutGroup _uiLayoutGroup;

    private UIStore _uiStore;
    public Coroutine _refreshCoroutine;
    private Store_Option_TableData _data;


    public void ShowTimer(DateTime dateTime, Store_Option_TableData type)
    {
        _uiStore = UIManager.Instance.GetUI<UIStore>();
        _data  = type;
            if (_data.REFRESH_COST_TYPE == ITEM_TYPE.none)
            {
                _refreshBtn.SetActive(false);
            }
            else
            {
                _refreshBtn.SetActive(true);
            }

      
        UpdateRefreshTime(dateTime);
    }


    public void UpdateRefreshTime(DateTime dateTime)
    {

        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine= null;
        }

        RestApiManager.Instance.RequestGetNowTimeNoLogin(() => {

            // if (dateTime < DateTime.UtcNow.ToAddHours())
            if (dateTime < GameInfoManager.Instance.NowTime)
            {

                _uiStore.SetIsEndTimer();
            }
            else
            {
                TimeSpan timeSpan = dateTime - GameInfoManager.Instance.NowTime;
                if (timeSpan.TotalSeconds > 0)
                {
                    _refreshCoroutine = StartCoroutine(CheckRefreshTime((int)timeSpan.TotalSeconds));
                }
            }
        });
    }

    private IEnumerator CheckRefreshTime(int second)
    {
        int secs = second;
        string timeStr;
        TimeSpan t = TimeSpan.FromSeconds(secs);
        UpdateTimerIndex(t);

        for (int i = 0; i < second; i++)
        {
            yield return new WaitForSecondsRealtime(1);
            secs--;
            _uiStore.SetIsEndTimer();
            if (secs == 0)
            {
                //GameInfoManager.Instance.InvokeResetTimeEvent(true);
                _resetTimer.text = "00";
                UpdateTimerIndex(t);
            }
            else
            {
                t = TimeSpan.FromSeconds(secs);
                UpdateTimerIndex(t);
            }
        }
    }

    public void UpdateTimerIndex(TimeSpan time)
    {
        string resetStr = "str_ui_time_count_mod_01".ToTableText(); //"갱신까지";
        
        if (time.Days > 0)
        {
            _resetTimer.ToTableText("str_ui_time_count_d", resetStr, time.Days, time.Hours.ToString("D2"));
        }
        else
        {
            if (time.Hours > 0)
            {
                _resetTimer.ToTableText("str_ui_time_count_h", resetStr, time.Hours.ToString("D2"));
            }
            else
            {
                if (time.Minutes > 0)
                {
                    _resetTimer.ToTableText("str_ui_time_count_m", resetStr, time.Minutes.ToString("D2"));
                }
                else
                {
                    // str = string.Format(" {0:D2}초", time.Seconds);
                    _resetTimer.ToTableText("str_ui_time_count_s", resetStr, time.Seconds.ToString("D2"));
                }
            }
        }
        //_uiLayoutGroup.UpdateLayoutGroup();
    }

    public void IsDisableObject()
    {
        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }
    }
}
