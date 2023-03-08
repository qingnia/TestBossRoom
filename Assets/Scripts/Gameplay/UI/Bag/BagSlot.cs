using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagSlot : MonoBehaviour
{
    public int slotID;
    public BagItem SlotItem;
    public Image SlotImage;//物品图片
    public TextMeshProUGUI slotNum;
    public GameObject itemInslot;
    public string slotInfo;
    public void ItemOnClick()
    {
        InventoryManager.UpdateItemInfo(slotInfo);//点击该物品时，物品描述将会更新
    }

    public void SetupSlot(BagItem item)
    {
        if (item == null)//如果物品不存在，物品栏并不会激活
        {
            itemInslot.SetActive(false);
            return;
        }

        SlotImage.sprite = item.itemImage;
        slotNum.text = item.itemHeld.ToString();
        slotInfo = item.itemInfo;
    }
}
