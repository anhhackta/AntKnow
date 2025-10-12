using System;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMessageTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    const float k_LongPressInterval = 0.6f;

    public Text TileNameText;
    public Text TileMessageText;
    public Button EditOrDeleteButton;

    VivoxMessage parentMessage;

    float currentTouchDuration;
    bool isTouching;
    UnityAction longPressActionCallback;

    void Update()
    {
        if (longPressActionCallback != null)
        {
            if (isTouching)
            {
                currentTouchDuration += Time.deltaTime;
                if (currentTouchDuration >= k_LongPressInterval)
                {
                    longPressActionCallback?.Invoke();
                    isTouching = false;
                    currentTouchDuration = 0;
                }
            }
        }
    }

    private void OnDestroy()
    {
        VivoxService.Instance.ChannelMessageDeleted -= OnChannelMessageDeleted;
        VivoxService.Instance.ChannelMessageEdited -= OnChannelMessageEdited;
        VivoxService.Instance.DirectedMessageDeleted -= OnDMMessageDeleted;
        VivoxService.Instance.DirectedMessageEdited -= OnDMMessageEdited;
    }

    public void SetupMessageTile(VivoxMessage messageDetails, UnityAction longPressAction = null)
    {
        parentMessage = messageDetails;
        TileNameText.text = string.Format($"{parentMessage.SenderDisplayName}{(parentMessage.FromSelf ? " (Me)" : string.Empty)} <color=#999999><size=20>{parentMessage.ReceivedTime}</size></color>");
        TileMessageText.text = parentMessage.MessageText;
        longPressActionCallback = longPressAction;
        EditOrDeleteButton.interactable = longPressActionCallback != null;
        if (!string.IsNullOrEmpty(messageDetails.ChannelName))
        {
            VivoxService.Instance.ChannelMessageDeleted += OnChannelMessageDeleted;
            VivoxService.Instance.ChannelMessageEdited += OnChannelMessageEdited;
        }
        else
        {
            VivoxService.Instance.DirectedMessageDeleted += OnDMMessageDeleted;
            VivoxService.Instance.DirectedMessageEdited += OnDMMessageEdited;
        }
    }

    public void SetupLocalPlayerDMMessageTile(string message)
    {
        TileNameText.text = string.Format($"{TextSampleUIManager.Instance.SignedInPlayerDisplayName} (Me) <color=#999999><size=20>{DateTime.UtcNow.ToLocalTime()}</size></color>");
        TileMessageText.text = message;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouching = true;
        currentTouchDuration = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouching = false;
        currentTouchDuration = 0;
    }

    void UpdateMessage(VivoxMessage updatedMessage)
    {
        parentMessage = updatedMessage;
        TileNameText.text = string.Format($"{parentMessage.SenderDisplayName}{(parentMessage.FromSelf ? " (Me)" : string.Empty)} (Edited) <color=#999999><size=20>{parentMessage.ReceivedTime}</size></color>");
        TileMessageText.text = parentMessage.MessageText;
    }

    void OnDMMessageDeleted(VivoxMessage message)
    {
        if (message.MessageId != parentMessage.MessageId)
        {
            return;
        }

        Cleanup();
    }

    void OnChannelMessageEdited(VivoxMessage message)
    {
        if (message.MessageId != parentMessage.MessageId)
        {
            return;
        }

        UpdateMessage(message);
    }

    void OnChannelMessageDeleted(VivoxMessage message)
    {
        if (message.MessageId != parentMessage.MessageId)
        {
            return;
        }

        Cleanup();
    }


    void OnDMMessageEdited(VivoxMessage message)
    {
        if (message.MessageId != parentMessage.MessageId)
        {
            return;
        }

        UpdateMessage(message);
    }

    void Cleanup()
    {
        Destroy(gameObject);
    }
}
