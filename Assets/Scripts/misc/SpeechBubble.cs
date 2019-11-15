using UnityEngine;  
using System.Collections;
using System;

public interface BubbleOwner {
	void OnBubbleClosed();
}

[ExecuteInEditMode]  
public class SpeechBubble : MonoBehaviour  
{  
	//this game object's transform  
	private Transform goTransform;  
	//the game object's position on the screen, in pixels  
	private Vector3 goScreenPos;  
	//the game objects position on the screen  
	private Vector3 goViewportPos;  
	


	#if UNITY_ANDROID
	//the width of the speech bubble  
	public int bubbleWidth = (int)(Screen.width/5f);  
	//the height of the speech bubble  
	public int bubbleHeight = (int)(Screen.height/3f); 
	#else
	//the width of the speech bubble  
	public int bubbleWidth = 200;  
	//the height of the speech bubble  
	public int bubbleHeight = 100;  
	#endif

	//an offset, to better position the bubble  
	public float offsetX;  
	public float offsetY;  
	
	//an offset to center the bubble  
	private int centerOffsetX;  
	private int centerOffsetY;  
	
	//a material to render the triangular part of the speech balloon  
	public Material mat;  
	//a guiSkin, to render the round part of the speech balloon  
	public GUISkin guiSkin;  

	public string text;
	public Action action;
	public bool render;

	public BubbleOwner owner;
	
	//use this for early initialization  
	void Awake ()  
	{  
		//get this game object's transform  
		goTransform = this.GetComponent<Transform>(); 
		offsetX = bubbleWidth/2;  
		offsetY = bubbleHeight/2;  
	}  
	
	//use this for initialization  
	void Start()  
	{  
		//if the material hasn't been found  
		if (!mat)  
		{  
			Debug.LogError("Please assign a material on the Inspector.");  
			return;  
		}  
		
		//if the guiSkin hasn't been found  
		if (!guiSkin)  
		{  
			Debug.LogError("Please assign a GUI Skin on the Inspector.");  
			return;  
		}  
		
		//Calculate the X and Y offsets to center the speech balloon exactly on the center of the game object  
		centerOffsetX = (int) (bubbleWidth/1.75f);  
		centerOffsetY = (int) (bubbleHeight/1.75f);  
	}  
	
	//Called once per frame, after the update  
	void LateUpdate()  
	{  
		//find out the position on the screen of this game object  
		goScreenPos = Camera.main.WorldToScreenPoint(goTransform.position);   
		
		//Could have used the following line, instead of lines 70 and 71  
		//goViewportPos = Camera.main.WorldToViewportPoint(goTransform.position);  
		goViewportPos.x = goScreenPos.x/(float)Screen.width;  
		goViewportPos.y = goScreenPos.y/(float)Screen.height;  
	}  

	#if UNITY_ANDROID
	int textSize = 0;
	#endif

	//Draw GUIs 
	void OnGUI()  
	{
		if (!render) {
			return;
		}
		//Begin the GUI group centering the speech bubble at the same position of this game object. After that, apply the offset  
		Rect area = new Rect(goScreenPos.x-centerOffsetX-offsetX,Screen.height-goScreenPos.y-centerOffsetY-offsetY,bubbleWidth,bubbleHeight);
		GUI.BeginGroup(area);  

		//Render the round part of the bubble  
		GUI.Label(new Rect(0,0,bubbleWidth,bubbleHeight),"",guiSkin.customStyles[0]);  

		//If the button is pressed, go back to 41 Post  
		if(GUI.Button(new Rect(bubbleWidth-bubbleWidth/4f-bubbleWidth/10,bubbleHeight/15,bubbleWidth/4,bubbleHeight/6),"X")) {
			owner.OnBubbleClosed();
			if (render)
				render = false;
		}

		Rect contentArea = new Rect(bubbleWidth/10, bubbleHeight/4, 4*bubbleWidth/5, bubbleHeight - bubbleHeight/3f);

		//Render the contents
		//if its an action bubble, create a button with the action in it
		if (action != null) {
			if (GUI.Button(contentArea, text)) {
				action();
			}
		}
		//else if its a string bubble, create a label with the string
		else if (text != null && text != "") {
			#if UNITY_ANDROID
			if (textSize == 0) {
				textSize = Utils.TextMaximumSize(text, (int)contentArea.width, (int)contentArea.height, guiSkin.label);
			}
			if (guiSkin.label.fontSize != textSize) {
				guiSkin.label.fontSize = textSize;
			}
			#endif
			GUI.Label(contentArea, text, guiSkin.label);
		}
			
		
		GUI.EndGroup();  
	}

	//Called after camera has finished rendering the scene  
	void OnRenderObject()  
	{  
		if (!render) {
			return;
		}
		//push current matrix into the matrix stack  
		GL.PushMatrix();  
		//set material pass  
		mat.SetPass(0);  
		//load orthogonal projection matrix  
		GL.LoadOrtho();  
		//a triangle primitive is going to be rendered  
		GL.Begin(GL.TRIANGLES);  
		
		//set the color  
		GL.Color(Color.white);  
		
		//Define the triangle vetices  
		GL.Vertex3(goViewportPos.x, goViewportPos.y+(offsetY/3)/Screen.height, 0.1f);  
		GL.Vertex3(goViewportPos.x - (bubbleWidth/3)/(float)Screen.width, goViewportPos.y+offsetY/Screen.height, 0.1f);  
		GL.Vertex3(goViewportPos.x - (bubbleWidth/8)/(float)Screen.width, goViewportPos.y+offsetY/Screen.height, 0.1f);  
		
		GL.End();  
		//pop the orthogonal matrix from the stack  
		GL.PopMatrix();  
	}  
}  