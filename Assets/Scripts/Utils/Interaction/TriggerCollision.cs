using UnityEngine;

namespace Interaction
{
    public class TriggerCollision : BaseTrigger
    {
        [SerializeField]
        Collider m_Collider;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning("trigger enter");
            TriggerInteraction(gameObject, other.gameObject);
        }
    }
}
