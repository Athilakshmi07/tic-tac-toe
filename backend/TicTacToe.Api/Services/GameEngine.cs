using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameEngine
{
    (bool IsWin, string? Winner, List<CellCoordinate> WinningCells) CheckWin(string[][] board);
    bool IsBoardFull(string[][] board);
    CellCoordinate? GetBestComputerMove(string[][] board);
    CellCoordinate? GetMinimaxMove(string[][] board);
    CellCoordinate? GetComputerMove(string[][] board, string difficulty);
}

public class GameEngine : IGameEngine
{
    private static readonly List<CellCoordinate[]> WinningCombinations = new()
    {
        // Rows
        new[] { new CellCoordinate(0, 0), new CellCoordinate(0, 1), new CellCoordinate(0, 2) },
        new[] { new CellCoordinate(1, 0), new CellCoordinate(1, 1), new CellCoordinate(1, 2) },
        new[] { new CellCoordinate(2, 0), new CellCoordinate(2, 1), new CellCoordinate(2, 2) },

        // Columns
        new[] { new CellCoordinate(0, 0), new CellCoordinate(1, 0), new CellCoordinate(2, 0) },
        new[] { new CellCoordinate(0, 1), new CellCoordinate(1, 1), new CellCoordinate(2, 1) },
        new[] { new CellCoordinate(0, 2), new CellCoordinate(1, 2), new CellCoordinate(2, 2) },

        // Diagonals
        new[] { new CellCoordinate(0, 0), new CellCoordinate(1, 1), new CellCoordinate(2, 2) },
        new[] { new CellCoordinate(0, 2), new CellCoordinate(1, 1), new CellCoordinate(2, 0) }
    };

    private static readonly CellCoordinate[] Corners = new[]
    {
        new CellCoordinate(0, 0),
        new CellCoordinate(0, 2),
        new CellCoordinate(2, 0),
        new CellCoordinate(2, 2)
    };

    public (bool IsWin, string? Winner, List<CellCoordinate> WinningCells) CheckWin(string[][] board)
    {
        foreach (var combo in WinningCombinations)
        {
            var c0 = combo[0];
            var c1 = combo[1];
            var c2 = combo[2];

            var val0 = board[c0.Row][c0.Column];
            var val1 = board[c1.Row][c1.Column];
            var val2 = board[c2.Row][c2.Column];

            if (!string.IsNullOrEmpty(val0) && val0 == val1 && val1 == val2)
            {
                return (true, val0, new List<CellCoordinate> { c0, c1, c2 });
            }
        }

        return (false, null, new List<CellCoordinate>());
    }

    public bool IsBoardFull(string[][] board)
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (string.IsNullOrEmpty(board[r][c]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public CellCoordinate? GetComputerMove(string[][] board, string difficulty)
    {
        if (difficulty.Equals(GameConstants.DifficultyStandard, StringComparison.OrdinalIgnoreCase))
        {
            return GetBestComputerMove(board);
        }

        return GetMinimaxMove(board);
    }

    /// <summary>
    /// Invincible Minimax Algorithm: Evaluates the complete game tree.
    /// Player O is maximizing (+10), Player X is minimizing (-10).
    /// Mathematically impossible to lose: always wins on opponent error or forces draw.
    /// </summary>
    public CellCoordinate? GetMinimaxMove(string[][] board)
    {
        int bestScore = int.MinValue;
        CellCoordinate? bestMove = null;

        // Iterate over empty cells
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (string.IsNullOrEmpty(board[r][c]))
                {
                    board[r][c] = GameConstants.PlayerO;
                    int score = Minimax(board, 0, false);
                    board[r][c] = GameConstants.EmptyCell;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new CellCoordinate(r, c);
                    }
                }
            }
        }

        return bestMove;
    }

    private int Minimax(string[][] board, int depth, bool isMaximizing)
    {
        var (isWin, winner, _) = CheckWin(board);
        if (isWin)
        {
            return (winner == GameConstants.PlayerO) ? (10 - depth) : (depth - 10);
        }

        if (IsBoardFull(board))
        {
            return 0;
        }

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (string.IsNullOrEmpty(board[r][c]))
                    {
                        board[r][c] = GameConstants.PlayerO;
                        int eval = Minimax(board, depth + 1, false);
                        board[r][c] = GameConstants.EmptyCell;
                        maxEval = Math.Max(maxEval, eval);
                    }
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (string.IsNullOrEmpty(board[r][c]))
                    {
                        board[r][c] = GameConstants.PlayerX;
                        int eval = Minimax(board, depth + 1, true);
                        board[r][c] = GameConstants.EmptyCell;
                        minEval = Math.Min(minEval, eval);
                    }
                }
            }
            return minEval;
        }
    }

    /// <summary>
    /// Standard 5-tier Priority Heuristic (as defined in original specification)
    /// </summary>
    public CellCoordinate? GetBestComputerMove(string[][] board)
    {
        // Priority 1: If O can win, play the winning move
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (string.IsNullOrEmpty(board[r][c]))
                {
                    board[r][c] = GameConstants.PlayerO;
                    var (isWin, winner, _) = CheckWin(board);
                    board[r][c] = GameConstants.EmptyCell;

                    if (isWin && winner == GameConstants.PlayerO)
                    {
                        return new CellCoordinate(r, c);
                    }
                }
            }
        }

        // Priority 2: If X can win next, block X
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (string.IsNullOrEmpty(board[r][c]))
                {
                    board[r][c] = GameConstants.PlayerX;
                    var (isWin, winner, _) = CheckWin(board);
                    board[r][c] = GameConstants.EmptyCell;

                    if (isWin && winner == GameConstants.PlayerX)
                    {
                        return new CellCoordinate(r, c);
                    }
                }
            }
        }

        // Priority 3: Take center if available
        if (string.IsNullOrEmpty(board[1][1]))
        {
            return new CellCoordinate(1, 1);
        }

        // Priority 4: Take a corner if available
        foreach (var corner in Corners)
        {
            if (string.IsNullOrEmpty(board[corner.Row][corner.Column]))
            {
                return new CellCoordinate(corner.Row, corner.Column);
            }
        }

        // Priority 5: Take any available cell
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (string.IsNullOrEmpty(board[r][c]))
                {
                    return new CellCoordinate(r, c);
                }
            }
        }

        return null;
    }
}
