using AutoMapper;
using FileStorageAPI.Models;
using ChatInDb = FileStorageApp.Data.InfoStorage.Models.Chat;

namespace FileStorageAPI.Converters
{
    public class ChatConverter : IChatConverter
    {
        private readonly IMapper _chatMapper = new MapperConfiguration(cfg => cfg.CreateMap<ChatInDb, Chat>())
            .CreateMapper();

        public Chat ConvertToChatInApi(ChatInDb chat) => _chatMapper.Map<Chat>(chat);
    }
}