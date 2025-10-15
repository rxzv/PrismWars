using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestRelay : Singleton<TestRelay>
{
    [SerializeField] private Button createRelayButton;
    [SerializeField] private Button joinRelayButton;
    [SerializeField] private string joinCode;

    private void Start()
    {
        if (createRelayButton != null)
        {
            createRelayButton.onClick.AddListener(CreateRelay);
        }

        if (joinRelayButton != null)
        {
            joinRelayButton.onClick.AddListener(() => JoinRelay(joinCode));
        }
    }
    public async void CreateRelay()
    {
        try
        {
            Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay created successfully! Join Code: {joinCode}");

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                isSecure: false
            );
            
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    public async Task<string> CreateReturnRelay()
    {
        try
        {
            Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay created successfully! Join Code: {joinCode}");

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                isSecure: false
            );
            
            NetworkManager.Singleton.StartHost();
            
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
    
    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"Joined relay successfully! Allocation ID: {joinAllocation.AllocationId}");
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData
            (
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                isSecure: false
            );
            
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
