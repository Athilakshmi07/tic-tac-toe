using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly IGameService _gameService;

    public ScoreboardController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public ActionResult<ScoreboardResponse> GetScoreboard()
    {
        var scoreboard = _gameService.GetScoreboard();
        return Ok(new ScoreboardResponse
        {
            XWins = scoreboard.XWins,
            OWins = scoreboard.OWins,
            Draws = scoreboard.Draws
        });
    }

    [HttpPost("reset")]
    public ActionResult<ScoreboardResponse> ResetScoreboard()
    {
        var scoreboard = _gameService.ResetScoreboard();
        return Ok(new ScoreboardResponse
        {
            XWins = scoreboard.XWins,
            OWins = scoreboard.OWins,
            Draws = scoreboard.Draws
        });
    }
}
