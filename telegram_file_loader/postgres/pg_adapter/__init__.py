from typing import Any, List

from peewee import DoesNotExist
from peewee_async import Manager
from postgres.basic import BaseModel


class Adapter:
    manager: Manager

    def __init__(self, db_manager: Manager):
        self.manager = db_manager

    async def create(self, model: BaseModel, **params) -> BaseModel:
        async with self.manager.atomic():
            return await self.manager.create(model, **params)

    async def get_or_create(self, model: BaseModel, **params) -> (BaseModel, bool):
        async with self.manager.atomic():
            return await self.manager.get_or_create(model, **params)

    async def get(self, model: BaseModel, **params) -> Any:
        try:
            return await self.manager.get(model, **params)
        except DoesNotExist:
            return None

    async def get_all(self, model: BaseModel) -> List[BaseModel]:
        query = model.select().order_by(model.Id)
        result = await self.manager.execute(query)

        return list(result)

    async def contains(self, model: BaseModel, **params) -> bool:
        result = list(await self.manager.execute(model.select().filter(**params)))

        return len(result) != 0

    async def update(self, model: BaseModel, **new_params):
        new_param = {fld: val for fld, val in new_params.items()
                     if fld in model._meta.fields}

        is_changed = model.is_changed(**new_param)
        if is_changed:
            for fld, value in new_param.items():
                setattr(model, fld, value)
            await self.manager.update(model)

        return is_changed

    async def first(self, model: BaseModel) -> Any:
        query = model.select()
        result = await self.manager.execute(query)

        if not result:
            return None

        return result[0]
