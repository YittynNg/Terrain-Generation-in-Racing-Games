package examples.StarterPacMan;

import pacman.game.Constants.*;

public class MyBestMove {
    public double reward;
    public MOVE move;
    MyBestMove(double reward, MOVE move) {
        this.reward = reward;
        this.move = move;
    }

    public double getReward() {
        return reward;
    }

    public MOVE getMove() {
        return move;
    }
}
