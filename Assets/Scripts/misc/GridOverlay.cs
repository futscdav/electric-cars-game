using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour {

	public Rect box;

	public bool showMain = true;
	public bool showSub = false;
	public bool enabled = true;
	
	public int gridSizeX;
	public int gridSizeY;
	public int gridSizeZ;
	
	public float smallStep;
	public float largeStep;
	
	public float startX;
	public float startY;
	public float startZ;
	
	private float offsetY = 0;
	private float scrollRate = 0.1f;
	private float lastScroll = 0f;
	
	private Material lineMaterial;
	
	private Color mainColor = new Color(0f,1f,0f,1f);
	private Color subColor = new Color(0f,0.5f,0f,1f);

	void CreateLineMaterial() {
		
		if( !lineMaterial ) {
			lineMaterial = new Material( Shader.Find("Custom/LineShader") );
			// holdover from when this was a dynamic shader
			lineMaterial.hideFlags = HideFlags.None;
			lineMaterial.shader.hideFlags = HideFlags.None;
			}
	}
	
	void OnPostRender() {
		if (!enabled) {
			return;
		}
		CreateLineMaterial();
		// set the current material
		lineMaterial.SetPass( 0 );
		
		GL.Begin( GL.LINES );

		GL.Color(mainColor);
	
		float offset = +0.5f;
		float top = box.yMin + offset;
		float left = box.xMin - offset;
		float widthstop = box.xMax + offset;
		float heightstop = box.yMin - box.height - offset;

		for (float j = top; j >= heightstop; j -= 1) {
			GL.Vertex3(left, j, 0);
			GL.Vertex3(widthstop, j, 0);
			//Debug.Log("Horizontal line");
		}
		for (float j = left; j <= widthstop; j += 1) {
			GL.Vertex3(j, top, 0);
			GL.Vertex3(j, heightstop, 0);
			//Debug.Log("Vertical line");
		}
		
		
		GL.End();
	}
}