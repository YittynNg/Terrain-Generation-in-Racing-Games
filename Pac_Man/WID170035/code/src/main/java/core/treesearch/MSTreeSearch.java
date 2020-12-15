package core.treesearch;

import core.treesearch.evaluators.*;
import pacman.controllers.PacmanController;
import pacman.game.Constants;
import pacman.game.Constants.MOVE;
import pacman.game.Game;

import java.util.ArrayList;
import java.util.Random;

/*
 * For the Ms Pac-Man first move is made on the direction it was.
 * If any ghosts is seen in the straight line Ms Pac-Man will try to avoid the ghosts.
 * If the Ms Pac-Man is in the junction it will choose the best path usin MCTS (Monte Carlo Tree Search) algorithm.
 */

public class MSTreeSearch extends PacmanController {

    public MOVE getMove(Game game, long timeDue) {

        // We will only simulate the next moves in a junction,
        // otherwise we will walk straight along the hallway
        int myNodeIndex = game.getPacmanCurrentNodeIndex();

        // First priority is to keep Ms. Pac-man alive.
        // If there is any Ghost near by then must EVADE from Ghost, don't bother anything else
        MOVE move = checkNearByGhost(game);
        if(move != MOVE.NEUTRAL) {
            // System.out.println("Run Away from Ghosts!");
            return move;
        }

        // Next check if any edible Ghost is present in the path. Go for it !
//        int minDistance = Integer.MAX_VALUE;
//        Constants.GHOST minGhost = null;
//        for (Constants.GHOST ghost : Constants.GHOST.values()) {
//            // If it is > 0 then it is visible.
//            if (game.getGhostEdibleTime(ghost) > 0) {
//                int distance = game.getShortestPathDistance(myNodeIndex, game.getGhostCurrentNodeIndex(ghost));
//
//                if (distance < minDistance) {
//                    minDistance = distance;
//                    minGhost = ghost;
//                }
//            }
//        }

//        if (minGhost != null) {
//            // System.out.println("Hunting Ghost");
//            return game.getNextMoveTowardsTarget(
//                    myNodeIndex,
//                    game.getGhostCurrentNodeIndex(minGhost),
//                    Constants.DM.PATH);
//        }

        if (game.isJunction(myNodeIndex)) {
            // return best direction determined through MCTS
            return mcts(game);
        } else {
            return nonJunctionSim(game);
        }
    }

    /**
     * Checks if there are any nearby Ghosts
     * @param game
     * @return NEUTRAL if no Ghost nearby else EVADE
     */
    public static MOVE checkNearByGhost(Game game) {

        int[] junctions;
        int nearestJunctionPoint = -1;
        int nearestJunctionDistance = -1;
        int pacManNodeIndex = game.getPacmanCurrentNodeIndex();
        int DEFAULT_MINIMUM_DISTANCE = 10;
        int[] junctionPath = null;
        int ghostLocation = -1;
        int closestJunctionPoint = -1;
        boolean foundPath = false;


        for (Constants.GHOST ghost : Constants.GHOST.values()) {
            // If can't see, this will be -1 so all fine there
            if (game.getGhostEdibleTime(ghost) == 0 && game.getGhostLairTime(ghost) == 0) {
                ghostLocation = game.getGhostCurrentNodeIndex(ghost);
                // If Ghost is within a certain Range: 10
                if (game.getShortestPathDistance(pacManNodeIndex, ghostLocation) <= DEFAULT_MINIMUM_DISTANCE) {
                    // System.out.println("Evading Ghost");
                    // ghostArray.add(new Integer(ghostLocation));
                    return game.getNextMoveAwayFromTarget(pacManNodeIndex, ghostLocation, Constants.DM.PATH);
                }
            }
        }

        return MOVE.NEUTRAL;
    }

    /**
     * Non Junction point simulation
     * @param game
     */
    public static MOVE nonJunctionSim(Game game){

        // get the current position of PacMan (returns -1 in case you can't see PacMan)
        int myNodeIndex = game.getPacmanCurrentNodeIndex();

        // From ExtendedGame.Java
        // Rather than simply following the path, lets try to chose the best path first

        //set pills
        int[] powerPills = game.getPowerPillIndices();
        int[] pills = game.getPillIndices();
        ArrayList<Integer> allPills = new ArrayList<>();

        for (int i = 0; i < powerPills.length; i++) {
            Boolean checkPowerPill = game.isPowerPillStillAvailable(i);
            if (checkPowerPill != null) { // Avoids NPE
                // System.out.println("checkPowerPill: " + checkPowerPill);

                 if (checkPowerPill == true){
                    allPills.add(powerPills[i]);
                 }
            }
        }

        for (int i = 0; i < pills.length; i++) {
            Boolean checkPill = game.isPillStillAvailable(i);
            if (checkPill != null) { // Avoids NPE
                // System.out.println("checkPill: " + checkPill);

                if (checkPill == true){
                    allPills.add(pills[i]);
                }
            }
        }

        // If there are some pills or power pills and we have seen Pac-Man
        if (!allPills.isEmpty() && myNodeIndex != -1) {
            int[] targetPath = new int[allPills.size()];
            for(int i = 0; i < targetPath.length; i++) {
                targetPath[i] = allPills.get(i);
            }

            return game.getNextMoveTowardsTarget(
                    myNodeIndex,
                    game.getClosestNodeIndexFromNodeIndex(myNodeIndex, targetPath, Constants.DM.PATH),
                    Constants.DM.PATH);
        }

        MOVE[] moves = game.getPossibleMoves(myNodeIndex, game.getPacmanLastMoveMade());
        if (moves.length > 0) {
            Random random = new Random();
            return moves[random.nextInt(moves.length)];
        }
        // Must be possible to turn around
        return game.getPacmanLastMoveMade().opposite();
    }

    /**
     * Simulation through MCS
     * @param game
     */
    public static MOVE mcts(Game game) {

        // create MCTS Tree object for simulation
        MonteCarloTree tree = new MonteCarloTree(game);
        MOVE move = MOVE.NEUTRAL;

        for (int i = 0; i < 10; i++) { // Run simulations
            System.out.println(i);
            tree.simulate();
        }

        // Run some evaluators every time of the simulations
        ITreeEvaluator[] additionalEvaluators = new ITreeEvaluator[] {
                new DistanceToOpportunityEvaluator(),
                new PowerPillActiveEvaluator(),
                new RuleBasedEvaluator()};

        for (ITreeEvaluator evaluator: additionalEvaluators) {
            evaluator.evaluateTree(tree);
        }

        // Pick the move with the best score
        MonteCarloGameNode node = tree.bestNode();
        if (node != null) {
            move = node.getMove();
        } else {
            move = MOVE.NEUTRAL; // just do the earlier move. hopefully this will never execute.
            // System.out.println("move: " + move);
        }
        tree.setRootNode(node);

        return move;
    }
}