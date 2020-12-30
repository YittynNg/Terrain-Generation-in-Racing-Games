using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{

	public float generationStepDelay;
	private int cellNumber = 0;

	public MazePassage passagePrefab;
	public MazeWall wallPrefab;

	public MazeCell cellPrefab;
	private MazeCell[,] cells;

	public IntVector2 size;

	public IntVector2 RandomCoordinates {
		get {
			return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
		}
	}

	public bool ContainsCoordinates (IntVector2 coordinate) {
		return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	}


	public MazeCell GetCell (IntVector2 coordinates) {
		return cells[coordinates.x, coordinates.z];
	}

    // create cell by instantiate cellPrefab
	private MazeCell CreateCell (IntVector2 coordinates, string cellType) {
		MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
		cells[coordinates.x, coordinates.z] = newCell;
		// fill new cell into specific coordinates within the cell matrix
		newCell.coordinates = coordinates;
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
		newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
        
        cellNumber += 1;

        // Entry cell set color as red
		if (cellType == "Entry"){
			newCell.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color",Color.red);
		}; 
        // Exit cell set color as green
		if (cellNumber == size.x * size.z){
		    newCell.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color",Color.green);
		}

	    return newCell;
	}

    // create passage for "current cell to next cell" and "next cell to current cell" by instantiate passagePrefab
	private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);                   // setEdge at direction, initializedEdgeCount + 1
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());     // setEdge at direction, initializedEdgeCount + 1
	}

    // create wall for "current cell towards next cell" by instantiate wallPrefab
	private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
		MazeWall wall = Instantiate(wallPrefab) as MazeWall;
		wall.Initialize(cell, otherCell, direction);                     // setEdge at direction, initializedEdgeCount + 1
		// if next cell is occupied, create wall for "next cell towards current cell" 
		if (otherCell != null) {
			wall = Instantiate(wallPrefab) as MazeWall;
			wall.Initialize(otherCell, cell, direction.GetOpposite());   // setEdge at direction, initializedEdgeCount + 1
		}
		// if next cell is not occupied or out of maze, no need create opposite wall
	}

	private void DoFirstGenerationStep (List<MazeCell> activeCells) {
		// start from random cell
		// activeCells.Add(CreateCell(RandomCoordinates));
		activeCells.Add(CreateCell(RandomCoordinates, "Entry"));
	}

	private void DoNextGenerationStep (List<MazeCell> activeCells) {
		int currentIndex = activeCells.Count - 1;     	          // start from last active Cells and explore (recursive backtracking algorithm)
		// int currentIndex = Random.Range(0, activeCells.Count);    // start from random active Cells and explore (prim's algorithm)
		// int currentIndex = (int) activeCells.Count / 2;           // start from middle of active Cells and explore 
		// int currentIndex = 0;    	                             // start from first active Cells and explore 

		MazeCell currentCell = activeCells[currentIndex];
        

		// if four direction fully initialized, remove current active cell and go for another active cell 
		if (currentCell.IsFullyInitialized) {
			activeCells.RemoveAt(currentIndex);
			return;
		}

		// if not fully initialized, pick a random direction that is not yet initialized (no passage created or no wall created)
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();

        // if next coordinates is within maze size range
		if (ContainsCoordinates(coordinates)) {
			MazeCell neighbor = GetCell(coordinates);
			// if next coordinates is not occupied, create cell and passage between current cell and next cell (based on next cell direction)
			if (neighbor == null) {
				neighbor = CreateCell(coordinates, "Middle");
				CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			// if next coordinates is occupied, create wall between current cell and next cell (based on next cell direction)
			else {
				CreateWall(currentCell, neighbor, direction);
			}
		} 

        // if next coordintaes out of maze size, build wall (based on next cell direction)
		else {
			CreateWall(currentCell, null, direction);	
		}
	}

	public IEnumerator Generate () {
		System.DateTime startTime = System.DateTime.Now;
		WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

		// request for maze matrix size
		cells = new MazeCell[size.x, size.z];
		// to keep track the list of active cells
		List<MazeCell> activeCells = new List<MazeCell>();
		

		DoFirstGenerationStep(activeCells);
		while (activeCells.Count > 0) {
			yield return delay;
			DoNextGenerationStep(activeCells);
		}

		System.DateTime endTime = System.DateTime.Now;
		string TotalTime = endTime.Subtract(startTime).ToString();
		print("Total time taken for algorithm: " + TotalTime);
	}
    

}
