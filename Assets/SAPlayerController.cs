using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public struct PlayerState
{
    public int id;
    public Vector3 pos;
}

public struct PlayerInput
{
    public Vector3 move;
}

public struct InputPacket
{
    public int id;
    public PlayerInput[] inputs;
}



public class SAPlayerController : NetworkBehaviour
{

    public float walkSpeed = 5.0f;
    public float spawnRange = 40.0f;

    public CharacterController cc;
    public Camera cam;
    public AudioListener listener;

    [SyncVar]
    public Vector3 syncScale;

    private List<InputPacket> inputSets = new List<InputPacket>();
    private List<PlayerInput> inputs = new List<PlayerInput>();
    private int stateIdCounter = 0;
    private int maxStateId = 10000;

    private PlayerState lastVerifiedState;
    private InputPacket packet;





    private void Start()
    {
        if(!isLocalPlayer)
        {
            GetComponent<MeshRenderer>().material.color = Color.green;
        }

        if(isServer)
        {
            syncScale = transform.localScale;
        }
    }

    void Update()
    {
        transform.localScale = syncScale;

        if (!isLocalPlayer)
        {
            cam.enabled = false;
            listener.enabled = false;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal") * walkSpeed * Time.deltaTime;
        float z = Input.GetAxisRaw("Vertical") * walkSpeed * Time.deltaTime;
        
        Vector3 amt = new Vector3(x, 0, z);
        cc.SimpleMove(amt);

        PlayerInput inp;
        inp.move = amt;
        inputs.Add(inp);

        if (transform.position.y < -5)
        {
            //transform.position = Vector3.zero;
            //CmdSetPos(Vector3.zero);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void FixedUpdate()
    {
        if(isLocalPlayer)
        {
            packet.inputs = new PlayerInput[inputs.Count];
            for(int i = 0; i < inputs.Count; ++i)
            {
                packet.inputs[i] = inputs[i];
            }
            inputs.Clear();

            CmdGiveInput(packet);
            inputSets.Add(packet);

            ++stateIdCounter;

            if(stateIdCounter > maxStateId)
            {
                stateIdCounter = 0;
            }

            packet.id = stateIdCounter;
            inputs.Clear();
        }

        if(isServer && inputSets.Count > 0)
        {
            foreach (InputPacket ipk in inputSets)
            {
                foreach(PlayerInput inp in ipk.inputs)
                {
                    cc.SimpleMove(inp.move);
                }
            }

            PlayerState st;
            st.id = inputSets[inputSets.Count - 1].id;
            st.pos = transform.position;
            RpcGiveState(st);

            inputSets.Clear();
        }
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
        packet.id = stateIdCounter;
    }

    [Command]
    public void CmdGiveInput(InputPacket ipk)
    {
        inputSets.Add(ipk);
    }

    [ClientRpc]
    public void RpcGiveState(PlayerState st)
    {
        lastVerifiedState = st;

        transform.position = st.pos;

        for(int i = 0; i < inputSets.Count; ++i)
        {
            if(inputSets[i].id == st.id)
            {
                inputSets.RemoveRange(0, i + 1);
                break;
            }
        }

        foreach (InputPacket ipk in inputSets)
        {
            foreach (PlayerInput inp in ipk.inputs)
            {
                cc.SimpleMove(inp.move);
            }
        }
    }
    
    

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isServer)
        {
            if (hit.gameObject.tag == "nom")
            {
                //transform.localScale += Vector3.one;
                syncScale += Vector3.one;
                Destroy(hit.gameObject);
            }
            else if (hit.gameObject.tag == "Player")
            {
                SAPlayerController loser = null;
                SAPlayerController winner = null;

                if (hit.transform.localScale.magnitude > transform.localScale.magnitude)
                {
                    loser = gameObject.GetComponent<SAPlayerController>();
                    winner = hit.gameObject.GetComponent<SAPlayerController>();
                }
                else if (hit.transform.localScale.magnitude < transform.localScale.magnitude)
                {
                    loser = hit.gameObject.GetComponent<SAPlayerController>();
                    winner = gameObject.GetComponent<SAPlayerController>();
                }

                if (loser != null)
                {
                    //winner.CmdSetScale(winner.syncScale +
                    //                   Vector3.one * loser.syncScale.magnitude);
                    //loser.CmdSetScale(Vector3.one);
                    //loser.CmdSetPos(new Vector3(Random.Range(-spawnRange, spawnRange),
                    //                            0,
                    //                            Random.Range(-spawnRange, spawnRange)));
                }
            }
        }
    }

}
