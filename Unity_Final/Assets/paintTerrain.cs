using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paintTerrain : MonoBehaviour
{
    [System.Serializable]      // makes splatheight expose at editor
    public class SplatHeights
    {
        public int textureIndex;   // store index of texture 
        public int startingHeight; // height of texture starts
        public int overlap;        // create seamless connection between textures
    }

    TerrainData terrainData;
    float[,] newHeightData;

    public SplatHeights[] splatHeights; // store all alpha values of textures (sand -> dirt -> grass -> rock -> snow)

    [Header("Perlin Noise Settings")]
    [Range(0.000f,0.01f)]
    public float bumpiness;             // state the bumpiness of terrain
    [Range(0.000f,1.000f)]
    public float damp;                  // restrict height of terrain layer

    [Header("Mountain Settings")]
    public int numMountains;
    [Range(0.001f,0.5f)]
    public float heightChange;          // height of mountain
    [Range(0.0001f,0.05f)]
    public float sideSlope;             // slope of mountain (smaller value, broader mountain slope)

    [Header("Hole Settings")]
    public int numHoles;
    [Range(0.0f,1.0f)]
    public float holeDepth;             // maximum depth to dig the hole
    [Range(0.001f,0.5f)]
    public float holeChange;            // first initial vertices that dig down, will then burrow up
    [Range(0.0001f,0.05f)]
    public float holeSlope;             // slope of hole

    [Header("River Settings")]
    public int numRivers;
    [Range(0.001f,0.05f)]
    public float digDepth;              // initial dig
    [Range(0.001f,1.0f)] 
    public float maxDepth;              // max depth of river bank
    [Range(0.0001f,0.05f)]
    public float bankSlope;             // slope of river bank

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
        // if height smaller than ground, return nothing
        if (height <= 0) return;
        if (newHeightData[x,y] >= height) return;
        newHeightData[x,y] = height;
        // recursive to create the slope randomly until the end of random position 
        Mountain(x-1, y, height-Random.Range(0.001f, slope), slope);
        Mountain(x+1, y, height-Random.Range(0.001f, slope), slope);
        Mountain(x, y-1, height-Random.Range(0.001f, slope), slope);
        Mountain(x, y+1, height-Random.Range(0.001f, slope), slope);
    }

    void Hole(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terrainData.alphamapWidth) return;
        if (y <= 0 || y >= terrainData.alphamapHeight) return;
        // if height smaller than hole depth, return nothing
        if (height <= holeDepth) return;
        if (newHeightData[x,y] <= height) return;
        newHeightData[x,y] = height;
        // recursive but opposition of mountain
        Hole(x-1, y, height+Random.Range(slope, slope+0.01f), slope);
        Hole(x+1, y, height+Random.Range(slope, slope+0.01f), slope);
        Hole(x, y-1, height+Random.Range(slope, slope+0.01f), slope);
        Hole(x, y+1, height+Random.Range(slope, slope+0.01f), slope);
    }

    void RiverCrawler(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terrainData.alphamapWidth) return;
        if (y <= 0 || y >= terrainData.alphamapHeight) return;
        // if height smaller than river depth, return nothing
        if (height <= maxDepth) return;
        if (newHeightData[x,y] <= height) return;
        newHeightData[x,y] = height;
        // recursive but opposition of mountain
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
            // change in the height but push things down
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
            // add a height change or plucking a single vertex 
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
        // loop thru each single vertices and apply perlin noise function to pull up vertices with smooth randomly format
        // generate value [0,1] as will multiply max height mentioned in editor
        for (int y=0; y<terrainData.alphamapHeight; y++)
        {
            for (int x=0; x<terrainData.alphamapWidth; x++)
            {
                // the bigger the bumpiness value, the smaller the noise, the bumpier the hill
                // damp to show its visibility  
                newHeightData[x,y] = Mathf.PerlinNoise(x*bumpiness, y*bumpiness)*damp;
            }
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        // retrieve terrain layers data
        terrainData = Terrain.activeTerrain.terrainData;
        // create splatmaps for respective terraain layers and stores them 
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

        // set heights after all function applies
        terrainData.SetHeights(0,0,newHeightData);

        // applying texture to different heights with splat maps
        for (int y=0; y<terrainData.alphamapHeight; y++)
        {
            for (int x=0; x<terrainData.alphamapWidth; x++)
            {

                float terrainHeight = terrainData.GetHeight(y,x);
                
                // five values in array represent how much alpha for each terrain layers
                float [] splat = new float [splatHeights.Length];

                for (int i=0; i<splatHeights.Length; i++)
                {
                    // perlin noise give a nice random sequence of numbers
                    // smaller value, more jaggerness; higher value smoother
                    // clamp value between 0.5 and 1, then remap it into new range
                    // ovelap will stirred up nicely with random smooth value
                    float thisNoise = map(Mathf.PerlinNoise(x*0.03f,y*0.03f), 0, 1, 0.5f, 1);
                    // add overlap to stir the connetion seamlessly
                    float thisHeightStart = splatHeights[i].startingHeight * thisNoise 
                                             - splatHeights[i].overlap * thisNoise;

                    float nextHeightStart = 0;
                    // add overlap to stir the connetion seamlessly
                    if (i != splatHeights.Length-1)
                    {
                        nextHeightStart = splatHeights[i+1].startingHeight * thisNoise 
                                           + splatHeights[i+1].overlap * thisNoise;
                    }
                    
                    // special case
                    if (i==splatHeights.Length-1 && terrainHeight >= thisHeightStart)
                        splat[i]=1;

                    // if terrain height is greater or equal than starting height, set full value for opacity 
                    // start at starting height and stop at next height
                    else if (terrainHeight >= thisHeightStart && terrainHeight <= nextHeightStart)
                        splat[i]=1;
                }
                
                // normalize so that color not that bright
                normalize(splat);

                // set values into splatmap data
                for (int j=0; j<splatHeights.Length; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        // updated the values
        terrainData.SetAlphamaps(0, 0, splatmapData);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
