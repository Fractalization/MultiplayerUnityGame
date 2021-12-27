using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public float accel;         // How fast the player accelerates on the ground
    [SerializeField]
    public float airAccel;      // How fast the player accelerates in the air
    [SerializeField]
    public float maxSpeed;      // Maximum player speed on the ground
    [SerializeField]
    public float maxAirSpeed;   // "Maximum" player speed in the air
    [SerializeField]
    public float friction;        // How fast the player decelerates on the ground
    [SerializeField]
    public float jumpSpeed;       // speed the player leaves the ground (instant)
    [SerializeField]
    private LayerMask groundLayers;
    [SerializeField]
    private LayerMask itemLayers;

    [SerializeField]
    private GameObject camObj;

    [SerializeField]
    private GameObject itemHolder;



    [SerializeField]
    Item[] items;
    int itemIndex;
    int previousItemIndex = -1;

    
    private float lastJumpPress = -1f;
    private float futureJumpTimeBuffer = 0.1f;
	private bool onGround = false;
    PhotonView view;
    Rigidbody playerRb;
    Vector3 playerVelocity;
    Vector2 input;
    
    private void Start(){
        view = GetComponent<PhotonView>();
        if(!view.IsMine){
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(transform.Find("UI Canvas").gameObject);
            Destroy(GetComponent<Rigidbody>());
        }
        playerRb = GetComponent<Rigidbody>();
    }

	private void Update()
    {
        if(!view.IsMine)
            return;
        //print(new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z).magnitude);
        if (Input.GetButton("Jump"))
		{
		    lastJumpPress = Time.time;
		}

        //this is where I want to shoot a ray from the camera to pick up weapons and put it in the first available slot or drop your current weapon if not
        RaycastHit hit;
        if (Physics.Raycast(camObj.transform.position, camObj.transform.TransformDirection(Vector3.forward), out hit, 5f, itemLayers))
        {
            //only items
            if(Input.GetKeyDown("e")){
                
                Debug.Log("Tried picking up item");

                hit.transform.SetParent(itemHolder.transform);
                hit.transform.position = itemHolder.transform.position;
                hit.transform.localScale = Vector3.one;
                hit.transform.gameObject.layer = 0;
                Debug.Log("destroying "+hit.transform.GetComponentInChildren<Collider>());
                Destroy(hit.transform.GetComponentInChildren<Collider>());
                
                hit.transform.rotation = transform.rotation;
                Debug.Log("destroying"+hit.rigidbody);
                Destroy(hit.rigidbody);
            }

            Debug.DrawRay(camObj.transform.position, camObj.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log(hit.transform.gameObject);
        }
        else
        {
            Debug.DrawRay(camObj.transform.position, camObj.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
        //return result;    
	}

	private void FixedUpdate()
	{
        if(!view.IsMine)
            return;
		input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Get player velocity
        playerVelocity = playerRb.velocity;
        // Slow down if on ground
        playerVelocity = CalculateFriction(playerVelocity);
        // Add player input
        playerVelocity += CalculateMovement(input, playerVelocity);
        // Assign new velocity to player object
		playerRb.velocity = playerVelocity;
	}

    void EquipItem(int index){
        itemIndex = index;
        items[itemIndex].itemGameObject.SetActive(true);
        if(previousItemIndex != -1){
            items[previousItemIndex].itemGameObject.SetActive(false);
        }
        previousItemIndex = itemIndex;
    }


	private Vector3 CalculateFriction(Vector3 currentVelocity)
	{
        onGround = CheckGround();
		float speed = currentVelocity.magnitude;

        if (!onGround || ((Time.time < (lastJumpPress + futureJumpTimeBuffer)) && onGround) || speed == 0f)
            return currentVelocity;

        float drop = speed * friction * Time.fixedDeltaTime;
        return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
    }


	private Vector3 CalculateMovement(Vector2 input, Vector3 velocity)
	{
        onGround = CheckGround();

        //Different acceleration values for ground and air
        float curAccel = accel;
        if (!onGround)
            curAccel = airAccel;

        //Ground speed
        float curMaxSpeed = maxSpeed;

        //Air speed
        if (!onGround)
            curMaxSpeed = maxAirSpeed;
        
        //Get rotation input and make it a vector
        Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f);
        Vector3 inputVelocity = Quaternion.Euler(camRotation) *
                                new Vector3(input.x * curAccel, 0f, input.y * curAccel);

        //Ignore vertical component of rotated input
        Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.fixedDeltaTime;

        //Get current velocity
        Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);

        //How close the current speed to max velocity is (1 = not moving, 0 = at/over max speed)
        float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));

        //How perpendicular the input to the current velocity is (0 = 90°)
        float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

        //Scale the input to the max speed
        Vector3 modifiedVelocity = alignedInputVelocity * max;

        //The more perpendicular the input is, the more the input velocity will be applied
        Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

        float beforeJumpYVelocity = correctVelocity.y;
        //Apply jump
        correctVelocity += Jump();

        if (correctVelocity.y > beforeJumpYVelocity){
            correctVelocity.y = jumpSpeed;
        }
        

        //Return
        return correctVelocity;
    }

	private Vector3 Jump()
	{
		Vector3 jumpVelocity = Vector3.zero;

		if(Time.time < (lastJumpPress + futureJumpTimeBuffer) && CheckGround()){
			lastJumpPress = -1f;
            jumpVelocity.y = jumpSpeed;
            return jumpVelocity;
		}

		return jumpVelocity;
	}
	
	private bool CheckGround()
	{
        Ray ray = new Ray(transform.position, Vector3.down);
        bool result = Physics.Raycast(ray, GetComponent<Collider>().bounds.extents.y + 0.1f, groundLayers);
        return result;
	}
}
