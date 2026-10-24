using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest? request)
    {
        var mode = request?.Mode;
        var difficulty = request?.Difficulty;
        var game = _gameService.CreateGame(mode, difficulty);
        var scoreboard = _gameService.GetScoreboard();
        _logger.LogInformation("Created new game {GameId} with mode {Mode} and difficulty {Difficulty}", game.Id, game.Mode, game.Difficulty);
        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, GameStateResponse.FromState(game, scoreboard));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GameStateResponse> GetGame(Guid id)
    {
        var game = _gameService.GetGame(id);
        if (game == null)
        {
            return NotFound(new { error = $"Game {id} was not found." });
        }

        var scoreboard = _gameService.GetScoreboard();
        return Ok(GameStateResponse.FromState(game, scoreboard));
    }

    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateResponse> MakeMove(Guid id, [FromBody] MakeMoveRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body cannot be null." });
        }

        try
        {
            var game = _gameService.MakeMove(id, request.Player, request.Row, request.Column);
            var scoreboard = _gameService.GetScoreboard();
            return Ok(GameStateResponse.FromState(game, scoreboard));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateResponse> UndoMove(Guid id)
    {
        try
        {
            var game = _gameService.UndoMove(id);
            var scoreboard = _gameService.GetScoreboard();
            return Ok(GameStateResponse.FromState(game, scoreboard));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateResponse> ResetGame(Guid id)
    {
        try
        {
            var game = _gameService.ResetGame(id);
            var scoreboard = _gameService.GetScoreboard();
            return Ok(GameStateResponse.FromState(game, scoreboard));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
