using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UI : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI speedText;
    public GameObject fpsToggle;
    public GameObject speedToggle;
    public GameObject resDropdown;
    private bool menuActive;
    public Rigidbody playerSpeedTracker;
    public PhotonView UIView;

    private int frameCount = 0;
    private float time = 0f;
    private float pollTime = 1f;

    void ToggleMenu(bool firstTime){
        if(firstTime){
            menuActive = false;
            fpsToggle.SetActive(false);
            resDropdown.SetActive(false);
            speedToggle.SetActive(false);
            Cursor.visible=false;
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }
        menuActive = !menuActive;
        fpsToggle.SetActive(!fpsToggle.activeSelf);
		resDropdown.SetActive(!resDropdown.activeSelf);
        speedToggle.SetActive(!speedToggle.activeSelf);
		Cursor.visible=!Cursor.visible;
		if(Cursor.visible){
		    Cursor.lockState = CursorLockMode.None;
		}
		else{
		    Cursor.lockState = CursorLockMode.Locked;
		}
    }

    void Start(){
        if(UIView.IsMine){
            ToggleMenu(true);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(!UIView.IsMine)
            return;
        time += Time.deltaTime;
        frameCount++;

        if(time >= pollTime){
            int fps = (int) (frameCount / time);
            fpsText.text = "FPS:" + fps.ToString();

            time = 0;//subtracting pollTime doesn't make sense to me
            frameCount = 0;
        }

        speedText.text = "Speed: "+(int)(playerSpeedTracker.velocity.magnitude)+" m/s";

        if(Input.GetKeyDown(KeyCode.Escape)){
            ToggleMenu(false);
        }

        if(menuActive){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
	
    }
}
