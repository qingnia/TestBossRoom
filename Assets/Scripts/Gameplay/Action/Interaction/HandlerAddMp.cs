using System;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace Interaction
{
    [Serializable]
    public class HandlerAddMp : BaseHandler
    {
        [SerializeField]
        private int addManaNum;

        protected override void Excute()
        {
            GameObject go = GetObj();
            if(go.TryGetComponent(out ServerCharacter serverCharacter))
            {
                Debug.Log("handler add Hp, " + go.name + ", add:" + addManaNum);
                serverCharacter.ManaPoints.Value += addManaNum;
            }
        }
    }
}
