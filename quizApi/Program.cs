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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// *** Custom Services ***
builder.Services
    .AddMongoClient()
    .AddDatabase()
    .AddCollection<Quiz>("quizes")
    .AddCollection<Submission>("submissions");
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestionValidator>();
builder.Services.AddExceptionHandler<DevelopmentExceptionHandler>();
builder.Services.AddProblemDetails();
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
        CreateMap<Question, QuestionDTO>();
        CreateMap<Answer, AnswerDTO>();
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