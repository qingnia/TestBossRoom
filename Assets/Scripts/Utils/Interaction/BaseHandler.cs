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
        From,
        Target
    }

    public abstract class BaseHandler : NetworkBehaviour
    {
        [SerializeField]
        protected TargetType targetType;
        [SerializeField]
        public HandlerPot handlerPot;

        protected GameObject from;
        protected GameObject target;

        protected virtual void Init(GameObject from, GameObject target)
        {
            this.from = from;
            this.target = target;
        }

        protected virtual bool CanHanler()
        {
            if (IsServer)
            {

            }
            return false;
        }

        protected virtual GameObject GetObj()
        {
            if (targetType == TargetType.From)
            {
                return from;
            }
            else
            {
                return target;
            }
        }
        protected abstract void Excute();
        public void HandlerInit(GameObject from, GameObject target)
        {
            Init(from, target);
            Excute();
        }
    }
}
