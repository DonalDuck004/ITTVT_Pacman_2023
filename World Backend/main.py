
from sys import platform

from starlette.responses import JSONResponse
from uvicorn import run
from fastapi import FastAPI
from modules import DatabaseHandler, World, Search
from modules.exceptions import ApiError, WorldTitleError
from modules.constants import Constants
from typing import Generic

if platform == "win32":
    from asyncio import set_event_loop_policy
    from asyncio import WindowsSelectorEventLoopPolicy

    set_event_loop_policy(WindowsSelectorEventLoopPolicy())

app = FastAPI()


def validate_request(T):
    def _wp(fn):
        async def wp(param: T):
            if isinstance(param, World):
                if not Constants.MIN_TITLE_LEN < len(param.title) < Constants.MAX_TITLE_LEN or param.title.isspace():
                    raise WorldTitleError(param.title)
            # todo ...
            return await fn(param)

        return wp
    return _wp


@app.post("/add_world")
# @validate_request(World)
async def add_world(world: World):
    dbObj = DatabaseHandler()
    await dbObj.RegisterWorld(world)
    return {"ok": True}


@app.post("/search_worlds")
# @validate_request(Search)
async def search_worlds(search: Search):
    dbObj = DatabaseHandler()
    return {"ok": True, "result": await dbObj.SearchWorld(search)}


@app.get("/get_world")
# @validate_request(Search)
async def get_world(world_id: str):
    dbObj = DatabaseHandler()
    return {"ok": True, "result": await dbObj.GetWorld(world_id)}


@app.post("/get_random_worlds")
async def get_random_worlds():
    dbObj = DatabaseHandler()
    return {"ok": True, "result": await dbObj.GetRandomWorlds()}


@app.get("/get_counts")
async def get_counts():
    dbObj = DatabaseHandler()
    return {"ok": True, "result": await dbObj.GetCount()}


@app.get("/get_constants")
async def get_constants():
    return {"ok": True, "result": Constants.get_dict()}


@app.exception_handler(ApiError)
async def exc_handler(request, exception: ApiError):
    return JSONResponse(exception.detail, status_code=exception.status_code)


if __name__ == '__main__':
    run(app)
