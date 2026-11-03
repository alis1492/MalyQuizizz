namespace quizUi.Services;

public class TokenService
{
    public string? Token { get; private set; }
    public string? UserId { get; private set; }
    public string? Role { get; private set; }

    public void SetToken(string token, string userId, string role)
    {
        Token = token;
        UserId = userId;
        Role = role;
    }

    public void Clear()
    {
        Token = null;
        UserId = null;
        Role = null;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}