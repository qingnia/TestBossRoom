using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public class BaseTrigger : NetworkBehaviour
    {
        [SerializeField]
        private List<BaseHandler> m_BaseHandlers; 

        //protected bool serverCheck()
        //{
        //    bool serverCheck, clientCheck;
        //    foreach (BaseHandler handler in m_BaseHandlers)
        //    {
        //        // todo: 点击事件是客户端触发，碰撞既可以客户端又可以服务器，场景事件只能是服务器
        //        // 所以要处理事件的同步，如果触发效果没有需要在另一端执行的，就不同步，否则就要同步过去执行
        //        // 暂时简单处理：点击都是客户端，其他都是服务器
        //        switch (handler.handlerPot)
        //        {
        //            case HandlerPot.Server: serverCheck = true; break;
        //            case HandlerPot.ClientAll: serverCheck = true; break;
        //            case HandlerPot.ClientTarget: clientCheck = true; break;
        //        }
        //        if (serverCheck && logicCanCheckServer)
        //        {

        //        }
        //    }
        //}

        protected virtual void TriggerInteraction(GameObject self, GameObject interacter)
        {
            foreach(BaseHandler handler in m_BaseHandlers)
            {
                // todo: 点击事件是客户端触发，碰撞既可以客户端又可以服务器，场景事件只能是服务器
                // 所以要处理事件的同步，如果触发效果没有需要在另一端执行的，就不同步，否则就要同步过去执行
                handler.HandlerInit(self, interacter);
            }
        }
    }

}
