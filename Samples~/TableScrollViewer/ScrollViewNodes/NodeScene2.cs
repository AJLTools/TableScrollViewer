using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uindies.TableScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class NodeScene2 : TableNodeElement
{
    [SerializeField]
    TextMeshProUGUI    No = null;
    [SerializeField]
    TextMeshProUGUI    Desc = null;
    [SerializeField]
    Image              Icon = null;
    [SerializeField]
    Image              Focus = null;
    [SerializeField]
    Sprite[]           IconSprites = null;

    /// <summary>
    /// 初始化时调用
    /// </summary>
    public override void onInitialize()
    {
    }

    /// <summary>
    /// 在此处描述焦点 ON/OFF 的显示
    /// </summary>
    public override void onEffectFocus(bool focus, bool isAnimation)
    {
        Focus.color = new Color(0,0,0, focus == true ? 0.2f : 0.1f);
    }

    /// <summary>
    /// 当行的显示更新通知到达时，在此处更新显示
    /// </summary>
    public override void onEffectChange(int itemIndex)
    {
        var row = (TestScene2.Row)table[itemIndex];

        No.SetText("Line: " + row.No.ToString("00"));
        Desc.SetText(row.PlaceName);
        Icon.sprite = IconSprites[row.No % IconSprites.Length];

        this.name = No.text;
    }

}
