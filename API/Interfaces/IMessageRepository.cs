using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMesssage(Message message);
    Task<Message> GetMessage(int id);
    Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams);
    Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName);
    Task<bool> SaveAllAsync();
    void AddGroup(Group group);
    void RemoveConnnection(Connection connection);
    Task<Connection> GetConnnection(string connectionId);
    Task<Group> GetMessageGroup(string groupName);
}
