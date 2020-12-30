# Maze Generation

Original [code](https://catlikecoding.com/unity/tutorials/maze/) by Catlike Coding
 



## Comparison between Recursive Backtracking Algorithm (RBA) and Prim's Algorithm (PA) with 10 x 10 maze (in secs)
| Algorithm|  RBA  |  PA   |
|----------|-------|-------|
|Trial 1   | 5.521 | 6.362 |
|Trial 2   | 6.546 | 7.739 |
|Trial 3   | 6.835 | 6.837 |
|Average   | 6.301 | 6.977 |

## Comparison between Recursive Backtracking Algorithm (RBA) and Prim's Algorithm (PA) with 20 x 20 maze (in secs)
| Algorithm|  RBA  |  PA   |
|----------|-------|-------|
|Trial 1   | 32.89 | 25.46 |
|Trial 2   | 34.15 | 28.36 |
|Trial 3   | 28.19 | 25.85 |
|Average   | 31.74 | 26.56 |

## Discussion
RBA and PA has no much difference when running in 10 x 10 maze, but there's big difference when running in 20 x 20 maze. This is because RBA need to do backtracking recursively which require longer time moving forth and back. However, RBA is more aesthetically pleasing and produce lesser dead end compare to PA.
