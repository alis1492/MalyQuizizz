using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using quizShared.DTOs;
using quizShared;
using quizApi.Services;

namespace quizApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class SubmissionController(
    ISubmissionService submissionService,
    IMapper mapper,
    ILogger<SubmissionController> logger
) : ControllerBase
{
    private readonly ISubmissionService _submissionService = submissionService;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<SubmissionController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> GetSubmissions(int? limit = null, int? skip = null)
    {
        var submissions = await _submissionService.GetSubmissions(limit, skip);
        return Ok(submissions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubmissionById([FromRoute] ObjectId id)
    {
        var submission = await _submissionService.GetSubmissionById(id);
        return Ok(submission);
    }

    /// <summary>
    /// For test purposes only, normally submission must be created with quizStart, 
    /// submitted with quizSubmit and graded either automatically or after some time
    /// </summary>
    /// <param name="dto">CreateSubmissionDTO</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateSubmission([FromBody] CreateSubmissionDTO dto)
    {
        var submission = await _submissionService.CreateSubmission(dto);
        var submissionDto = _mapper.Map<SubmissionDTO>(submission);

        _logger.LogInformation($"Submission Created: {submissionDto.Id} (Id)");
        return Ok(submissionDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubmission([FromRoute] ObjectId id)
    {
        await _submissionService.DeleteSubmission(id);

        _logger.LogInformation($"Submission Created: {id} (Id)");
        return NoContent();
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> SubmitAnswers([FromRoute] ObjectId id, [FromBody] List<SubmitUserAnswerDTO> answers)
    {
        await _submissionService.SubmitAnswers(id, answers);
        _logger.LogInformation($"Submitted Answers: {id} (SubmissionId)");

        // NOTE:in this project i use Grade in the same route as SubmitAnswers
        // normally there should be a global service implemented to manage grading
        // the service isnt bond to a specific user so it can be Singleton
        // Alternatively, grading can be always automatical but the graded submission
        // can only be accessed after a set period of time.
        // For the most part tho, just keep it in simple, its a quiz app not a serious testing platform
        var sDTO = await _submissionService.Grade(id);
        _logger.LogInformation($"Graded Submission: {id} (Id)");
        
        return Ok(sDTO);
    }
}