using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TRBScript : MonoBehaviour
{
    public float throwingSpeed;
    public float maxSpeed;
    public float gravityMultiplier;
    public float maxRange;
    public int nrOldPositions;
    public Transform leftHandTransform;

    public GameObject player;
    public AudioClip teleportSound;
    public GameObject slidersChild;

    private static readonly string playerTag = "Player";
    private Rigidbody rb;
    private readonly Queue<Tuple<Vector3, Vector3>> oldPositions = new();
    private bool isTouchedByHand;
    private bool isCurrentlyGrabbed;
    private float defaultDrag;
    private readonly float overLimitDrag = 2;
    
    public bool IsGrabbingRight => (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > .1f
                            || OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > .1f)
                            && isTouchedByHand;
    public bool IsGrabbingLeft => (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > .1f
                            || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > .1f)
                            && isTouchedByHand;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = maxSpeed;
        defaultDrag = rb.linearDamping;
    }

    void Update()
    {
        //Debug Info
        if (IsGrabbingRight && !isCurrentlyGrabbed) Debug.Log("Is now grabbing");
        if (!IsGrabbingRight && isCurrentlyGrabbed) Debug.Log("Is no longer grabbing");

        //calculate control inputs - grab, teleport, return
        bool letGo = isCurrentlyGrabbed && !IsGrabbingRight;
        bool takeInHand = !isCurrentlyGrabbed && IsGrabbingRight;
        isCurrentlyGrabbed = IsGrabbingRight;
        bool doTeleport = OVRInput.GetDown(OVRInput.Button.One);
        bool doReturn = OVRInput.Get(OVRInput.Button.Three);

        //Slider Interaction
        if (IsGrabbingLeft)
        {
            slidersChild.SetActive(true);
            transform.LookAt(player.transform.position);
            transform.Rotate(new Vector3 (45, 0, 180));
        }
        else
        {
            slidersChild.SetActive(false);
        }

        if (takeInHand)
        {
            rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
            rb.angularVelocity = Vector3.zero;
            oldPositions.Clear();
        }
        
        rb.linearDamping = ((transform.position - player.transform.position).magnitude > maxRange) ? overLimitDrag : defaultDrag;
       
        //resolve let go (throw)
        if (letGo)
        {
            Vector3 movement = (transform.position - oldPositions.Peek().Item1) * throwingSpeed;
            if (movement.magnitude > maxSpeed) movement = movement.normalized * maxSpeed;
            rb.AddForce(movement, ForceMode.VelocityChange);
            rb.angularVelocity = (transform.eulerAngles - oldPositions.Peek().Item2) / nrOldPositions;
            Debug.LogWarning("Throw with movement:" + movement.ToString() + " and AngularVelocity: " + rb.angularVelocity);
        }

        //resolve return to hand
        if (doReturn)
        {
            transform.position = leftHandTransform.position + leftHandTransform.right * .1f;

        }

        //resolve teleport player
        if (doTeleport)
        {
            player.transform.position = transform.position + new Vector3(0, -.04f, 0);
            GetComponent<AudioSource>().PlayOneShot(teleportSound, 1);
            //maybe put the board into the right hand
        }
    }

    private void FixedUpdate()
    {
        if (!IsGrabbingRight)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Force);
        }
        //update position queue (don't care about delta)
        oldPositions.Enqueue(Tuple.Create(transform.position, transform.eulerAngles));
        if (oldPositions.Count > nrOldPositions) oldPositions.Dequeue();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("hand"))
        {
            isTouchedByHand = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("hand"))
        {
            isTouchedByHand = false;
        }
    }
}
