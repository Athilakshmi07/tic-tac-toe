using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Tests;

public class GameServiceTests
{
    private readonly GameEngine _engine = new();
    private readonly GameService _service;

    public GameServiceTests()
    {
        _service = new GameService(_engine);
    }

    [Fact]
    public void CreateGame_Default_InitializesTwoPlayerGame()
    {
        var game = _service.CreateGame(null);

        Assert.NotNull(game);
        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal(GameConstants.ModeTwoPlayer, game.Mode);
        Assert.Equal(GameConstants.PlayerX, game.CurrentPlayer);
        Assert.Equal(GameConstants.StatusInProgress, game.Status);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void CreateGame_ComputerMode_InitializesComputerGame()
    {
        var game = _service.CreateGame(GameConstants.ModeComputer);

        Assert.NotNull(game);
        Assert.Equal(GameConstants.ModeComputer, game.Mode);
    }

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoardAndSwitchesTurn()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        var updated = _service.MakeMove(game.Id, "X", 0, 0);

        Assert.Equal("X", updated.Board[0][0]);
        Assert.Equal(GameConstants.PlayerO, updated.CurrentPlayer);
        Assert.Single(updated.MoveHistory);
        Assert.Equal("Row 1, Column 1", updated.MoveHistory[0].Position);
    }

    [Fact]
    public void MakeMove_InvalidMove_OccupiedCell_ThrowsException()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);
        _service.MakeMove(game.Id, "X", 0, 0);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.MakeMove(game.Id, "O", 0, 0));

        Assert.Contains("already occupied", ex.Message);
    }

    [Fact]
    public void MakeMove_InvalidMove_OutOfBounds_ThrowsException()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.MakeMove(game.Id, "X", 3, 0));
    }

    [Fact]
    public void MakeMove_InvalidMove_WrongPlayerTurn_ThrowsException()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.MakeMove(game.Id, "O", 0, 0));

        Assert.Contains("It is Player X's turn", ex.Message);
    }

    [Fact]
    public void MakeMove_RowWin_UpdatesGameStatusAndScoreboard()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        // X plays (0,0), O plays (1,0)
        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 1, 0);
        // X plays (0,1), O plays (1,1)
        _service.MakeMove(game.Id, "X", 0, 1);
        _service.MakeMove(game.Id, "O", 1, 1);
        // X plays (0,2) -> WIN for X
        var finished = _service.MakeMove(game.Id, "X", 0, 2);

        Assert.Equal(GameConstants.StatusWon, finished.Status);
        Assert.Equal("X", finished.Winner);
        Assert.Equal(3, finished.WinningCells.Count);

        var scoreboard = _service.GetScoreboard();
        Assert.Equal(1, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void MakeMove_ColumnWin_DetectsWinner()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 0, 1);
        _service.MakeMove(game.Id, "X", 1, 0);
        _service.MakeMove(game.Id, "O", 1, 1);
        var finished = _service.MakeMove(game.Id, "X", 2, 0); // X completes col 0

        Assert.Equal(GameConstants.StatusWon, finished.Status);
        Assert.Equal("X", finished.Winner);
    }

    [Fact]
    public void MakeMove_DiagonalWin_DetectsWinner()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 0, 1);
        _service.MakeMove(game.Id, "X", 1, 1);
        _service.MakeMove(game.Id, "O", 0, 2);
        var finished = _service.MakeMove(game.Id, "X", 2, 2); // X completes diagonal

        Assert.Equal(GameConstants.StatusWon, finished.Status);
        Assert.Equal("X", finished.Winner);
    }

    [Fact]
    public void MakeMove_Draw_UpdatesGameStatusAndScoreboard()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        /*
           X O X
           X O O
           O X X
        */
        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 0, 1);
        _service.MakeMove(game.Id, "X", 0, 2);
        _service.MakeMove(game.Id, "O", 1, 1);
        _service.MakeMove(game.Id, "X", 1, 0);
        _service.MakeMove(game.Id, "O", 1, 2);
        _service.MakeMove(game.Id, "X", 2, 1);
        _service.MakeMove(game.Id, "O", 2, 0);
        var finished = _service.MakeMove(game.Id, "X", 2, 2);

        Assert.Equal(GameConstants.StatusDraw, finished.Status);
        Assert.Null(finished.Winner);

        var scoreboard = _service.GetScoreboard();
        Assert.Equal(1, scoreboard.Draws);
    }

    [Fact]
    public void MakeMove_AfterGameCompletion_ThrowsException()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 1, 0);
        _service.MakeMove(game.Id, "X", 0, 1);
        _service.MakeMove(game.Id, "O", 1, 1);
        _service.MakeMove(game.Id, "X", 0, 2); // Game Won

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.MakeMove(game.Id, "O", 2, 2));

        Assert.Contains("completed game", ex.Message);
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesLastMoveAndRestoresTurn()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 1, 1);

        var undone = _service.UndoMove(game.Id);

        Assert.Equal("", undone.Board[1][1]);
        Assert.Equal("X", undone.Board[0][0]);
        Assert.Equal(GameConstants.PlayerO, undone.CurrentPlayer);
        Assert.Single(undone.MoveHistory);
    }

    [Fact]
    public void Undo_ComputerMode_RemovesBothComputerAndHumanMoves()
    {
        var game = _service.CreateGame(GameConstants.ModeComputer);

        // Human plays X at (0, 0), computer automatically responds
        var afterMove = _service.MakeMove(game.Id, "X", 0, 0);
        Assert.Equal(2, afterMove.MoveHistory.Count);

        var undone = _service.UndoMove(game.Id);

        Assert.Empty(undone.MoveHistory);
        Assert.Equal("", undone.Board[0][0]);
        Assert.Equal(GameConstants.PlayerX, undone.CurrentPlayer);
    }

    [Fact]
    public void Undo_WhenNoMoves_ThrowsException()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.UndoMove(game.Id));

        Assert.Contains("No moves available", ex.Message);
    }

    [Fact]
    public void Undo_WhenGameCompleted_OptionA_ThrowsException()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 1, 0);
        _service.MakeMove(game.Id, "X", 0, 1);
        _service.MakeMove(game.Id, "O", 1, 1);
        _service.MakeMove(game.Id, "X", 0, 2); // Won

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.UndoMove(game.Id));

        Assert.Contains("disabled for completed games", ex.Message);
    }

    [Fact]
    public void ResetGame_ClearsBoardAndHistory_KeepsScoreboard()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);

        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 1, 0);
        _service.MakeMove(game.Id, "X", 0, 1);
        _service.MakeMove(game.Id, "O", 1, 1);
        _service.MakeMove(game.Id, "X", 0, 2); // X won -> scoreboard has 1 win

        var reset = _service.ResetGame(game.Id);

        Assert.Equal(GameConstants.StatusInProgress, reset.Status);
        Assert.Null(reset.Winner);
        Assert.Equal(GameConstants.PlayerX, reset.CurrentPlayer);
        Assert.Empty(reset.MoveHistory);
        Assert.Equal("", reset.Board[0][0]);

        var scoreboard = _service.GetScoreboard();
        Assert.Equal(1, scoreboard.XWins);
    }

    [Fact]
    public void ResetScoreboard_ClearsStats()
    {
        var game = _service.CreateGame(GameConstants.ModeTwoPlayer);
        _service.MakeMove(game.Id, "X", 0, 0);
        _service.MakeMove(game.Id, "O", 1, 0);
        _service.MakeMove(game.Id, "X", 0, 1);
        _service.MakeMove(game.Id, "O", 1, 1);
        _service.MakeMove(game.Id, "X", 0, 2); // X won

        var resetScore = _service.ResetScoreboard();

        Assert.Equal(0, resetScore.XWins);
        Assert.Equal(0, resetScore.OWins);
        Assert.Equal(0, resetScore.Draws);
        Assert.Equal(0, resetScore.TotalGames);
    }
}
