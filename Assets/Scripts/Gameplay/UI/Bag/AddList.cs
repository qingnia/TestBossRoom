using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dataa
{
    public string name;
    public int index;
}
public class AddList : MonoBehaviour
{
    public UIList list;
    public GameObject skin;
    public Button btn;
    // Start is called before the first frame update
    void Start()
    {
        list.setSkin = skin.GetComponent<RectTransform>();
        List<dataa> dic = new List<dataa>();
        for (int i = 0; i < 10; i++)
        {
            dataa date = new dataa();
            date.name = "名字" + i;
            date.index = i;
            dic.Add(date);
        }
        list.OnUpdateItem += change;
        list.m_left = 10;
        list.m_top = 20;
        list.m_down = 30;
        list.setData(dic);
        List<dataa> dica = new List<dataa>();
        btn.onClick.AddListener(delegate ()
        {
            dica.Clear();
            for (int i = 0; i < 10; i++)
            {
                dataa date = new dataa();
                date.name = "已经改变";
                date.index = i;
                dica.Add(date);
            }
            list.setData(dica);
        });
    }
    void change(Item item)
    {
        TextMeshProUGUI text = item.m_childName["Text"].GetComponent<TextMeshProUGUI>();
        Button on_btn = item.m_childName["Button"].GetComponent<Button>();
        text.text = "xx" + (item.data as dataa).index + "--" + (item.data as dataa).name;
        //按钮点击消息事件
        on_btn.onClick.AddListener(() =>
        {
            Debug.Log((item.data as dataa).index);
        });
    }
    void Update()
    {

    }
}

