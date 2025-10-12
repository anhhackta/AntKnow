using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ConversationUI : MonoBehaviour
{
    IList<KeyValuePair<string, GameObject>> m_MessageObjPool = new List<KeyValuePair<string, GameObject>>();

    public Sprite DMIcon;
    public Sprite ChannelIcon;
    public Image ConversationTypeIcon;
    public ScrollRect ConversationScrollView;
    public GameObject ConversationPanelUI;
    public GameObject PlayerMessageTilePrefab;
    public GameObject ConversationScrollViewContent;
    public GameObject ChannelRosterUI;
    public InputField MessageInputField;
    public Text TitleText;
    public Text InfoText;
    public Button LeaveConversationButton;
    public Button ViewChannelRosterButton;
    public GameObject EditOrDeleteModal;
    public GameObject EditMessageModal;
    public GameObject DeleteConfirmationModal;

    // Needed as a faux message ID since the local player doesn't get notified when their DM is sent to another person.
    int m_localPlayerDMIdCount = 0;
    ConversationType m_CurrentConversationParadigm => TextSampleUIManager.Instance.CurrentConversationParadigm;
    string m_CurrentConversationId;

    Task m_InProgressMessageHistoryFetch = null;
    DateTime? m_OldestHistoryMessage = null;

    void OnEnable()
    {
        TitleText.text = string.Empty;
        InfoText.text = string.Empty;
        MessageInputField.text = string.Empty;
        ConversationPanelUI.SetActive(false);
        EditOrDeleteModal.SetActive(false);
        EditMessageModal.SetActive(false);
        DeleteConfirmationModal.SetActive(false);
        LeaveConversationButton.onClick.AddListener(LeaveConversation);
    }

    async void OnDisable()
    {
        ConversationPanelUI.SetActive(false);
        m_OldestHistoryMessage = null;
        m_InProgressMessageHistoryFetch = null;
        m_CurrentConversationId = string.Empty;
        ClearMessageTiles();
        BindToVivoxChannelMessageEvents(false);
        BindToVivoxDirectMessageEvents(false);
        LeaveConversationButton.onClick.RemoveAllListeners();
        await TextSampleUIManager.Instance.CloseConversation();
    }

    public async Task SetupConversation(string conversationId, ConversationType conversationType)
    {
        m_CurrentConversationId = conversationId;
        ConversationPanelUI.SetActive(true);
        MessageInputField.Select();
        MessageInputField.ActivateInputField();
        switch (conversationType)
        {
            case ConversationType.ChannelConversation:
            {
                BindToVivoxChannelMessageEvents(true);
                m_InProgressMessageHistoryFetch = FetchHistory(true);
                TitleText.text = conversationId;
                InfoText.text = $"{VivoxService.Instance.ActiveChannels[conversationId].Count} online";
                ConversationTypeIcon.sprite = ChannelIcon;
                ViewChannelRosterButton.onClick.AddListener(OpenChannelRoster);
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                BindToVivoxDirectMessageEvents(true);
                m_InProgressMessageHistoryFetch = FetchHistory(true);
                // If you're entering a conversation with yourself...
                TitleText.text = conversationId == VivoxService.Instance.SignedInPlayerId ? $"{TextSampleUIManager.Instance.SignedInPlayerDisplayName} (Me)" : string.Empty;
                InfoText.text = $"Player ID: {conversationId}";
                ConversationTypeIcon.sprite = DMIcon;
            }
            break;
            default:
                break;
        }
    }

    private async Task FetchHistory(bool sendToBottom = false, bool fetchedOlderMessages = false)
    {
        try
        {
            var chatHistoryOptions = new ChatHistoryQueryOptions()
            {
                TimeEnd = m_OldestHistoryMessage
            };
            var historyMessages = m_CurrentConversationParadigm == ConversationType.ChannelConversation ?
                await VivoxService.Instance.GetChannelTextMessageHistoryAsync(m_CurrentConversationId, 100, chatHistoryOptions) :
                await VivoxService.Instance.GetDirectTextMessageHistoryAsync(m_CurrentConversationId, 100, chatHistoryOptions);

            if (historyMessages.FirstOrDefault()?.ReceivedTime == m_OldestHistoryMessage)
            {
                return;
            }

            PopulateChatBox(historyMessages, sendToBottom, fetchedOlderMessages);
        }
        catch (TaskCanceledException e)
        {
            Debug.Log($"Chat history request was canceled, likely because of a logout or the data is no longer needed: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Tried to fetch chat history and failed with error: {e.Message}");
        }
    }

    void BindToVivoxChannelMessageEvents(bool doBind)
    {
        if (doBind)
        {
            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

            VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
            VivoxService.Instance.ChannelMessageDeleted += OnChannelMessageDeleted;
        }
        else
        {
            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;

            VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
            VivoxService.Instance.ChannelMessageDeleted -= OnChannelMessageDeleted;
        }
    }

    void BindToVivoxDirectMessageEvents(bool doBind)
    {
        if (doBind)
        {
            TextSampleUIManager.Instance.OnDMSentFromLocalUser += OnDMSentFromLocalUser;
            VivoxService.Instance.DirectedMessageReceived += OnDirectMessageReceived;
            VivoxService.Instance.DirectedMessageDeleted += OnDirectMessageDeleted;
        }
        else
        {
            TextSampleUIManager.Instance.OnDMSentFromLocalUser -= OnDMSentFromLocalUser;
            VivoxService.Instance.DirectedMessageReceived -= OnDirectMessageReceived;
            VivoxService.Instance.DirectedMessageDeleted -= OnDirectMessageDeleted;
        }
    }

    void OnParticipantAdded(VivoxParticipant participant)
    {
        UpdateOnlineChannelParticipantInfo(participant);
    }

    void OnParticipantRemoved(VivoxParticipant participant)
    {
        UpdateOnlineChannelParticipantInfo(participant);
    }

    void OnChannelMessageReceived(VivoxMessage message)
    {
        AddNewPlayerMessageTile(message);
        SendChatBoxToBottom();
    }

    void OnChannelMessageDeleted(VivoxMessage message)
    {
        m_MessageObjPool?.Remove(m_MessageObjPool.FirstOrDefault(x => x.Key == message.MessageId));
        SendChatBoxToBottom();
    }

    void OnDMSentFromLocalUser(string message)
    {
        AddLocalPlayerDMMessageTile(message);
        SendChatBoxToBottom();
    }

    void OnDirectMessageReceived(VivoxMessage message)
    {
        AddNewPlayerMessageTile(message);
        SendChatBoxToBottom();
    }

    void OnDirectMessageDeleted(VivoxMessage message)
    {
        m_MessageObjPool?.Remove(m_MessageObjPool.FirstOrDefault(x => x.Key == message.MessageId));
        SendChatBoxToBottom();
    }

    void PopulateChatBox(ReadOnlyCollection<VivoxMessage> historyMessages, bool sendToBottom = false, bool fetchedOlderMessages = false)
    {
        m_OldestHistoryMessage = historyMessages.FirstOrDefault()?.ReceivedTime;
        if (fetchedOlderMessages)
        {
            var reversedMessages = historyMessages.Reverse();
            foreach (var message in reversedMessages)
            {
                AddNewPlayerMessageTile(message, fetchedOlderMessages);
            }
        }
        else
        {
            foreach (var message in historyMessages)
            {
                AddNewPlayerMessageTile(message);
            }
        }

        if (sendToBottom)
        {

            SendChatBoxToBottom();
        }
    }

    void AddLocalPlayerDMMessageTile(string message)
    {
        var playerMessageTile = Instantiate(PlayerMessageTilePrefab, ConversationScrollViewContent.transform);
        playerMessageTile.transform.SetAsLastSibling();
        m_MessageObjPool.Add(new KeyValuePair<string, GameObject>(m_localPlayerDMIdCount++.ToString(), playerMessageTile));
        playerMessageTile.transform.SetParent(ConversationScrollViewContent.transform);
        playerMessageTile.GetComponent<PlayerMessageTile>().SetupLocalPlayerDMMessageTile(message);
    }

    void AddNewPlayerMessageTile(VivoxMessage messageDetails, bool isOlderHistoryMessage = false)
    {
        var playerMessageTile = Instantiate(PlayerMessageTilePrefab, ConversationScrollViewContent.transform);
        m_MessageObjPool.Add(new KeyValuePair<string, GameObject>(messageDetails.MessageId, playerMessageTile));
        playerMessageTile.transform.SetParent(ConversationScrollViewContent.transform);
        if (isOlderHistoryMessage)
        {
            playerMessageTile.transform.SetSiblingIndex(0);
        }
        else
        {
            playerMessageTile.transform.SetAsLastSibling();
        }
        var playerMessageTileComponent = playerMessageTile.GetComponent<PlayerMessageTile>();
        playerMessageTileComponent.SetupMessageTile(messageDetails, messageDetails.FromSelf ? () => ShowEditOrDeleteModal(messageDetails.MessageId, messageDetails.MessageText) : null);
    }

    void ShowEditOrDeleteModal(string messageId, string originalMessage = "")
    {
        EditOrDeleteModal.GetComponent<EditOrDeleteModal>().Show(() => ShowDeleteConfirmationModal(messageId), () => ShowEditMessageModal(messageId, originalMessage));
    }

    void ShowDeleteConfirmationModal(string messageId)
    {
        EditOrDeleteModal.SetActive(false);
        DeleteConfirmationModal.GetComponent<DeleteConfirmationModal>().Show(async () => await DeleteVivoxMessage(messageId), () => DeleteConfirmationModal.SetActive(false));
    }

    void OpenChannelRoster()
    {
        ChannelRosterUI.SetActive(true);
    }

    void LeaveConversation()
    {
        gameObject.SetActive(false);
    }

    async Task DeleteVivoxMessage(string messageId)
    {
        switch (TextSampleUIManager.Instance.CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
            {
                await VivoxService.Instance.DeleteChannelTextMessageAsync(TextSampleUIManager.Instance.ActiveChannelConversationId, messageId);
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                await VivoxService.Instance.DeleteDirectTextMessageAsync(messageId);
            }
            break;
            default:
                break;
        }
        DeleteConfirmationModal.SetActive(false);
    }

    void ShowEditMessageModal(string messageId, string originalMessage)
    {
        EditOrDeleteModal.SetActive(false);
        EditMessageModal
            .GetComponent<EditMessageModal>()
            .Show(originalMessage,
            () => EditMessageModal.SetActive(false),
            async (string updatedMessage) => await SaveEditedMessage(messageId, updatedMessage));
    }

    async Task SaveEditedMessage(string messageId, string updatedMessage)
    {
        if (string.IsNullOrEmpty(updatedMessage))
        {
            return;
        }

        switch (TextSampleUIManager.Instance.CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
            {
                await VivoxService.Instance.EditChannelTextMessageAsync(TextSampleUIManager.Instance.ActiveChannelConversationId, messageId, updatedMessage);
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                await VivoxService.Instance.EditDirectTextMessageAsync(messageId, updatedMessage);
            }
            break;
            default:
                break;
        }
        EditMessageModal.SetActive(false);
    }

    void SendChatBoxToBottom()
    {
        if (ConversationScrollView != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ConversationScrollView.content);
            ConversationScrollView.verticalNormalizedPosition = 0;
        }
    }

    void ClearMessageTiles()
    {
        if (m_MessageObjPool.Count > 0)
        {
            for (var i = 0; i < m_MessageObjPool.Count; i++)
            {
                Destroy(m_MessageObjPool[i].Value);
            }
            m_MessageObjPool.Clear();
        }
    }

    void UpdateOnlineChannelParticipantInfo(VivoxParticipant participant)
    {
        InfoText.text = $"{VivoxService.Instance.ActiveChannels[participant.ChannelName].Count} online";
    }
}
