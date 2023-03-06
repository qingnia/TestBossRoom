using UnityEngine;

namespace Interaction
{
    public class HandlerFly : BaseHandler
    {
        [SerializeField]
        private int sencondTime;
        [SerializeField]
        private int sencondSpeed;

        private bool canFly = false;
        // Start is called before the first frame update
        void Start()
        {

        }

        private void FixedUpdate()
        {
            if (!canFly)
            {
                return;
            }
            Vector3 dir = target.transform.position - from.transform.position;
            dir = dir.normalized;
            from.transform.position += dir * Time.deltaTime;
            if (Vector3.Distance(from.transform.position, target.transform.position) < 0.5)
            {
                canFly = false;
            }
        }

        protected override void Excute()
        {
            canFly = true;
        }
    }
}
