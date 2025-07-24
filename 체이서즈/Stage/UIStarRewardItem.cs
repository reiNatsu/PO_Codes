using Consts;
using LIFULSE.Manager;
using UnityEngine;

public class UIStarRewardItem : MonoBehaviour
{
    [SerializeField] private RectTransform _parentRect;
    [SerializeField] private RectTransform _rect;

    [SerializeField] private int _rewardNo;
    [SerializeField] private GameObject _default;
    [SerializeField] private GameObject _enable;
    [SerializeField] private GameObject _complete;
    [SerializeField] private ExTMPUI _count;
    [SerializeField] private ExButton _onClick;

    private int _chapterId;
    private int _rewardCnt;
    private LEVEL_DIFFICULTY _level;
    private bool _isComplete = false;
    private string _rdKey;

    public void InitData(int rewardno,int missionCount, int maxStarCount, RewardState rewardState,
        int chapterId, LEVEL_DIFFICULTY level, string reddotKey)
    {
        _rewardNo = rewardno;
        _chapterId = chapterId;
        _count.text = missionCount.ToString();
        _level =level;
        _rdKey = reddotKey;

        SetState(rewardState);
        SetRewardPosition(missionCount, maxStarCount);
    }

    //각 미션 갯수에 따라 보상 위치 조절
    private void SetRewardPosition(int missionCount, int maxCount)
    {
        _rect.anchoredPosition = new Vector2(_parentRect.sizeDelta.x * (missionCount / (float)maxCount), _rect.anchoredPosition.y);
    }

    public void SetState(RewardState state)
    {
        switch (state)
        {
            case RewardState.DeActive:
                _count.SetColor("#4D4D4D");
                _default.SetActive(true);
                _enable.SetActive(false);
                _complete.SetActive(false);
                _onClick.enabled = false;
                break;
            case RewardState.Active:
                _count.SetColor("#3CFFFD");
                _default.SetActive(false);
                _enable.SetActive(true);
                _complete.SetActive(false);
                _onClick.enabled = true;
                break;
            case RewardState.Received:
                _count.SetColor("#3CFFFD");
                _default.SetActive(false);
                if (_enable.activeInHierarchy)
                {
                    _enable.SetActive(false);
                }
                _complete.SetActive(true);
                _onClick.enabled = false;
                break;
            default:
                break;
        }
    }
}//