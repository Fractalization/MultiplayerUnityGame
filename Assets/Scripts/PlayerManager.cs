using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    PhotonView view;
    public GameObject bean;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        if(view.IsMine){
            CreateBean();
        }
    }

    void CreateBean(){
        PhotonNetwork.Instantiate(bean.name, Vector3.zero, Quaternion.identity);
    }
}
