import datetime
import uuid

from postgres.basic import manager
from postgres.models.db_models import Chat, File, FileSender, FileTypeEnum
from postgres.models.external_models import File as FileExternal
from tests.test_postgres_models.conftest import (
    create_chat,
    create_marked_text_tags,
    create_sender,
)


async def test_create_chat(db_manager):
    chat, is_created = await db_manager.get_or_create(
        model=Chat, Name='test name', ImageId=uuid.uuid4(),
        TelegramId=123
    )

    result = list(await manager.execute(Chat.select()))

    assert chat in result


async def test_contains_chat(db_manager):
    chat, is_created = await db_manager.get_or_create(
        model=Chat, Name='test name', ImageId=uuid.uuid4(),
        TelegramId=122223
    )
    await create_chat(chat_name='mysupername')

    result = await db_manager.contains(Chat, TelegramId=chat.TelegramId)

    assert result


async def test_false_contains_chat(db_manager):
    await db_manager.get_or_create(model=Chat, Name='test name', ImageId=uuid.uuid4(), TelegramId=122223)
    result = await db_manager.contains(Chat, TelegramId=000000000)

    assert not result


async def test_find_chat_by_telegram_id(db_manager):
    own_id = 228
    await create_chat(telegram_id=123)
    await create_chat(telegram_id=own_id)

    chat: Chat = await db_manager.get(model=Chat, TelegramId=own_id)

    assert chat.TelegramId == own_id


async def test_update_chat_information(db_manager):
    chat = await create_chat()
    chat_data_old = chat.as_dict()

    new_name = 'new name here'
    result = await db_manager.update(chat, Name=new_name)

    assert result

    result = await Chat.get(Id=chat.Id)
    new_chat_data = result.as_dict()
    assert new_chat_data != chat_data_old
    assert new_chat_data['Name'] == new_name


async def test_get_all_chats(db_manager):
    chat1 = await create_chat(telegram_id=33212)
    chat2 = await create_chat(chat_name='abcd')

    result = await db_manager.get_all(Chat)

    assert chat1 in result
    assert chat2 in result


async def test_create_sender(db_manager):
    sender, is_created = await db_manager.get_or_create(
        model=FileSender, TelegramId=123, TelegramUserName='my name',
        FullName='full name'
    )

    result = list(await manager.execute(FileSender.select()))

    assert sender in result


async def test_contains_sender(db_manager):
    sender, is_created = await db_manager.get_or_create(
        model=FileSender, TelegramId=1233, TelegramUserName='my name',
        FullName='full name'
    )
    await create_sender(name='mysupername')

    result = await db_manager.contains(FileSender, TelegramId=sender.TelegramId)

    assert result


async def test_false_contains_sender(db_manager):
    await db_manager.get_or_create(model=FileSender, TelegramId=123, TelegramUserName='my name', FullName='full name')
    result = await db_manager.contains(FileSender, TelegramId=0000)

    assert not result


async def test_find_sender_by_telegram_id(db_manager):
    own_id = 228
    await create_sender(telegram_id=123)
    await create_sender(telegram_id=own_id)

    chat: FileSender = await db_manager.get(model=FileSender, TelegramId=own_id)

    assert chat.TelegramId == own_id


async def test_update_sender_information(db_manager):
    sender = await create_sender()
    chat_data_old = sender.as_dict()

    new_name = 'new name here'
    result = await db_manager.update(sender, FullName=new_name)

    assert result

    result = await FileSender.get(FullName=FileSender.FullName)
    new_chat_data = result.as_dict()
    assert new_chat_data != chat_data_old
    assert new_chat_data['FullName'] == new_name


async def test_get_all_senders(db_manager):
    sender1 = await create_sender()
    sender2 = await create_sender(name='abcd', telegram_id=1111)

    result = await db_manager.get_all(FileSender)

    assert sender1 in result
    assert sender2 in result


async def test_create_file(db_manager):
    chat = await create_chat()
    sender = await create_sender()
    file, is_created = await db_manager.get_or_create(
        model=File,
        Name='name',
        Extension='extension',
        TypeId=FileTypeEnum.Document,
        UploadDate=datetime.datetime.now(),
        FileSenderId=sender.Id,
        ChatId=chat.Id,
    )

    result = list(await manager.execute(File.select()))

    assert file in result


async def test_create_existing_chat(db_manager):
    chat: Chat = await create_chat()
    new_chat, is_created = await db_manager.get_or_create(Chat, **chat.as_dict())

    assert not is_created
    assert new_chat.Id == chat.Id


async def test_update_chat(db_manager):
    chat = await create_chat()
    chat.TelegramId = 55555
    chat_old = await db_manager.get(Chat, Id=chat.Id)

    await db_manager.update(chat_old, **chat.as_dict())

    chat_update = await db_manager.get(chat_old, Id=chat.Id)

    assert chat_update.TelegramId == 55555


async def test_create_chat_without_image(db_manager):
    chat, is_created = await db_manager.get_or_create(model=Chat, TelegramId=15514, Name='test', ImageId=None)

    assert is_created
    assert chat


async def test_create_two_unique_file(db_manager):
    sender = await create_sender()
    chat = await create_chat(telegram_id=-400)

    first_file_external = FileExternal(
        name='photo1212',
        extension='jpg',
        type=FileTypeEnum.Image,
        upload_date=datetime.datetime.now(),
        sender_telegram_id=sender.TelegramId,
        chat_telegram_id=chat.TelegramId
    )
    second_file_external = FileExternal(
        name='YouTube',
        type=FileTypeEnum.Link,
        sender_telegram_id=sender.TelegramId,
        chat_telegram_id=chat.TelegramId
    )

    first_file, is_created_first = await db_manager.get_or_create(
        model=File,
        ChatId=chat.Id,
        FileSenderId=sender.Id,
        **first_file_external.dict(by_alias=True, exclude_none=True, exclude={'sender_telegram_id', 'chat_telegram_id'})
    )
    second_file, is_created_second = await db_manager.get_or_create(
        model=File,
        ChatId=chat.Id,
        FileSenderId=sender.Id,

        **second_file_external.dict(
            by_alias=True, exclude_none=True,
            exclude={'sender_telegram_id', 'chat_telegram_id'}
        )
    )
    assert is_created_first
    assert is_created_second

    assert first_file.Id != second_file.Id


async def test_create_marked_text_tags(db_manager):
    title_tag = '<t>'
    description_tag = '<d>'

    marked_text_tags = await create_marked_text_tags(title_tag, description_tag)

    assert marked_text_tags
    assert marked_text_tags.TitleTag == title_tag
    assert marked_text_tags.DescriptionTag == description_tag
