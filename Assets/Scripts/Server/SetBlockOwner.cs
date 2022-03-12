using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SetBlockOwner : MonoBehaviour
{
    private PhotonView PV;
    [SerializeField] private int BSNum;
    [SerializeField] private int ownerNum;

    private void Awake() {
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void GiveParam(int BSNum, int ownerNum){
        this.BSNum = BSNum;
        this.ownerNum = ownerNum;
        PV.RPC("SetBSParent", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void CallRPC(){
        PV.RPC("SetBSParent", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void SetBSParent(){
        Transform holder;
        if(ownerNum == 0)
            holder = GameManager_M._instance.holders1.transform;
        else
            holder = GameManager_M._instance.holders2.transform;

        switch(BSNum){
            case 0:
                this.transform.SetParent(holder.GetChild(0));
                holder.GetChild(0).GetComponent<BSHolder_M>().holdingBS = true;
                break;
            case 1:
                this.transform.SetParent(holder.GetChild(1));
                holder.GetChild(1).GetComponent<BSHolder_M>().holdingBS = true;
                break;
            case 2:
                this.transform.SetParent(holder.GetChild(2));
                holder.GetChild(2).GetComponent<BSHolder_M>().holdingBS = true;
                break;
        }
        this.transform.localPosition = Vector3.zero;
        this.transform.localScale = new Vector3(3,3,3);
    }
}
