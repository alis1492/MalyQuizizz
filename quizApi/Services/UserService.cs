using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using quizApi.Models;
using quizShared;
using quizShared.DTOs;
using static BCrypt.Net.BCrypt;

namespace quizApi.Services;

public class UserService(
    IMongoCollection<User> collection,
    IMapper mapper
) : IUserService
{
    private readonly IMongoCollection<User> _users = collection;
    private readonly IMapper _mapper = mapper;

    public Task<User?> GetByEmailAsync(string email) =>
        _users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync()!;

    public Task<User?> GetByIdAsync(ObjectId id) =>
        _users.Find(u => u.Id == id).FirstOrDefaultAsync()!;

    public async Task<User> CreateAsync(RegisterUserDTO dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = HashPassword(dto.Password, workFactor: 12),
            UserType = (UserType)dto.UserType
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<List<UserDTO>> GetAllAsync(int? limit, int? skip)
    {
        var query = _users.Find(FilterDefinition<User>.Empty)
            .SortBy(u => u.CreatedAtUtc)
            .Skip(skip)
            .Limit(limit);

        var users = await query.ToListAsync();
        return _mapper.Map<List<UserDTO>>(users);
    }
}