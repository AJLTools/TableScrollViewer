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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Uindies.TableScrollViewer
{

[RequireComponent(typeof(CanvasGroup))]

public partial class TableScrollViewer : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    /// <summary>
    /// 滚动方向
    /// </summary>
    public enum eOrientation
    {
        Vertical,
        Horizontal,
    }

    /// <summary>
    /// 显示对齐方式
    /// </summary>
    public enum eAlignment
    {
        Near,
        Center,
        Far,
    }

    /// <summary>
    /// 触发按键移动的标志
    /// </summary>
    public enum eKeyMoveFlag
    {
        /// <summary>
        /// 不移动
        /// </summary>
        None,
        /// <summary>
        /// 选择
        /// </summary>
        Select,
        /// <summary>
        /// 取消选择
        /// </summary>
        Cancel,
        /// <summary>
        /// 向上移动
        /// </summary>
        Up,
        /// <summary>
        /// 向下移动
        /// </summary>
        Down,
        /// <summary>
        /// 向左移动
        /// </summary>
        Left,
        /// <summary>
        /// 向右移动
        /// </summary>
        Right,
        /// <summary>
        /// 向上翻页
        /// </summary>
        PageUp,
        /// <summary>
        /// 向下翻页
        /// </summary>
        PageDown,
        /// <summary>
        /// 向左翻页
        /// </summary>
        PageLeft,
        /// <summary>
        /// 向右翻页
        /// </summary>
        PageRight,
        /// <summary>
        /// 移至顶部
        /// </summary>
        ToTop,
        /// <summary>
        /// 移至末尾
        /// </summary>
        ToBottom,
    }
    /// <summary>
    /// KeyDown EventArgs
    /// </summary>
    public class KeyDownArgs
    {
        public eKeyMoveFlag Flag;
    }
    /// <summary>
    /// 执行 SetSelectedIndex 时的位置滚动方式
    /// </summary>
    public enum ePositionMoveMode
    {
        /// <summary>
        /// 在1帧内移动到目标位置
        /// </summary>
        OneFrame,
        /// <summary>
        /// 一边滚动一边移动到目标位置
        /// </summary>
        ScrollMove,
        /// <summary>
        /// 不移动位置
        /// </summary>
        DontMove,
    }

    /// <summary>
    /// CheckSelectable 的返回值
    /// </summary>
    public class SelectableResult
    {
        public bool Enabled = true;
    };

    /// <summary>
    /// 需要按键输入时触发的事件
    /// </summary>
    [Serializable]
    public class OnKeyDownEvent : UnityEvent<KeyDownArgs> {}

    /// <summary>
    /// 被选择或取消时触发的事件
    /// object[] table, int itemIndex, int subIndex, bool isCancel
    /// 
    /// (table) 表格
    /// (itemIndex) 选中的行
    /// (subIndex) 选中的列（行的子项）
    /// (isCancel) 按下取消按钮时为 true
    /// </summary>
    [Serializable]
    public class OnSelectEvent : UnityEvent<List<object>, int, int, bool> {}

    /// <summary>
    /// 光标移动时触发的事件
    /// object[] table, int itemIndex, int subIndex, bool userInput
    /// 
    /// (table) 表格
    /// (itemIndex) 选中的行
    /// (subIndex) 选中的列（行的子项）
    /// (userInput) 用户选择导致变化时为 true，SetSelectedIndex() 时为 false
    /// </summary>
    [Serializable]
    public class OnCursorMoveEvent : UnityEvent<List<object>, int, int, bool> {}

    /// <summary>
    /// 确认是否为可选项的事件
    /// object[] table, int itemIndex, int subIndex : SelectableResult
    /// 
    /// 
    /// (table) 表格
    /// (itemIndex) 选中的行
    /// (subIndex) 选中的列（行的子项）
    /// (SelectableResult) 返回值。可选时为 true，禁止选择时为 false
    /// </summary>
    [Serializable]
    public class OnCheckSelectableEvent : UnityEvent<List<object>, int, int, SelectableResult> {}

    /// <summary>
    /// Node Prefab
    /// </summary>
    [SerializeField]
    public TableNodeElement
                          SourceNode = null;
    /// <summary>
    /// 滚动方向
    /// </summary>
    [SerializeField]
    public eOrientation   Orientation = eOrientation.Vertical;
    /// <summary>
    /// 显示对齐方式
    /// </summary>
    [SerializeField]
    public eAlignment     Alignment = eAlignment.Near;
    /// <summary>
    /// 与 Vertical Layout Group 的 Padding.Top 相同
    /// </summary>
    [SerializeField, Space(10)]
    public float          PaddingTop = 0;
    /// <summary>
    /// 与 Vertical Layout Group 的 Padding.Bottom 相同
    /// </summary>
    [SerializeField]
    public float          PaddingBottom = 0;
    /// <summary>
    /// 与 Vertical Layout Group 的 Spacing 相同
    /// </summary>
    [SerializeField]
    public float          Spacing = 0;
    /// <summary>
    /// 滚动所需时间
    /// </summary>
    [SerializeField, Space(10), Range(0.01f, 1f)]
    public float          ScrollTime = 0.2f;
    /// <summary>
    /// 翻页滚动时移动的项数
    /// </summary>
    [SerializeField, Range(1, 1000)]
    public int            SkipIndexByPageScroll = 10;
    /// <summary>
    /// 滚动条自动淡出
    /// </summary>
    [SerializeField, Space(10), Tooltip("滚动条自动淡出")]
    public bool           ScrollbarAutoFadeout = true;
    /// <summary>
    /// 拖拽移动时，自动将项吸附到固定位置
    /// </summary>
    [SerializeField, Tooltip("拖拽移动时，自动将项吸附到固定位置")]
    public bool           AdsorptionTarget = true;
    /// <summary>
    /// 为 true 时先转移焦点再选择。为 false 时立即选择
    /// </summary>
    [SerializeField, Tooltip("为 true 时先转移焦点再选择。为 false 时立即选择")]
    public bool           SelectAfterFocus = false;
    /// <summary>
    /// 鼠标光标移到项上时，自动获取焦点
    /// 如果想将鼠标和点击作为相同操作，设为 false 更安全
    /// </summary>
    [SerializeField, Tooltip("鼠标光标移到项上时，自动获取焦点")]
    public bool           EasyFocusForMouse = false;
    /// <summary>
    /// 选择后自动禁止视图选择。要恢复请调用 InputEnabled()
    /// </summary>
    [SerializeField, Tooltip("选择后自动禁止视图选择。要恢复请调用 InputEnabled()")]
    bool                  DisabledAfterSelect = false;

    /// <summary>
    /// OnClick
    /// </summary>
    [SerializeField, Header("Event")]
    public OnSelectEvent  OnSelect = null;
    /// <summary>
    /// 按键输入确认事件
    /// </summary>
    [SerializeField]
    public OnKeyDownEvent OnKeyDown = null;
    /// <summary>
    /// 音效请求事件
    /// </summary>
    [SerializeField]
    public OnCursorMoveEvent
                          OnCursorMove = null;
    /// <summary>
    /// 确认是否为可选项的事件
    /// (table) 表格
    /// (itemIndex) 选中的行
    /// (subIndex) 选中的列（行的子项）
    /// (result) true..可选，false..禁止选择
    /// </summary>
    [SerializeField]
    public OnCheckSelectableEvent
                          OnCheckSelectable = null;

//[SerializeField]
//TextMeshProUGUI text;

    class NodeGroup
    {
        public GameObject       Object;
        public RectTransform    Rect;
        public TableNodeElement Node;
    }

    class RowDisplay
    {
        public float    Position;
        public float    Size;
        public float    LastPosition;
    }

    /// <summary>
    /// 当前选中的列表序号
    /// </summary>
    public int          SelectedIndex
    {
        get
        {
            if (reserveSelectedIndex >= 0)
            {
                return reserveSelectedIndex;
            }
            return selectedIndex;
        }
        private set
        {
            selectedIndex = value;
        }
    }
    int                 selectedIndex = -1;
    /// <summary>
    /// 选中行的数据
    /// </summary>
    public object       SelectedRow
    {
        get
        {
            if (SelectedIndex < 0 || SelectedIndex >= table.Count)
            {
                return null;
            }
            return table[SelectedIndex];
        }
    }
    /// <summary>
    /// 选中的表格节点
    /// </summary>
    public TableNodeElement SelectedTableNode
    {
        get
        {
            if (reserveSelectedIndex >= 0)
            {
                Debug.LogWarning("SetSelectedIndex() 之后的表格节点可能是过时的。");
            }
            return selectedNodeGroup?.Node;
        }
    }
    NodeGroup           selectedNodeGroup;
    /// <summary>
    /// 表格最大行数
    /// </summary>
    public int          ItemCount
    {
        get; private set;
    }

    /// <summary>
    /// 
    /// </summary>
    int                 selectedSubIndex = TableNodeElement.SUBINDEX_ROOT;
    /// <summary>
    /// 下一个被选中的列表序号（预约）
    /// </summary>
    int                 reserveSelectedIndex;
    /// <summary>
    /// 为 true 时瞬间移动滚动位置
    /// 实际由 ForceSelectedIndex() 调用
    /// </summary>
    ePositionMoveMode   positionMoveMode;

    /// <summary>
    /// CanvasGroup
    /// </summary>
    public CanvasGroup  CanvasGroup
    {
        get; private set;
    }
    CanvasGroup[]       parentGroups;

    /// <summary>
    /// 主体
    /// </summary>
    ScrollRect          scrollRect;
    RectTransform       scrollRectTransform;
    /// <summary>
    /// 初始尺寸
    /// </summary>
    Vector2             viewWH;
    /// <summary>
    /// 所有显示节点。仅分配显示所需数量
    /// </summary>
    List<NodeGroup>     nodeGroups;
    Dictionary<TableNodeElement, NodeGroup>
                        nodeSearch;
    Dictionary<int, NodeGroup>
                        nodeIndex;

    /// <summary>
    /// 按键信息
    /// </summary>
    KeyDownArgs         keyDownArgs;

    /// <summary>
    /// 所有行的显示位置等
    /// </summary>
    List<RowDisplay>    rowDisplays;


    /// <summary>
    /// 与 Vertical Layout Group 的 Padding.Top/Bottom 相同的值
    /// </summary>
    float               paddingTop;
    float               paddingBottom;
    /// <summary>
    /// Vertical Layout Group 的 Spacing
    /// </summary>
    float               nodeSpace;
    /// <summary>
    /// 在画面外额外保留的节点数。基本为 0 即可，但在窗口可变可能导致节点增加时设置
    /// </summary>
    int                 nodeExtraNumber;
    /// <summary>
    /// 表格
    /// </summary>
    List<object>        table;
    /// <summary>
    /// 显示中的顶部行
    /// </summary>
    int                 itemStart;
    /// <summary>
    /// 用于修改表格的临时缓冲区
    /// </summary>
    List<object>        changeTable;
    int                 changeSelectedIndex;

    // 按键移动的目标位置与当前位置
    float               targetNormPos;
    float               currentNormPos;
    float               timeNormPos;

    // 吸附事件
    Coroutine           co_autoTarget;

    // Content（所有滚动区域）的高度
    float               contentSize;
    // ScrollView（显示画面部分的滚动区域）的高度
    float               scrollSize;

    bool                focusIsAnimation = false;

    bool                touchEnabled = true;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_nodeExtraNumber">在画面外额外保留的节点数。基本为 0 即可，但在窗口可变可能导致节点增加时设置</param>
    public void Initialize(int _nodeExtraNumber = 0)
    {
        if (SourceNode == null)
        {
            Debug.LogError("SourceNode is not found. Please set by inspector.");
            return;
        }
        if (CanvasGroup != null)
        {
            return;
        }

        scrollRect          = GetComponent<ScrollRect>();
        scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        CanvasGroup         = GetComponent<CanvasGroup>();
        parentGroups        = GetComponentsInParent<CanvasGroup>();

        nodeExtraNumber     = _nodeExtraNumber;

        RectTransform rect = SourceNode.GetComponent<RectTransform>();
        if (Orientation == eOrientation.Vertical)
        {
            scrollRect.content.anchorMin = new Vector2(0, 1);
            scrollRect.content.anchorMax = new Vector2(1, 1);
        }
        else
        {
            scrollRect.content.anchorMin = new Vector2(0, 0);
            scrollRect.content.anchorMax = new Vector2(0, 1);
        }

        viewWH = new Vector2(rectGetWidth(scrollRectTransform), rectGetHeight(scrollRectTransform));
        reserveSelectedIndex = -1;
        positionMoveMode     = ePositionMoveMode.OneFrame;

        initScrollbar();
        SetTouchEnable(true);

        // event
        scrollRect.onValueChanged.AddListener(onValueChanged);
    }

    /// <summary>
    /// 触摸允许、禁止
    /// </summary>
    public void SetTouchEnable(bool enabled)
    {
        touchEnabled = enabled;

        if (enabled == true)
        {
            if (scrollRect != null)
            {
                scrollRect.movementType = ScrollRect.MovementType.Elastic;
                scrollRect.scrollSensitivity = 35;
            }
            if (scrollbar != null)
            {
                scrollbar.interactable = true;
            }
        }
        else
        {
            if (scrollRect != null)
            {
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.scrollSensitivity = 0;
            }
            if (scrollbar != null)
            {
                scrollbar.interactable = false;
            }
        }
    }

    /// <summary>
    /// 触摸允许、禁止的确认
    /// </summary>
    public bool CheckTouchEnable()
    {
        return touchEnabled;
    }

    /// <summary>
    /// 表格重置
    /// </summary>
    public void ResetTable()
    {
        SetTable((List<object>)null);
    }

    /// <summary>
    /// 显示的表格设置
    /// </summary>
    public void SetTable(object[] _table)
    {
        SetTable(_table.ToList());
    }

    /// <summary>
    /// 显示的表格设置
    /// </summary>
    public void SetTable(IList _table)
    {
        SetTable(_table.Cast<object>().ToList());
    }

    /// <summary>
    /// 显示的表格设置
    /// </summary>
    public void SetTable(List<object> _table)
    {
        if (SourceNode == null)
        {
            Debug.LogError("SourceNode is not found. Please set by inspector.");
            return;
        }
        if (CanvasGroup == null)
        {
            Initialize();
        }

        paddingTop    = PaddingTop;
        paddingBottom = PaddingBottom;
        nodeSpace     = Spacing;

        if (table != null)
        {
            for (int i = 0; i < nodeGroups.Count; i++)
            {
                NodeGroup group  = nodeGroups[i];
                Destroy(group.Object);
            }
            nodeGroups = null;
            nodeSearch = null;
            nodeIndex  = null;
        }

        table             = _table;
        ItemCount         = table == null ? 0 : table.Count;
        if (selectedIndex < -1)
        {
            selectedIndex = -1;
        }
        if (selectedIndex >= ItemCount)
        {
            selectedIndex = ItemCount - 1;
        }
        if (Orientation == eOrientation.Vertical)
        {
            scrollSize    = rectGetHeight(scrollRectTransform);
        }
        else
        {
            scrollSize    = rectGetWidth(scrollRectTransform);
        }

        rowDisplays = new List<RowDisplay>();

        float position = 0;
        float sizeMin  = scrollSize;

        if (table == null || table.Count == 0)
        {
            //
        }
        else
        {
            // 计算列表所有项的显示位置
            if (Orientation == eOrientation.Vertical)
            {
                position = SourceNode.GetCustomHeight(table, 0) / 2;
                sizeMin  = SourceNode.GetCustomHeight(table, 0);
                rowDisplays.Add(
                    new RowDisplay()
                    {
                        Position = position,
                        Size = sizeMin,
                        LastPosition = position + sizeMin / 2
                    }
                );
            }
            else
            {
                position = 0;
                sizeMin  = SourceNode.GetCustomWidth(table, 0);
                rowDisplays.Add(
                    new RowDisplay()
                    {
                        Position = position,
                        Size = sizeMin,
                        LastPosition = position + sizeMin
                    }
                );
            }

            for (int i = 1; i < table.Count; i++)
            {
                float size0;
                float size1;

                if (Orientation == eOrientation.Vertical)
                {
                    size0 = SourceNode.GetCustomHeight(table, i-1);
                    size1 = SourceNode.GetCustomHeight(table, i);
                }
                else
                {
                    size0 = SourceNode.GetCustomWidth(table, i-1);
                    size1 = SourceNode.GetCustomWidth(table, i); //  + nodeSpace;
                }

                if (sizeMin > size1)
                {
                    sizeMin = size1;
                }

                if (Orientation == eOrientation.Vertical)
                {
                    position += (size0 + size1) / 2 + nodeSpace;
                }
                else
                {
                    position += size0 + nodeSpace;
                }

                float lastPosition;

                if (Orientation == eOrientation.Vertical)
                {
                    lastPosition = position + size1 / 2;
                }
                else
                {
                    lastPosition = position + size1;
                }

                rowDisplays.Add(
                    new RowDisplay()
                    {
                        Position = position,
                        Size = size1,
                        LastPosition = lastPosition
                    }
                );
            }
        }

        contentSize       = paddingTop + paddingBottom;
        if (rowDisplays.Count > 0)
        {
            contentSize += rowDisplays[rowDisplays.Count-1].LastPosition;
        }

        nodeGroups        = new List<NodeGroup>();
        nodeSearch        = new Dictionary<TableNodeElement, NodeGroup>();
        selectedNodeGroup = null;
        selectedSubIndex  = TableNodeElement.SUBINDEX_ROOT;

        keyDownArgs       = new KeyDownArgs();

        int viewMax = (int)(scrollSize / sizeMin);
        int nodeMax = viewMax + 2 + nodeExtraNumber;
        if (nodeMax > ItemCount)
        {
            nodeMax = ItemCount;
        }

        for (int i = 0; i < nodeMax; i++)
        {
            TableNodeElement obj = Instantiate(SourceNode, scrollRect.content.transform);
            obj.Initialize();
            NodeGroup group = new NodeGroup();
            group.Object    = obj.gameObject;
            group.Node      = obj;
            group.Rect      = obj.GetComponent<RectTransform>();
            
            if (group.Node == null)
            {
                Debug.LogError("SourceNode 中不存在继承自 ScrollViewerNode 的 Node 类。");
            }

            group.Node.SetEvent(nodeEnter, nodeClick);
            group.Node.SetViewAndTable(this, table);

            nodeGroups.Add(group);
            nodeSearch.Add(group.Node, group);
        }

        float viewSize;
        
        if (Orientation == eOrientation.Vertical)
        {
            rectSetHeight(scrollRect.content, contentSize);
            viewSize = rectGetHeight(scrollRectTransform) - contentSize;
        }
        else
        {
            rectSetWidth(scrollRect.content, contentSize);
            viewSize = rectGetWidth(scrollRectTransform) - contentSize;
        }

        scrollRect.content.transform.localPosition = Vector3.zero;

        // Alignment
        if (viewSize < 0)
        {
            scrollRect.viewport.anchoredPosition = new Vector2(0, 0);
            scrollRect.viewport.sizeDelta = new Vector2(0, 0);
        }
        else
        {
            //★ 因为会自动修改 ViewPort 的值，
            //★ 在检视面板中单独设置的值会失效

            if (Orientation == eOrientation.Vertical)
            {
                if (Alignment == eAlignment.Near)
                {
                    scrollRect.viewport.anchoredPosition = new Vector2(0, 0);
                    scrollRect.viewport.sizeDelta = new Vector2(0, 0);
                }
                else
                if (Alignment == eAlignment.Center)
                {
                    if (scrollRect.verticalScrollbar != null)
                    {
                        Debug.LogError("Alignment cannot be specified because 'Vertical Scrollbar' exists. ");
                    }
                    scrollRect.viewport.anchoredPosition = new Vector2(0, -viewSize/2);
                    scrollRect.viewport.sizeDelta = new Vector2(0, viewSize);
                }
                else
                if (Alignment == eAlignment.Far)
                {
                    if (scrollRect.verticalScrollbar != null)
                    {
                        Debug.LogError("Alignment cannot be specified because 'Vertical Scrollbar' exists. ");
                    }
                    scrollRect.viewport.anchoredPosition = new Vector2(0, viewSize);
                    scrollRect.viewport.sizeDelta = new Vector2(0, 0);
                }
            }
            else
            {
                if (Alignment == eAlignment.Near)
                {
                    scrollRect.viewport.anchoredPosition = new Vector2(0, 0);
                    scrollRect.viewport.sizeDelta = new Vector2(0, 0);
                }
                else
                if (Alignment == eAlignment.Center)
                {
                    if (scrollRect.horizontalScrollbar != null)
                    {
                        Debug.LogError("Alignment cannot be specified because 'Horizontal Scrollbar' exists. ");
                    }
                    scrollRect.viewport.anchoredPosition = new Vector2(viewSize / 2, 0);
                    scrollRect.viewport.sizeDelta = new Vector2(-viewSize, 0);
                }
                else
                if (Alignment == eAlignment.Far)
                {
                    if (scrollRect.horizontalScrollbar != null)
                    {
                        Debug.LogError("Alignment cannot be specified because 'Horizontal Scrollbar' exists. ");
                    }
                    scrollRect.viewport.anchoredPosition = new Vector2(viewSize, 0);
                    scrollRect.viewport.sizeDelta = new Vector2(0, 0);
                }
            }
        }

        viewerScroll(new Vector2(0, 1), true);
    }

    /// <summary>
    /// 将光标移动到指定行
    /// </summary>
    /// <param name="selindex">指定的行号</param>
    /// <param name="_positionMove">以何种方式滚动到指定行号</param>
    public void SetSelectedIndex(object row, ePositionMoveMode _positionMove = ePositionMoveMode.OneFrame)
    {
        int index = table.FindIndex( (a) => a == row );
        SetSelectedIndex(index, _positionMove);
    }

    /// <summary>
    /// 将光标移动到指定行
    /// </summary>
    /// <param name="selIndex">指定的行号</param>
    /// <param name="_positionMove">以何种方式滚动到指定行号</param>
    public void SetSelectedIndex(int selIndex, ePositionMoveMode _positionMove = ePositionMoveMode.OneFrame)
    {
        if (checkUsable() == false)
        {
            return;
        }

        var result = new SelectableResult();
        OnCheckSelectable?.Invoke(table, selIndex, selectedSubIndex, result);
        if (result.Enabled == false)
        {
            selIndex = indexRight(selIndex + 1, selIndex);
        }

        if (selIndex >= 0)
        {
            selIndex = Mathf.Clamp(selIndex, 0, table.Count-1);
            reserveSelectedIndex = selIndex;
            positionMoveMode     = _positionMove;
            selectedSubIndex     = -1;
        }
    }
    
    /// <summary>
    /// 将光标移动指定行数
    /// </summary>
    /// <param name="amount">增减行数</param>
    public void AddSelectedIndex(int amount, ePositionMoveMode _positionMove = ePositionMoveMode.OneFrame)
    {
        amount += SelectedIndex;
        amount  = Mathf.Clamp(amount, 0, table.Count-1);
        if (amount != SelectedIndex)
        {
            SetSelectedIndex(amount, _positionMove);
        }
    }

    /// <summary>
    /// 获取当前可选的最顶部项
    /// </summary>
    public int GetSelectableTopIndex()
    {
        int i = 0;

        for ( ; i < ItemCount-1; i++)
        {
            var result = new SelectableResult();
            OnCheckSelectable?.Invoke(table, i, selectedSubIndex, result);
            if (result.Enabled == true)
            {
                return i;
            }
        }

        return i;
    }

    /// <summary>
    /// 获取当前可选的最底部项
    /// </summary>
    public int GetSelectableBottomIndex()
    {
        int i = ItemCount-1;

        for ( ; i > 0; i--)
        {
            var result = new SelectableResult();
            OnCheckSelectable?.Invoke(table, i, selectedSubIndex, result);
            if (result.Enabled == true)
            {
                return i;
            }
        }

        return i;
    }

    /// <summary>
    /// TableSubNodeElement 的位置设置 
    /// </summary>
    /// <param name="subIndex"></param>
    public void SetSubIndex(int subIndex)
    {
        if (selectedNodeGroup != null)
        {
            NodeGroup group = selectedNodeGroup;
            if (group.Node.GetSubIndexMax() == 0)
            {
                subIndex = TableNodeElement.SUBINDEX_ROOT;
            }
            else
            {
                subIndex = Mathf.Clamp(subIndex, 0, group.Node.GetSubIndexMax()-1);
            }

            selectedSubIndex = subIndex;

            group.Node.SetSubIndex(subIndex);
            group.Node.SetFocus(true, false);
        }
    }

    /// <summary>
    /// TableSubNodeElement 的位置增减
    /// </summary>
    /// <param name="amount"></param>
    public void AddSubIndex(int amount)
    {
        if (selectedNodeGroup != null)
        {
            NodeGroup group = selectedNodeGroup;
            SetSubIndex(group.Node.GetSubIndex() + amount);
        }
    }

    /// <summary>
    /// 确认是否通过 SetSelectedIndex() 移动了光标
    /// </summary>
    /// <returns>true .. 通过 SetSelectedIndex() 移动了光标</returns>
    public bool CheckCallSetSelectedIndex()
    {
        return reserveSelectedIndex >= 0;
    }

    /// <summary>
    /// 更新显示内容
    /// </summary>
    public void Refresh(bool forceRefresh = false)
    {
        if (checkUsable() == false)
        {
            return;
        }
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group  = nodeGroups[i];
            if (forceRefresh == true || group.Rect.gameObject.activeInHierarchy == true)
            {
                group.Node.Refresh();
            }
        }
    }

    /// <summary>
    /// 更新显示内容（单行）
    /// </summary>
    public void Refresh(int index, bool forceRefresh = false)
    {
        if (checkUsable() == false)
        {
            return;
        }
        if (nodeIndex.ContainsKey(index) == false)
        {
            return;
        }

        NodeGroup group = nodeIndex[index];
        if (forceRefresh == true || group.Rect.gameObject.activeInHierarchy == true)
        {
            group.Node.Refresh();
        }
    }

    /// <summary>
    /// 更新显示内容（单行）
    /// </summary>
    public void Refresh(object obj, bool forceRefresh = false)
    {
        if (checkUsable() == false)
        {
            return;
        }

        foreach (var pair in nodeIndex)
        {
            NodeGroup group = pair.Value;
            if (group.Node.GetItem() == obj)
            {
                if (forceRefresh == true || group.Rect.gameObject.activeInHierarchy == true)
                {
                    group.Node.Refresh();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 开始表格的修改（添加、删除）
    /// </summary>
    public void BeginUpdateTable()
    {
        // 创建用于修改的表格信息
        if (table == null)
        {
            Debug.LogError("table is null. cannot update.");
            return;
        }
        changeTable         = new List<object>(table);
        changeSelectedIndex = selectedIndex;
    }

    /// <summary>
    /// 完成表格的修改（添加、删除）
    /// </summary>
    public void EndUpdateTable()
    {
        SetTable(changeTable);
        SetSelectedIndex(changeSelectedIndex);

        // 清除用于修改的表格信息
        changeTable         = null;
        changeSelectedIndex = -1;
    }

    /// <summary>
    /// 添加表格行
    /// </summary>
    public void InsertRow(int index, object row)
    {
        if (changeTable == null)
        {
            Debug.LogError("no modify table. You need to call BeginTableModify().");
            return;
        }
        if (table.Count > 0 && table[0].GetType() != row.GetType())
        {
            Debug.LogError("does not match Table.");
            return;
        }

        index = index < 0 ? 0 : (index >= table.Count ? table.Count-1 : index);

        changeTable.Insert(index, row);
        changeSelectedIndex = index;
    }

    /// <summary>
    /// 添加表格行
    /// </summary>
    public void AddRow(object row)
    {
        if (changeTable == null)
        {
            Debug.LogError("no modify table. You need to call BeginTableModify().");
            return;
        }
        if (table.Count > 0 && table[0].GetType() != row.GetType())
        {
            Debug.LogError("does not match Table.");
            return;
        }

        changeTable.Add(row);
        changeSelectedIndex = changeTable.Count-1;
    }

    /// <summary>
    /// 删除表格行
    /// </summary>
    public void RemoveRow(object row)
    {
        if (changeTable == null || changeTable.Count == 0)
        {
            return;
        }
        
        // 查找要删除的行
        int index = changeTable.FindIndex( (a) => a == row );

        changeTable.RemoveAt(index);

        if (index < changeSelectedIndex)
        {
            // 如果光标位置上方被删除，光标也向上移动1行
            changeSelectedIndex -= 1;
        }
        else
        if (changeSelectedIndex >= changeTable.Count-1)
        {
            // 如果超过最后一行，则对齐到最后一行
            changeSelectedIndex = changeTable.Count-1;
        }
    }

    /// <summary>
    /// 输入允许、禁止
    /// </summary>
    /// <param name="enabled">true..允许，false..禁止</param>
    public void InputEnabled(bool enabled)
    {
        CanvasGroup.blocksRaycasts = enabled;
    }

    /// <summary>
    /// 设置滚动灵敏度。为 0 时立即停止，为 1 时不停止
    /// </summary>
    /// <param name="rate">滚动灵敏度</param>
    public void SetDecelerationRate(float rate)
    {
        rate = Mathf.Clamp(rate, 0, 1);
        if (scrollRect != null)
        {
            scrollRect.decelerationRate = rate;
        }
    }

    /// <summary>
    /// 手柄控制的更新
    /// </summary>
    void Update()
    {
        if (checkUsable() == false)
        {
            return;
        }
        int selIndex = selectedIndex;

        keyDownArgs.Flag = eKeyMoveFlag.None;

        if (CheckBlockRaycasts() == true)
        {
            OnKeyDown?.Invoke(keyDownArgs);
        }

        if (keyDownArgs.Flag == eKeyMoveFlag.Select)
        {
            performSelect();
        }
        else
        if (keyDownArgs.Flag == eKeyMoveFlag.Cancel)
        {
            select(selectedIndex, TableNodeElement.SUBINDEX_ROOT, true);
        }
        else
        {
            eKeyMoveFlag[] keys = new eKeyMoveFlag[6];
            if (Orientation == eOrientation.Vertical)
            {
                keys[0] = eKeyMoveFlag.Up;
                keys[1] = eKeyMoveFlag.Down;
                keys[2] = eKeyMoveFlag.PageUp;
                keys[3] = eKeyMoveFlag.PageDown;
                keys[4] = eKeyMoveFlag.Left;
                keys[5] = eKeyMoveFlag.Right;
            }
            else
            {
                keys[0] = eKeyMoveFlag.Left;
                keys[1] = eKeyMoveFlag.Right;
                keys[2] = eKeyMoveFlag.PageLeft;
                keys[3] = eKeyMoveFlag.PageRight;
                keys[4] = eKeyMoveFlag.Up;
                keys[5] = eKeyMoveFlag.Down;
            }

            if (reserveSelectedIndex >= 0)
            {
                selIndex = reserveSelectedIndex;
            }
            else
            if (keyDownArgs.Flag == keys[0])
            {
                selIndex = indexLeft(selIndex - 1, selIndex);
            }
            else
            if (keyDownArgs.Flag == keys[1])
            {
                selIndex = indexRight(selIndex + 1, selIndex);
            }
            else
            if (keyDownArgs.Flag == keys[2])
            {
                selIndex = indexLeft(selIndex - SkipIndexByPageScroll, selIndex);
            }
            else
            if (keyDownArgs.Flag == keys[3])
            {
                selIndex = indexRight(selIndex + SkipIndexByPageScroll, selIndex);
            }
            else
            if (keyDownArgs.Flag == keys[4])
            {
                AddSubIndex(-1);
            }
            else
            if (keyDownArgs.Flag == keys[5])
            {
                AddSubIndex(1);
            }
            else
            if (keyDownArgs.Flag == eKeyMoveFlag.ToTop)
            {
                selIndex = indexTop(0, selIndex);
            }
            else
            if (keyDownArgs.Flag == eKeyMoveFlag.ToBottom)
            {
                selIndex = indexBottom(ItemCount-1, selIndex);
            }
        }

        if (selIndex != selectedIndex || reserveSelectedIndex >= 0)
        {
            if (Orientation == eOrientation.Vertical)
            {
                currentNormPos = scrollRect.verticalNormalizedPosition;
            }
            else
            {
                currentNormPos = scrollRect.horizontalNormalizedPosition;
            }
            targetNormPos = getTargetNormalizedPosition(selIndex);

            selectedIndex = selIndex;
            setFocus(selectedIndex);

            // SetSelectedIndex Mode
            var move = positionMoveMode;

            if (reserveSelectedIndex == -1)
            {
                // 普通选择的情况
                move = ePositionMoveMode.ScrollMove;
            }

            if (move == ePositionMoveMode.DontMove)
            {
                // no operation
            }
            else
            if (move == ePositionMoveMode.ScrollMove)
            {
                if (table.Count > 0)
                {
                    OnCursorMove?.Invoke(table, selectedIndex, selectedSubIndex, true);
                }

                focusIsAnimation = true;
                timeNormPos = Time.time;
            }
            else
            if (move == ePositionMoveMode.OneFrame)
            {
                if (table.Count > 0)
                {
                    OnCursorMove?.Invoke(table, selectedIndex, selectedSubIndex, false);
                }

                timeNormPos = Time.time - ScrollTime;
            }
        }
        reserveSelectedIndex  = -1;

        // 朝目标减速移动
        if (timeNormPos > 0)
        {
            float t = Mathf.Clamp(Time.time - timeNormPos, 0, ScrollTime);
            float pos;

            if (t >= ScrollTime)
            {
                pos = targetNormPos;
                timeNormPos = 0;
            }
            else
            {
                pos = currentNormPos + (targetNormPos - currentNormPos) * cubicOut(t / ScrollTime);
            }

            if (Orientation == eOrientation.Vertical)
            {

                scrollRect.verticalNormalizedPosition = pos;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = pos;
            }
        }

        updateScrollbar();

        if (Orientation == eOrientation.Vertical)
        {
            scrollSize = rectGetHeight(scrollRectTransform);
        }
        else
        {
            scrollSize = rectGetWidth(scrollRectTransform);
        }
//viewerScroll(spos, false);
    }

    /// <summary>
    /// OnBeginDrag
    /// </summary>
    public void OnBeginDrag(PointerEventData data)
    {
        // 仅限左键（与 ScrollRect 主体的拖拽条件保持一致）
        if (data.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (checkUsable() == false)
        {
            return;
        }
        // 用户开始拖拽，停止吸附
        if (co_autoTarget != null)
        {
            StopCoroutine(co_autoTarget);
        }
    }

    /// <summary>
    /// OnEndDrag
    /// </summary>
    public void OnEndDrag(PointerEventData data)
    {
        // 仅限左键（与 ScrollRect 主体的拖拽条件保持一致）
        if (data.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (checkUsable() == false)
        {
            return;
        }
        // 应该不需要，但以防万一
        if (co_autoTarget != null)
        {
            StopCoroutine(co_autoTarget);
        }

        // 拖拽完成后，启动自动吸附协程
        if (AdsorptionTarget == true)
        {
            co_autoTarget = StartCoroutine(autoTarget());
        }
    }

    /// <summary>
    /// 包含父对象在内，确认 blockRaycasts
    /// </summary>
    public bool CheckBlockRaycasts()
    {
        if (checkUsable() == false)
        {
            return false;
        }
        foreach (CanvasGroup group in parentGroups)
        {
            if (group.blocksRaycasts == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 吸附主体
    /// </summary>
    IEnumerator autoTarget()
    {
        while (true)
        {
            Vector2 velocity = scrollRect.velocity;
            float   v;
            // 等待速度降到一定值以下
            if (Orientation == eOrientation.Vertical)
            {
                v = velocity.y;
            }
            else
            {
                v = velocity.x;
            }
            if (Math.Abs(v) < 750)
            {
                break;
            }
            yield return null;
        }

        int visibleNodeCount = 0;
        foreach (var node in nodeGroups)
        {
            if (node.Object.activeSelf == true)
            {
                visibleNodeCount++;
            }
        }

        int   sel0 = itemStart + visibleNodeCount/2;
        int   sel1 = itemStart + visibleNodeCount/2 - 1;
        if (sel0 < 0)
        {
            yield break;
        }
        if (sel1 < 0)
        {
            sel1 = sel0;
        }
        float tgt0 = getTargetNormalizedPosition(sel0);
        float tgt1 = getTargetNormalizedPosition(sel1);

        float pos;
        if (Orientation == eOrientation.Vertical)
        {
            pos = scrollRect.verticalNormalizedPosition;
        }
        else
        {
            pos = scrollRect.horizontalNormalizedPosition;
        }

        float targetPos;

        if (pos < 0)
        {
            targetPos = 0;
        }
        else
        if (pos > 1)
        {
            targetPos = 1;
        }
        else
        // 吸附到更近的一方
        if (Math.Abs(tgt0 - pos) < Math.Abs(tgt1 - pos))
        {
            targetPos = tgt0;
        }
        else
        {
            targetPos = tgt1;
        }

        float time = Time.time;

        while (true)
        {
            float t = Time.time - time;
            float p;

            if (t >= ScrollTime)
            {
                p = targetPos;
            }
            else
            {
                p = pos + (targetPos - pos) * cubicOut(t / ScrollTime);
            }

            if (Orientation == eOrientation.Vertical)
            {
                scrollRect.verticalNormalizedPosition = p;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = p;
            }

            if (t >= ScrollTime)
            {
                break;
            }
            yield return null;
        }

        co_autoTarget = null;
    }

//Vector2 spos;
    /// <summary>
    /// 滚动时调用
    /// </summary>
    void onValueChanged(Vector2 pos)
    {
//spos = pos;
        viewerScroll(pos, false);
    }

    int findIndexOfNextLargerNumber(float n, List<RowDisplay> list)
    {
        int left = 0;
        int right = list.Count - 1;
        int result = 0;

        while (left <= right)
        {
            int mid = left + ((right - left) / 2);

            if (list[mid].LastPosition <= n)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
                result = mid;
            }
        }

        return result;
    }

    void viewerScroll(Vector2 pos, bool initialize)
    {
        if (checkUsable() == false)
        {
            return;
        }

        // 根据当前 Content 上的显示位置，获取应显示在最顶部的行号
        float top;
        int   itemIndex;

        if (Orientation == eOrientation.Vertical)
        {
            top =  scrollRect.content.transform.localPosition.y;
        }
        else
        {
            top = -scrollRect.content.transform.localPosition.x;
        }

        itemIndex = findIndexOfNextLargerNumber(top, rowDisplays);

        if (nodeIndex == null)
        {
            nodeIndex = new Dictionary<int, NodeGroup>();

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                NodeGroup group  = nodeGroups[i];
                int       rindex = itemIndex + i;

                redrawNode(group, rindex);
                nodeIndex.Add(rindex, group);
            }
        }
        else
        {
            var nindex = new Dictionary<int, NodeGroup>();

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                int rindex = itemIndex + i;
                if (nodeIndex.ContainsKey(rindex) == true)
                {
                    nindex.Add(rindex, nodeIndex[rindex]);
                    nodeIndex.Remove(rindex);
                }
                else
                {
                    nindex.Add(rindex, null);
                }
            }

//DDisp.Log($"{spos.y} {top} {rectGetWidth(scrollRectTransform)} {rectGetHeight(scrollRectTransform)}");
            var blankGroup = new List<NodeGroup>();
            foreach (var pair in nodeIndex)
            {
                blankGroup.Add(pair.Value);
            }

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                int rindex = itemIndex + i;
                if (nindex[rindex] != null)
                {
                    // 保持不变
                }
                else
                {
                    nindex[rindex] = blankGroup[0];
                    blankGroup.RemoveAt(0);

                    redrawNode(nindex[rindex], rindex);
                }

                var group = nindex[rindex];

                if (Orientation == eOrientation.Vertical)
                {
                    float y = rectGetY(group.Rect) + top;
                    float nodeSizeHalf = rectGetHeight(group.Rect) * 0.5f;
//DDisp.Log($"{rindex} {group.Node.GetItemIndex()} {group.Object.name} {y <= -rectGetHeight(scrollRectTransform) - nodeSizeHalf} {y >= nodeSizeHalf} {y} {nodeSizeHalf} {-rectGetHeight(scrollRectTransform) - nodeSizeHalf}");
                    if (y <= -rectGetHeight(scrollRectTransform) - nodeSizeHalf || y >= nodeSizeHalf)
                    {
                        group.Object.SetActive(false);
//DDisp.Log($"kieta {group.Object.name}");
                    }
                    else
                    {
                        group.Object.SetActive(true);
                    }
                }
                else
                {
                    float x = rectGetX(group.Rect) - top;
                    float nodeSizeHalf = rectGetWidth(group.Rect) * 0.5f;
//DDisp.Log($"{rindex} {group.Node.GetItemIndex()} {group.Object.name} {x <= -nodeSizeHalf} {x >= nodeSizeHalf} {x} {nodeSizeHalf} {rectGetWidth(scrollRectTransform) + nodeSizeHalf}");

                    if (x <= -nodeSizeHalf || x >= rectGetWidth(scrollRectTransform) + nodeSizeHalf)
                    {
                        group.Object.SetActive(false);
//DDisp.Log($"kieta {group.Object.name}");
                    }
                    else
                    {
                        group.Object.SetActive(true);
                    }
                }
            }

            nodeIndex = nindex;
        }
        
        if (initialize == false)
        {
            setFocus(selectedIndex);
        }
        else
        {
            setFocus(-1);
        }

        itemStart = itemIndex;
    }

    /// <summary>
    /// 重绘节点组
    /// </summary>
    /// <param name="group">节点组</param>
    /// <param name="rindex">从表格顶部起第几行</param>
    void redrawNode(NodeGroup group, int rindex)
    {
        if (rindex < 0 || rindex >= ItemCount)
        {
            // out of range
        }
        else
        {
            group.Object.SetActive(true);
            group.Node.SetItemIndex(rindex);

            if (Orientation == eOrientation.Vertical)
            {
                rectSetY(group.Rect, -(getPos(rindex)));
            }
            else
            {
                rectSetX(group.Rect,   getPos(rindex) - group.Rect.rect.x);
            }
        }
    }

    int indexTop(int selIndex, int currentIndex)
    {
        int i = selIndex;

        for ( ; i < currentIndex; i++)
        {
            var result = new SelectableResult();
            OnCheckSelectable?.Invoke(table, i, selectedSubIndex, result);
            if (result.Enabled == true)
            {
                break;
            }
        }

        return i;
    }

    int indexBottom(int selIndex, int currentIndex)
    {
        int i = selIndex;

        for ( ; i > currentIndex; i--)
        {
            var result = new SelectableResult();
            OnCheckSelectable?.Invoke(table, i, selectedSubIndex, result);
            if (result.Enabled == true)
            {
                break;
            }
        }

        return i;
    }

    int indexLeft(int selIndex, int currentIndex)
    {
        int i = selIndex;

        for ( ; ; i--)
        {
            if (i < 0)
            {
                i = ItemCount-1;
            }
            if (i == currentIndex)
            {
                break;
            }

            var result = new SelectableResult();
            OnCheckSelectable?.Invoke(table, i, selectedSubIndex, result);
            if (result.Enabled == true)
            {
                break;
            }
        }
        
        return i;
    }

    int indexRight(int selIndex, int currentIndex)
    {
        int i = selIndex;

        for ( ; ; i++)
        {
            if (i >= ItemCount)
            {
                i = 0;
            }
            if (i == currentIndex)
            {
                break;
            }

            var result = new SelectableResult();
            OnCheckSelectable?.Invoke(table, i, selectedSubIndex, result);
            if (result.Enabled == true)
            {
                break;
            }
        }

        return i;
    }

    /// <summary>
    /// 鼠标光标进入选项时调用
    /// </summary>
    /// <param name="searchkey"></param>
    /// <param name="click">true..点击，false..按键操作</param>
    void nodeEnter(TableNodeElement searchkey, bool click)
    {
        if (click == true && touchEnabled == false)
        {
            return;
        }

        if (timeNormPos > 0)
        {
            // 按键选择（移动）期间禁止鼠标事件
            return;
        }

        NodeGroup search = null;
        if (searchkey != null && nodeSearch.ContainsKey(searchkey) == true)
        {
            search = nodeSearch[searchkey];
        }

        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group = nodeGroups[i];
            if (group == search)
            {
                if (selectedIndex != group.Node.GetItemIndex())
                {
                    selectedIndex = group.Node.GetItemIndex();
                    OnCursorMove?.Invoke(table, selectedIndex, selectedSubIndex, true);
                }

                selectedNodeGroup = group;
                group.Node.SetFocus(true, true);
            }
            else
            {
                group.Node.SetFocus(false);
            }
        }

    }
    
    /// <summary>
    /// 被选择时调用
    /// </summary>
    /// <param name="node"></param>
    /// <param name="click">true..点击，false..按键操作</param>
    void nodeClick(TableNodeElement node, bool click)
    {
        if (click == true && touchEnabled == false)
        {
            return;
        }

        select(node.GetItemIndex(), node.GetSubIndex(), false);
    }

    /// <summary>
    /// 选择（或取消）
    /// </summary>
    /// <param name="itemIndex">被选中的行</param>
    /// <param name="subIndex"></param>
    /// <param name="isCancel">取消时为 true</param>
    void select(int itemIndex, int subIndex, bool isCancel)
    {
        selectedSubIndex = subIndex;
        OnSelect?.Invoke(table, itemIndex, subIndex, isCancel);

        if (DisabledAfterSelect == true)
        {
            CanvasGroup.blocksRaycasts = false;
        }
    }
    
    /// <summary>
    /// 焦点设置
    /// </summary>
    void setFocus(int selIndex)
    {
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group = nodeGroups[i];

            if (selIndex >= 0 && selIndex == group.Node.GetItemIndex())
            {
                selectedNodeGroup = group;

                group.Node.SetSubIndex(selectedSubIndex);
                group.Node.SetFocus(true, focusIsAnimation);
                focusIsAnimation = false;
            }
            else
            {
                group.Node.SetFocus(false);
            }
        }
    }

    /// <summary>
    /// 选项选择
    /// </summary>
    void performSelect()
    {
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group = nodeGroups[i];

            if (group.Node.CheckFocus() == true)
            {
                group.Node.PerformClick(group.Node.GetSubIndex());
                break;
            }
        }
    }

    /// <summary>
    /// 检查是否有使用所需的最低限度设置
    /// </summary>
    /// <returns>true..可用</returns>
    bool checkUsable()
    {
        if (CanvasGroup == null)
        {
            return false;
        }
        if (SourceNode == null)
        {
            return false;
        }
        if (table == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 根据行号计算并返回项应位于 Content 的哪个位置
    /// </summary>
    float getPos(int no)
    {
        if (no < 0 || no >= rowDisplays.Count)
        {
            return paddingTop;
        }
        else
        {
            return paddingTop + rowDisplays[no].Position;
        }
    }

    /// <summary>
    /// 计算指定项序号的 NormalizedPosition
    /// </summary>
    /// <param name="selIndex">项序号</param>
    /// <returns>Target Normalized Position</returns>
    float getTargetNormalizedPosition(int selIndex)
    {
        float contentCenter;
        
        if (Orientation == eOrientation.Vertical)
        {
            contentCenter = contentSize - getPos(selIndex); // - nodeSize * 0.5f;
        }
        else
        {
            var rowdisp = rowDisplays[selIndex];
            contentCenter = contentSize - getPos(selIndex) - rowdisp.Size / 2; // - nodeSize * 0.5f;
        }
        float scrollCenter  = scrollSize * 0.5f;

        if (Orientation == eOrientation.Vertical)
        {
            return Mathf.Clamp01((contentCenter - scrollCenter) / (contentSize - scrollSize));
        }
        else
        {
            return 1 - Mathf.Clamp01((contentCenter - scrollCenter) / (contentSize - scrollSize));
        }
    }
    
    /// <summary>
    /// rect control
    /// </summary>
    float rectGetWidth(RectTransform rect)
    {
        return rect.rect.size.x;
    }
    float rectGetHeight(RectTransform rect)
    {
        return rect.rect.size.y;
    }
    void rectSetWidth(RectTransform rect, float width)
    {
        var size = rect.sizeDelta;
        size.x = width;
        rect.sizeDelta = size;
    }
    void rectSetHeight(RectTransform rect, float height)
    {
        var size = rect.sizeDelta;
        size.y = height;
        rect.sizeDelta = size;
    }
    void rectSetX(RectTransform rect, float x)
    {
        Vector3 trans = rect.gameObject.transform.localPosition;
        trans.x = x;
        rect.gameObject.transform.localPosition = trans;
    }
    void rectSetY(RectTransform rect, float y)
    {
        Vector3 trans = rect.gameObject.transform.localPosition;
        trans.y = y;
        rect.gameObject.transform.localPosition = trans;
    }
    float rectGetX(RectTransform rect)
    {
        return rect.gameObject.transform.localPosition.x;
    }
    float rectGetY(RectTransform rect)
    {
        return rect.gameObject.transform.localPosition.y;
    }

    float cubicOut(float t)
    {
        t -= 1;
        float v = t * t * t + 1;
        return v;
    }

}
}
