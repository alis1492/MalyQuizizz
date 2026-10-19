using quizApi.Models;
using quizShared.DTOs;
using quizShared;
using MongoDB.Driver;
using AutoMapper;
using quizApi.Exceptions;
using MongoDB.Bson;

namespace quizApi.Services;

public class SubmissionService(
    IMongoCollection<Submission> submissionCollection,
    IMongoCollection<Quiz> quizCollection,
    IQuizService quizService,
    IMapper mapper
) : ISubmissionService
{
    private readonly IMongoCollection<Submission> _submissionCollection = submissionCollection;
    private readonly IMongoCollection<Quiz> _quizCollection = quizCollection;
    private readonly IQuizService _quizService = quizService;
    private readonly IMapper _mapper = mapper;

    public async Task<List<SubmissionDTO>> GetSubmissions(int? limit = null, int? skip = null)
    {
        var submissions = await GetFullSubmissions(limit, skip);

        // map them to DTO
        List<SubmissionDTO> submissionDTOs = [];
        foreach (var s in submissions)
        {
            var sDTO = _mapper.Map<SubmissionDTO>(s);
            submissionDTOs.Add(sDTO);
        }

        // return list of dtos
        return submissionDTOs;
    }
    
    public async Task<List<Submission>> GetFullSubmissions(int? limit = null, int? skip = null)
    {
        var submissions = await _submissionCollection
            .Find(_ => true)
            .Limit(limit)
            .Skip(skip)
            .ToListAsync();
        return submissions;
    }

    // Get individual submission
    public async Task<SubmissionDTO> GetSubmissionById(ObjectId id)
    {
        var submission = await GetFullSubmissionById(id);
        return _mapper.Map<SubmissionDTO>(submission);
    }
    
    public async Task<Submission> GetFullSubmissionById(ObjectId id)
    {
        var submission = await _submissionCollection
            .Find(s => s.Id == id)
            .FirstOrDefaultAsync()
            ?? throw new DocumentNotFoundException("Quiz", id.ToString());
        return submission;
    }

    public async Task<Submission> CreateSubmission(CreateSubmissionDTO dto)
    {
        var submission = _mapper.Map<Submission>(dto);
        // checks go after mapping because string to ObjectId validates string automatically
        // Quiz check
        var quiz = await _quizService.GetFullQuizById(submission.QuizId);

        // TODO: User check once auth gets implemented

        await _submissionCollection.InsertOneAsync(submission);
        return submission;
    }

    public async Task DeleteSubmission(ObjectId id)
    {
        var result = await _submissionCollection.DeleteOneAsync(s => s.Id == id);
        if(result.DeletedCount <= 0)
            throw new MongoException($"Failed to delete submission with id {id}");
    }

    public async Task<Submission> SubmitAnswers(ObjectId id, List<SubmitUserAnswerDTO> answerDTOs)
    {
        List<UserAnswer> answers = [];
        foreach(var answerDTO in answerDTOs)
        {
            var answer = _mapper.Map<UserAnswer>(answerDTO);
            answers.Add(answer);
        }
        var update = Builders<Submission>.Update
            .PushEach(s => s.UserAnswers, answers)
            .Set(s => s.Status, SubmissionStatus.Submitted)
            .Set(s => s.SubmittedAtUtc, DateTime.UtcNow);

        // TODO: a check for submission status before update in case if Grading will be separated from submitting answers
        // as it is, its now a big deal if someone submits answers to say already graded submission
        // we're working with simple quizes after all
        var submission = await _submissionCollection.FindOneAndUpdateAsync(s => s.Id == id, update,
            new FindOneAndUpdateOptions<Submission, Submission>{ReturnDocument = ReturnDocument.After})
            ?? throw new MongoException($"Failed to submit answers to quiz with id {id}");
        
        return submission;
    }

    public async Task<SubmissionDTO> Grade(ObjectId id)
    {
        var submission = await GetFullSubmissionById(id);
        var quiz = await _quizService.GetFullQuizById(submission.QuizId);

        foreach(var userAnswer in submission.UserAnswers)
        {
            ObjectId questionId = userAnswer.QuestionId;
            // throwing on not found here is an agressive handling of tampering
            var question = quiz.Questions.FirstOrDefault(q => q.Id == questionId)
                ?? throw new DocumentNotFoundException("Question", questionId.ToString());
            // add points to Score, 
            // NOTE: normally this should be separated, but for the sake of simplicity ill keep it here
            submission.Score.MaxPoints += question.Points;
            
            var userAnswerDTO = _mapper.Map<UserAnswerDTO>(userAnswer);

            double pointsAwarded = GradeQuestion(question, userAnswerDTO.SelectedAnswers);
            userAnswer.CorrectAnswers = [.. question.Answers.Where(a => a.IsCorrect).Select(a => a.Id)];
            userAnswer.PointsAwarded = pointsAwarded;

            // NOTE: again this should be separated but theres no reason to do so now
            submission.Score.TotalPoints += pointsAwarded;
        }

        // calculate Percentage
        submission.Score.Percentage = submission.Score.TotalPoints/submission.Score.MaxPoints * 100;

        submission.Status = SubmissionStatus.Graded;

        // replace old submission with new one
        // seriously, submission is few kb at max, its not that big of an overhead to assemble update pipeline for
        var result = await _submissionCollection.ReplaceOneAsync(s => s.Id == submission.Id, submission);
        if(result.ModifiedCount <= 0)
            throw new MongoException($"Failed to grade submission with id {submission.Id}");
        
        return _mapper.Map<SubmissionDTO>(submission);
    }

    /// <summary>
    /// checks if user selected answers are correct, returns 0 if none are correct or too many answers are selected
    /// </summary>
    /// <param name="question"></param>
    /// <param name="selectedAnswers"></param>
    /// <returns>points awarded</returns>
    public double GradeQuestion(Question question, List<string> selectedAnswers)
    {
        if(selectedAnswers.Count <= 0)
            return 0;
        if(question.Type == QuestionType.singleChoice)
        {
            // simply compare the first selected answer to correct one
            var correctAnswerId = question.Answers.First(a => a.IsCorrect).Id.ToString();
            return correctAnswerId == selectedAnswers[0] ? question.Points : 0;
        } 
        else if(question.Type == QuestionType.multipleChoice)
        {
            double pointsAwarded = 0;
            var correctAnswers = question.Answers.Where(a => a.IsCorrect).ToList<Answer>();
            var pointsPerAnswer = question.Points/correctAnswers.Count;

            // for every correct answer selected award pointsPerAnswer
            foreach(var correctAnswer in correctAnswers)
            {
                bool correctAnswerSelected = selectedAnswers.Any(a => a == correctAnswer.Id.ToString());
                pointsAwarded += correctAnswerSelected ? pointsPerAnswer : 0;
            }

            // for every unnecessarily selected answer deduct pointsPerAnswer
            int moreAnswersSelectedThanAreCorrect = selectedAnswers.Count - correctAnswers.Count;
            if(moreAnswersSelectedThanAreCorrect > 0)
                pointsAwarded -= moreAnswersSelectedThanAreCorrect * pointsPerAnswer;
            
            return pointsAwarded > 0 ? pointsAwarded : 0;
        }
        return 0;
    }
}