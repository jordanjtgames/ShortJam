using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Utils
{
	public static class Helpers{
		public static void CLOG(string message){ // Logs to system console
			Debug.Log(message);
		}
		public static void GLOG(string message){ // Logs to in-game console
			if(DevTools.inGameConsoleOutput.Length < 131072) DevTools.inGameConsoleOutput += message + "\n";
		}
		public static void WLOG(string value, string name){ // Adds and watches variable
			if(DevTools.watchedNames.Contains(name)){
				DevTools.watchedValues[DevTools.watchedNames.IndexOf(name)] = value;
			}else{
				DevTools.watchedNames.Add(name);
				DevTools.watchedValues.Add(value);
			}
		}
		/*
		public static void DB_LINE(Vector3 startWorldPosition, Vector3 endWorldPosition){
			DevTools.lineStart = startWorldPosition;
			DevTools.lineEnd = endWorldPosition;
		}
		public static void DB_SPHERE(Vector3 worldPosition, float radius){
			DevTools.spherePosition = worldPosition;
			DevTools.sphereRadius = radius;
		}
		public static void DB_BOX(Vector3 worldPosition, Basis basis){
			DevTools.boxPosition = worldPosition;
			DevTools.boxBasis = basis;
		}
		*/
		//public static Action<string> CMD; // Commands event. All commands are lower case
		public static Vector2 WrapMouse(Vector2 rawMousePosition) // Will need changing if play window offset changes
		{
			float xThresh = 3479.61f;
			float bottomOffset = 413.79f;
			// ^ These need to equal bottom left of floating window
			bool otherScreen = rawMousePosition.x >= xThresh;

			return otherScreen ? new Vector2(rawMousePosition.x % xThresh * 0.6445f, (rawMousePosition.y + bottomOffset) * 0.6445f) : rawMousePosition;
		}
	}
}

public class DevTools : MonoBehaviour
{
	public static string inGameConsoleOutput = "";
	public static List<string> watchedNames = new List<string>();
	public static List<string> watchedValues = new List<string>();
	public static float scrollNormPos = 0f;

	public List<TextMeshProUGUI> watchLabels;

	public TextMeshProUGUI inputFieldString, consoleTMP;
	public ScrollRect consoleScrollView;
	private string inputString;

	public GameObject devConsole, devConsoleCommandLine;
	private bool consoleIsOpen = false, enteringCommand = false;
	private bool needsToFocusCommandLine = false;
	private float focusDelay = 0.15f;

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.BackQuote)){
			bool leftShift = Input.GetKey(KeyCode.LeftShift);
			if(consoleIsOpen){
				if(leftShift)
					ShowDevConsole(true);
				else HideDevConsole();
			}else{
				ShowDevConsole(leftShift);
			}
		}

		if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape))
		{
			EditorApplication.ExecuteMenuItem("Edit/Play");
		}

		consoleTMP.text = DevTools.inGameConsoleOutput;

		if(consoleIsOpen)
		{
			for (int i = 0; i < 12; i++)
			{
				if(i > watchedNames.Count-1){
					watchLabels[i * 2].text = "-";
					watchLabels[(i * 2)+1].text = "-";
				}else{
					watchLabels[i * 2].text = watchedNames[i];
					watchLabels[(i * 2)+1].text = watchedValues[i];
				}
			}

			bool mouseIsOverScrollView = Utils.Helpers.WrapMouse(Input.mousePosition).y > (Screen.height * 0.7425);
			if(mouseIsOverScrollView){
				scrollNormPos = Mathf.Clamp01(scrollNormPos + (Input.mouseScrollDelta.y * 0.1f));
			}
			consoleScrollView.verticalNormalizedPosition = scrollNormPos;
		}

		if(needsToFocusCommandLine && focusDelay > 0) focusDelay -= Time.deltaTime;

		if(enteringCommand && focusDelay <= 0)
		{
			foreach (char c in Input.inputString)
			{
				if (c == '\b') // has backspace/delete been pressed?
				{
					if (inputString.Length != 0)
					{
						inputString = inputString.Substring(0, inputString.Length - 1);
					}
				}
				else if ((c == '\n') || (c == '\r')) // enter/return
				{
					Debug.Log("User entered their name: " + inputString);
				}
				else
				{
					inputString += c;
				}
			}
			inputFieldString.text = inputString;
		}
	}

	void ShowDevConsole(bool isCommand)
	{
		//if(consoleIsOpen) return;

		inputString = "";
		inputFieldString.text = "";

		devConsole.SetActive(true);
		devConsoleCommandLine.SetActive(isCommand);
		needsToFocusCommandLine = isCommand;

		focusDelay = 0.15f;
		enteringCommand = isCommand;
		Cursor.lockState = isCommand ? CursorLockMode.None : CursorLockMode.Locked;
		consoleIsOpen = true;
	}

	void HideDevConsole()
	{
		//if(!consoleIsOpen) return;

		devConsole.SetActive(false);
		devConsoleCommandLine.SetActive(false);
		needsToFocusCommandLine = false;

		enteringCommand = false;
		Cursor.lockState = CursorLockMode.Locked;
		consoleIsOpen = false;
	}
}
