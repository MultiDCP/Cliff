﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameBoard_M : MonoBehaviourPunCallbacks, IPunObservable
{
    public static int xGrid = 7;
    public static int yGrid = 8;
    private GameObject[,] board = new GameObject[xGrid, yGrid];
    private bool[,] shownBlock = new bool[xGrid,yGrid]; //임시적으로 보여지는 블록 추적
    private int[] playerLoc;
    //public bool[,] renderedBlock = new bool[xGrid, yGrid]; //이미 올려진 블록인지

    [SerializeField] private ParticleSystem m_ExplosionParticles;
    [SerializeField] private float particleDuration;

    public AudioClip ExplosionAudio;
    public AudioClip audioFall;

    private AudioSource theAudio;

    private GameObject p1;
    private GameObject p2;
    public GameObject ground1;
    public GameObject ground2;

    public PhotonView PV;

    void Start()    //초기화
    {
        theAudio = gameObject.AddComponent<AudioSource>();
        //if(PhotonNetwork.IsMasterClient){
            int index = 0;
            for (int i = 0; i < yGrid; i++)
            {
                for (int j = 0; j < xGrid; j++)
                {
                    board[j, i] = this.transform.GetChild(index++).gameObject;
                    shownBlock[j, i] = false;
                }
            }
            playerLoc = new int[2];
            playerLoc[0] = -1;
            playerLoc[1] = -1;
        //}
    }

    IEnumerator SetCharacter(){
        yield return new WaitUntil(() => GameManager_M._instance.isLoadingComplete);
        p1 = GameObject.Find("P1_M(Clone)");
        p2 = GameObject.Find("P2_M(Clone)");
    }

    public bool AllBSOnBoard(GameObject BS)
    {
        int shownNum = 0;
        for (int i = 0; i < xGrid; i++)
        {
            for (int j = 0; j < yGrid; j++)
            {
                if (shownBlock[i, j])
                {
                    shownNum = shownNum + 1;
                }
            }
        }
        //Debug.Log("shownNum =" + shownNum);
        //Debug.Log("chilCount = " + BS.transform.childCount);
        return (shownNum == BS.transform.childCount);
    }

    public bool BlockOverlayed()    //이미 블록이 올라간 자리에 겹쳤는가
    {
        for (int i = 0; i < xGrid; i++)
        {
            for (int j = 0; j < yGrid; j++)
            {
                if (shownBlock[i, j] && (GameManager_M.Instance().checking[i + xGrid * (j+1)] == 1))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ShowBlockOnBoard(GameObject block){
        for (int i = 0; i < xGrid; i++)
        {
            if (board[i, 0].transform.position.x-0.6 < block.transform.position.x && block.transform.position.x <= board[i, 0].transform.position.x + 0.6)
            {
                for (int j = 0; j < yGrid; j++)
                {
                    if (board[0, j].transform.position.z - 0.6 < block.transform.position.z && block.transform.position.z <= board[0, j].transform.position.z + 0.6)
                    {
                        if(PhotonNetwork.IsMasterClient){
                            shownBlock[i, j] = true;
                            GameManager_M.Instance().blockmap[i + xGrid * j].SetActive(true);
                        }
                        else
                            PV.RPC("ShowBlockForAdmin", RpcTarget.MasterClient, i, j);
                        return;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void ShowBlockForAdmin(int i, int j){
        shownBlock[i, j] = true;
        GameManager_M.Instance().blockmap[i + xGrid * j].SetActive(true);
    }

    public void HideShownBlocks()
    {
        for (int i = 0; i < xGrid; i++)
        {
            for (int j = 0; j < yGrid; j++)
            {
                if (shownBlock[i, j])
                {
                    if(PhotonNetwork.IsMasterClient){
                        shownBlock[i, j] = false;
                        if (GameManager_M.Instance().checking[i + xGrid * (j+1)] == 0)
                            GameManager_M.Instance().blockmap[i + xGrid * j].SetActive(false);
                    }
                    else
                        PV.RPC("HideBlockForAdmin", RpcTarget.MasterClient, i, j);
                }
            }
        }
    }

    [PunRPC]
    public void HideBlockForAdmin(int i, int j){
        shownBlock[i, j] = false;
        if (GameManager_M.Instance().checking[i + xGrid * (j+1)] == 0)
            GameManager_M.Instance().blockmap[i + xGrid * j].SetActive(false);
    }

    public void ShowBS(GameObject BS) //원래 render되었던 Block 제거,block위치 체크하여 그 위치에 block render
    {
        HideShownBlocks();
        for (int i = 0; i < BS.transform.childCount; i++)
        {
            ShowBlockOnBoard(BS.transform.GetChild(i).gameObject);
        }
    }

    public void RenderBlockOnBoard(){
        for (int i = 0; i < xGrid; i++){
            for (int j = 0; j < yGrid; j++){
                if(PhotonNetwork.IsMasterClient)
                    if (shownBlock[i, j]){
                        shownBlock[i, j] = false;
                        GameManager_M.Instance().checking[i + xGrid * (j+1)] = 1;
                    }
                else{
                    PV.RPC("RenderBlockForAdmin", RpcTarget.MasterClient, i, j);
                }
            }
        }
        for (int j=0; j < yGrid; j++)
        {
            if (LineCheck(j))
            {
                DeleteRow(j);
            }
        }
    }

    [PunRPC]
    public void RenderBlockForAdmin(int i, int j){
        if (shownBlock[i, j]){
            shownBlock[i, j] = false;
            GameManager_M.Instance().checking[i + xGrid * (j+1)] = 1;
        }
    }

    public bool LineCheck(int j)  //만약 i 라인이 채워져있으면->모두 checking 이면 라인을 없애고 메리트를 줌.
    {
        int blocks = 0;
        for (int i = 0; i < xGrid; i++)
        {
            if (GameManager_M.Instance().checking[i + xGrid * (j+1)] == 1)
            {
                blocks++;
            }
        }
        return (blocks == xGrid);
    }
    
    IEnumerator Falling(GameObject p)
    {
        float temp = p.transform.position.y;
        yield return new WaitForSeconds(0.5f);
        theAudio.clip = audioFall;
        theAudio.Play();
        for (int i=0; i<40; i++)
        {
            temp = temp-(0.05f*i );
            p.transform.position = new Vector3 (p.transform.position.x, temp, p.transform.position.z);
            yield return new WaitForSeconds(0.005f);
        }
        
        yield return new WaitForSeconds(0.4f);
        if (p.name == "P1_M")
        {
            p1.transform.position = new Vector3(0, 2, -5.4f);
            
        }
        else if (p.name == "P2_M")
        {
            p2.transform.position = new Vector3(0, 2, 5.4f);
            
        }

    }

    IEnumerator ExplodeBlock(int j)
    {
        for (int i = 0; i < xGrid; i++)
        {
            
            if (GameManager_M.Instance().checking[i + xGrid * (j + 1)] ==1)
            {
                ParticleSystem m_instance = Instantiate(m_ExplosionParticles, new Vector3(-3.6f + i * 1.2f, 1 + Random.Range(-20, 20) / 100, -5 + j * 1.2f + Random.Range(-20, 20) / 100), Quaternion.identity);
                m_instance.Play();
                
                //Destroy(m_instance.gameObject, m_ExplosionParticles.duration);
                Destroy(m_instance.gameObject, particleDuration);
            }
            GameManager_M.Instance().blockmap[i + xGrid * j].SetActive(false);
            GameManager_M.Instance().checking[i + xGrid * (j + 1)] = 0;

            int block = i + xGrid * (j + 1);

            if (GameManager_M.Instance().ThisTurn() == 0)
            {
                
                if (GameManager_M.Instance().currentpos == block)
                {
                    GameManager_M.Instance().currentpos = 3;
                    StartCoroutine(Falling(p1));
                }
                else if (GameManager_M.Instance().oppositepos == block)
                {
                    GameManager_M.Instance().oppositepos = 66;
                    StartCoroutine(Falling(p2));
                }
            }
            if (GameManager_M.Instance().ThisTurn() == 1)
            {
                
                if (GameManager_M.Instance().currentpos == block)
                {
                    GameManager_M.Instance().currentpos = 66;
                    StartCoroutine(Falling(p2));
                    
                }
                else if (GameManager_M.Instance().oppositepos == block)
                {
                    GameManager_M.Instance().oppositepos = 3;
                    StartCoroutine(Falling(p1));
                    
                }
            }
            yield return new WaitForSeconds(0.08f);
        }
        
    }

    public void DeleteRow(int j)
    {
        theAudio.clip = ExplosionAudio;
        theAudio.Play();
        if (GameManager_M.Instance().ThisTurn() == 0)
        {
            GameManager_M.Instance().p1merit+=2;
            
        }
        if (GameManager_M.Instance().ThisTurn() == 1)
        {
            GameManager_M.Instance().p2merit+=2;
        }
        for (int i = 0; i < xGrid; i++)
        {
            
            StartCoroutine(ExplodeBlock(j));
        }
        //merit();
    }

    public void DeleteAllRow()
    {
        theAudio.clip = ExplosionAudio;
        theAudio.Play();
        /*
        ParticleSystem m_instance = Instantiate(m_ExplosionParticles, new Vector3(GameManager_M.Instance().oppositepos%7*1.2f-3.6f, 1 ,-5+ (GameManager.Instance().oppositepos/7))*1.2f, Quaternion.identity);
        GameManager_M.Instance().loser.SetActive(false);
        m_instance.Play();

        Destroy(m_instance.gameObject, m_ExplosionParticles.duration);
        */
        for (int i = 0; i < yGrid; i++)
        {
            StartCoroutine(ExplodeBlock(i));
        }

    }

    //IE
    public void merit() //Finish zone이 넓어지는 메리트를 준다.
    {
        if (GameManager_M.Instance().ThisTurn() == 0)
        {
            GameManager_M.Instance().p1merit++;
            for (int i = 0; i < xGrid; i++)
            {
                GameManager_M.Instance().blockmap[i + xGrid * (8 - GameManager_M.Instance().p1merit)].SetActive(false);
                GameManager_M.Instance().checking[i + xGrid * (8 - GameManager_M.Instance().p1merit + 1)] = 0;
            }
        }
        else if (GameManager_M.Instance().ThisTurn() == 1)
        {
            GameManager_M.Instance().p2merit++;
            for (int i = 0; i < xGrid; i++)
            {
                GameManager_M.Instance().blockmap[i + xGrid * GameManager_M.Instance().p2merit - 1].SetActive(false);
                GameManager_M.Instance().checking[i + xGrid * ((GameManager_M.Instance().p2merit))] = 0;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(GameManager_M._instance.isLoadingComplete)
            if(stream.IsWriting){
                for (int i = 0; i < yGrid; i++){
                    for (int j = 0; j < xGrid; j++){
                        stream.SendNext(shownBlock[j, i]);
                    }
                }
            }
            else{
                for (int i = 0; i < yGrid; i++){
                    for (int j = 0; j < xGrid; j++){
                        this.shownBlock[j, i] = (bool)stream.ReceiveNext();
                    }
                }             
            } 
    }
}
