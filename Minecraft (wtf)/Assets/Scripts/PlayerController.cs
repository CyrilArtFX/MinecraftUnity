using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float maxSpeed = 4f, maxSwimSpeed = 2f;
    [SerializeField, Range(0f, 30f)] private float maxAcceleration = 4f, maxAirAcceleration = 2f, maxSwimAcceleration = 10f;
    [SerializeField, Range(0f, 2f)] private float jumpHeight = 1.2f;
    [SerializeField, Range(0f, 180f)] private float camSensitivityX = 45f;
    [SerializeField, Range(0f, 180f)] private float camSensitivityY = 20f;
    [SerializeField] private float minCamAngle = -90f;
    [SerializeField] private float maxCamAngle = 90f;
    [SerializeField, Range(0f, 10f)] private float waterDrag = 1f;
    [SerializeField, Min(0f)] private float buoyancy = 1f;

    [Space(10f)]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform groundDetector;
    [SerializeField] private Transform waterDetectorForJump;
    [SerializeField] private LayerMask blocLayer;
    [SerializeField] private LayerMask waterLayer;


    private Rigidbody body;
    private Vector3 velocity, desiredVelocity;
    private float desiredRotation, desiredCamRotation;
    private bool desiredJump;

    private List<Water> watersIn = new List<Water>();

    bool inWater;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = inWater ? Input.GetAxis("Jump") : 0f;
        playerInput.z = Input.GetAxis("Vertical");
        playerInput = Vector3.ClampMagnitude(playerInput, 1f);
        float speed = inWater ? maxSwimSpeed : maxSpeed;
        desiredVelocity = new Vector3(playerInput.x, playerInput.y, playerInput.z) * speed;

        desiredJump |= Input.GetButtonDown("Jump");


        desiredRotation = Input.GetAxis("Mouse X") * camSensitivityX;
        desiredCamRotation = -Input.GetAxis("Mouse Y") * camSensitivityY;

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + desiredRotation * Time.deltaTime, 0f);

        float actualCamRotation = playerCamera.rotation.eulerAngles.x;
        if (actualCamRotation > 90) actualCamRotation -= 360;
        desiredCamRotation = actualCamRotation + desiredCamRotation * Time.deltaTime;
        desiredCamRotation = Mathf.Clamp(desiredCamRotation, minCamAngle, maxCamAngle);
        playerCamera.localRotation = Quaternion.Euler(desiredCamRotation, 0f, 0f);
    }

    private void FixedUpdate()
    {
        velocity = body.velocity;
        if (inWater)
        {
            velocity *= 1f - waterDrag * Time.deltaTime;
            velocity += (-Physics.gravity * buoyancy) * Time.deltaTime;
        }

        float currentX = Vector3.Dot(velocity, transform.right);
        float currentY = Vector3.Dot(velocity, transform.up);
        float currentZ = Vector3.Dot(velocity, transform.forward);

        float acceleration = inWater ? maxSwimAcceleration : OnGround() ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newY = Mathf.MoveTowards(currentY, desiredVelocity.y, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += transform.right * (newX - currentX) + transform.forward * (newZ - currentZ);
        if (inWater && desiredVelocity.y > 0) velocity += transform.up * (newY - currentY);

        if(watersIn.Count > 0)
        {
            Vector3 waterFlowForce = Vector3.zero;
            foreach (Water water in watersIn) waterFlowForce += water.flowForce;
            waterFlowForce.Normalize();
            velocity += waterFlowForce * Time.deltaTime * 10f;
        }


        if (desiredJump)
        {
            if(!inWater || OnGround()) desiredJump = false;
            Jump();
        }

        body.velocity = velocity;
        watersIn.Clear();
        inWater = false;
    }

    private void Jump()
    {
        if (OnGround())
        {
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            velocity += Vector3.up * jumpSpeed;
        }
    }

    private bool OnGround()
    {
        if(inWater)
        {
            Collider[] colliders = Physics.OverlapSphere(waterDetectorForJump.transform.position, 0.1f, waterLayer);
            if (colliders.Length == 0) return true;
            else return false;
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(groundDetector.transform.position, 0.1f, blocLayer);
            if (colliders.Length > 0) return true;
            else return false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if((waterLayer & (1 << other.gameObject.layer)) != 0)
        {
            if (!watersIn.Contains(other.gameObject.GetComponent<Water>())) watersIn.Add(other.gameObject.GetComponent<Water>());
            inWater = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if ((waterLayer & (1 << other.gameObject.layer)) != 0)
        {
            if (!watersIn.Contains(other.gameObject.GetComponent<Water>())) watersIn.Add(other.gameObject.GetComponent<Water>());
            inWater = true;
        }
    }
}
