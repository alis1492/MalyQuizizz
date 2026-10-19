using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DnsClient.Protocol;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using quizShared.DTOs;
using quizShared;
using quizApi.Exceptions;
using quizApi.Models;
using quizApi.Validators;

namespace quizApi.Services;

public class QuizService(
    IMongoCollection<Quiz> collection,
    IMapper mapper,
    IValidator<CreateQuestionDTO> createQuestionValidator,
    IValidator<UpdateQuestionDTO> updateQuestionValidator
) : IQuizService
{
    private readonly IMongoCollection<Quiz> _collection = collection;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<CreateQuestionDTO> _createQuestionValidator = createQuestionValidator;
    private readonly IValidator<UpdateQuestionDTO> _updateQuestionValidator = updateQuestionValidator;

    // Get all quizes asyncronously
    public async Task<List<QuizDTO>> GetQuizzes(int? limit = null, int? skip = null)
    {
        // get all full quizes
        var quizzes = await GetFullQuizzes(limit, skip);

        // map them to DTO
        List<QuizDTO> quizDTOs = [];
        foreach (var quiz in quizzes)
        {
            var quizDTO = _mapper.Map<QuizDTO>(quiz);
            quizDTOs.Add(quizDTO);
        }

        // return list of dtos
        return quizDTOs;
    }
    public async Task<List<Quiz>> GetFullQuizzes(int? limit = null, int? skip = null)
    {
        var quizzes = await _collection
            .Find(_ => true)
            .Limit(limit)
            .Skip(skip)
            .ToListAsync();
        return quizzes;
    }

    // Get a single quiz by id
    public async Task<QuizDTO> GetQuizById(ObjectId id)
    {
        var quiz = await GetFullQuizById(id);
        return _mapper.Map<QuizDTO>(quiz);
    }
    public async Task<Quiz> GetFullQuizById(ObjectId id)
    {
        return await _collection.Find<Quiz>(q => q.Id == id).FirstOrDefaultAsync()
            ?? throw new DocumentNotFoundException("Quiz", id.ToString());
    }

    public async Task<Quiz> CreateQuiz(CreateQuizDTO dto)
    {
        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            TeacherId = new ObjectId(dto.TeacherId),
            // dto validation allows unsafe cast
            Subject = (Subject)dto.Subject,
            Settings = dto.Settings
        };

        // if this doesnt throw everything is fine
        await _collection.InsertOneAsync(quiz);

        return quiz;
    }

    public async Task<Quiz> UpdateQuiz(ObjectId id, UpdateQuizDTO dto)
    {
        // helper
        var updateBuilder = Builders<Quiz>.Update;
        // update definitions to be combined
        List<UpdateDefinition<Quiz>> updateDefinitions = [];
        
        // foreach not null field add an UpdateDefinition to the list
        if(dto.Title != null)
            updateDefinitions.Add(updateBuilder.Set(q => q.Title, dto.Title));
        if(dto.Description != null)
            updateDefinitions.Add(updateBuilder.Set(q => q.Description, dto.Description));
        if(dto.Subject != null)
            updateDefinitions.Add(updateBuilder.Set(q => q.Subject, (Subject)dto.Subject));
        if(dto.Settings != null)
            updateDefinitions.Add(updateBuilder.Set(q => q.Settings, dto.Settings));

        // combine all UpdateDefinitions into one
        var update = updateBuilder.Combine(updateDefinitions);

        // if doesnt throw everything is fine
        var quiz = await _collection.FindOneAndUpdateAsync(
            q => q.Id == id,
            update,
            // ReturnDocument allows the same db call to return updated document
            new FindOneAndUpdateOptions<Quiz>{ReturnDocument = ReturnDocument.After}
        ) ?? throw new DocumentNotFoundException("Quiz", id.ToString());

        return quiz;
    }

    public async Task DeleteQuiz(ObjectId id)
    {
        // fine if doesnt throw
        var quiz = await _collection.FindOneAndDeleteAsync(q => q.Id == id)
            ?? throw new DocumentNotFoundException("Quiz", id.ToString());
    }

    public async Task<Question> AddQuestion(ObjectId quizId, CreateQuestionDTO dto)
    {
        // Custom validation defined in validator
        // basically just checks for correct answers depending on a question type
        _createQuestionValidator.ValidateAndThrow(dto);

        var question = _mapper.Map<Question>(dto);
        var update = Builders<Quiz>.Update.Push(
            q => q.Questions,
            question
        );
        var result = await _collection.UpdateOneAsync(
            q => q.Id == quizId,
            update
        );
        if(result.ModifiedCount <= 0)
            throw new MongoException("Failed to Add Question");
        
        return question;
    }

    public async Task<Question> UpdateQuestion(ObjectId quizId, ObjectId questionId, UpdateQuestionDTO dto)
    {
        Quiz? quiz = await GetFullQuizById(quizId);
        // this ?? check must be here since neither quizId nor questionId are a part of UpdateQuestionDTO
        var question = quiz.Questions
            .FirstOrDefault(q => q.Id == questionId) 
            ?? throw new DocumentNotFoundException("Question", questionId.ToString());
        var context = new ValidationContext<UpdateQuestionDTO>(dto);
        context.RootContextData["Question"] = question;
        

        // validate the dto properly
        // NOTE: ValidateAndThrow doesn't work with nested context so if() is used instead
        var validationResult = _updateQuestionValidator.Validate(context);
        if(!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);


        // Update filter to find the specific question
        var filter = Builders<Quiz>.Filter.Eq(q => q.Id, quizId) &
                     Builders<Quiz>.Filter.ElemMatch(q => q.Questions, _q => _q.Id == questionId);
        
        // helper
        var updateBuilder = Builders<Quiz>.Update;
        // update definitions to be combined
        List<UpdateDefinition<Quiz>> updateDefinitions = [];
        // this is ugly, but its still better than magical strings of "i hope this is how the element is called in database"
        if(dto.Type != null)
            updateDefinitions.Add(updateBuilder.Set(q => 
                q.Questions.FirstMatchingElement().Type, (QuestionType)dto.Type));
        if(dto.Text != null)
            updateDefinitions.Add(updateBuilder.Set(q => 
                q.Questions.FirstMatchingElement().Text, dto.Text));
        if(dto.Points != null)
            updateDefinitions.Add(updateBuilder.Set(q => 
                q.Questions.FirstMatchingElement().Points, dto.Points));
        // we know from validation that every answer id is valid
        if(dto.Answers != null)
        {
            updateDefinitions.Add(updateBuilder.Set(q => 
                q.Questions.FirstMatchingElement().Answers, _mapper.Map<List<Answer>>(dto.Answers)));
        }


        // null checks are unnecessary since first call ensures that the <filter> is valid
        var update = updateBuilder.Combine(updateDefinitions);
        // reused variables from the start
        quiz = await _collection.FindOneAndUpdateAsync(filter, update,
            new FindOneAndUpdateOptions<Quiz, Quiz>
            {
                ReturnDocument = ReturnDocument.After
            });
            // ?? throw new DocumentNotFoundException("Question", questionId.ToString())

        // debug artifacts
        // Console.WriteLine(JsonSerializer.Serialize(quiz, new JsonSerializerOptions{WriteIndented = true}));

        question = quiz.Questions.First(q => q.Id == questionId);
        return question;
    }

    public async Task DeleteQuestion(ObjectId quizId, ObjectId questionId)
    {
        // ensure that both quizId and questionId are viable
        Quiz? quiz = await _collection
            .Find(q => q.Id == quizId)
            .FirstOrDefaultAsync()
            ?? throw new DocumentNotFoundException("Quiz", quizId.ToString());
        // this ?? check must be here since neither quizId nor questionId are a part of UpdateQuestionDTO
        var question = quiz.Questions
            .FirstOrDefault(q => q.Id == questionId) 
            ?? throw new DocumentNotFoundException("Question", questionId.ToString());
        

        // update filter and definition
        var filter = Builders<Quiz>.Filter.Eq(q => q.Id, quizId);
        var update = Builders<Quiz>.Update.PullFilter(
            quiz => quiz.Questions, 
            question => question.Id == questionId);
        
        var result = await _collection.UpdateOneAsync(filter, update);
        if(result.ModifiedCount <= 0)
            throw new MongoException($"Failed to delete question with id {questionId}");
    }
}