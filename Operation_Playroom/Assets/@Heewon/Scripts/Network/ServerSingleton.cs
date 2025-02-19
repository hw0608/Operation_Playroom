using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    static ServerSingleton instance;

    public Dictionary<ulong, UserData> clientIdToUserData = new Dictionary<ulong, UserData>();
    public Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

    public Action<string> OnClientLeft;

    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;

    public ServerGameManager serverManager;

    public static ServerSingleton Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject singletonObject = new GameObject("ServerSingleton");
                instance = singletonObject.AddComponent<ServerSingleton>();

                DontDestroyOnLoad(singletonObject);
            }
            return instance;
        }
    }

    public void Init()
    {
    }

    void OnEnable()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }


    void OnDisable()
    {
        if (NetworkManager.Singleton!=null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }


    public async Task CreateServer()
    {
        await UnityServices.InitializeAsync();

        serverManager = new ServerGameManager(
                ApplicationData.IP(),
                (ushort)ApplicationData.Port(),
                (ushort)ApplicationData.QPort(),
                NetworkManager.Singleton
            );
    }

    public bool OpenConnection(string ip, ushort port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);

        return NetworkManager.Singleton.StartServer();
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonConvert.DeserializeObject<UserData>(payload);
        Debug.Log("Approval User Data : " + userData.userName);

        clientIdToUserData[request.ClientNetworkId] = userData;
        authIdToUserData[userData.userAuthId] = userData;

        OnUserJoined?.Invoke(userData);

        response.Approved = true;
        response.CreatePlayerObject = true;
        //response.Position = SpawnPoint.GetRandomSpawnPoint();
        response.Rotation = Quaternion.identity;

    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToUserData.ContainsKey(clientId))
        {
            string authId = clientIdToUserData[clientId].userAuthId;

            OnUserLeft?.Invoke(authIdToUserData[authId]);

            clientIdToUserData.Remove(clientId);
            authIdToUserData.Remove(authId);

            OnClientLeft?.Invoke(authId);
        }
    }

}
