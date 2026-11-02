using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using quizApi.Services;
using quizShared.DTOs;

namespace quizApi.Controllers;

[ApiController]
[Route("/api/full/quiz")]
public class FullQuizController(ILogger<FullQuizController> logger, IQuizService quizService) : ControllerBase
{
    private readonly ILogger<FullQuizController> _logger = logger;
    private readonly IQuizService _quizService = quizService;

    [HttpGet(Name = "GetFullAll")]
    public async Task<IActionResult> GetFullQuizzes([FromQuery] int? limit,[FromQuery]  int? skip)
    {
        List<FullQuizDTO> quizzes = await _quizService.GetFullQuizzes(limit, skip);
        return Ok(quizzes);
    }

    [HttpGet("{id}", Name = "GetFullById")]
    public async Task<IActionResult> GetFullQuizById([FromRoute] ObjectId id) 
    {
        var quiz = await _quizService.GetFullQuizById(id);
        return Ok(quiz);
    }
}