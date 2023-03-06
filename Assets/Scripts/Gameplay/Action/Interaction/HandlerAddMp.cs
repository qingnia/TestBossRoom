using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace Interaction
{
    [Serializable]
    public class HandlerAddMp : BaseHandler
    {
        [SerializeField]
        private int addManaNum;

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
            if(go.TryGetComponent(out ServerCharacter serverCharacter))
            {
                Debug.Log("handler add Hp, " + go.name + ", add:" + addManaNum);
                serverCharacter.ManaPoints.Value += addManaNum;
            }
        }
    }
}
