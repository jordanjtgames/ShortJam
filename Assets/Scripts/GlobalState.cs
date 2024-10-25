using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils.Helpers;

public class GlobalState : MonoBehaviour
{
	GameObject cube;
	public Transform uiCursor;

	void Awake(){
		
	}

	void Start()
	{
		/*
		Debug.Log("First Log");
		Debug.Log("Second Log 2");
		Debug.Log("Third Log 3");
		Debug.Log("Fourth Log 4");
		*/

		cube = GameObject.Find("Cube_1");
	}

	void Update()
	{
		cube.transform.Rotate(Vector3.one * Time.deltaTime * 50f);

		//uiCursor.position = Input.mousePosition;
		//uiCursor.position = WrapMouse(playerInputs.PlayerMap.Pointer.ReadValue<Vector2>());
		//uiCursor.position = WrapMouse(Input.mousePosition);

		if(Input.GetKeyDown(KeyCode.H))
		{
			//GLOG("Hello there");

			for (int i = 0; i < 10; i++)
			{
				GLOG("Test Message Logged " + i.ToString());
			}
		}

		//WLOG(cube.transform.rotation.ToString(),"Cube Rotation");
	}
}
