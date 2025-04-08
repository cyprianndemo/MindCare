namespace MindCare.Services
{
    public interface IUserService
    {
        Task<string> GetUserMoodIcon(string userId);
        Task<IEnumerable<string>> GetUserConnectionIds(string userId);
        Task AddUserConnection(string userId, string connectionId);
        Task RemoveUserConnection(string userId, string connectionId);
    }
}
