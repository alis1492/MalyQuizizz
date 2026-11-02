using quizShared.DTOs;

namespace quizUi.Services;

public class QuizApiService
{
    private readonly HttpClient http; // http client we use to call api

    // Quiz related stuff
    public Task<List<QuizDTO>?> GetQuizzesAsync() =>
        http.GetFromJsonAsync<List<QuizDTO>>("quiz");

    public Task<QuizDTO?> GetQuizAsync(string id) =>
        http.GetFromJsonAsync<QuizDTO>($"quiz/{id}");

    public Task<HttpResponseMessage> CreateSubmissionAsync(CreateSubmissionDTO dto) =>
        http.PostAsJsonAsync("submission", dto);

    public Task<HttpResponseMessage> SubmitAnswersAsync(string submissionId, List<SubmitUserAnswerDTO> answers) =>
        http.PostAsJsonAsync($"submission/{submissionId}", answers);
    
    public QuizApiService(IHttpClientFactory factory)
    {
        http = factory.CreateClient("QuizApiClient");
    }

    public async Task<string?> CreateQuizAsync(CreateQuizDTO dto)
    {
        var response = await http.PostAsJsonAsync("quiz", dto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateQuizResponse>();
        return result?.QuizId;
    }

    public async Task AddQuestionAsync(string quizId, CreateQuestionDTO dto)
    {
        var response = await http.PostAsJsonAsync($"quiz/{quizId}/question", dto);
        response.EnsureSuccessStatusCode();
    }

    private record CreateQuizResponse(string QuizId);

    public async Task DeleteQuizAsync(string quizId)
    {
        var response = await http.DeleteAsync($"quiz/{quizId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateQuizAsync(string id, UpdateQuizDTO dto)
    {
        var response = await http.PatchAsJsonAsync($"quiz/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateQuestionAsync(string quizId, string questionId, UpdateQuestionDTO dto)
    {
        var response = await http.PatchAsJsonAsync($"quiz/{quizId}/question/{questionId}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteQuestionAsync(string quizId, string questionId)
    {
        var response = await http.DeleteAsync($"quiz/{quizId}/question/{questionId}");
        response.EnsureSuccessStatusCode();
    }

    public Task<FullQuizDTO?> GetFullQuizAsync(string id) =>
        http.GetFromJsonAsync<FullQuizDTO>($"full/quiz/{id}");

    public Task<List<FullQuizDTO>?> GetFullQuizzesAsync(int? limit = null, int? skip = null)
    {
        var query = new List<string>();
        if (limit is not null) query.Add($"limit={limit}");
        if (skip is not null) query.Add($"skip={skip}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return http.GetFromJsonAsync<List<FullQuizDTO>>($"full/quiz{qs}");
    }
    // End of quiz related stuff
}