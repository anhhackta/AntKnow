using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

public class TextSampleUIManager : MonoBehaviour
{
    public static TextSampleUIManager Instance;

    public Action<string> OnDMSentFromLocalUser;

    public const string CachedConversationTypeKey = "ActiveConversationType";

    public GameObject SignInScreenUI;
    public GameObject ActiveConversationUI;
    public GameObject ConversationListUI;
    public GameObject ChannelRosterUI;
    public GameObject ProfilePaneUI;
    public GameObject LoadingSpinnerUI;

    [HideInInspector]
    public ConversationType CurrentConversationParadigm;
    [HideInInspector]
    public string SignedInPlayerDisplayName;
    [HideInInspector]
    public string ActiveChannelConversationId = string.Empty;
    [HideInInspector]
    public string ActiveDMConversationId = string.Empty;

    void Awake()
    {
        Instance = this;
        LoadPrefs();
        CloseAllMenus();
    }

    void Start()
    {
        VivoxService.Instance.LoggedOut += ShowSignInScreenUI;

        SignInScreenUI.SetActive(true);
    }

    private void OnDestroy()
    {
    }

    public void ShowSignInScreenUI()
    {
        CloseAllMenus();
        SignInScreenUI.SetActive(true);
    }

    public void ShowActiveConversationUI()
    {
        CloseAllMenus();
        ActiveConversationUI.SetActive(true);
    }

    public void ShowConversationsUI()
    {
        CloseAllMenus();
        ConversationListUI.SetActive(true);
    }

    public void ShowChannelRosterUI()
    {
        CloseAllMenus();
        ChannelRosterUI.SetActive(true);
    }

    public void ShowProfilePaneUI(bool doShow)
    {
        ProfilePaneUI.SetActive(doShow);
    }

    void CloseAllMenus()
    {
        SignInScreenUI.SetActive(false);
        ActiveConversationUI.SetActive(false);
        ConversationListUI.SetActive(false);
        ChannelRosterUI.SetActive(false);
        ProfilePaneUI.SetActive(false);
    }

    void RefreshConversationUIView()
    {
        ConversationListUI.SetActive(false);
        ConversationListUI.SetActive(true);
        ProfilePaneUI.SetActive(false);
    }

    public void SwitchConversationContext(ConversationType conversationContext)
    {
        if (CurrentConversationParadigm == conversationContext)
            return;

        CurrentConversationParadigm = conversationContext;
        SaveConversationContextPreference(conversationContext);
        RefreshConversationUIView();
    }

    public void SaveConversationContextPreference(ConversationType conversationType)
    {
        switch (conversationType)
        {
            case ConversationType.ChannelConversation:
            {
                PlayerPrefs.SetString(CachedConversationTypeKey, "Channel");
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                PlayerPrefs.SetString(CachedConversationTypeKey, "DM");
            }
            break;
            default:
                break;
        }
        PlayerPrefs.Save();
    }

    public async Task JoinConversation(string conversationId)
    {
        ConversationListUI.SetActive(false);
        ActiveConversationUI.SetActive(true);
        switch (CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
            {
                await RunTaskWithLoadingUI(JoinChannelConversation(conversationId));
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                await JoinDMConversation(conversationId);
            }
            break;
            default:
                break;
        }
    }

    public async Task CloseConversation()
    {
        await RunTaskWithLoadingUI(VivoxService.Instance.LeaveAllChannelsAsync());
        ConversationListUI.SetActive(true);
        ActiveConversationUI.SetActive(false);
    }

    public async Task RunTaskWithLoadingUI(Task task)
    {
        // Ensure the loading UI is active
        LoadingSpinnerUI.SetActive(true);

        try
        {
            // Wait for the task to complete
            await task;
        }
        catch (System.Exception e)
        {
            throw e;
        }
        finally
        {
            // Hide the loading UI
            LoadingSpinnerUI.SetActive(false);
        }
    }

    public void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    async Task JoinChannelConversation(string channelName)
    {
        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextOnly);
            ActiveChannelConversationId = channelName;
            await ActiveConversationUI.GetComponent<ConversationUI>().SetupConversation(channelName, CurrentConversationParadigm);
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    async Task JoinDMConversation(string playerId)
    {
        ActiveDMConversationId = playerId;
        await ActiveConversationUI.GetComponent<ConversationUI>().SetupConversation(playerId, CurrentConversationParadigm);
    }

    public async Task SendVivoxMessage(string message)
    {
        switch (CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
            {
                await VivoxService.Instance.SendChannelTextMessageAsync(ActiveChannelConversationId, message);
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                await VivoxService.Instance.SendDirectTextMessageAsync(ActiveDMConversationId, message);
                OnDMSentFromLocalUser?.Invoke(message);
            }
            break;
            default:
                break;
        }
    }

    public void LoadPrefs()
    {
        var cachedConversationType = PlayerPrefs.GetString(CachedConversationTypeKey, "Channel");
        switch (cachedConversationType)
        {
            case "DM":
            {
                CurrentConversationParadigm = ConversationType.DirectedMessageConversation;
            }
            break;
            case "Channel":
            {
                CurrentConversationParadigm = ConversationType.ChannelConversation;
            }
            break;
            default:
                break;
        }
    }
}
