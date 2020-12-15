package examples.StarterPacMan;

import pacman.controllers.PacmanController;
import pacman.game.Constants.*;
import pacman.game.Game;
import pacman.game.internal.PacMan;

import java.util.*;


public class MyOnlyPacMan extends PacmanController {
    @Override
    public MOVE getMove(Game game, long l) {

        int pacmanCurrentNodeIndex = game.getPacmanCurrentNodeIndex();

        ArrayList<MyGhost> ghostsList = new ArrayList<>();
        ghostsList.add(new MyGhost(game, GHOST.BLINKY));
        ghostsList.add(new MyGhost(game, GHOST.SUE));
        ghostsList.add(new MyGhost(game, GHOST.INKY));
        ghostsList.add(new MyGhost(game, GHOST.PINKY));
        HashSet<Integer> visited = new HashSet<>();
        visited.add(pacmanCurrentNodeIndex);

        HashSet<Integer> pillsList = new HashSet<>();

        for (int index: game.getActivePillsIndices()) {
            pillsList.add(index);
        }

        MyBestMove pacmanBestMove = getPacmanBestMove(
                game,
                MOVE.LEFT,
                visited,
                ghostsList,
                pillsList,
                pacmanCurrentNodeIndex,
                120,
                0

        );
        return pacmanBestMove.move;
    }

    MyBestMove getPacmanBestMove(
            Game game,
            MOVE selectedMove,
            HashSet<Integer> visited,
            ArrayList<MyGhost> ghosts,
            HashSet<Integer> pillsList,
            int pacmanCurrentNodeIndex,
            int depth,
            double reward

    ) {
        if (depth == 0 || reward < 0) {
            return new MyBestMove(reward + 0.01 / visited.size(), selectedMove);
        }
        MOVE[] pacmanMoves = game.getPossibleMoves(pacmanCurrentNodeIndex);

        MOVE bestMove = MOVE.NEUTRAL;
        double highestReward = Integer.MIN_VALUE;
        for (MOVE move: pacmanMoves) {
            int newPacmanNodeIndex = game.getNeighbour(pacmanCurrentNodeIndex, move);

            if (visited.contains(newPacmanNodeIndex) || move == MOVE.NEUTRAL) {
                continue;
            } else {
                visited.add(newPacmanNodeIndex);
            }
            int currentReward = 0;

            boolean containPill = false;
            if (pillsList.contains(newPacmanNodeIndex)) {
                currentReward ++;
                containPill = true;
                pillsList.remove(newPacmanNodeIndex);
            }

            ArrayList<MyGhost> copyGhosts = new ArrayList<>();
            ghostsMoves:for (MyGhost ghost: ghosts) {
                if (ghost.index != -1) {
                    MyBestMove ghostBestMoveIndex = getGhostBestMoveIndex(game, ghost, newPacmanNodeIndex);
                    int ghostIndex = game.getNeighbour(ghost.index, ghostBestMoveIndex.move);

                    copyGhosts.add(new MyGhost(ghostIndex, ghost.id));

                    if (ghostBestMoveIndex.reward == Integer.MAX_VALUE) {
                        currentReward += Integer.MIN_VALUE;
                        break ghostsMoves;
                    }
                }
            }

            MyBestMove bestMove1 = getPacmanBestMove(
                    game,
                    move,
                    visited,
                    copyGhosts,
                    pillsList,
                    newPacmanNodeIndex,
                    depth - 1,
                    reward + currentReward
            );
            if (bestMove1.reward > highestReward) {
                highestReward = bestMove1.reward;
                bestMove = move;
            }

            if (containPill) {
                pillsList.add(newPacmanNodeIndex);
            }
            visited.remove(newPacmanNodeIndex);
        }

        return new MyBestMove(highestReward, bestMove);
    }

    MyBestMove getGhostBestMoveIndex(Game game, MyGhost ghost, int pacmanIndex) {
        MOVE[] moves = game.getPossibleMoves(ghost.index);
        int shortestDistance = Integer.MAX_VALUE;
        MOVE bestMove = MOVE.NEUTRAL;
        for (MOVE move: moves) {

            int ghostNewIndex = game.getNeighbour(ghost.index, move);
            int distance = game.getShortestPathDistance(ghostNewIndex, pacmanIndex);
            if (distance < shortestDistance) {
                shortestDistance = distance;
                bestMove = move;
            }
        }
        double reward = -shortestDistance;
        if (shortestDistance < 5) {
            reward = Integer.MAX_VALUE;
        }
        return new MyBestMove(reward, bestMove);
    }
}
