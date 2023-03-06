using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    public class HandlerPick : BaseHandler
    {
        [SerializeField]
        private int sencondSpeed;
        [SerializeField]
        private List<BaseHandler> m_BaseHandlers;

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
            Vector3 targetPos = interacter.transform.position + new Vector3(0, 1f, 0);
            Vector3 dir = targetPos - self.transform.position;
            dir = dir.normalized;
            self.transform.position += dir * sencondSpeed * Time.deltaTime;
            self.transform.localScale -= Vector3.one * Time.deltaTime;
            if (Vector3.Distance(self.transform.position, targetPos) < 0.5f)
            {
                HandlerEnd();
                canFly = false;
            }
        }

        protected override void Excute()
        {
            canFly = true;
        }
    }
}
