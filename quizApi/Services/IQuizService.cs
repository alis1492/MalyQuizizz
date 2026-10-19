using MongoDB.Bson;
using quizShared.DTOs;
using quizShared;
using quizApi.Models;

namespace quizApi.Services;

public interface IQuizService
{
    // Get all quizes asyncronously
    /// <summary>
    /// 
    /// </summary>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns>list of quizDTOs or empty list</returns>
    public Task<List<QuizDTO>> GetQuizzes(int? limit = null, int? skip = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns>list of full quizzes or empty list</returns>
    public Task<List<Quiz>> GetFullQuizzes(int? limit = null, int? skip = null);

    // Get a single quiz by id
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>quizDTO or throws</returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task<QuizDTO> GetQuizById(ObjectId id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>full quiz or throws</returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task<Quiz> GetFullQuizById(ObjectId id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>created quiz or throws</returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task<Quiz> CreateQuiz(CreateQuizDTO dto);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns>updated quiz or throws</returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task<Quiz> UpdateQuiz(ObjectId Id, UpdateQuizDTO dto);

    /// <summary>
    /// deletes or throws
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    public Task DeleteQuiz(ObjectId Id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="quizId"></param>
    /// <param name="dto"></param>
    /// <returns>question or throws</returns>
    /// <exception cref="MongoException"></exception>
    public Task<Question> AddQuestion(ObjectId quizId, CreateQuestionDTO dto);

    // There are two database calls bruh, idk how to optimize it to one
    /// <summary>
    /// 
    /// </summary>
    /// <param name="quizId"></param>
    /// <param name="questionId"></param>
    /// <param name="dto"></param>
    /// <returns>updated question or throws</returns>
    /// <exception cref="DocumentNotFoundException"></exception>
    /// <exception cref="ValidationException"></exception>
    public Task<Question> UpdateQuestion(ObjectId quizId, ObjectId questionId, UpdateQuestionDTO dto);

    // two db calls again ._.
    /// <summary>
    /// Delete a question from quiz by id, throws DocumentNotFoundException or MongoException on failure
    /// </summary>
    /// <param name="quizId"></param>
    /// <param name="questionId"></param>
    /// <returns></returns>
    /// <exception cref="DocumentNotFoundException">Bad ids</exception>
    /// <exception cref="MongoException">DB failure</exception>
    public Task DeleteQuestion(ObjectId quizId, ObjectId questionId);
}