using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Tests;

public class GameEngineTests
{
    private readonly GameEngine _engine = new();

    private static string[][] CreateEmptyBoard() => new[]
    {
        new[] { "", "", "" },
        new[] { "", "", "" },
        new[] { "", "", "" }
    };

    [Fact]
    public void CheckWin_RowWin_DetectsWinnerAndCoordinates()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "X";
        board[0][1] = "X";
        board[0][2] = "X";

        var (isWin, winner, winningCells) = _engine.CheckWin(board);

        Assert.True(isWin);
        Assert.Equal("X", winner);
        Assert.Equal(3, winningCells.Count);
        Assert.Contains(winningCells, c => c.Row == 0 && c.Column == 0);
        Assert.Contains(winningCells, c => c.Row == 0 && c.Column == 1);
        Assert.Contains(winningCells, c => c.Row == 0 && c.Column == 2);
    }

    [Fact]
    public void CheckWin_ColumnWin_DetectsWinnerAndCoordinates()
    {
        var board = CreateEmptyBoard();
        board[0][1] = "O";
        board[1][1] = "O";
        board[2][1] = "O";

        var (isWin, winner, winningCells) = _engine.CheckWin(board);

        Assert.True(isWin);
        Assert.Equal("O", winner);
        Assert.Equal(3, winningCells.Count);
        Assert.Contains(winningCells, c => c.Row == 0 && c.Column == 1);
        Assert.Contains(winningCells, c => c.Row == 1 && c.Column == 1);
        Assert.Contains(winningCells, c => c.Row == 2 && c.Column == 1);
    }

    [Fact]
    public void CheckWin_MainDiagonalWin_DetectsWinnerAndCoordinates()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "X";
        board[1][1] = "X";
        board[2][2] = "X";

        var (isWin, winner, winningCells) = _engine.CheckWin(board);

        Assert.True(isWin);
        Assert.Equal("X", winner);
        Assert.Equal(3, winningCells.Count);
        Assert.Contains(winningCells, c => c.Row == 0 && c.Column == 0);
        Assert.Contains(winningCells, c => c.Row == 1 && c.Column == 1);
        Assert.Contains(winningCells, c => c.Row == 2 && c.Column == 2);
    }

    [Fact]
    public void CheckWin_AntiDiagonalWin_DetectsWinnerAndCoordinates()
    {
        var board = CreateEmptyBoard();
        board[0][2] = "O";
        board[1][1] = "O";
        board[2][0] = "O";

        var (isWin, winner, winningCells) = _engine.CheckWin(board);

        Assert.True(isWin);
        Assert.Equal("O", winner);
        Assert.Equal(3, winningCells.Count);
    }

    [Fact]
    public void IsBoardFull_WhenFull_ReturnsTrue()
    {
        var board = new[]
        {
            new[] { "X", "O", "X" },
            new[] { "X", "O", "O" },
            new[] { "O", "X", "X" }
        };

        Assert.True(_engine.IsBoardFull(board));
    }

    [Fact]
    public void IsBoardFull_WhenEmptyOrPartial_ReturnsFalse()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "X";
        Assert.False(_engine.IsBoardFull(board));
    }

    [Fact]
    public void GetBestComputerMove_Priority1_TakesWinningMove()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "O";
        board[0][1] = "O";
        board[1][0] = "X";
        board[1][1] = "X";

        var move = _engine.GetBestComputerMove(board);

        Assert.NotNull(move);
        Assert.Equal(0, move.Row);
        Assert.Equal(2, move.Column);
    }

    [Fact]
    public void GetBestComputerMove_Priority2_BlocksOpponentWinningMove()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "X";
        board[1][1] = "X";
        board[0][1] = "O";

        var move = _engine.GetBestComputerMove(board);

        Assert.NotNull(move);
        Assert.Equal(2, move.Row);
        Assert.Equal(2, move.Column);
    }

    [Fact]
    public void GetBestComputerMove_Priority3_TakesCenterIfAvailable()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "X";

        var move = _engine.GetBestComputerMove(board);

        Assert.NotNull(move);
        Assert.Equal(1, move.Row);
        Assert.Equal(1, move.Column);
    }

    [Fact]
    public void GetBestComputerMove_Priority4_TakesCornerIfCenterTaken()
    {
        var board = CreateEmptyBoard();
        board[1][1] = "X";

        var move = _engine.GetBestComputerMove(board);

        Assert.NotNull(move);
        bool isCorner = (move.Row == 0 && move.Column == 0) ||
                        (move.Row == 0 && move.Column == 2) ||
                        (move.Row == 2 && move.Column == 0) ||
                        (move.Row == 2 && move.Column == 2);
        Assert.True(isCorner);
    }

    [Fact]
    public void GetBestComputerMove_Priority5_TakesAnyAvailableCell()
    {
        var board = CreateEmptyBoard();
        board[1][1] = "X";
        board[0][0] = "O";
        board[0][2] = "X";
        board[2][0] = "O";
        board[2][2] = "X";

        var move = _engine.GetBestComputerMove(board);

        Assert.NotNull(move);
        bool isEdge = (move.Row == 0 && move.Column == 1) ||
                      (move.Row == 1 && move.Column == 0) ||
                      (move.Row == 1 && move.Column == 2) ||
                      (move.Row == 2 && move.Column == 1);
        Assert.True(isEdge);
    }

    [Fact]
    public void GetMinimaxMove_ImmediateWin_TakesIt()
    {
        var board = CreateEmptyBoard();
        board[0][0] = "O";
        board[0][1] = "O";
        board[1][0] = "X";
        board[1][1] = "X";

        var move = _engine.GetMinimaxMove(board);

        Assert.NotNull(move);
        Assert.Equal(0, move.Row);
        Assert.Equal(2, move.Column);
    }

    [Fact]
    public void GetMinimaxMove_BlocksOpponentFork_ChoosesSafeMove()
    {
        // Classic fork trap setup: X at opposite corners (0,0) and (2,2), O at center (1,1)
        var board = CreateEmptyBoard();
        board[0][0] = "X";
        board[1][1] = "O";
        board[2][2] = "X";

        var move = _engine.GetMinimaxMove(board);

        Assert.NotNull(move);
        // Playing an edge (0,1), (1,0), (1,2), or (2,1) creates a counter-threat and prevents fork!
        bool isEdge = (move.Row == 0 && move.Column == 1) ||
                      (move.Row == 1 && move.Column == 0) ||
                      (move.Row == 1 && move.Column == 2) ||
                      (move.Row == 2 && move.Column == 1);
        Assert.True(isEdge);
    }
}
