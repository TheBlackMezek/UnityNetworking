using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerController : NetworkBehaviour {

    public float walkSpeed = 5.0f;
    public float spawnRange = 40.0f;

    public CharacterController cc;
    public Camera cam;
    public AudioListener listener;

    [SyncVar]
    private Vector3 scale = Vector3.one;



    private void Start()
    {
        if(isLocalPlayer)
        {
            scale = transform.localScale;
        }
    }

    void Update () {
        if(transform.localScale != scale)
        {
            transform.localScale = scale;
        }

        if (!isLocalPlayer)
        {
            cam.enabled = false;
            listener.enabled = false;
            return;
        }

        float x = Input.GetAxis("Horizontal") * walkSpeed;
        float z = Input.GetAxis("Vertical") * walkSpeed;

        //transform.position += new Vector3(x, 0, z);
        //body.AddForce(new Vector3(x, 0, z));
        // cc.SimpleMove(new Vector3(x, 0, z));
        Vector3 amt = new Vector3(x, 0, z);
        cc.SimpleMove(amt);

        if (transform.position.y < -5)
        {
            transform.position = Vector3.zero;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    [ClientRpc]
    public void RpcSetPos(Vector3 pos)
    {
        transform.position = pos;
    }

    [ClientRpc]
    public void RpcSetScale(Vector3 s)
    {
        //ransform.localScale = s;
        scale = s;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(isServer)
        {
            if (hit.gameObject.tag == "nom")
            {
                RpcSetScale(transform.localScale + Vector3.one);
                //transform.localScale += Vector3.one;
                Destroy(hit.gameObject);
            }
            else if(hit.gameObject.tag == "Player")
            {
                PlayerController loser = null;
                PlayerController winner = null;

                if(hit.transform.localScale.magnitude > transform.localScale.magnitude)
                {
                    loser = gameObject.GetComponent<PlayerController>();
                    winner = hit.gameObject.GetComponent<PlayerController>();
                }
                else if(hit.transform.localScale.magnitude < transform.localScale.magnitude)
                {
                    loser = hit.gameObject.GetComponent<PlayerController>();
                    winner = gameObject.GetComponent<PlayerController>();
                }

                if(loser != null)
                {
                    winner.RpcSetScale(winner.transform.localScale +
                                       Vector3.one * loser.transform.localScale.magnitude);
                    loser.RpcSetScale(Vector3.one);
                    loser.RpcSetPos(new Vector3(Random.Range(-spawnRange, spawnRange),
                                                0,
                                                Random.Range(-spawnRange, spawnRange)));
                }
            }
        }
    }

}
