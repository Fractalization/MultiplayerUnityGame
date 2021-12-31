using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullAuto : Gun
{
    //public Transform bulletSpawn;
    public override void Use(){
        // if(!bulletSpawn){
        //     bulletSpawn = transform.parent.parent;
        // }

        Debug.Log("used full auto");

    }
}
