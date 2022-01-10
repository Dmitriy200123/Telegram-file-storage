from common.utils import now_utc
from postgres.models.db_models import Chat, File, FileSender, FileTypeEnum
from postgres.models.db_models.marked_text_tags import MarkedTextTags
from postgres.models.external_models import Chat as chat_ext_model


async def create_chat(image_id=None, telegram_id=None, chat_name=None) -> Chat:
    chat_model = chat_ext_model(
        name=chat_name or 'test',
        telegram_id=telegram_id or 123,
        image_id=image_id
    )
    chat = await Chat.create(**chat_model.dict(by_alias=True))
    return chat


async def create_sender(name=None, telegram_id=None, telegram_username=None) -> FileSender:
    sender = await FileSender.create(
        TelegramId=telegram_id or 123,
        TelegramUserName=telegram_username or 'tele user name',
        FullName=name or 'test name',
    )
    return sender


async def create_file(
    sender: FileSender,
    chat: Chat,
    filename=None,
    extension=None,
    typeId=None,
    upload_date=None,
):
    file = await File.create(
        Name=filename or 'my filename',
        Extension=extension or 'file extension',
        TypeId=typeId or FileTypeEnum.Document,
        UploadDate=upload_date or now_utc(),
        FileSenderId=sender.Id,
        ChatId=chat.Id,
    )
    return file


async def create_marked_text_tags(title_tag: str = None, description_tag: str = None):
    marked_text_tags = await MarkedTextTags.create(
        TitleTag=title_tag or '#',
        DescriptionTag=description_tag or '*'
    )
    return marked_text_tags
