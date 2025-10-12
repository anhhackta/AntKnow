using System;
using System.Collections.Generic;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RosterListUI : MonoBehaviour
{
    [SerializeField]
    GameObject RosterUI;
    [SerializeField]
    GameObject m_rosterItemPrefab;
    [SerializeField]
    Transform m_rosterContentParent;
    [SerializeField]
    Button BackButton;

    private void Awake()
    {
        RosterUI.SetActive(false);
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;

        PopulateRosterListEntries();
        RosterUI.SetActive(true);
        BackButton.onClick.AddListener(HideRosterUI);
    }

    void OnDisable()
    {
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
    }

    void AddRosterEntry(VivoxParticipant participant)
    {
        var participantTile = Instantiate(m_rosterItemPrefab, m_rosterContentParent);
        participantTile.GetComponent<ChannelParticipantTile>().SetupRosterEntry(participant);
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        AddRosterEntry(participant);
    }

    private void PopulateRosterListEntries()
    {

        if (!VivoxService.Instance.ActiveChannels.ContainsKey(TextSampleUIManager.Instance.ActiveChannelConversationId))
        {
            return;
        }

        var participantList = VivoxService.Instance.ActiveChannels[TextSampleUIManager.Instance.ActiveChannelConversationId];
        for (var i = 0; i < participantList.Count; i++)
        {
            AddRosterEntry(participantList[i]);
        }
    }

    void HideRosterUI()
    {
        RosterUI.SetActive(false);
        gameObject.SetActive(false);
    }
}
