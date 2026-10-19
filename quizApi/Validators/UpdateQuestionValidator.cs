using System.Data;
using System.Runtime.CompilerServices;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using quizShared.DTOs;
using quizShared;
using quizApi.Models;

namespace quizApi.Validators;

/// <summary>
/// Takes RootContextData["Question"] as Question
/// </summary>
public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionDTO>
{
    // private readonly IMongoCollection<Quiz> _collection = collection;

    public UpdateQuestionValidator()
    {
        // NOTE: WithMessage() on RuleForEach provides the parent object, not the item.
        // ChildRules is required to access individual Answer DTO properties.
        RuleForEach(q => q.Answers).ChildRules(a =>
        {
            a.RuleFor(a => a.Id)
                .Must((dto, id, context) => {
                    var question = context.RootContextData["Question"] as Question 
                    ?? throw new Exception("Missing Question context");

                    return question.Answers.Exists(db => db.Id == new ObjectId(id));
                })
                .WithMessage(a => $"Answer with id {a.Id} not found in existing question");
        }).When(q => q.Answers != null);
        RuleFor(q => q.Answers)
            .NotEmpty()
            .WithMessage("Answers field can not be empty, must contain an array of updated answers or be null")
            .When(q => q.Answers != null);
        RuleFor(q => q.Answers)
            .Must((dto, answerDTOs, context) =>
            {
                var question = context.RootContextData["Question"] as Question
                ?? throw new Exception("Missing Question context");

                if(question.Type == QuestionType.singleChoice)
                {
                    return answerDTOs.Any(a => a.IsCorrect);
                }
                else if(question.Type == QuestionType.multipleChoice)
                {
                    return answerDTOs.Count(a => a.IsCorrect) > 1;
                }

                return false;
            })
            .WithMessage("Wrong amount of correct answers, check question type")
            .When(q => q.Answers != null);
    }
}