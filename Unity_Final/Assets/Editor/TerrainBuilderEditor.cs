using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(paintTerrain))]
public class TerrainBuilderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		paintTerrain myScript = (paintTerrain)target;
		if (GUILayout.Button("Generate Terrain"))
		{
			myScript.Start();
		}
	}
 
}

