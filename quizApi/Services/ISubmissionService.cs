using quizApi.Models;
using quizShared.DTOs;
using quizShared;
using MongoDB.Bson;

namespace quizApi.Services;

public interface ISubmissionService
{
    // Get submissions
    /// <summary>
    /// Get all submissions as dtos, can return empty list
    /// </summary>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public Task<List<SubmissionDTO>> GetSubmissions(int? limit = null, int? skip = null);

    /// <summary>
    /// Get all full submissions, can return empty list
    /// </summary>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public Task<List<Submission>> GetFullSubmissions(int? limit = null, int? skip = null);

    // // Get a single quiz by id
    /// <summary>
    /// Get submissionDTO or throw
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task<SubmissionDTO> GetSubmissionById(ObjectId id);
    /// <summary>
    /// Get full submission or throw
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task<Submission> GetFullSubmissionById(ObjectId id);

    /// <summary>
    /// Create a new submission with status InProgress
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="DocumentNotFoundException">when QuizId is invalid</exception>
    public Task<Submission> CreateSubmission(CreateSubmissionDTO dto);
    
    /// <summary>
    /// Delete submission or throw
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="MongoException"></exception>
    public Task DeleteSubmission(ObjectId id);

    /// <summary>
    /// Add User submitted answers to submission by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="answers"></param>
    /// <returns>updated submission or throw</returns>
    /// <exception cref="MongoException"></exception>
    public Task<Submission> SubmitAnswers(ObjectId id, List<SubmitUserAnswerDTO> answers);

    /// <summary>
    /// Grades the submission, throw if quiz or question id is wrong, or on db fail.
    /// Replaces old submission with new graded one
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    /// <exception cref="MongoException"></exception>
    public Task<SubmissionDTO> Grade(ObjectId id);
}