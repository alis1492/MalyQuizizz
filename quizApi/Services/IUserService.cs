using MongoDB.Bson;
using quizApi.Models;
using quizShared;
using quizShared.DTOs;

namespace quizApi.Services;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(ObjectId id);
    Task<User> CreateAsync(RegisterUserDTO dto);
    Task<List<UserDTO>> GetAllAsync(int? limit, int? skip);
}