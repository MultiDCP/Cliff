using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerMove_M : MonoBehaviourPunCallbacks, IPunObservable {
	public GameObject Player;
	public GameObject Move;
	public GameObject Up;
	public GameObject Down;
	public GameObject Right;
	public GameObject Left;

    public AudioClip audioMove;
    private AudioSource theAudio;

    public GameObject p1;
    public GameObject p2;

    //private bool ispause = false;

    //private int mode = 0;

    private int playerNum = 0;

	int touch = 1;

    private PhotonView PV;
    private GameObject otherPlayer;
    [SerializeField] private bool myPlayerSpawn = false;
    [SerializeField] private bool otherPlayerSpawn = false;
    [SerializeField] private bool currentSpawn;
    Vector3 curPos;

    private void Start()
    {
        theAudio = gameObject.AddComponent<AudioSource>();
        StartCoroutine(SetCharacter());
    }

    IEnumerator SetCharacter(){
        yield return new WaitUntil(() => GameManager_M._instance.isLoadingComplete);

        if(PhotonNetwork.IsMasterClient)
        {
            Player = GameObject.Find("P1_M(Clone)");
            Move = Player.transform.Find("Move").gameObject;
            Up = Move.transform.Find("UP").gameObject;
            Down = Move.transform.Find("DOWN").gameObject;
            Right = Move.transform.Find("RIGHT").gameObject;
            Left = Move.transform.Find("LEFT").gameObject;
            otherPlayer = GameObject.Find("P2_M(Clone)");
            playerNum = 0;
        }
        else
        {
            Player = GameObject.Find("P2_M(Clone)");
            Move = Player.transform.Find("Move").gameObject;
            Up = Move.transform.Find("UP").gameObject;
            Down = Move.transform.Find("DOWN").gameObject;
            Right = Move.transform.Find("RIGHT").gameObject;
            Left = Move.transform.Find("LEFT").gameObject;
            otherPlayer = GameObject.Find("P1_M(Clone)");
            playerNum = 1;
        }
        PV = Player.gameObject.GetComponent<PhotonView>();
        myPlayerSpawn = true;
    }

    IEnumerator WinMotion(int k)
    {
        for (int i = 0; i < 10; i++)
        {
            if(k==0)
                StartCoroutine(MoveChar("up"));
            else if(k==1)
                StartCoroutine(MoveChar("down"));
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator Turn_to_see()
    {

        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator MoveChar(string s)
    {
        Vector3 ame = Player.transform.position;
        Vector3 Original = ame;
        yield return new WaitForSeconds(0.1f);
        for (int i=0; i < 20; i++)
        {
            if (s == "up")
                ame.z = ame.z + 0.06f;
            else if (s == "right")
                ame.x = ame.x + 0.06f;
            else if (s == "left")
                ame.x = ame.x - 0.06f;
            else if (s == "down")
                ame.z = ame.z - 0.06f;
            ame.y = ame.y +(-0.1f*(i*0.06f)+0.06f);
            Player.transform.position = ame;
            yield return new WaitForSeconds(0.002f);
        }
        if (s == "up")
        {
            Player.transform.position = new Vector3(Original.x, Original.y, Original.z + 1.2f);
            GameManager_M._instance.CheckGameFinished();
        }
        else if (s == "right")
        {
            Player.transform.position = new Vector3(Original.x + 1.2f, Original.y, Original.z);
        }
        else if (s == "left")
        {
            Player.transform.position = new Vector3(Original.x - 1.2f, Original.y, Original.z);
        }
        else if (s == "down")
        {
            Player.transform.position = new Vector3(Original.x, Original.y, Original.z - 1.2f);
            GameManager_M._instance.CheckGameFinished();
        }
        GameManager_M._instance.NextTurn();
    }


    // Update is called once per frame
    void Update () {
        if(myPlayerSpawn){
            if (Input.GetKey(KeyCode.D))
            {
                for(int i=0; i<GameManager_M.Instance().checking.Length; i++)
                {
                    Debug.Log(GameManager_M.Instance().checking[i]);
                }
            }

            if(Input.GetMouseButtonDown(0)){
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray,out hit)){
                    
                    Debug.Log(hit.transform.name);
                    if((hit.transform.name == "P1_M(Clone)" && GameManager_M._instance.ThisTurn() == 0 && hit.transform.GetComponent<PhotonView>().IsMine) 
                        || (hit.transform.name == "P2_M(Clone)" && GameManager_M._instance.ThisTurn() == 1 && hit.transform.GetComponent<PhotonView>().IsMine))
                    {
                        touch = touch +1;
                        //게임 매니저의 turn_num 변수는 단순히 누구의 턴인지만 알려주며,
                        //이를 이용해 어떤 기능을 막을지는 알아서 코딩해주세요
                    }

                    if (touch % 2 == 0 ){
                        Move.gameObject.SetActive(true);
                        
                        
                    }
                    if(touch % 2 == 1){
                        Move.gameObject.SetActive(false);
                    }
                    
                    if(hit.transform.name == "RIGHT" )
                    {
                        if ((GameManager_M._instance.currentpos + 1 >= 0 && GameManager_M._instance.currentpos + 1 < 70) && GameManager_M._instance.checking[GameManager_M._instance.currentpos + 1] == 1 && GameManager_M._instance.currentpos + 1!= GameManager_M._instance.oppositepos)
                        {
                            Move.gameObject.SetActive(false);
                            GameManager_M._instance.currentpos = GameManager_M._instance.currentpos + 1;
                            theAudio.clip = audioMove;
                            theAudio.Play();
                            StartCoroutine(MoveChar("right"));
                            
                            Debug.Log("k");
                        }
                        else
                        {
                            Debug.Log("error");
                        }

                    }
                    else if(hit.transform.name == "UP"){
                        if ((GameManager_M._instance.currentpos +7 >= 0 && GameManager_M._instance.currentpos +7 < 70) && GameManager_M._instance.checking[GameManager_M._instance.currentpos +7]==1 && GameManager_M._instance.currentpos + 7 != GameManager_M._instance.oppositepos)
                        {
                            Move.gameObject.SetActive(false);
                            GameManager_M._instance.currentpos = GameManager_M._instance.currentpos + 7;
                            theAudio.clip = audioMove;
                            theAudio.Play();
                            StartCoroutine(MoveChar("up"));
                            GameManager_M._instance.CheckGameFinished();
                            Debug.Log("k");
                            
                        }
                        else
                        {
                            Debug.Log("error");
                        }
                    }
                    else if(hit.transform.name == "DOWN"){
                        if ((GameManager_M._instance.currentpos - 7 >= 0 && GameManager_M._instance.currentpos - 7 < 70) && GameManager_M._instance.checking[GameManager_M._instance.currentpos - 7] == 1 && GameManager_M._instance.currentpos -7 != GameManager_M._instance.oppositepos)
                        {
                            Move.gameObject.SetActive(false);
                            GameManager_M._instance.currentpos = GameManager_M._instance.currentpos - 7;
                            theAudio.clip = audioMove;
                            theAudio.Play();
                            StartCoroutine(MoveChar("down"));
                            GameManager_M._instance.CheckGameFinished();
                            Debug.Log("k");
                        }
                        else
                        {
                            Debug.Log("error");
                        }
                    }
                    else if(hit.transform.name == "LEFT"){
                        if ((GameManager_M._instance.currentpos -1 >= 0 && GameManager_M._instance.currentpos -1 < 70) && GameManager_M._instance.checking[GameManager_M._instance.currentpos -1] == 1 && GameManager_M._instance.currentpos - 1 != GameManager_M._instance.oppositepos)
                        {
                            Move.gameObject.SetActive(false);
                            GameManager_M._instance.currentpos = GameManager_M._instance.currentpos - 1;
                            theAudio.clip = audioMove;
                            theAudio.Play();
                            StartCoroutine(MoveChar("left"));
                            Debug.Log("k");
                            
                        }
                        else
                        {
                            Debug.Log("error");
                        }
                    }
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(GameManager_M._instance.isLoadingComplete)
            if(stream.IsWriting){
                stream.SendNext(myPlayerSpawn);
            }
            else{
                this.otherPlayerSpawn = (bool)stream.ReceiveNext();
            }
    }
}
