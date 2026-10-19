using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using quizShared.DTOs;
using quizShared;
using quizApi.Models;
using quizApi.Services;

namespace quizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController(IQuizService quizService, ILogger<QuizController> logger) : ControllerBase
{
    private readonly IQuizService _quizService = quizService;
    private readonly ILogger<QuizController> _logger = logger;

    [HttpGet(Name = "GetAll")]
    public async Task<IActionResult> GetQuizzes([FromQuery] int? limit,[FromQuery]  int? skip)
    {
        List<QuizDTO> quizzes = await _quizService.GetQuizzes(limit, skip);
        return Ok(quizzes);
    }

    [HttpGet("{id}", Name = "GetById")]
    public async Task<IActionResult> GetQuizById([FromRoute] ObjectId id) 
    {
        QuizDTO quiz = await _quizService.GetQuizById(id);
        return Ok(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz([FromBody]CreateQuizDTO dto)
    {
        Quiz quiz = await _quizService.CreateQuiz(dto);

        _logger.LogInformation($"Quiz Created: {quiz.Id} (Id)");
        return Ok(new { QuizId = quiz.Id.ToString() });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateQuiz([FromRoute]ObjectId id, [FromBody]UpdateQuizDTO dto)
    {
        Quiz quiz = await _quizService.UpdateQuiz(id, dto);

        _logger.LogInformation($"Quiz Updated: {id} (Id)");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz([FromRoute]ObjectId id)
    {
        await _quizService.DeleteQuiz(id);
        _logger.LogInformation($"Quiz Deleted: {id} (Id)");
        
        return NoContent();
    }

    // Question managment
    [HttpPost("{id}/question")]
    public async Task<IActionResult> AddQuestion([FromRoute] ObjectId id, [FromBody] CreateQuestionDTO dto)
    {
        Question question = await _quizService.AddQuestion(id, dto);
        _logger.LogInformation($"Question Added: \n\t{id} (Quiz Id)\n\t{question.Id} (Question Id)");

        return Created();
    }

    [HttpPatch("{quizId}/question/{questionId}")]
    public async Task<IActionResult> UpdateQuestion([FromRoute] ObjectId quizId, [FromRoute] ObjectId questionId, [FromBody] UpdateQuestionDTO dto)
    {
        await _quizService.UpdateQuestion(quizId, questionId, dto);
        _logger.LogInformation($"Question Updated: \n\t{quizId} (Quiz Id)\n\t{questionId} (Question Id)");

        return NoContent();
    }

    [HttpDelete("{quizId}/question/{questionId}")]
    public async Task<IActionResult> DeleteQuestion([FromRoute] ObjectId quizId, [FromRoute] ObjectId questionId)
    {
        await _quizService.DeleteQuestion(quizId, questionId);
        _logger.LogInformation($"Question Deleted: \n\t{quizId} (Quiz Id)\n\t{questionId} (Question Id)");

        return NoContent();
    }
}