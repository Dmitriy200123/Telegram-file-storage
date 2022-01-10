from uuid import UUID

from common.repository.base_repository import BaseRepository
from postgres.models.db_models import SenderToChat


class SenderToChatRepository(BaseRepository):

    async def create_or_get(self, sender_id: UUID, chat_id: UUID) -> SenderToChat:
        sender_to_chat_tuple: (SenderToChat, bool) = await self.adapter.get_or_create(
            model=SenderToChat,
            SenderId=sender_id,
            ChatId=chat_id
        )

        return sender_to_chat_tuple[0]

    async def find_by_sender_and_chat_ids(self, sender_id: UUID, chat_id: UUID) -> SenderToChat:
        return await self.adapter.get(model=SenderToChat, SenderId=sender_id, ChatId=chat_id)

    @staticmethod
    async def delete(sender_to_chat: SenderToChat):
        sender_to_chat.delete_instance()
