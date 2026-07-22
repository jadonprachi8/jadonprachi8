namespace StudentManagementSystem.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(string username);
    }
}
