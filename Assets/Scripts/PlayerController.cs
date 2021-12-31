using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
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
    GameObject[] itemSlots;
    int itemIndex = -1;
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

        itemSlots[0].SetActive(true);
        itemSlots[1].SetActive(false);
        itemSlots[2].SetActive(false);
        itemSlots[3].SetActive(false);
        EquipItem(0);
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

        for(int i = 1; i <= itemSlots.Length; i++){
            if(Input.GetKeyDown(KeyCode.Alpha0 + i)){
                Debug.Log("pressed "+i);
                EquipItem(i-1);
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            if(itemSlots[itemIndex].transform.childCount > 0){
                Debug.Log(itemSlots[itemIndex].GetComponentInChildren<Item>());
                itemSlots[itemIndex].GetComponentInChildren<Item>().Use();
            }
        }

        //this is where I want to shoot a ray from the camera to pick up weapons and put it in the first available slot or drop your current weapon if not
        RaycastHit hit;
        if (Physics.Raycast(camObj.transform.position, camObj.transform.TransformDirection(Vector3.forward), out hit, 5f, itemLayers))
        {
            //only itemSlots
            if(Input.GetKeyDown("e")){
                
                Debug.Log("Tried picking up item");

                Transform t = hit.transform;
                Debug.Log(t.gameObject.GetComponent<Item>().itemInfo.itemType);
                int itemType = t.gameObject.GetComponent<Item>().itemInfo.itemType;
                int indexInserted = -1;
                switch (itemType){
                    case 0: //equppied slot
                        t.SetParent(itemSlots[itemIndex].transform, true);
                        indexInserted = itemIndex;
                        break;
                    case 1: //primary
                        t.SetParent(itemSlots[0].transform, true);
                        indexInserted = 0;
                        break;
                    case 2: //secondary
                        t.SetParent(itemSlots[1].transform, true);
                        indexInserted = 1;
                        break;
                    case 3: //knife
                        t.SetParent(itemSlots[2].transform, true);
                        indexInserted = 2;
                        break;
                    case 4: //nade
                        t.SetParent(itemSlots[3].transform, true);
                        indexInserted = 3;
                        break;
                }
                // if(indexInserted > -1)
                //     itemSlots[indexInserted].transform.GetChild(0).position = Vector3.zero;
                t.transform.position = new Vector3(t.transform.parent.position.x + 0.6f, t.transform.parent.position.y - .6f, t.transform.parent.position.z);
                t.transform.rotation = camObj.transform.rotation;
                int children = t.childCount;
                t.gameObject.layer = 0;
                for(int i = 0; i < children; i++){
                    t.GetChild(i).gameObject.layer = 0;
                }
                //Debug.Log("destroying "+hit.transform.GetComponentInChildren<Collider>());
                // Debug.Log(itemHolder.GetComponentInChildren<Item>());
                // Debug.Log(itemHolder.GetComponentInChildren<Item>().itemInfo);
                //Destroy(hit.transform.GetComponentInChildren<Collider>());
                
                
                Debug.Log("destroying"+t.gameObject.GetComponent<Rigidbody>());
                Destroy(t.gameObject.GetComponent<Rigidbody>());
            }

            //Debug.DrawRay(camObj.transform.position, camObj.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
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

    void EquipItem(int index){//index vals: 0 = primary, 1 = secondary, 2 = melee, 3 = nade
        if(itemIndex == index)
            return;
        itemIndex = index;
        itemSlots[itemIndex].SetActive(true);
        if(previousItemIndex != -1){
            itemSlots[previousItemIndex].SetActive(false);
        }
        previousItemIndex = itemIndex;

        if(view.IsMine){
            Hashtable hash = new Hashtable();
            hash.Add("ItemIndex",itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
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
        correctVelocity.y = Jump(correctVelocity.y);
        

        //Return
        return correctVelocity;
    }

	private float Jump(float currentVelocity)
	{

		if(Time.time < (lastJumpPress + futureJumpTimeBuffer) && CheckGround()){
			lastJumpPress = -1f;
            return jumpSpeed;
		}

		return currentVelocity;
	}
	
	private bool CheckGround()
	{
        Ray ray = new Ray(transform.position, Vector3.down);
        bool result = Physics.Raycast(ray, GetComponent<Collider>().bounds.extents.y + 0.1f, groundLayers);
        return result;
	}

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){
        if(!view.IsMine && targetPlayer == view.Owner){
            EquipItem((int)changedProps["itemIndex"]);
        }
    }
}
