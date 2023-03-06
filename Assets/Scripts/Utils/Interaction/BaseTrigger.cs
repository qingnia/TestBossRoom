using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public class BaseTrigger : NetworkBehaviour
    {
        [SerializeField]
        private List<BaseHandler> m_BaseHandlers;

        protected virtual void TriggerInteraction(GameObject from, GameObject target)
        {
            foreach(BaseHandler handler in m_BaseHandlers)
            {
                handler.Excute(from, target);
            }
        }
    }

}
