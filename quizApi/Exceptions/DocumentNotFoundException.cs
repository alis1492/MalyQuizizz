namespace quizApi.Exceptions;

public class DocumentNotFoundException : Exception
{
    public override string Message { get; } = "Document Not Found.";

    /// <summary>
    /// Create a new NotFound Exception "Document Not Found."
    /// </summary>
    public DocumentNotFoundException() {}
    /// <summary>
    /// Create a new NotFound Exception with specified id
    /// "Document with id {id} Not Found."
    /// </summary>
    /// <param name="id"></param>
    public DocumentNotFoundException(string id)
    {
        Message = $"Document with id {id} Not Found";
    }
    /// <summary>
    /// Create a new NotFound Exception with specified document name and id
    /// "{docName} with id {id} Not Found."
    /// </summary>
    /// <param name="docName"></param>
    /// <param name="id"></param>
    public DocumentNotFoundException(string docName, string id)
    {
        Message = $"{docName} with id {id} Not Found";
    }
}