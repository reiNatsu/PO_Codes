using Consts;
using LeTai.TrueShadow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFieldNode : MonoBehaviour
{
    [SerializeField] private TrueShadow _trueShadow;

    [SerializeField] private ExImage _nodeImage;
    [SerializeField] private ExImage _warpImage;
    [SerializeField] private ExImage _questImage;

    public void Show(bool questable, bool existWarp, bool visited, Color color)
    {

    }
}
