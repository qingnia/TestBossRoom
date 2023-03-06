using UnityEngine;

namespace Interaction
{
    public class TriggerCollision : BaseTrigger
    {
        private void Start()
        {
            if (IsClient)
            {
                //gameObject.GetComponent<Collider>().enabled = false;
            }
        }

        [SerializeField]
        private bool triggerOnce = false;
        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce)
            {
                gameObject.GetComponent<Collider>().enabled = false;
            }

            Debug.LogWarning("trigger enter");
            TriggerInteraction(gameObject, other.gameObject);
        }
    }
}
