using System;
using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public enum TargetType
    {
        From,
        Target
    }

    [Serializable]
    public abstract class BaseHandler : NetworkBehaviour
    {
        [SerializeField]
        protected TargetType targetType;
        public abstract void Excute(GameObject from, GameObject target);
    }
}
