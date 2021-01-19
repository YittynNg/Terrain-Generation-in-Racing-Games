using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Make the plane hilly

public class GenerateTerrain : MonoBehaviour
{
    int heightScale = 5;
	float detailScale = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
    	// attach code to plane and get its mesh
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // looping over entire mesh
        // lifting up each of the vertices (position y) using pearlin noise to calculate height (return random number)
        // also using position of plane in the world as offset so multiple plane create	 wont look exactly the same and is seamless
        for (int v=0; v<vertices.Length; v++){
        	vertices[v].y = Mathf.PerlinNoise((vertices[v].x + this.transform.position.x)/detailScale,
        		                              (vertices[v].z + this.transform.position.z)/detailScale) * heightScale;
        }

        // set new calculated vertices back to mesh.vertices
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        // add collider so can walk on the surface
        this.gameObject.AddComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


