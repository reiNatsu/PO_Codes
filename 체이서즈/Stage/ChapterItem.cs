using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChapterItem : MonoBehaviour
{
    [Header("Chapter UI")]
    [SerializeField] public ExImage _image;
    [SerializeField] public ExTMPUI _title;
    [SerializeField] public ExTMPUI _chpaternoTitle;
    [SerializeField] public ExTMPUI _chpaternoTitle2;
   // [SerializeField] private GameObject _dimmed;

    [Header("Complete UI")]
    [SerializeField] private GameObject _starValueOb;
    [SerializeField] private GameObject _gaugeOb;
    [SerializeField] private GameObject _processingObje;
    [SerializeField] private ExTMPUI _maxCount;
    [SerializeField] private ExTMPUI _getCount;
    [SerializeField] private GameObject _checkObj;
    [SerializeField] private Slider _gauge;

    [SerializeField] private Button _dimedBtn;

    [SerializeField] private UILayoutGroup _uiLayoutGroup;

    private Content_TableData _data;
    public Button DimedBtn { get { return _dimedBtn; } }

    public void InitData(Content_TableData datas, Action onclick = null)
    {
        _data = datas;

        if (onclick != null && _dimedBtn != null)
        {
            _dimedBtn.onClick.AddListener(() => onclick());
        }

        SetStarGaugeUI(true);

        UpdateUI();
    }

    private void UpdateUI()
    {
        SetTitle();
        SetGauge();

        _uiLayoutGroup.UpdateLayoutGroup();
    }
    private void SetTitle()
    {
        _chpaternoTitle.text = _data.Content_Chapter_Id.ToString();
        _chpaternoTitle2.text = _data.Content_Chapter_Id.ToString();
        //var titleStr = LocalizeManager.Instance.GetString(_data.Str).Split(".")[1];
        //_title.text = titleStr;
        // 챕터 이미지
        _image.SetSprite(_data.Content_Bg_Icon);
    }

    private void SetGauge()
    {
        var total = GameInfoManager.Instance.GetAllRewardStarCount(_data.Content_Chapter_Id);
        var get = GameInfoManager.Instance.GetRewardStarCount(_data.Content_Chapter_Id);

        _gauge.minValue = 0;
        _gauge.maxValue = total; //total;
        _gauge.value = get;
        _maxCount.text =  total.ToString();
        _getCount.text = get.ToString();

        // 체크 마크 노출 여부
        _checkObj.SetActive(total > 0 && total == get);
    }

    public void SetDimedUI(bool isShow)
    {
       // _dimmed.SetActive(isShow);
    }

    public void SetStarGaugeUI(bool isShow)
    {
        _starValueOb.SetActive(isShow);
        //_gaugeOb.SetActive(isShow);
        _processingObje.SetActive(isShow);
    }

    public void SetChapterItemDimed(bool isOn)
    {
        //if (_dimmed == null)
        //    return;

        //_dimmed.SetActive(isOn);
    }
}
