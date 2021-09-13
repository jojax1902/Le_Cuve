using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float turnSpeed = .1f;
    public float jumpForce;

    //score
    private int kills = 0;

    //state
    public float health = 100.0f;
    public bool alive = true;


    //guns
    public float fireRate = 5.0f;
    public float recoilTime = 2.5f;
    private float nextFire = 0;
    private float stable = 0;
    private bool reloading = false;
    private bool canMove = true;



    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Transform tran;
    public Player photonPlayer;
    public CharacterController cc;
    public LineRenderer lr;
    public ParticleSystem ps;

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            rig.isKinematic = true;
        }

        lr.enabled = false;


            
    }


    private void Update()
    {
        if (photonView.IsMine)
        {
            //recoil will stun the player firing their cannon
            canMove = Time.time > stable;
            //Debug.Log("TIme: " + Time.time + "Next fire: " + nextFire);
            if (canMove && Time.time > stable && alive)
            {
                Move();
            }
            
            //firing will unload the primary chamber of the tank
            reloading = (Time.time < nextFire);
            //Debug.Log("TIme: " + Time.deltaTime + "Next fire: " + nextFire);
            if (Input.GetKeyDown(KeyCode.Space) && !reloading && alive)
            {
                TryFire();
                nextFire = Time.time + fireRate;
                stable = Time.time + recoilTime;
            }
        }
    }

    void Move()
    {
        float turn = Input.GetAxis("Horizontal") * turnSpeed;
        //float z = Input.GetAxis("Vertical") * moveSpeed;
        transform.Rotate(Vector3.up * turn);

        Vector3 move = new Vector3(0, 0, Input.GetAxisRaw("Vertical") * Time.deltaTime);
        move = this.transform.TransformDirection(move);
        cc.Move(move * moveSpeed);
    }

    void TryFire()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward, Color.blue, 10.5f);
        if(Physics.Raycast(transform.position, transform.forward, out hit, 10.5f))
        {
            if (hit.rigidbody != null)
            {
                if (hit.rigidbody.gameObject.tag == "Player")
                {
                    hit.rigidbody.gameObject.SendMessage("TakenDamage", this);
                }
                else
                {
                    hit.rigidbody.gameObject.SendMessage("Destruction");
                }
            }

            GameManager gm = FindObjectOfType<GameManager>();
            gm.CheckLiving();
        }

        lr.enabled = true;

        Invoke("ShotDisable", .125f);
        

    }

    public void ShotDisable()
    {
        lr.enabled = false;
    }

    public void TakenDamage(PlayerController pc)
    {
        //replace the tank with the destroyed tank child model

        //disable the mobility of the player
        //Mark the player as dead
        alive = false;
        ps.Play();
        health = 0;
        transform.position = Vector3.down*100;
        //credit the killer with the kill
        pc.kills++;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }
        else if (stream.IsReading)
        {
            health = (float)stream.ReceiveNext();
        }
    }
}