using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;

    public BagInventory myBag; // 获取背包的信息
    public GameObject slotGrid;//获取Grid网格对象
                               //public Slot SlotPrefab;
    public GameObject emptySlot;//slot预制体
    public TextMeshProUGUI itemInformation;//物品描述
    public List<GameObject> slots = new List<GameObject>();//物品栏中的物品对象
    void Awake()//单例模式，保证只有一个实例
    {
        if (instance != null)
        {
            Destroy(this);
        }

        instance = this;
    }

    private void OnEnable()
    {
        RefreshItem();//每次启动程序时，刷新背包里的物品
        instance.itemInformation.text = "";//物品描述设置为空
    }

    public static void UpdateItemInfo(string itemDes)
    {
        instance.itemInformation.text = itemDes;//设置物品描述信息
    }

    public void CloseBag()
    {
        gameObject.SetActive(false);
    }

    public static void RefreshItem()
    {
        //先删除物品栏中的所有游戏对象，遍历Grid挂载的子对象
        for (int i = 0; i < instance.slotGrid.transform.childCount; i++)
        {

            if (instance.slotGrid.transform.childCount == 0)//如果Grid没有挂载子对象
            {
                break;
            }
            //每遍历一个就删除一个子对象
            Destroy(instance.slotGrid.transform.GetChild(i).gameObject);
            instance.slots.Clear();
        }
        //重新加载Invetory中的物品对象。
        for (int j = 0; j < instance.myBag.itemList.Count; j++)
        {
            instance.slots.Add(Instantiate(instance.emptySlot));
            instance.slots[j].transform.SetParent(instance.slotGrid.transform, false);
            instance.slots[j].GetComponent<BagSlot>().slotID = j;
            instance.slots[j].GetComponent<BagSlot>().SetupSlot(instance.myBag.itemList[j]);
        }

    }
}
