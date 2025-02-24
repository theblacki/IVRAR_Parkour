using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.Input.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TRBScript : MonoBehaviour
{
    public float throwingSpeed;
    public float maxSpeed;
    public float gravityMultiplier;
    public float maxRange;
    public int nrOldPositions;
    public Transform leftHandTransform;
    //Offset to player height from the floor to reach it at the bottom of the playing area. Is always -0.22 for me.
    public float playerYOffset = -0.22f;
    public float flyInputStrength;

    public GameObject player;
    public AudioClip teleportSound;
    public GameObject slidersChild;
    public SliderValueProvider speedSlider;
    public SliderValueProvider rangeSlider;
    public ParkourCounter metrics;

    private static readonly string playerTag = "Player";
    private Rigidbody rb;
    private readonly Queue<Tuple<Vector3, Vector3>> oldPositions = new();
    private readonly LinkedList<Vector3> floatAfterQueue = new();
    private bool isTouchedByHand;
    private bool isCurrentlyGrabbed;
    private float defaultDrag;
    private readonly float overLimitDrag = 2;
    private LayerMask geometryLayer;
    private bool justLetGo = false;

    public bool IsGrabbingRight => RightTrigger && isTouchedByHand;
    public bool IsGrabbingLeft => LeftTrigger && isTouchedByHand;
    public float CurrentDistance => (transform.position - player.transform.position).magnitude;
    public bool CurrentlyOutsideRange => (CurrentDistance > maxRange * rangeSlider.sliderValue);

    private bool DoTeleport => OVRInput.GetDown(OVRInput.Button.One);
    private bool DoFly => OVRInput.Get(OVRInput.Button.Two);
    private bool DoReturn => OVRInput.Get(OVRInput.Button.Three);
    private bool DoFollowAfter => OVRInput.Get(OVRInput.Button.Four);
    private bool RightTrigger => (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > .4f
                               || OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > .4f);
    private bool LeftTrigger => (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > .4f
                               || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > .4f);

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = maxSpeed;
        defaultDrag = rb.linearDamping;
        geometryLayer = LayerMask.NameToLayer("geometry");
    }

    private void Update()
    {
        //calculate control inputs - grab, teleport, return
        bool letGo = isCurrentlyGrabbed && !IsGrabbingRight;
        bool takeInHand = !isCurrentlyGrabbed && (IsGrabbingRight || IsGrabbingLeft);
        isCurrentlyGrabbed = IsGrabbingRight;

        rb.linearDamping = CurrentlyOutsideRange ? overLimitDrag : defaultDrag;

        if (IsGrabbingLeft)
        {
            slidersChild.SetActive(true);
        }
        else if (!LeftTrigger)
        {
            slidersChild.SetActive(false);
        }

        if (takeInHand)
        {
            Stop();
        }

        if (isCurrentlyGrabbed) floatAfterQueue.Clear();

        //resolve let go (throw)
        if (letGo)
        {
            Vector3 movement = (transform.position - oldPositions.Peek().Item1) * throwingSpeed;
            if (movement.magnitude > maxSpeed * speedSlider.sliderValue) movement = maxSpeed * speedSlider.sliderValue * movement.normalized;
            rb.AddForce(movement, ForceMode.VelocityChange);
            rb.angularVelocity = new Vector3(0, (transform.eulerAngles.y - oldPositions.Peek().Item2.y) / nrOldPositions, 0);
            justLetGo = true;
            metrics.ThrowActionUses++;
        }

        //resolve return to hand
        if (DoReturn)
        {
            transform.position = leftHandTransform.position + leftHandTransform.right * .1f;
            floatAfterQueue.Clear();
            Stop();

            if (OVRInput.GetDown(OVRInput.Button.Three)) metrics.RecallActionUses++;
        }

        //resolve teleport player
        if (DoTeleport)
        {
            TeleportPlayer(transform.position);
            GetComponent<AudioSource>().PlayOneShot(teleportSound, 1);
            floatAfterQueue.Clear();
            metrics.TeleportActionUses++;
        }

        //resolve fly on TRB
        if (DoFly)
        {
            TeleportPlayer(transform.position);

            float horizontalInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            rb.AddForce(flyInputStrength * horizontalInput * transform.right, ForceMode.Force);
            rb.angularVelocity = new Vector3(0, rb.angularVelocity.y, 0);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            if (OVRInput.GetDown(OVRInput.Button.Two)) metrics.MountActionUses++;
        }
    }

    public void TeleportPlayer(Vector3 newPosition)
    {
        player.transform.position = Physics.Raycast(newPosition, Vector3.down, out RaycastHit hit, 1f, geometryLayer) ?
            hit.point + new Vector3(0, playerYOffset, 0) : newPosition;
    }

    private void Stop()
    {
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!RightTrigger && !LeftTrigger && !DoReturn)
        {
            rb.AddForce((CurrentlyOutsideRange ? 5 : DoFly ? 0.3f : 1) * gravityMultiplier * Vector3.down, ForceMode.Force);
        }
        else
        {
            rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
        }

        //update position queue (don't care about delta)
        oldPositions.Enqueue(Tuple.Create(transform.position, transform.eulerAngles));
        if (oldPositions.Count > nrOldPositions) oldPositions.Dequeue();

        //fill float after queue
        if (floatAfterQueue.Count == 0) floatAfterQueue.AddLast(transform.position);
        else if ((transform.position - floatAfterQueue.Last.Value).magnitude > 0.02f) floatAfterQueue.AddLast(transform.position);

        //handle follow after
        if (DoFollowAfter && floatAfterQueue.Count > 0)
        {
            TeleportPlayer(floatAfterQueue.First.Value);
            floatAfterQueue.RemoveFirst();
            if (OVRInput.GetDown(OVRInput.Button.Four)) metrics.FollowActionUses++;
        }

        if (justLetGo)
        { //Cannot be done in letgo if, because the final Grabbable script execution (with its own transformation setting) happens last in a frame
            justLetGo = false;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        //metrics for avg player height
        Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 1f, geometryLayer);
        float currentHeight = player.transform.position.y - hit.point.y;
        metrics.AvgDistToGround = (metrics.AvgDistToGround * metrics.AvgCount + currentHeight) / ++metrics.AvgCount;
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
