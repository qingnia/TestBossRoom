using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.BossRoom.Infrastructure;
using TMPro;
using VContainer;

namespace Unity.BossRoom.Utils
{
    public class CommonChatHelper : NetworkBehaviour
    {
        [Inject]
        IPublisher<CommonChatMessage> m_ChatPublisher;

        public TMP_InputField inputBox;
        public Button sendButton;

        private void Awake()
        {
            sendButton.onClick.AddListener(ClickSendButton);
        }
        [Inject]
        public void Constructor(ChatPanelClass injectChatPanel)
        {
            sendButton = injectChatPanel.GetComponentInChildren<Button>();
            inputBox = injectChatPanel.GetComponentInChildren<TMP_InputField>();
        }

        public void ClickSendButton()
        {
            if (inputBox.text.Length > 0)
            {
                ChatSendServerRpc(inputBox.text);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void ChatSendServerRpc(string str)
        {
            ChatSendClientRpc(str);
        }

        [ClientRpc]
        void ChatSendClientRpc(string str)
        {
            if (IsClient)
            {
                Debug.Log("chat helper client rpc: " + gameObject.name);
                Debug.Log(str);
                m_ChatPublisher.Publish(new CommonChatMessage()
                {
                    message = str,
                    CharacterName = "aaa"
                });
            }
        }
    }

}
