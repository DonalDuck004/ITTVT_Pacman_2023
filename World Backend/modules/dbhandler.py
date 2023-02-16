from itertools import chain
from typing import List, TYPE_CHECKING
from psycopg import connect as pg_connect, AsyncConnection
from psycopg.errors import UniqueViolation
from hashlib import md5

from .exceptions import WorldAlreadyExists

if TYPE_CHECKING:
    from .types import World, Search
    from psycopg.cursor_async import AsyncCursor


class DatabaseHandler(object):
    USER = "postgres"
    PASSWORD = "D0D04W4sH3re"
    PORT = 5432
    HOST = "192.168.1.65"
    DB_NAME = "ittvt_pacman"

    __slots__ = "_conn",

    def __init__(self):
        conn = pg_connect(user=DatabaseHandler.USER,
                          host=DatabaseHandler.HOST,
                          password=DatabaseHandler.PASSWORD,
                          port=DatabaseHandler.PORT,
                          dbname=DatabaseHandler.DB_NAME)
        conn.autocommit = False
        self._conn = AsyncConnection(conn.pgconn)

    async def RegisterWorld(self,
                            world: "World"):
        sql = "INSERT INTO Worlds(md5, World, Title, ImgPreview) VALUES (%s, %s, %s, %s)"
        await self._conn.commit()
        cur = self._conn.cursor()
        world_id = md5(world.byte_map).hexdigest()

        try:
            await cur.execute(sql, (world_id,
                                    world.byte_map,
                                    world.title,
                                    world.preview))
        except UniqueViolation:
            raise WorldAlreadyExists()

        if world.tags:
            await self.AddTags(world_id, world.tags, cur)

        await self._conn.commit()

    async def AddTags(self,
                      world_id: str,
                      tags: List[str],
                      cur: "AsyncCursor" = None) -> None:
        # if len(tags) > 10: err
        cur = cur or self._conn.cursor()
        binding = ("(%s)," * len(tags))[:-1]
        sql = f"INSERT INTO tags (name) VALUES {binding} ON CONFLICT DO NOTHING"
        await cur.execute(sql, tags)

        sql = "SELECT ID FROM Tags WHERE name = ANY(%s)"
        await cur.execute(sql, (tags,))
        ids = await cur.fetchall()

        binding = (f"('{world_id}', %s)," * len(tags))[:-1]
        sql = f"INSERT INTO WorldsTags(refertoworld, refertotag) VALUES {binding}"
        await cur.execute(sql, list(chain(*ids)))
        await self._conn.commit()

    async def SearchWorld(self, search: "Search"):
        sql = """SELECT md5,
                        title,
                        ImgPreview, 
                        t.Name
                    FROM Worlds
                         LEFT JOIN WorldsTags as l ON worlds.md5 = l.refertoworld
                         LEFT JOIN tags AS t ON t.id = l.refertotag
                    WHERE lower(title) LIKE '%%'||%s||'%%'"""

        if search.tags:
            sql += " AND t.name = ANY(%s)"

        sql += f" LIMIT 10 OFFSET {search.offset}"
        cur = self._conn.cursor()
        await cur.execute(sql, (search.q.lower(), search.tags) if search.tags else (search.q.lower(), ))
        result = {}
        for world_id, title, ImgPreview, tag in await cur.fetchall():
            result.setdefault(world_id, {"preview": ImgPreview, "title": title, "tags": []})["tags"].append(tag)
        print(result, search.offset, search)
        return [{"world_id": k, **v} for k, v in result.items()]

    async def GetCount(self):
        sql = "SELECT COUNT(1) FROM worlds"
        cur = self._conn.cursor()
        await cur.execute(sql)
        return await cur.fetchone()

    async def GetRandomWorlds(self):
        sql = """SELECT md5,
                        title,
                        ImgPreview, 
                        t.Name
                    FROM Worlds
                         LEFT JOIN WorldsTags as l ON worlds.md5 = l.refertoworld
                         LEFT JOIN tags AS t on t.id = l.refertotag
                    ORDER BY random() LIMIT 10"""
        cur = self._conn.cursor()
        await cur.execute(sql)
        result = {}
        for world_id, title, ImgPreview, tag in await cur.fetchall():
            result.setdefault(world_id, {"preview": ImgPreview, "title": title, "tags": []})["tags"].append(tag)

        return [{"world_id": k, **v} for k, v in result.items()]

    async def GetWorld(self, world_id: str) -> bytes:
        sql = "SELECT world FROM worlds WHERE md5 = %s"
        cur = self._conn.cursor()
        await cur.execute(sql, (world_id,))
        return (await cur.fetchone())[0]
