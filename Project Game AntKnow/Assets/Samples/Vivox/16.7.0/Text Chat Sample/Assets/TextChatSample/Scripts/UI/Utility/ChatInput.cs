using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ChatInput : MonoBehaviour
{
    public InputField MessageInputField;
    public Button SendMessageButton;

    void OnEnable()
    {
        ResetInputField();
        switch (TextSampleUIManager.Instance.CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
            {
                SendMessageButton.onClick.AddListener(async () => { await SendMessage(); });
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                SendMessageButton.onClick.AddListener(async () => { await SendMessage(); });
            }
            break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        SendMessageButton.onClick.RemoveAllListeners();
    }

    async Task SendMessage()
    {
        if (string.IsNullOrEmpty(MessageInputField.text))
        {
            return;
        }

        await TextSampleUIManager.Instance.SendVivoxMessage(MessageInputField.text);
        ResetInputField();
    }

    void ResetInputField()
    {
        MessageInputField.text = string.Empty;
        MessageInputField.Select();
        MessageInputField.ActivateInputField();
    }
}
