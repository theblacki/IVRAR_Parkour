using System.Collections.Generic;
using UnityEngine;

public class TRBScript : MonoBehaviour
{
    public float maxSpeed;
    public float maxRange;
    public int nrOldPositions;

    public GameObject Player;

    private static readonly string playerTag = "Player";
    private Rigidbody rb;
    private Queue<Vector3> oldPositions = new();
    private bool isCurrentlyGrabbed;
    //private bool isGrabbing =>;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag(playerTag);
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = maxSpeed;
    }

    void Update()
    {
        //calculate control inputs - grab, teleport, return
        //bool letGo = isCurrentlyGrabbed && !isGrabbing;
        //isCurrentlyGrabbed = isGrabbing;
        //bool doTeleport = interaction.button;
        //bool doReturn = interaction.button;

        //update position queue
        //if (isGrabbing) 
        //{
        Transform transform = GetComponent<Transform>();
        oldPositions.Enqueue(transform.position);
        if (oldPositions.Count > nrOldPositions) oldPositions.Dequeue();
        //}

        //resolve let go
        //if (letGo)
        //{
        //    Vector3 movement = transform.position - oldPositions.Peek();
        //    //rb.AddForce(movement);
        //    oldPositions.Clear();
        //}

        //resolve return to hand
        //if (doReturn)
        //{
        //    //maybe there's a way to directly put something into a grab?
        //    //if not, put the board below the player and the player on top
        //}

        //resolve teleport player
        //if (doTeleport)
        //{
        //    Player.transform.position = transform.position + new Vector3(0, Player.transform.position.y, 0);
        //    //maybe put the board into the right hand
        //}
    }
}
