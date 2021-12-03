import datetime
import uuid
from typing import Optional

from postgres.models.db_models import FileTypeEnum
from pydantic import BaseModel, Field

type_map = {
    'document': FileTypeEnum.Document,
    'image': FileTypeEnum.Image,
    'video': FileTypeEnum.Video,
    'audio': FileTypeEnum.Audio
}


class File(BaseModel):
    name: str = Field(..., title='Название файла',
                      max_length=255, alias='Name')
    extension: Optional[str] = Field(title='Расширение',
                                     max_length=255, alias='Extension')
    type: FileTypeEnum = Field(..., title='Тип', alias='TypeId')
    upload_date: datetime.datetime = Field(...,
                                           title='Дата загрузки', alias='UploadDate')
    sender_telegram_id: int
    chat_telegram_id: int
    # file_sender_id: uuid.UUID = Field(...,
    #                                   title='Телеграм id отправителя', alias='FileSenderId')
    # chat_id: Optional[uuid.UUID] = Field(
        title='Телеграм id чата', alias='ChatId')

    class Config:
        allow_population_by_field_name = True
