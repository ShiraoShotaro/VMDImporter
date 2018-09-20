using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AxisAndGridRenderer : MonoBehaviour {

	// Draws a line from "startVertex" var to the curent mouse position.
	public Material mat;
	public Material matZ;
	Vector3 startVertex;
	Vector3 mousePos;

	
	public Color gridColor = new Color(0.2f, 0.2f, 0.2f);

	public Color axisXColor = new Color(1, 0, 0);

	public Color axisYColor = new Color(0, 1, 0);

	public Color axisZColor = new Color(0, 0, 1);

	[Range(0.01f, 50f)]
	public float axisSize = 10;

	[Range(0.01f, 50f)]
	public float gridSize = 1;

	public int gridResolution = 10;

	public bool isShowGrid = true;

	void Start()
	{
	}

	void Update() {}

	void OnRenderObject(){
		Assert.IsNotNull(mat, "Grid material is null.");
		Assert.IsNotNull(matZ, "Axis material is null.");

		Vector3 vPos = Vector3.zero;
		Quaternion qRot = Quaternion.identity;
		Matrix4x4 mtx = Matrix4x4.TRS(vPos, qRot, Vector3.one);

		//////////////////////////////////////////////////////////////////////
		// Grid
		if (isShowGrid)
		{
			mat.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(mtx);
			GL.Begin(GL.LINES);
			GL.Color(gridColor);
			for (int i = -gridResolution; i <= gridResolution; i++)
			{
				GL.Vertex(new Vector3(gridSize * gridResolution, 0, i * gridSize));
				GL.Vertex(new Vector3(-gridSize * gridResolution, 0, i * gridSize));
				GL.Vertex(new Vector3(i * gridSize, 0, gridSize * gridResolution));
				GL.Vertex(new Vector3(i * gridSize, 0, -gridSize * gridResolution));
			}
			GL.End();
			GL.PopMatrix();
		}

		/////////////////////////////////////////////////////////////////////
		// 軸の描写
		matZ.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(mtx);
		GL.Begin(GL.LINES);
		GL.Color(axisXColor);
		GL.Vertex(Vector3.zero);
		GL.Vertex(new Vector3((int)axisSize, 0, 0));
		GL.Color(axisYColor);
		GL.Vertex(Vector3.zero);
		GL.Vertex(new Vector3(0, (int)axisSize, 0));
		GL.Color(axisZColor);
		GL.Vertex(Vector3.zero);
		GL.Vertex(new Vector3(0, 0, (int)axisSize));
		GL.End();
		GL.PopMatrix();
	}
}
