using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollFocuse : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Scrollbar _scrollBar;
    private int _size;
    private float[] _pos;

    private float _distance = 0;
    private float _targetPos = 0;

    private bool _isDrag = false;

    public void InitData(int count)
    {
        _size = count;
        _pos = new float[count];
        _distance = 1f / (_size -1);

        for (int n = 0; n < _size; n++)
        {
            _pos[n] = _distance * n;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        _isDrag = true;
    }

    //드레그가 시작중일때
    public void OnBeginDrag(PointerEventData eventData)
    {

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        _isDrag = false; //드레그가 끝

        for (int i = 0; i< _size; i++)
        {
            if (_scrollBar.value <_pos[i] + _distance+0.5f && _scrollBar.value > _pos[i] - _distance*0.5f)
            {
                _targetPos = _pos[i];
            }
        }
    }
}
