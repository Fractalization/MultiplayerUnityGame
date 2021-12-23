using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouse : MonoBehaviour
{
    Vector2 mouseRotation = Vector2.zero;
	public float mouseSensitivity = 20;
	public Rigidbody player;
	Vector3 playerRotation;

	void Start() {
		Cursor.visible=false;
		Cursor.lockState = CursorLockMode.Locked;
	}

    // Update is called once per frame
    void FixedUpdate()
	{
		float xRotate = Input.GetAxis ("Mouse Y");
		if((mouseRotation.x + -xRotate) < 30 && (mouseRotation.x + -xRotate) > -30)
			mouseRotation.x += -xRotate;
		float mouseX= Input.GetAxis ("Mouse X");
		mouseRotation.y += mouseX;
		transform.eulerAngles = (Vector2)mouseRotation * mouseSensitivity;
		playerRotation.Set(0,mouseSensitivity*mouseX,0);
		player.rotation = Quaternion.Euler(player.rotation.eulerAngles + playerRotation);
		//player.MoveRotation(player.rotation * Quaternion.Euler(playerRotation));
		//player.rotation = Quaternion.Euler(player.rotation.eulerAngles + new Vector3(0f, mouseSensitivity*mouseX, 0f));
		Debug.Log(Quaternion.Euler(playerRotation * Time.deltaTime));
		
		//player.rotation = playerRotation;
    }
}
