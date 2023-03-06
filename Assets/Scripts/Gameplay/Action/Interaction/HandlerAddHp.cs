using System;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace Interaction
{
    [Serializable]
    public class HandlerAddHp : BaseHandler
    {
        [SerializeField]
        private int addHpNum;

        protected override void Excute()
        {
            GameObject go = GetObj();
            if (go.TryGetComponent(out IDamageable damage))
            {
                Debug.Log("handler add Mp, " + go.name + ", add:" + addHpNum);
                // actually deal the damage
                damage.ReceiveHP(interacter.GetComponent<ServerCharacter>(), addHpNum);
            }
        }
    }
}
