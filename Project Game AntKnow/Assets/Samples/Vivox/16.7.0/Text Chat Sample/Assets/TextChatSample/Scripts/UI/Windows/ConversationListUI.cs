using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ConversationListUI : MonoBehaviour
{
    ReadOnlyCollection<VivoxConversation> Conversations;
    List<VivoxConversation> DMConversations => Conversations != null ? Conversations.Where(c => c.ConversationType == ConversationType.DirectedMessageConversation).ToList() : new List<VivoxConversation>();
    List<VivoxConversation> ChannelConversations => Conversations != null ? Conversations.Where(c => c.ConversationType == ConversationType.ChannelConversation).ToList() : new List<VivoxConversation>();
    List<GameObject> ConversationItems = new List<GameObject>();

    public ConversationModalWindow Modal;
    public List<Sprite> ConversationTypeIcons = new List<Sprite>();
    public Image ConversationTypeIcon;
    public Text ConversationTypeText;
    public GameObject ConversationTilePrefab;
    public GameObject ConversationScrollViewContent;
    public GameObject ConversationUI;
    public Button ViewProfileButton;

    private void Start()
    {
        ViewProfileButton.onClick.AddListener(OpenProfileMenu);
        Modal.gameObject.SetActive(false);
    }

    async void OnEnable()
    {
        Cleanup();

        await TextSampleUIManager.Instance.RunTaskWithLoadingUI(SetupConversationUI());
    }

    private void OnDisable()
    {
        Cleanup();
        Conversations = null;
        ConversationUI.SetActive(false);
    }

    private void OnDestroy()
    {
        ViewProfileButton.onClick.RemoveAllListeners();
    }

    async Task SetupConversationUI()
    {
        var currentConversationType = TextSampleUIManager.Instance.CurrentConversationParadigm;
        ConversationTypeIcon.sprite = currentConversationType == ConversationType.ChannelConversation ? ConversationTypeIcons[1] : ConversationTypeIcons[0];
        ConversationTypeText.text = currentConversationType == ConversationType.ChannelConversation ? "Channels" : "Direct Messages";

        // Add the conversation creation tile at the end
        var conversationCreationTile = Instantiate(ConversationTilePrefab, ConversationScrollViewContent.transform);
        conversationCreationTile.transform.SetAsLastSibling();
        conversationCreationTile.GetComponent<Button>().onClick.AddListener(() => ShowConversationCreationModal(currentConversationType));
        ConversationItems.Add(conversationCreationTile);
        conversationCreationTile.GetComponent<ConversationTile>().SetupCreateNewConversationTile(currentConversationType);
        ConversationUI.SetActive(true);

        Conversations = await FetchConversations();
        var conversationsToShow = currentConversationType == ConversationType.ChannelConversation ? ChannelConversations : DMConversations;

        foreach (var conversation in conversationsToShow)
        {
            AddNewConversationTile(conversation);
        }
    }

    void AddNewConversationTile(VivoxConversation conversation)
    {
        AddNewConversationTile(conversation.ConversationType, conversation.Name);
    }

    void AddNewConversationTile(ConversationType conversationType, string id, string displayName = null)
    {
        var conversationTile = Instantiate(ConversationTilePrefab, ConversationScrollViewContent.transform);
        conversationTile.transform.SetAsFirstSibling();
        ConversationItems.Add(conversationTile);
        conversationTile.transform.SetParent(ConversationScrollViewContent.transform);
        conversationTile.GetComponent<ConversationTile>().SetupExisitingConversationTile(conversationType, id, displayName);
    }

    void ShowConversationCreationModal(ConversationType conversationType)
    {
        Modal.gameObject.SetActive(true);
        Modal.ShowModal(conversationType, (string id) => CreateConversationTile(conversationType, id));
    }

    void CreateConversationTile(ConversationType conversationType, string id)
    {
        AddNewConversationTile(conversationType, id);
    }

    async Task<ReadOnlyCollection<VivoxConversation>> FetchConversations()
    {
        return await VivoxService.Instance.GetConversationsAsync();
    }

    void Cleanup()
    {
        if (ConversationItems.Count > 0)
        {
            for (var i = 0; i < ConversationItems.Count; i++)
            {
                ConversationItems[i].GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(ConversationItems[i]);
            }
            ConversationItems.Clear();
        }
    }

    void OpenProfileMenu()
    {
        TextSampleUIManager.Instance.ShowProfilePaneUI(true);
    }
}
