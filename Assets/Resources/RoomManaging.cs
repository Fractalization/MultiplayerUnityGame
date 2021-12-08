using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManaging : MonoBehaviourPunCallbacks
{
    public GameObject playerManager;
    public Transform spawnPoint;
    public static RoomManaging instance;

    void Awake()
    {
        if(instance){
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("Room",null,null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room");
        GameObject localPlayer = PhotonNetwork.Instantiate(playerManager.name, spawnPoint.position, Quaternion.identity);
    }


}
