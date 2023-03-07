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

        private TMP_InputField inputBox;
        private Button sendButton;
        // Start is called before the first frame update
        void Start()
        {
            inputBox = GameObject.Find("CharacterSelectCanvas/Chat/Input").GetComponent<TMP_InputField>();
            sendButton = GameObject.Find("CharacterSelectCanvas/Chat/Send").GetComponent<Button>();
            sendButton.onClick.AddListener(ClickSendButton);
        }

        // Update is called once per frame
        void Update()
        {

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
