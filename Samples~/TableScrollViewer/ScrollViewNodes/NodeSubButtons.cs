using TMPro;
using Uindies.TableScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class NodeSubButtons : TableSubNodeElement
{
    [SerializeField]
    Image                 Frame = null;
    [SerializeField]
    TextMeshProUGUI       Text = null;
    
    public override void onEffectFocus(bool focus, bool isAnimation)
    {
        if (focus == true)
        {
            Frame.color = new Color(0,0,0,1);
            Text.color = new Color(1,1,1,1);
        }
        else
        {
            Frame.color = new Color(0,0,0,0.33f);
            Text.color = new Color(0,0,0,1);
        }
    }

    /// <summary>
    /// 当行的显示更新通知到达时，在此处更新显示
    /// </summary>
    public override void onEffectChange(int itemIndex, int subIndex)
    {
        string abc  = "ABC";
        Text.SetText($"{itemIndex+1}-{abc[subIndex]}");

    }

    /// <summary>
    /// 在此处描述点击通知到达时的显示效果
    /// </summary>
    public override void onEffectClick()
    {
    }

}
