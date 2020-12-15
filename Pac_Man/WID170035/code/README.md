# Tree Search Ms. Pac-Man
Original repo by https://github.com/solar-1992/PacManEngine.<br/>

Run Main.java<br/>
![Alt text](screenshot.png?raw=true "tree search Ms. pacman")


In this repo, Ms. Pacman will decide the best path by considering the following criteria:

1. keep Ms. Pacman alive. if any Ghosts are present within a certain range and the ghosts are non edible, then Ms. Pacman will move away from target.
2. Ms. Pacman use Monte Carlo Tree Search to make a decision if it is in any junction points.
    - Power Pill active evaluator checks if the Ms. Pacman eats a power pill even if the current Power Pill is still active. It will penalize and deduct a certain score.
    - Rule based evaluator which adds bonus to a move if Ms. Pacman eats a pill or Ms. Pacman eats a pill on next move or any move that decreases the distance to the nearest pill.
    - Distance evaluator which increases the score of the move that brings Ms. Pacman closer to eating a pill.
3. If Ms.Pacman is not in a junction, then it will choose the move which maximize the points of pill locations.
