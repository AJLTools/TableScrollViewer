using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uindies.TableScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class NodeVerticalFreeSize : TableNodeElement
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
    string[]           Descriptions = null;
    [SerializeField]
    Sprite[]           IconSprites = null;

    RectTransform      focusRect;

    /// <summary>
    /// 初始化时调用
    /// </summary>
    public override void onInitialize()
    {
        focusRect = Focus.GetComponent<RectTransform>();
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
        int no = (int)table[itemIndex];

        No.SetText("Line: " + (no+1).ToString("00"));
        Desc.SetText(Descriptions[no % Descriptions.Length]);
        Icon.sprite = IconSprites[no % IconSprites.Length];

        float height = GetCustomHeight(table, itemIndex);

        RectSetHeight(focusRect, height-10);
        RectSetHeight(NodeRect, height);

        this.name = No.text;
    }

    public override float GetCustomHeight(List<object> tbl, int itemIndex)
    {
        int no = (int)tbl[itemIndex];

        if ((no % 3) == 0)
        {
            return 200;
        }
        else
        if ((no % 3) == 1)
        {
            return 500;
        }
        else
        {
            return 300;
        }
    }

}
