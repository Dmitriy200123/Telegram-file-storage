import dataclasses
import urllib.parse
from typing import List

import config
from clients.base_client import BaseClient


@dataclasses.dataclass
class DocumentsSearchClient(BaseClient):
    base_url: str = config.DOCUMENTS_API_URL
    search_url = base_url + '/search'

    async def contains_in_name(self, document_id: str, queries: List[str]):
        query = urllib.parse.urlencode({'queries': param for param in queries})

        return await self.get(path=f'{self.search_url}/documents/{document_id}/containsInName?{query}')