using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
/*
 * 턴 시작 시 블록이 없으면 랜덤으로 블록 자식으로 0
 * 마우스 진입시 하이라이트 0
 * 마우스 나갈 때 하이라이트 끄기 0
 * 2초 이하 클릭 & 방향 드래그 -> 회전 0
 * 2초 이상 클릭 & 드래그 -> 블록 옮기기 x
 * 게임보드에 블록이 올라간후 블록이 자식이 아님
 */

public class BSHolder_M : MonoBehaviourPunCallbacks, IPunObservable
{
    private Behaviour halo;
    private GameObject chilBS;
    private float mDirection;

    public int player_num;//1번 플레이어는 0, 2번 플레이어는 1로 설정
    public bool holdingBS;
    public bool selected;

    public AudioClip audiospin;
    public AudioClip audioput;

    private AudioSource theAudio;


    IEnumerator SelectedChecker()    //코루틴 시작 후 1초가 지나면 SELECTED로
    {
        Debug.Log("Selected Checker Start.");
        yield return new WaitForSeconds(0.7f);
        /*
        selected = true;
        chilBS.transform.parent = null;
        holdingBS = false;
        chilBS.transform.localScale = new Vector3(1, 1, 1);
        */
        chilBS.GetComponent<PhotonView>().RPC("CallResetParentRPC", RpcTarget.AllBuffered);
        Debug.Log("BS selected.");
        yield return null;
    }

    private void Awake()
    {
        halo = (Behaviour)gameObject.GetComponent("Halo");
        halo.enabled = false;
        selected = false;
        if (chilBS != null)
        {
            holdingBS = true;
        }
    }

    private void Start()
    {
        theAudio = gameObject.AddComponent<AudioSource>();
    }

    public void spawnBS(int BSNum, int ownerNum)
    {
        if (player_num == GameManager_M.Instance().ThisTurn())
        {
            if (!holdingBS)
            {
                int i = Random.Range(0, GameManager_M.Instance().BlockSets.Length);
                chilBS = PhotonNetwork.Instantiate(GameManager_M.Instance().BlockSets[i].name, this.transform.position, Quaternion.identity);
                /*
                chilBS.transform.SetParent(this.transform);
                holdingBS = true;
                */
                chilBS.GetComponent<PhotonView>().RPC("GiveParam", RpcTarget.AllBuffered, BSNum, ownerNum);
                /*
                else{
                    chilBS.transform.SetParent(opposite_BSTransform);
                    opposite_BS.holdingBS = true;
                }
                */
                
            }
        }
    }

    private void OnMouseEnter()
    {
        halo.enabled = true;
    }

    private void OnMouseDown()
    {
        Debug.Log("On mouse down.");
        StartCoroutine(SelectedChecker());
        mDirection = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z)).x;
    }

    private void OnMouseUp()
    {
        Debug.Log("On mouse Up");
        StopAllCoroutines();    //작동은 되지만 수정 필요
        if (selected )
        {
            mDirection = 0;
            if (GameManager_M.Instance().gameBoard.AllBSOnBoard(chilBS) && !GameManager_M.Instance().gameBoard.BlockOverlayed() && player_num == GameManager_M.Instance().ThisTurn()) //게임보드 위에 있으면 정확한 자리에 블록 배치 & SELEcted false
            {
                GameManager_M.Instance().gameBoard.RenderBlockOnBoard();
                theAudio.clip = audioput;
                theAudio.Play();
                PhotonNetwork.Destroy(chilBS);
                selected = false;
                GameManager_M.Instance().CheckGameFinished();
                GameManager_M.Instance().Resetholder();
                GameManager_M._instance.NextTurn();   //턴 넘김

            }
            else //게임보드 위에 없으면 다시 제자리로 & Selected false
            {
                //GameManager_M.Instance().gameBoard.HideShownBlocks();
                GameManager_M.Instance().gameBoard.HideShownBlocks();
                //PV.RPC("SetBSParent", RpcTarget.All, chilBS);
                /*
                chilBS.transform.parent = this.transform;
                holdingBS = true;
                */
                /*
                if(chilBS.GetComponent<PhotonView>().IsMine){
                    chilBS.transform.SetParent(this.transform);
                    holdingBS = true;
                }*/
                chilBS.GetComponent<PhotonView>().RPC("CallSetParentRPC", RpcTarget.AllBuffered);
                selected = false;
            }
        }
        else
        {
            mDirection = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z)).x - mDirection;   //
            if (mDirection > 0) //BS 오른쪽 회전
            {
                chilBS.transform.Rotate(new Vector3(0, 90, 0));
                theAudio.clip = audiospin;
                theAudio.Play();
            }
            else if (mDirection < 0)    //BS 왼쪽 회전
            {
                chilBS.transform.Rotate(new Vector3(0, -90, 0));
                theAudio.clip = audiospin;
                theAudio.Play();
            }
            else
            {
                //DO NOTHING
            }
        }
    }

    // 향후 플레이어들이 분리되어 카메라 위치를 다르게 할 때도 사용
    private void OnMouseDrag()
    {
        if (selected && player_num==GameManager_M.Instance().ThisTurn())   // 블록 드래그
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            if(player_num == 0)
                chilBS.transform.position = new Vector3(mousePos.x, 4, mousePos.z+2.8f) * Mathf.Sqrt(2);
            else
                chilBS.transform.position = new Vector3(mousePos.x-2.6f, 4, mousePos.z-16.5f) * Mathf.Sqrt(2);
            GameManager_M.Instance().gameBoard.ShowBS(chilBS);
        }
    }

    private void OnMouseExit()
    {
        halo.enabled = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /*
        if(GameManager_M._instance.isLoadingComplete)
            if(stream.IsWriting){
                stream.SendNext(chilBS.transform.position);
                stream.SendNext(chilBS.transform.rotation);
            }
            else{
                chilBS.transform.position = (Vector3)stream.ReceiveNext();
                chilBS.transform.rotation = (Quaternion)stream.ReceiveNext();
            }
        */
    }
}
