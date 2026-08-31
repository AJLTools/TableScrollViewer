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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Uindies.TableScrollViewer
{

public partial class TableScrollViewer : MonoBehaviour
{
    Scrollbar       scrollbar;
    CanvasGroup     cgroup_scrollbar;
    Vector2         prePosition;

    bool            isScrollbarAutoFadeOut = false;
    
    Coroutine       co_scrollbarOn;

    void initScrollbar()
    {
        if (scrollRect.verticalScrollbar != null)
        {
            scrollbar = scrollRect.verticalScrollbar;
        }
        else
        if (scrollRect.horizontalScrollbar != null)
        {
            scrollbar = scrollRect.horizontalScrollbar;
        }
        
        if (scrollbar == null)
        {
            return;
        }

        // 如果不这样做，用鼠标移动滚动条后，按键输入时位置会自行上下跳动导致卡住…
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        scrollbar.navigation = nav;

        cgroup_scrollbar = safeGetCanvasGroup(scrollbar);
        prePosition = new Vector2();
        prePosition.x = scrollRect.content.transform.localPosition.x;
        prePosition.y = scrollRect.content.transform.localPosition.y;

        if (ScrollbarAutoFadeout == true)
        {
            setScrollbarAlpha(0);
        }
    }

    /// <summary>
    /// update
    /// </summary>
    void updateScrollbar()
    {
        if (scrollbar == null)
        {
            return;
        }

        scrollbarAutoFadeOut(ScrollbarAutoFadeout);

        if (isScrollbarAutoFadeOut == false)
        {
            return;
        }

//Debug.Log($"{scrollRect.content.transform.localPosition.x} {scrollRect.content.transform.localPosition.y}");
        // 按键、鼠标滚轮
        if (prePosition.x != scrollRect.content.transform.localPosition.x ||
            prePosition.y != scrollRect.content.transform.localPosition.y)
        {
            dispOnScrollbar();
        }
    }
    
    /// <summary>
    /// 滚动条自动淡出功能的启用、禁用
    /// </summary>
    /// <param name="enabled">true..启用，false..禁用</param>
    void scrollbarAutoFadeOut(bool enabled)
    {
        if (isScrollbarAutoFadeOut == enabled)
        {
            return;
        }

        isScrollbarAutoFadeOut = enabled;

        if (enabled == true)
        {
            setScrollbarAlpha(0);
        }
        else
        {
            setScrollbarAlpha(1);
        }
    }

    /// <summary>
    /// 滚动条显示一秒
    /// </summary>
    void dispOnScrollbar()
    {
        if (isScrollbarAutoFadeOut == false)
        {
            return;
        }

        if (co_scrollbarOn != null)
        {
            return;
        }
        co_scrollbarOn = StartCoroutine(scrollbarOn());
    }

    /// <summary>
    /// On/Off animation
    /// </summary>
    IEnumerator scrollbarOn()
    {
        if (cgroup_scrollbar == null)
        {
            yield break;
        }

        float a    = cgroup_scrollbar.alpha;
        float time = Time.time;

        // On
        while (true)
        {
            float t = (Time.time - time) * 5;
            t = Mathf.Clamp01(t);

            setScrollbarAlpha(a + (1-a) * t);

            if (t >= 1)
            {
                break;
            }

            yield return null;
        }

        while (prePosition.x != scrollRect.content.transform.localPosition.x ||
               prePosition.y != scrollRect.content.transform.localPosition.y)
        {
            prePosition.x = scrollRect.content.transform.localPosition.x;
            prePosition.y = scrollRect.content.transform.localPosition.y;
            yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(1);

        time = Time.time;

        // Off
        while (true)
        {
            float t = (Time.time - time) * 5;
            t = Mathf.Clamp01(t);

            setScrollbarAlpha(1 * (1 - t));

            if (t >= 1)
            {
                break;
            }

            yield return null;
        }

        co_scrollbarOn = null;
    }

    /// <summary>
    /// 如果有 CanvasGroup 则使用它，没有则创建
    /// </summary>
    /// <param name="bar">要附加的滚动条</param>
    CanvasGroup safeGetCanvasGroup(Scrollbar bar)
    {
        CanvasGroup group = bar.gameObject.GetComponentInChildren<CanvasGroup>();
        if (group != null)
        {
            return group;
        }
        return bar.gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// set alpha
    /// </summary>
    void setScrollbarAlpha(float a)
    {
        if (cgroup_scrollbar != null)
        {
            cgroup_scrollbar.alpha = a;
        }
    }
}
}
