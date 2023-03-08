using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int index;
    public object data;
    public UIList m_parent;
    public Dictionary<string, Transform> m_childName;
    public void refData()
    {
        index = 0;
        data = null;
        m_parent = null;
        m_childName = new Dictionary<string, Transform>();
    }
}
public struct ItemDate
{
    public int index;
    public object data;
    public UIList m_parent;
}
public class UIList : ScrollRect
{
    RectTransform rectTr;

    private float m_width;
    private float m_height;
    private List<ItemDate> m_datas;
    private RectTransform m_Skin;
    private List<RectTransform> comList;
    private int startIndex;
    private int endIndex;
    /// <summary>
    /// 宽高改变时触发
    /// </summary>
    public event Action OnChangeViewWH;
    /// <summary>
    /// 数据跟新时调用
    /// </summary>
    public event Action<Item> OnUpdateItem;
    /// <summary>
    /// 距离顶部
    /// </summary>
    private float _m_top;
    public float m_top
    {
        set { _m_top = value; }
    }
    /// <summary>
    /// 距离底部
    /// </summary>
    private float _m_down;
    public float m_down
    {
        set { _m_down = value; }
    }
    /// <summary>
    /// 距离左部
    /// </summary>
    private float _m_left;
    public float m_left
    {
        set { _m_left = value; }
    }
    /// <summary>
    /// 间距
    /// </summary>
    private float _Spacing;
    public float Spacing
    {
        get { return _Spacing; }
        set { _Spacing = value; }
    }
    /// <summary>
    /// 文本测量高度和文本测量宽度
    /// </summary>
    private Vector2 _sizeData;
    public Vector2 sizeData
    {
        get { return _sizeData; }
    }
    override protected void Awake()
    {
        base.Awake();
        rectTr = GetComponent<RectTransform>();
        comList = new List<RectTransform>();
        m_datas = new List<ItemDate>();
        startIndex = 0;
        _Spacing = 10;
        m_width = rectTr.sizeDelta.x;
        m_height = rectTr.sizeDelta.y;
        OnChangeViewWH += ChangeViewWH;
        OnUpdateItem += UpdataChild;
        onValueChanged.AddListener(OnChange);
    }
    override protected void Start()
    {
        base.Start();

    }
    // Update is called once per frame
    void Update()
    {
        if (m_width != rectTr.sizeDelta.x || m_height != rectTr.sizeDelta.y)
        {
            m_width = rectTr.sizeDelta.x;
            m_height = rectTr.sizeDelta.y;
            //调用宽高改变时触发
            OnChangeViewWH.DynamicInvoke();
        }
    }
    /// <summary>
    /// 设置list子控件皮肤
    /// </summary>
    public RectTransform setSkin
    {
        set
        {
            m_Skin = value;
        }
    }
    /// <summary>
    /// 设置数据源
    /// </summary>
    /// <param name="datas"></param>
    public void setData<T>(List<T> datas)
    {
        if (m_Skin == null)
        {
            Debug.LogError("Item皮肤没有设置");
        }
        //初始化数据
        this.m_datas.Clear();
        this.content.localPosition = new Vector3(this.content.localPosition.x, 0, 0);
        startIndex = 0;
        int len = datas.Count;
        //计算容器宽度+距离底部的距离
        _sizeData = new Vector2(m_Skin.sizeDelta.x, (m_Skin.sizeDelta.y * len + _Spacing * len - _Spacing) + _m_down + _m_top);
        //更新数据
        for (int i = 0; i < datas.Count; i++)
        {
            ItemDate data = new ItemDate();
            data.index = i;
            data.data = datas[i];
            data.m_parent = this;
            this.m_datas.Add(data);
        }
        //宽高发生改变
        ChangeViewWH();
    }
    /// <summary>
    /// 储存ui成员变量
    /// </summary>
    /// <param name="skin"></param>
    /// <returns></returns>
    private Dictionary<string, Transform> setUIChildName(Transform skin)
    {
        Dictionary<string, Transform> map = new Dictionary<string, Transform>();
        for (int i = 0; i < skin.childCount; i++)
        {
            Transform childobj = skin.GetChild(i);
            map[childobj.gameObject.name] = childobj;
            if (childobj.childCount > 0)
            {
                setUIChildName(childobj);
            }
        }
        return map;
    }
    /// <summary>
    /// 宽高发生改变时
    /// </summary>
    public void ChangeViewWH()
    {
        //向上取整得到数量
        int colunm = Mathf.CeilToInt((m_height + _Spacing) / (m_Skin.sizeDelta.y + _Spacing));
        endIndex = startIndex + colunm;
        //超出的删除
        if (comList.Count > colunm + 1 && colunm > 0)
        {
            for (int s = colunm + 1; s < comList.Count; s++)
            {
                Destroy(comList[s].gameObject);
                comList.RemoveAt(s);
            }
            return;
        }
        //生成
        if (colunm <= m_datas.Count)
        {
            for (int i = comList.Count; i <= colunm; i++)
            {
                //加个保护
                if (i >= this.m_datas.Count) break;

                RectTransform skin = Instantiate(m_Skin);
                //添加item
                skin.gameObject.AddComponent<Item>();
                comList.Add(skin);
                //设置父节点
                skin.SetParent(this.content, false);
            }
        }
        //更新所有item
        updateItem();
        //赋值文本测量高度
        this.content.sizeDelta = new Vector2(this.content.sizeDelta.x, _sizeData.y);
    }
    /// <summary>
    /// 数据初始化
    /// </summary>
    private void updateItem()
    {
        for (int i = 0; i < comList.Count; i++)
        {
            //计算item的坐标，以距离左边和距离上边为基准
            comList[i].localPosition = new Vector3(_m_left, -(_m_top + i * (m_Skin.sizeDelta.y + _Spacing)), 0);
            updataView(comList[i], i);
        }
    }
    /// <summary>
    /// 刷新数据--根据item刷新
    /// </summary>
    /// <param name="_comList"></param>
    /// <param name="i"></param>
    private void updataView(RectTransform _comList, int i)
    {
        Item data = _comList.gameObject.GetComponent<Item>();
        data.index = this.m_datas[i].index;
        data.data = this.m_datas[i].data;
        data.m_parent = this.m_datas[i].m_parent;
        data.m_childName = setUIChildName(_comList);
        OnUpdateItem.DynamicInvoke(data);
    }
    /// <summary>
    /// 滑动改变时
    /// </summary>
    /// <param name="pos"></param>
    private void OnChange(Vector2 pos)
    {
        int colunm = endIndex - startIndex;
        int curindex = Mathf.FloorToInt(this.content.localPosition.y / (m_Skin.sizeDelta.y + _Spacing));
        if (curindex > startIndex && endIndex + 1 < this.m_datas.Count)
        {
            //符合当前的item
            for (int i = 0; i < comList.Count; i++)
            {
                Item data = comList[i].gameObject.GetComponent<Item>();
                if (data.index == startIndex)
                {
                    comList[i].localPosition = new Vector3(_m_left, -_m_top - (endIndex + 1) * (m_Skin.sizeDelta.y + _Spacing), 0);
                    updataView(comList[i], endIndex + 1);
                    break;
                }
            }
            startIndex++;
            endIndex = startIndex + colunm;
        }
        else if (curindex < startIndex && startIndex - 1 >= 0 && pos.y < 1)
        {
            for (int i = 0; i < comList.Count; i++)
            {
                Item data = comList[i].gameObject.GetComponent<Item>();
                if (data.index == endIndex)
                {
                    comList[i].localPosition = new Vector3(_m_left, -_m_top - (startIndex - 1) * (m_Skin.sizeDelta.y + _Spacing), 0);
                    //刷新单个数据
                    updataView(comList[i], startIndex - 1);
                    break;
                }
            }
            startIndex--;
            endIndex = startIndex + colunm;
        }
    }
    public virtual void UpdataChild(Item data) { }
}

