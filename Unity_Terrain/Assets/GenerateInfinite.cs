using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generate all tiles and keep track of them

class Tile{
	
	public GameObject theTile; // each of the smartplane will be places in tile
	public float creationTime; // record creation time of tile

	public Tile(GameObject t, float ct){
		// constructor 
		theTile = t;
		creationTime = ct;
	}
}


public class GenerateInfinite : MonoBehaviour
{
	public GameObject plane;
	public GameObject player; // know player location to continually generate more terrain

	int planeSize = 10; // record planesize to know where to put next tile that can match up nicely
	int halfTilesX = 5; // radius X of how many tiles around player
	int halfTilesZ = 5; // radius Z of how many tiles around player

	Vector3 startPos; // keep track where player is and where the player was

    // dont continually constatly create terrain for every update

	Hashtable tiles = new Hashtable(); // hold game object and index them based on name which generate according to its x and z position

    // Start is called before the first frame update
    void Start()
    {
    	this.gameObject.transform.position = Vector3.zero;
    	startPos = Vector3.zero;

    	float updateTime = Time.realtimeSinceStartup;

        // create a 2D ground made up by connecting plane
    	for (int x=-halfTilesX; x<halfTilesX; x++)
    	{
    		for (int z=-halfTilesZ; z<halfTilesZ; z++)
    		{
    			Vector3 pos = new Vector3((x*planeSize+startPos.x), 0, (z*planeSize+startPos.z));  // get tiles positionn
    			
    			// create plane
    			GameObject t = (GameObject) Instantiate(plane, pos, Quaternion.identity);
    			// give name
    			string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();
    			// update name
    			t.name = tilename;
    			// create tile and stamp with update time (in this loop will have same time)
    			Tile tile = new Tile(t, updateTime);
    			// add tile to hashtable
    			tiles.Add(tilename, tile);
    		}
    	}
        
    }

    // Update is called once per frame
    void Update()
    {
        // keep generate terrain infront of player so never walk until edges
        // check where the player is, if move certain distance then gonna update the terrain around it
        
        // determine how far the player has moved since last terrain update (current position - previous position)
        int xMove = (int)(player.transform.position.x - startPos.x);
        int zMove = (int)(player.transform.position.z - startPos.z);

        // if move more than 1 plane size, need to update tiles  
        if (Mathf.Abs(xMove) >= planeSize || Mathf.Abs(zMove) >= planeSize)
        {
        	// update time to (i) remove tiles with old timestamp that are out of range or (ii) update tiles that are in the range with new timestamp 
        	float updateTime = Time.realtimeSinceStartup; 

            // get nice and clean player position - force interger position and round to nearest tilesize
            int playerX = (int)(Mathf.Floor(player.transform.position.x/planeSize)*planeSize);
            int playerZ = (int)(Mathf.Floor(player.transform.position.z/planeSize)*planeSize);

            for (int x=-halfTilesX; x<halfTilesX; x++)
            {
            	for (int z=-halfTilesZ; z<halfTilesZ; z++)
            	{
            		// get offset around the player
            		Vector3 pos = new Vector3((x*planeSize+playerX), 0 , (z*planeSize+playerZ));
            		// setting name for tile with position
            		string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();

                    // create new tiles only if the tiles not exist before @ not in hashtable
            		if (!tiles.ContainsKey(tilename))
            		{
            			GameObject t =(GameObject)Instantiate(plane, pos, Quaternion.identity);
            			t.name = tilename;
            			Tile tile = new Tile(t, updateTime);
            			// add newly create tiled to hashtable
            			tiles.Add(tilename, tile);
            		}
            		// else update the old tiles which still in the range with new timestamp
            		else
            		{
            			(tiles[tilename] as Tile).creationTime = updateTime;
            		}
            	}
            }

            // create a new Hashtable that put all new tiles in
            Hashtable newTerrain = new Hashtable();
            // loop around old hashtable
            foreach (Tile tls in tiles.Values)
            {
            	// if creation time is not equal update time == not in the range of player current position, destroy the tiles
            	if (tls.creationTime != updateTime)
            	{
            		// delete gameobject
            		Destroy(tls.theTile);
            	}
            	else
            	{
            		// if the tiles is within range of player current position
                    newTerrain.Add(tls.theTile.name, tls);	
            	}
            }

            // copy whole new Hashtable contents to override working global hashtable @ destroying old hashtable because
            // delete element in old hashtable will caused leaving empty spaces and eventually become inefficient
            tiles = newTerrain;
            // update player current position
            startPos = player.transform.position;
            

        }
    }
}
