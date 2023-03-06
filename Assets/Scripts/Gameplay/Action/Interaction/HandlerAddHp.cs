using System;
using System.Collections;
using System.Collections.Generic;
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

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void Excute(GameObject from, GameObject target)
        {
            GameObject go;
            if (targetType == TargetType.From)
            {
                go = from;
            }
            else
            {
                go = target;
            }
            if (go.TryGetComponent(out IDamageable damage))
            {
                Debug.Log("handler add Mp, " + go.name + ", add:" + addHpNum);
                // actually deal the damage
                damage.ReceiveHP(target.GetComponent<ServerCharacter>(), addHpNum);
            }
        }
    }
}
