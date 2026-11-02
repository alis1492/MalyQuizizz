using AutoMapper;
using FluentValidation;
using MongoDB.Bson;
using quizApi.Converters;
using quizShared.DTOs;
using quizShared;
using quizApi.ExceptionHandlers;
using quizApi.Extensions;
using quizApi.Models;
using quizApi.OperationFilters;
using quizApi.Services;
using quizApi.Validators;
using quizApi.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// App Configuration
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// *** Custom Services ***
// MongoDB Configuration
builder.Services
    .AddMongoClient()
    .AddDatabase()
    .AddCollection<Quiz>("quizes")
    .AddCollection<Submission>("submissions");

// General Services
builder.Services
    .AddScoped<IQuizService, QuizService>()
    .AddScoped<ISubmissionService, SubmissionService>()
    .AddAutoMapper(cfg => { }, typeof(Program).Assembly)
    .AddValidatorsFromAssemblyContaining<CreateQuestionValidator>()
    .AddExceptionHandler<DevelopmentExceptionHandler>()
    .AddProblemDetails();
// *** Custom Services ***

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<ObjectIdOperationFilter>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// *** Added Methods ***
app.UseExceptionHandler("/Error");
// *** Added Methods ***

app.Run();

// AutoMapper profile
class AutoMapperProfile : Profile
{   
    public AutoMapperProfile()
    {
        // ObjectId to string conversion for preserving sanity
        CreateMap<ObjectId, string>()
            .ConvertUsing<ObjectIdToStringConverter>();
        CreateMap<string, ObjectId>()
            .ConvertUsing<StringToObjectIdConverter>();

        // Quiz related maps
        CreateMap<Quiz, QuizDTO>();
        CreateMap<Quiz, FullQuizDTO>();
        CreateMap<FullQuizDTO, QuizDTO>();
        CreateMap<Question, QuestionDTO>();
        CreateMap<Question, FullQuestionDTO>();
        CreateMap<FullQuestionDTO, QuestionDTO>();
        CreateMap<Answer, AnswerDTO>();
        CreateMap<Answer, FullAnswerDTO>();
        CreateMap<FullAnswerDTO, AnswerDTO>();
        CreateMap<CreateAnswerDTO, Answer>();
        CreateMap<CreateQuestionDTO, Question>();
        CreateMap<UpdateAnswerDTO, Answer>();

        // Submission related maps
        CreateMap<CreateSubmissionDTO, Submission>();
        CreateMap<UserAnswer, UserAnswerDTO>();
        CreateMap<Submission, SubmissionDTO>();
        CreateMap<SubmitUserAnswerDTO, UserAnswer>();
    }
}