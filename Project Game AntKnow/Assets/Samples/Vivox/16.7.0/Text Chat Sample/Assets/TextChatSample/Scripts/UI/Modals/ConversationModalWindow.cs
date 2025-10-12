using System.Collections;
using System.Collections.Generic;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConversationModalWindow : MonoBehaviour
{
    public Text TitleText;
    public Text HintText;
    public Text ConfirmButtonText;
    public GameObject ModalWindow;
    public Button CancelButton;
    public Button ConfirmButton;
    public InputField ConversationNameInputField;

    public void ShowModal(ConversationType conversationType, UnityAction<string> createAction, UnityAction closeAction = null)
    {
        ModalWindow.SetActive(false);
        ConfirmButton.interactable = false;
        ConversationNameInputField.onValueChanged.AddListener(InputConversationName);
        switch (conversationType)
        {
            case ConversationType.ChannelConversation:
            {
                TitleText.text = "Create new channel";
                HintText.text = "Enter channel name";
                ConfirmButtonText.text = "Create";
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                TitleText.text = "Send new message";
                HintText.text = "Enter Player ID";
                ConfirmButtonText.text = "OK";
            }
            break;
            default:
                break;
        }
        ModalWindow.SetActive(true);
        ConversationNameInputField.text = string.Empty;
        ConversationNameInputField.Select();
        ConversationNameInputField.ActivateInputField();
        ConfirmButton.onClick.AddListener(() => CreateButtonClick(createAction));
        CancelButton.onClick.AddListener(closeAction ?? CloseModal);
    }

    void CreateButtonClick(UnityAction<string> createAction)
    {
        createAction?.Invoke(ConversationNameInputField.text);
        CloseModal();
    }

    public void CloseModal()
    {
        ConfirmButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();
        ConversationNameInputField.onValueChanged.RemoveAllListeners();
        ModalWindow.SetActive(false);
    }

    private void InputConversationName(string conversationName)
    {
        ConfirmButton.interactable = !string.IsNullOrEmpty(conversationName);
    }
}
