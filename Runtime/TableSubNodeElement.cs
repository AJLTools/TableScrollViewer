// Copyright (c) catsnipe
// Released under the MIT license

// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions:
   
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEngine.EventSystems;

namespace Uindies.TableScrollViewer
{

public class TableSubNodeElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected TableNodeElement node      = null;
    int                        subIndex  = 0;
    bool                       initFocus = false;
    bool                       focused;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        onInitialize();
    }

    /// <summary>
    /// 子节点的构建
    /// </summary>
    /// <param name="_node">父节点</param>
    /// <param name="_subIndex">子节点的顺序</param>
    public void SetSubNode(TableNodeElement _node, int _subIndex)
    {
        node     = _node;
        subIndex = _subIndex;
    }

    /// <summary>
    /// set focus
    /// </summary>
    /// <param name="focus">on..true, off.false</param>
    public void SetFocus(bool focus, bool isAnimation = true)
    {
        if (initFocus == true && focus == focused)
        {
            return;
        }
//Debug.Log($"focus:{rowIndex} {focus}");
        // 在此处自定义焦点 ON/OFF 时的显示
        onEffectFocus(focus, isAnimation);

        initFocus = true;
        focused   = focus;
    }

    /// <summary>
    /// get focus
    /// </summary>
    public bool GetFocus()
    {
        return focused;
    }

    /// <summary>
    /// Mouse event
    /// </summary>
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        node.OnPointerEnter(subIndex);
    }
    
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // 仅限左键（右键、中键的点击不作为确定。触摸作为 Left 传入）
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        node.OnPointerClick(subIndex);
    }

    /// <summary>
    /// 初始化时调用
    /// </summary>
    public virtual void onInitialize()
    {
    }

    /// <summary>
    /// 在此处描述焦点 ON/OFF 的显示效果
    /// </summary>
    public virtual void onEffectFocus(bool focus, bool initialize)
    {
    }

    /// <summary>
    /// 当行的显示更新通知到达时，在此处更新显示
    /// </summary>
    public virtual void onEffectChange(int itemIndex, int subIndex)
    {
    }

    /// <summary>
    /// 在此处描述点击通知到达时的显示效果
    /// </summary>
    public virtual void onEffectClick()
    {
    }

}
}
