using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public GameObject fpsToggle;
    public GameObject resDropdown;

    private int frameCount = 0;
    private float time = 0f;
    private float pollTime = 1f;

    void Start(){
	//fpsToggle = transform.Find("FPSToggle").gameObject;
	fpsToggle.SetActive(false);
	//resDropdown = transform.Find("ResolutionDropdown").gameObject;
	resDropdown.SetActive(false);
	Cursor.visible=false;
	Cursor.lockState = CursorLockMode.Locked;
    }
    
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        if(time >= pollTime){
            int fps = (int) (frameCount / time);
            fpsText.text = "FPS:" + fps.ToString();

            time = 0;//subtracting pollTime doesn't make sense to me
            frameCount = 0;
        }
	
	if(Input.GetKeyDown(KeyCode.Escape)){
		fpsToggle.SetActive(!fpsToggle.activeSelf);
		resDropdown.SetActive(!resDropdown.activeSelf);
		Cursor.visible=!Cursor.visible;
		if(Cursor.visible){
		    Cursor.lockState = CursorLockMode.None;
		}
		else{
		    Cursor.lockState = CursorLockMode.Locked;
		}
	}
	
    }
}
