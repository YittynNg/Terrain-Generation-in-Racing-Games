package examples.StarterPacMan;

import pacman.game.Constants.*;
import pacman.game.Game;

public class MyGhost {
    GHOST id;
    int index;
    MyGhost(Game game, GHOST ghost) {
        this.index = game.getGhostCurrentNodeIndex(ghost);
        this.id = ghost;
    }
    MyGhost(int index, GHOST id) {
        this.index = index;
        this.id = id;
    }
}
