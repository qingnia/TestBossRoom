using Interaction;
using UnityEngine;

namespace Interaction
{
    public class TriggerClick : BaseTrigger
    {
        public void OnTriggerClick(GameObject target)
        {
            Debug.LogWarning("trigger click");
            TriggerInteraction(gameObject, target);
        }
    }
}
