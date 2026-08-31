using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uindies.TableScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class NodeHorizontal : TableNodeElement
{
    [SerializeField]
    Image              Icon = null;
    [SerializeField]
    Image              Focus = null;
    [SerializeField]
    Sprite[]           IconSprites = null;

    /// <summary>
    /// 初始化时被调用
    /// </summary>
    public override void onInitialize()
    {
    }

    /// <summary>
    /// 在此编写焦点 ON/OFF 的显示
    /// </summary>
    public override void onEffectFocus(bool focus, bool isAnimation)
    {
        Focus.color = new Color(0,0,0, focus == true ? 0.5f : 0.1f);
    }

    /// <summary>
    /// 当收到行显示更新通知时，在此更新显示
    /// </summary>
    public override void onEffectChange(int itemIndex)
    {
        int no = (int)table[itemIndex];

        Icon.sprite = IconSprites[no % IconSprites.Length];

        this.name = "Line: " + (no+1).ToString("00");
    }
}
