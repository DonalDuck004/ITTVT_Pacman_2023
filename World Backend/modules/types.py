from pydantic import BaseModel
from typing import List


class World(BaseModel):
    title: str
    byte_map: bytes
    preview: bytes
    tags: List[str]


class Search(BaseModel):
    q: str
    tags: List[str]
    offset: int
