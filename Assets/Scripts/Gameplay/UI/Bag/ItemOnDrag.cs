using UnityEngine;
using UnityEngine.EventSystems;

public class ItemOnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform orignalParent;
    private RectTransform m_RT;
    public BagInventory myBag;
    private int currentItemID;
    //开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        orignalParent = transform.parent;//记录物体原先的位置
        currentItemID = orignalParent.GetComponent<BagSlot>().slotID;
        transform.SetParent(transform.parent.parent);//把物体从父节点中分离，使其能够在最上层显示
        transform.position = eventData.position;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    //拖拽
    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = eventData.position;
        Vector3 pos;
        m_RT = gameObject.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(m_RT, eventData.position, eventData.enterEventCamera, out pos);
        m_RT.position = pos;
        //Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
    }
    //放开拖拽
    public void OnEndDrag(PointerEventData eventData)
    {
        //使用射线来检测物体下方是不是有物品，如果有物品则交换位置
        if (eventData.pointerCurrentRaycast.gameObject.name == "ItemImage")
        {
            transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform.parent.parent);
            transform.position = eventData.pointerCurrentRaycast.gameObject.transform.parent.parent.position;

            //itemList的物品存储位置改变。
            var temp = myBag.itemList[currentItemID];
            myBag.itemList[currentItemID] =
                myBag.itemList[eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<BagSlot>().slotID];
            myBag.itemList[eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<BagSlot>().slotID] = temp;

            eventData.pointerCurrentRaycast.gameObject.transform.parent.position = orignalParent.position;
            eventData.pointerCurrentRaycast.gameObject.transform.parent.SetParent(orignalParent);

            GetComponent<CanvasGroup>().blocksRaycasts = true;

            return;
        }
        //如果放开位置没有物品，则直接放下。
        transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform);
        transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;
        myBag.itemList[eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<BagSlot>().slotID] = myBag.itemList[currentItemID];
        myBag.itemList[currentItemID] = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

}
