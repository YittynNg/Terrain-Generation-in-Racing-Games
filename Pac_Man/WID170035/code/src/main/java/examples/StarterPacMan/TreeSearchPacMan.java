package examples.StarterPacMan;

import pacman.Executor;
import pacman.controllers.PacmanController;
import pacman.game.Constants;
import pacman.game.Constants.DM;
import pacman.game.Constants.GHOST;
import pacman.game.Constants.MOVE;
import pacman.game.Game;
import pacman.game.GameView;

import java.awt.Color;
import java.lang.reflect.Array;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Random;

/*
 * This is the class you need to modify for your entry. In particular, you need to
 * fill in the getMove() method. Any additional classes you write should either
 * be placed in this package or sub-packages (e.g., entrants.pacman.username).
 */
public class TreeSearchPacMan extends PacmanController {
	 private static final Random RANDOM = new Random(); 
	 private Game game;
	 private int pacmanCurrentNodeIndex;
	 MOVE pacmanLastMoveMade; 
	 int pathLengthBase = 94; // 70, 70 // Make it longer when no pills around
	 int minGhostDistanceBase = 100; // 80, 100
	 private List<Path> paths = new ArrayList<>();
	 
	private int getRandomInt(int min, int max){
		if (min >= max) {
			throw new IllegalArgumentException("max must be greater than min");
		}

		Random r = new Random();
		return r.nextInt((max - min) + 1) + min;		
	}
	 
    @Override
    public MOVE getMove(Game game, long timeDue) {
		this.game = game;
    	pacmanCurrentNodeIndex = game.getPacmanCurrentNodeIndex();
    	pacmanLastMoveMade = game.getPacmanLastMoveMade();
    		   	
    	// Random path length and minGhostDistance
    	int pathLength = pathLengthBase /*+ getRandomInt(-50, 10)*/;
    	
    	// Get possible paths
    	paths = getPaths(pathLength);
    	
    	// Sort the path with highest value DESC
    	Collections.sort(paths, new PathValueComparator());

    	for (Path path: paths)
    	{
        	path.summary(game); 
    	}
    	
    	Path bestPath = paths.get(0);
    	MOVE bestPathMove = game.getMoveToMakeToReachDirectNeighbour(pacmanCurrentNodeIndex, bestPath.start);
    	
    	// No pills around while at junction but has safe paths, choose random safe path
		if (bestPath.value == 0 && game.isJunction(pacmanCurrentNodeIndex))
		{
			// Get only safe paths from paths
    		List<MOVE> safeMoves = new ArrayList<>();
    		for (Path path: paths)
    		{
    			if(path.safe)
    			{
    				MOVE safeMove = game.getMoveToMakeToReachDirectNeighbour(pacmanCurrentNodeIndex, path.start);	
    				safeMoves.add(safeMove);
    			}
    		}
    		
    		// Random safe path
    		while(true)
    		{    			
    			MOVE randomMove = getRandomMove();
    			if (safeMoves.contains(randomMove))
    			{
        			bestPathMove = randomMove;
        			break;
    			}   				
    		}
		}
		
		// No safe paths
		else if (bestPath.value < 0)
		{
			bestPathMove = pacmanLastMoveMade;
		}

		// if the current best move is no better than previous move, then we maintain previous move, this is to avoid pacman flickering movement 
    	else if (bestPathMove != pacmanLastMoveMade) 
		{
    		for (Path path: paths)
    		{
    			MOVE move = game.getMoveToMakeToReachDirectNeighbour(pacmanCurrentNodeIndex, path.start);

    			if (move == pacmanLastMoveMade && path.value == bestPath.value)
    			{
    				bestPathMove = move;
    				break;
    			}
    		}
		}
    	
    	return bestPathMove;
    }
    
    private MOVE getRandomMove()
    {
    	MOVE[] possibleMoves = game.getPossibleMoves(pacmanCurrentNodeIndex, pacmanLastMoveMade);    	
    	
    	return possibleMoves[RANDOM.nextInt(possibleMoves.length)];
    }
    
    public class PathValueComparator implements Comparator<Path>
    {    	
		@Override
		public int compare(Path path1, Path path2)
		{						
			return path2.value - path1.value;
		}
    }
    
    public class Path {
    	public int start;    	
    	public int end;
    	public List<GHOST> ghosts = new ArrayList<GHOST>();
    	public int powerPillsCount = 0;  	
    	public int pillsCount = 0;
    	public List<Segment> segments = new ArrayList<Segment>();
    	public int length;
    	public String description = "";
    	public boolean safe = true;
    	public int value = 0;
    	    
    	// Important: Segments must be in sequence
    	Path(List<Segment> segments)
    	{
    		this.segments = segments;	  		
    	}
    	
    	public void render(Game game)
    	{
    		for (Segment segment : segments)
    			GameView.addLines(game, segment.color, segment.start, segment.end);
    	}
    	
    	public void summary(Game game)
    	{
    		String ghostsName = "";
    		for (GHOST ghost : ghosts)
    			ghostsName += ghost.name() + " ";
    			
    		String text = description + "::" + " value:" + value + ", safe:" + (safe ? "safe":"unsafe") + ", pills:" + pillsCount + ", power pills:" + powerPillsCount + ", ghost:" + ghostsName;
    		
    		if (!safe)
    			System.err.println(text);
    		else
    			System.out.println(text);

    		render(game);
    	}
    
    	public void process()
    	{
    		// TODO: calculate path value
    		
    		int segmentsCount = segments.size();
    		
    		if(segmentsCount > 0)
    		{
    			Segment firstSegment = segments.get(0);
    			Segment lastSegment = segments.get(segmentsCount - 1);
    			start = firstSegment.start;
    			end = lastSegment.end;
        		length = lastSegment.lengthSoFar;
        		pillsCount = lastSegment.pillsCount;
        		value = pillsCount;
        		powerPillsCount = lastSegment.powerPillsCount;
        		int unsafeSegmentsCount = 0;      		                		
        		
        		for (Segment segment : segments)
        		{              			
        			if (!segment.ghosts.isEmpty())
        			{
        				ghosts.addAll(segment.ghosts);
        				for (GHOST ghost: ghosts)
            				if (game.isGhostEdible(ghost))
            				{
            					int distance = game.getShortestPathDistance(pacmanCurrentNodeIndex, game.getGhostCurrentNodeIndex(ghost));
            					if (distance < 10)
                					value += 1;//15;
            					else
            						value += 1;//10;
            				}
        			}
        			
        			if (segment.parent != null && !segment.parent.safe)
        				segment.safe = segment.parent.safe;
        				
        			if (!segment.safe) 
        			{
        				unsafeSegmentsCount++;
        				value -= 10;
        				segment.color = Color.RED;
        			}
        			       			
    				value += segment.powerPillsCount * 5;          				 				
        			
        			description += segment.direction.toString() + " ";
        		}
        		
        		if (unsafeSegmentsCount > 0)
        			safe = false;	
    		}
    	}
    }  
    
    public class Segment {
    	public int start;
    	public int end;
    	public int pillsCount = 0;
    	public int powerPillsCount = 0;
    	public int lengthSoFar;
    	public MOVE direction;
    	public Segment parent;
    	public List<GHOST> ghosts = new ArrayList<>();
    	public Color color = Color.GREEN;
    	public boolean safe = true;
    }
    
    public List<Path> getPaths(int maxPathLength)
    {
    	MOVE[] startingPossibleMoves = game.getPossibleMoves(pacmanCurrentNodeIndex);
    	List<Path> paths = new ArrayList<>();
    	int minGhostDistance = minGhostDistanceBase /*+ getRandomInt(10, 30)*/;

    	// Start searching from the possible moves at the current pacman location
    	for (MOVE startingPossibleMove : startingPossibleMoves)
		{   
    		List<Segment> pendingSegments = new ArrayList<Segment>();
    		
			// Step into next node
    		int currentNode = game.getNeighbour(pacmanCurrentNodeIndex, startingPossibleMove);

    		// Create new segment starting from the node next to pacman
    		Segment currentSegment = new Segment();
    		currentSegment.start = currentNode;
    		currentSegment.parent = null;
    		currentSegment.direction = startingPossibleMove;
    		currentSegment.lengthSoFar++;
    		
    		// Get all ghosts node index in a list
    		List<Integer> ghostNodeIndices = new ArrayList<>();
    		GHOST[] ghosts= GHOST.values();
    		for (GHOST ghost: ghosts)
    			ghostNodeIndices.add(game.getGhostCurrentNodeIndex(ghost));
    		    		
    		// Loop each step
    		do
    		{    			
        		// Check pills and power pills
				int pillIndex = game.getPillIndex(currentNode);	
        		int powerPillIndex = game.getPowerPillIndex(currentNode);
        		
        		try 
        		{
            		if (pillIndex != -1 && game.isPillStillAvailable(pillIndex))
            		{
            			//GameView.addPoints(game, Color.blue, currentNode);
        				currentSegment.pillsCount++;  	
            		}
            		else if (powerPillIndex != -1 && game.isPowerPillStillAvailable(powerPillIndex))
            			currentSegment.powerPillsCount++;
        		}
        		catch (Exception e)
        		{
        			System.out.println("currentNode:" + currentNode + ", pillIndex:" + pillIndex + ", powerPillIndex:" + powerPillIndex + ", please increase executor radius size by setting Executor.Builder().setSightLimit(1000)");
        			throw e; 
        		}
				
        		// Segment contains ghost(s), not safe if ghost direction is opposite of segment direction and is not edible
        		if (ghostNodeIndices.contains(currentNode))
            		for (GHOST ghost: ghosts)
            		{            			
            			if(game.getGhostCurrentNodeIndex(ghost) == currentNode)
            			{
            				currentSegment.ghosts.add(ghost);
            				
            				if (!game.isGhostEdible(ghost) 
            						&& game.getGhostLastMoveMade(ghost) == currentSegment.direction.opposite()
            						&& game.getEuclideanDistance(pacmanCurrentNodeIndex, currentNode) <= minGhostDistance )
            				{
            					currentSegment.safe = false;
            					if (currentSegment.parent != null)
            						currentSegment.parent.safe = false;
            				}
            			}
            		}
		
        		// Check if length is max
        		if (currentSegment.lengthSoFar >= maxPathLength)
        		{
        			currentSegment.end = currentNode;    			
        			//System.out.println("Current segment " + "start:" + currentSegment.start + ", end:" + currentSegment.end + ", direction:" + currentSegment.direction + ", ended");

        			// Create a new path and insert segments that make up the path
        			List<Segment> pathSegments = new ArrayList<>();
        			do
        			{
        				pathSegments.add(currentSegment);
        				currentSegment = currentSegment.parent;
        			}while(currentSegment != null);
        			
        			Collections.reverse(pathSegments);      			
        			Path path = new Path(pathSegments);
        			paths.add(path);
        			
        			// Pop out the latest pending segment and set it as current segment
        			if (!pendingSegments.isEmpty()) 
        			{
        				currentSegment = pendingSegments.remove(pendingSegments.size()-1);
        				currentNode = currentSegment.start;
        				currentSegment.lengthSoFar++;
        				//System.out.println("Current segment " + "start:" + currentSegment.start + ", direction:" + currentSegment.direction + ", length so far:" + currentSegment.lengthSoFar + " begins");
        				continue;
        			}
        			else
        				break;				
        		}
    			     
    			MOVE[] possibleMoves = game.getPossibleMoves(currentNode, currentSegment.direction);

        		// If neighbor is a junction or a corner, end the current segment and create a new segment
        		if (possibleMoves.length > 1 || (possibleMoves.length == 1 && possibleMoves[0] != currentSegment.direction))
        		{
        			currentSegment.end = currentNode;
        			Segment parentSegment = currentSegment;
        			
        			for (int i = 0; i < possibleMoves.length; i++)
        			{
        				MOVE possibleMove = possibleMoves[i];  				
        				int neighborNode = game.getNeighbour(currentNode, possibleMove);
        				
        				// Create new segment for each neighbor node
        				Segment segment = new Segment();
        				segment.start = neighborNode;
        				segment.direction = possibleMove;
        				segment.parent = parentSegment;
        				segment.pillsCount = parentSegment.pillsCount;
        				segment.powerPillsCount = parentSegment.powerPillsCount;
        				segment.lengthSoFar = currentSegment.lengthSoFar;
        				segment.safe = parentSegment.safe;
        				        				
        				if (i == 0)
        					currentSegment = segment;
        				else
        					pendingSegments.add(segment);  				
        			}
        		}
        		        		
        		// Step into next node
    			currentNode = game.getNeighbour(currentNode, currentSegment.direction);
    			currentSegment.lengthSoFar++; 

    		}while(!pendingSegments.isEmpty() || currentSegment.lengthSoFar <= maxPathLength);
		}
    	
    	// Required to calculate the required data in each path
    	for (Path path : paths)
			path.process();
    	
    	System.out.println("\nPath search complete found " + paths.size() + " path");
    	return paths;
    }
}