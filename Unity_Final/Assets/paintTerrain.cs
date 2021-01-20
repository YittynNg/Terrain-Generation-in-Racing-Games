using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paintTerrain : MonoBehaviour
{
    [System.Serializable]
    public class SplatHeights
    {
    	public int textureIndex;
    	public int startingHeight;
    	public int overlap;
    }

    TerrainData terrainData;
    float[,] newHeightData;

    public SplatHeights[] splatHeights;

    [Header("Perlin Noise Settings")]
    [Range(0.000f,0.01f)]
    public float bumpiness;
    [Range(0.000f,1.000f)]
    public float damp;

    [Header("Mountain Settings")]
    public int numMountains;
    [Range(0.001f,0.5f)]
    public float heightChange;
    [Range(0.0001f,0.05f)]
    public float sideSlope;

    [Header("Hole Settings")]
    public int numHoles;
    [Range(0.0f,1.0f)]
    public float holeDepth;
    [Range(0.001f,0.5f)]
    public float holeChange;
    [Range(0.0001f,0.05f)]
    public float holeSlope;

    [Header("River Settings")]
    public int numRivers;
    [Range(0.001f,0.05f)]
    public float digDepth;
    [Range(0.001f,1.0f)]
    public float maxDepth;
    [Range(0.0001f,0.05f)]
    public float bankSlope;

    [Header("Rough Settings")]
    [Range(0.000f,0.05f)]
    public float roughAmount;
    [Range(0,5)]
    [Header("Smooth Settings")]
    public int smoothAmount;

    void normalize(float[] v)
    {
    	float total = 0;
    	for(int i=0; i<v.Length; i++)
    	{
    		total += v[i];
    	}
    	for (int i=0; i<v.Length; i++)
    	{
    		v[i] /= total;
    	}
    }
    
    public float map (float value, float sMin, float sMax, float mMin, float mMax)
    {
    	return (value-sMin) * (mMax-mMin) / (sMax-sMin) + mMin;
    }

    void Mountain(int x, int y, float height, float slope)
    {
    	if (x <= 0 || x >= terrainData.alphamapWidth) return;
    	if (y <= 0 || y >= terrainData.alphamapHeight) return;
    	if (height <= 0) return;
    	if (newHeightData[x,y] >= height) return;
    	newHeightData[x,y] = height;

    	Mountain(x-1, y, height-Random.Range(0.001f, slope), slope);
    	Mountain(x+1, y, height-Random.Range(0.001f, slope), slope);
    	Mountain(x, y-1, height-Random.Range(0.001f, slope), slope);
    	Mountain(x, y+1, height-Random.Range(0.001f, slope), slope);
    }

    void Hole(int x, int y, float height, float slope)
    {
    	if (x <= 0 || x >= terrainData.alphamapWidth) return;
    	if (y <= 0 || y >= terrainData.alphamapHeight) return;
    	if (height <= holeDepth) return;
    	if (newHeightData[x,y] <= height) return;
    	newHeightData[x,y] = height;

    	Hole(x-1, y, height+Random.Range(slope, slope+0.01f), slope);
    	Hole(x+1, y, height+Random.Range(slope, slope+0.01f), slope);
    	Hole(x, y-1, height+Random.Range(slope, slope+0.01f), slope);
    	Hole(x, y+1, height+Random.Range(slope, slope+0.01f), slope);
    }

    void RiverCrawler(int x, int y, float height, float slope)
    {
    	if (x <= 0 || x >= terrainData.alphamapWidth) return;
    	if (y <= 0 || y >= terrainData.alphamapHeight) return;
    	if (height <= maxDepth) return;
    	if (newHeightData[x,y] <= height) return;
    	newHeightData[x,y] = height;

    	RiverCrawler(x+1, y, height+Random.Range(slope, slope+0.01f), slope);
    	RiverCrawler(x-1, y, height+Random.Range(slope, slope+0.01f), slope);
    	RiverCrawler(x+1, y+1, height+Random.Range(slope, slope+0.01f), slope);
    	RiverCrawler(x-1, y+1, height+Random.Range(slope, slope+0.01f), slope);
    	RiverCrawler(x, y-1, height+Random.Range(slope, slope+0.01f), slope);
    	RiverCrawler(x, y+1, height+Random.Range(slope, slope+0.01f), slope);
    }

    void ApplyRiver()
    {
    	for (int i=0; i<numRivers; i++)
    	{
    		int cx = Random.Range(10, terrainData.alphamapWidth-10);
    		int cy = 0;
    		// int cy = Random.Range(10, terrainData.alphamapHeight-10);
    		int xdir = Random.Range(-1,2);
    		int ydir = Random.Range(-1,2);
    		while(cy >=0 && cy < terrainData.alphamapHeight && cx > 0 && cx < terrainData.alphamapWidth)
    		{
    			RiverCrawler(cx, cy, newHeightData[cx, cy] - digDepth, bankSlope);

    			if (Random.Range(0,50)<5)
    			    xdir = Random.Range(-1,2);
    			if (Random.Range(0,50)<5)
    			    ydir = Random.Range(0,2);

    			cx = cx + xdir;
    			cy = cy + ydir;
    		}
    	}
    }

    void ApplyHoles()
    {
    	for (int i=0; i<numHoles; i++)
    	{
            int xpos = Random.Range(10, terrainData.alphamapWidth-10);
            int ypos = Random.Range(10, terrainData.alphamapHeight-10);
            float newHeight = newHeightData[xpos,ypos] - holeChange;
            Hole(xpos, ypos, newHeight, holeSlope);
        }
    }


    void ApplyMountains()
    {
    	for (int i=0; i<numMountains; i++)
    	{
            int xpos = Random.Range(10, terrainData.alphamapWidth-10);
            int ypos = Random.Range(10, terrainData.alphamapHeight-10);
            float newHeight = newHeightData[xpos,ypos] + heightChange;
            Mountain(xpos, ypos, newHeight, sideSlope);
        }
    }

    void RoughTerrain()
    {
    	for (int y=0; y < terrainData.alphamapHeight; y++)
    	{
    		for (int x=0; x < terrainData.alphamapWidth; x++)
    		{
    			newHeightData[x,y] += Random.Range(0, roughAmount);
    		}
    	}
    }

    void SmoothTerrain()
    {
    	for (int y=1; y < terrainData.alphamapHeight-1; y++)
    	{
    		for (int x=1; x < terrainData.alphamapWidth-1; x++)
    		{
    			float avgheight = (newHeightData[x,y] +
                                   newHeightData[x+1,y] +
                                   newHeightData[x-1,y] +
                                   newHeightData[x+1,y+1] +
                                   newHeightData[x-1,y-1] +
                                   newHeightData[x+1,y-1] +
                                   newHeightData[x-1,y+1] +
                                   newHeightData[x,y+1] + 
                                   newHeightData[x,y-1])/9.0f; 

    			newHeightData[x,y] = avgheight;

    		}
    	}
    }

    void ApplyPerlin()
    {
    	for (int y=0; y<terrainData.alphamapHeight; y++)
    	{
    		for (int x=0; x<terrainData.alphamapWidth; x++)
    		{
    			newHeightData[x,y] = Mathf.PerlinNoise(x*bumpiness, y*bumpiness)*damp;
    		}
    	}
    }

    // Start is called before the first frame update
    public void Start()
    {
        terrainData = Terrain.activeTerrain.terrainData;
        float[, ,] splatmapData = new float [terrainData.alphamapWidth,
                                             terrainData.alphamapHeight,
                                             terrainData.alphamapLayers];

        newHeightData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight];

        ApplyPerlin();
        RoughTerrain();
        ApplyMountains();
        ApplyHoles();
        ApplyRiver();
        for (int i=0; i<smoothAmount; i++)
            SmoothTerrain();
        terrainData.SetHeights(0,0,newHeightData);

        for (int y=0; y<terrainData.alphamapHeight; y++)
        {
            for (int x=0; x<terrainData.alphamapWidth; x++)
            {
                float terrainHeight = terrainData.GetHeight(y,x);

                float [] splat = new float [splatHeights.Length];

                for (int i=0; i<splatHeights.Length; i++)
                {
                    float thisNoise = map(Mathf.PerlinNoise(x*0.03f,y*0.03f), 0, 1, 0.5f, 1);
                    float thisHeightStart = splatHeights[i].startingHeight * thisNoise 
                                             - splatHeights[i].overlap * thisNoise;

                    float nextHeightStart = 0;
                    if (i != splatHeights.Length-1)
                    {
                        nextHeightStart = splatHeights[i+1].startingHeight * thisNoise 
                                           + splatHeights[i+1].overlap * thisNoise;
                    }

                    if (i==splatHeights.Length-1 && terrainHeight >= thisHeightStart)
                        splat[i]=1;

                    else if (terrainHeight >= thisHeightStart && terrainHeight <= nextHeightStart)
                        splat[i]=1;
                }
                
                normalize(splat);
                for (int j=0; j<splatHeights.Length; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
