"""This file contains all the classes you must complete for this project.

You can use the test cases in agent_test.py to help during development, and
augment the test suite with your own test cases to further test your code.

You must test your agent's strength against a set of agents with known
relative strength using tournament.py and include the results in your report.
"""
import random


class Timeout(Exception):
    """Subclass base exception for code clarity."""
    pass


def custom_score(game, player):
    """Calculate the heuristic value of a game state from the point of view
    of the given player.

    Note: this function should be called from within a Player instance as
    `self.score()` -- you should not need to call this function directly.

    Parameters
    ----------
    game : `isolation.Board`
        An instance of `isolation.Board` encoding the current state of the
        game (e.g., player locations and blocked cells).

    player : object
        A player instance in the current game (i.e., an object corresponding to
        one of the player objects `game.__player_1__` or `game.__player_2__`.)

    Returns
    -------
    float
        The heuristic value of the current game state to the specified player.
    """

    # TODO: finish this function!
    return weighted_chances_heuristic(game, player)


def aggressive_heuristic(game, player):
    """
    Minimizing opponent's moves
    ---------------------------
    Explanation: This heuristic is based on the logic that opponent's moves should be minimized.
    Comment: By penalizing the opponent moves, attempting to force or weaken opponent
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))

    return my_moves - 1.5 * opponent_moves


def defensive_heuristic(game, player):
    """
    Maximizing player's moves
    ---------------------------
    Explanation: This heuristic is based on the logic that player's moves should be maximized.
    Comment: By reward the player moves,keep the players move largst cost
   
    """  
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))

    return 1.5 * my_moves - opponent_moves


def maximizing_win_chances_heuristic(game, player):
    """
    Maximizing ratio of player to opponent's moves
    ---------------------------
    Explanation: This heuristic is based on the logic that player should have more moves in comparison to opponent.
    Comment: player moves larger than opponent moves will return value > 0 
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = 1.0 * len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))

    if my_moves == 0:
        return float("-inf")

    if opponent_moves == 0:
        return float("inf")

    return my_moves/opponent_moves


def minimizing_losing_chances_heuristic(game, player):
    """
    Minimizing ratio of opponent to player's moves
    ---------------------------
    Explanation: This heuristic is based on the logic that opponent should have less moves in comparison to player.
    Comment: opponent moves smaller than player moves will return more negative value 
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = 1.0 * len(game.get_legal_moves(game.get_opponent(player)))

    if my_moves == 0:
        return float("-inf")

    if opponent_moves == 0:
        return float("inf")

    return -opponent_moves/my_moves


def chances_heuristic(game, player):
    """
    Combining heuristic 3 and 4
    ---------------------------
    Comment: Maximize the winning chance and minimize the losing chance
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))

    return my_moves*my_moves - opponent_moves*opponent_moves


def weighted_chances_heuristic(game, player):
    """
    Weighted combination of heuristic 3 and 4
    ---------------------------
    Comment: Maximize the winning chance and minimize the losing chance, by penalizing opponent move
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))

    return my_moves*my_moves - 1.5*opponent_moves*opponent_moves


def weighted_chances_heuristic_2(game, player):
    """
    Weighted combination of heuristic 3 and 4
    ---------------------------
    Comment: Maximize the winning chance and minimize the losing chance, by reward player move
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))

    return 1.5*my_moves*my_moves - opponent_moves*opponent_moves


def defensive_to_offensive(game, player):
    """
    Early-game: defensive strategy, player tries to initially maximize number of available moves
    Mid-game: offensive strategy, player tries to limit and block opponent
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))
    offensive = my_moves - 2 * opponent_moves
    defensive = 2 * my_moves - opponent_moves

    ratio = game.move_count / (game.width * game.height)
    
    if ratio <= 0.5:
        return defensive
    else:
        return offensive


def offensive_to_defensive(game, player):
    """
    Early-game: offensive strategy, player tries to limit and block opponent
    Mid-game: defensive strategy, player tries to initially maximize number of available moves
   
    """
    if game.is_loser(player):
        return float("-inf")

    if game.is_winner(player):
        return float("inf")

    my_moves = len(game.get_legal_moves(player))
    opponent_moves = len(game.get_legal_moves(game.get_opponent(player)))
    offensive = my_moves - 2 * opponent_moves
    defensive = 2 * my_moves - opponent_moves
    ratio = game.move_count / (game.width * game.height)
    
    if ratio <= 0.5:
        return offensive
    else:
        return defensive


class CustomPlayer:
    """Game-playing agent that chooses a move using your evaluation function
    and a depth-limited minimax algorithm with alpha-beta pruning. You must
    finish and test this player to make sure it properly uses minimax and
    alpha-beta to return a good move before the search time limit expires.

    Parameters
    ----------
    search_depth : int (optional)
        A strictly positive integer (i.e., 1, 2, 3,...) for the number of
        layers in the game tree to explore for fixed-depth search. (i.e., a
        depth of one (1) would only explore the immediate sucessors of the
        current state.)

    score_fn : callable (optional)
        A function to use for heuristic evaluation of game states.

    iterative : boolean (optional)
        Flag indicating whether to perform fixed-depth search (False) or
        iterative deepening search (True).

    method : {'minimax', 'alphabeta'} (optional)
        The name of the search method to use in get_move().

    timeout : float (optional)
        Time remaining (in milliseconds) when search is aborted. Should be a
        positive value large enough to allow the function to return before the
        timer expires.
    """

    def __init__(self, search_depth=3, score_fn=custom_score,
                 iterative=True, method='minimax', timeout=10.):
        self.search_depth = search_depth
        self.iterative = iterative
        self.score = score_fn
        self.method = method
        self.time_left = None
        self.TIMER_THRESHOLD = timeout - 0.5

    def get_move(self, game, legal_moves, time_left):
        """Search for the best move from the available legal moves and return a
        result before the time limit expires.

        This function must perform iterative deepening if self.iterative=True,
        and it must use the search method (minimax or alphabeta) corresponding
        to the self.method value.

        **********************************************************************
        NOTE: If time_left < 0 when this function returns, the agent will
              forfeit the game due to timeout. You must return _before_ the
              timer reaches 0.
        **********************************************************************

        Parameters
        ----------
        game : `isolation.Board`
            An instance of `isolation.Board` encoding the current state of the
            game (e.g., player locations and blocked cells).

        legal_moves : list<(int, int)>
            A list containing legal moves. Moves are encoded as tuples of pairs
            of ints defining the next (row, col) for the agent to occupy.

        time_left : callable
            A function that returns the number of milliseconds left in the
            current turn. Returning with any less than 0 ms remaining forfeits
            the game.

        Returns
        -------
        (int, int)
            Board coordinates corresponding to a legal move; may return
            (-1, -1) if there are no available legal moves.
        """

        self.time_left = time_left

        # Perform any required initializations, including selecting an initial
        # move from the game board (i.e., an opening book), or returning
        # immediately if there are no legal moves


        # if no available moves
        move = (-1, -1)

        if not legal_moves:
            return move
        # no more legal @empty moves

        # if have available moves
        search_strategy = eval('self.' + self.method)
        # minimax or alpha-beta pruning

        try:
            # The search method call (alpha beta or minimax) should happen in
            # here in order to avoid timeout. The try/except block will
            # automatically catch the exception raised by the search method
            # when the timer gets close to expiring

            if self.iterative:
                for depth in range(1, game.width * game.height):
                    score, move = search_strategy(game, depth)
                    if score == float('inf'):
                        break
            else:
                score, move = search_strategy(game, self.search_depth)
            pass
            # if iterative, need to search till the end of nodes
            # else just search till fix depth mentioned

        except Timeout:
            # Handle any actions required at timeout, if necessary
            pass

        if move == (-1, -1):
            move = legal_moves[random.randint(0, len(legal_moves) - 1)]
        # no more move after search

        # Return the best move from the last completed search iteration
        return move

    def minimax(self, game, depth, maximizing_player=True):
        """Implement the minimax search algorithm as described in the lectures.

        Parameters
        ----------
        game : isolation.Board
            An instance of the Isolation game `Board` class representing the
            current game state

        depth : int
            Depth is an integer representing the maximum number of plies to
            search in the game tree before aborting

        maximizing_player : bool
            Flag indicating whether the current search depth corresponds to a
            maximizing layer (True) or a minimizing layer (False)

        Returns
        -------
        float
            The score for the current search branch

        tuple(int, int)
            The best move for the current branch; (-1, -1) for no legal moves

        Notes
        -----
            (1) You MUST use the `self.score()` method for board evaluation
                to pass the project unit tests; you cannot call any other
                evaluation function directly.
        """
        if self.time_left() < self.TIMER_THRESHOLD:
            raise Timeout()

        if depth == 0:
            return self.score(game, self), game.get_player_location(self)

        ret_score = float('-inf') if maximizing_player else float('inf')
        ret_move = (-1, -1)
        player = self if maximizing_player else game.get_opponent(self)
        possible_moves = game.get_legal_moves(player)

        if len(possible_moves) == 0:
            return self.score(game, self), (-1, -1)

        for move in possible_moves:
            score, _move = self.minimax(game.forecast_move(move), depth - 1, not maximizing_player)

            if maximizing_player:
                if score >= ret_score:  # initial ret_score = -inf
                    ret_score = score
                    ret_move = move

            else:
                if score <= ret_score:  # initial ret_score = inf
                    ret_score = score
                    ret_move = move

        return ret_score, ret_move

    def alphabeta(self, game, depth, alpha=float("-inf"), beta=float("inf"), maximizing_player=True):
        """Implement minimax search with alpha-beta pruning as described in the
        lectures.

        Parameters
        ----------
        game : isolation.Board
            An instance of the Isolation game `Board` class representing the
            current game state

        depth : int
            Depth is an integer representing the maximum number of plies to
            search in the game tree before aborting

        alpha : float
            Alpha limits the lower bound of search on minimizing layers

        beta : float
            Beta limits the upper bound of search on maximizing layers

        maximizing_player : bool
            Flag indicating whether the current search depth corresponds to a
            maximizing layer (True) or a minimizing layer (False)

        Returns
        -------
        float
            The score for the current search branch

        tuple(int, int)
            The best move for the current branch; (-1, -1) for no legal moves

        Notes
        -----
            (1) You MUST use the `self.score()` method for board evaluation
                to pass the project unit tests; you cannot call any other
                evaluation function directly.
        """
        if self.time_left() < self.TIMER_THRESHOLD:
            raise Timeout()

        if depth == 0:
            return self.score(game, self), game.get_player_location(self)

        ret_score = float('-inf') if maximizing_player else float('inf')
        ret_move = (-1, -1)
        possible_moves = game.get_legal_moves(self) if maximizing_player else game.get_legal_moves(
            game.get_opponent(self))

        if len(possible_moves) == 0:
            return self.score(game, self), (-1, -1)

        for move in possible_moves:
            score, _move = self.alphabeta(game.forecast_move(move), depth - 1, alpha, beta, not maximizing_player)

            if maximizing_player:
                if score >= ret_score:
                    ret_score = score
                    ret_move = move
                if ret_score >= beta:
                    return ret_score, ret_move
                    # break tree
                alpha = max(ret_score, alpha)

            else:
                if score <= ret_score:
                    ret_score = score
                    ret_move = move
                if ret_score <= alpha:
                    return ret_score, ret_move
                    # break tree
                beta = min(ret_score, beta)

        return ret_score, ret_move
