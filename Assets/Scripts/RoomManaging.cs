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
    public GameObject m4Prefab;
    

    void Awake()
    {
        if(instance){
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        //server update rate per second
        PhotonNetwork.SendRate = 360;
        PhotonNetwork.SerializationRate = 360;
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
        bool spawnGuns = (PhotonNetwork.CurrentRoom.PlayerCount == 1);
        GameObject localPlayer = PhotonNetwork.Instantiate(playerManager.name, spawnPoint.position, Quaternion.identity);
        if(spawnGuns){
            PhotonNetwork.Instantiate(m4Prefab.name, new Vector3(-1f,0f,2.5f), Quaternion.identity,0);
            PhotonNetwork.Instantiate("M4A1", new Vector3(0f,0f,2.5f), Quaternion.identity,0);
            PhotonNetwork.Instantiate("M4A1", new Vector3(1f,0f,2.5f), Quaternion.identity,0);
        }
    }


}
