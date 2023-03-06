using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public enum HandlerPot
    {
        Server,
        ClientAll,
        ClientTarget
    }
    public enum TargetType
    {
        Self,
        Interacter
    }

    public abstract class BaseHandler : NetworkBehaviour
    {
        [SerializeField]
        protected TargetType targetType;
        [SerializeField]
        public HandlerPot handlerPot;

        private List<BaseHandler> m_BaseHandlers;

        protected GameObject self;
        protected GameObject interacter;

        protected virtual void Init(GameObject self, GameObject interacter)
        {
            this.self = self;
            this.interacter = interacter;
        }

        protected virtual bool CanHanler()
        {
            if (IsServer)
            {

            }
            return false;
        }
        protected virtual void HandlerEnd()
        {
            if (m_BaseHandlers != null)
            {
                foreach (BaseHandler handler in m_BaseHandlers)
                {
                    handler.HandlerInit(self, interacter);
                }
            }
        }

        protected virtual GameObject GetObj()
        {
            if (targetType == TargetType.Self)
            {
                return self;
            }
            else
            {
                return interacter;
            }
        }
        protected abstract void Excute();
        public void HandlerInit(GameObject self, GameObject interacter)
        {
            Init(self, interacter);
            Excute();
        }
    }
}
