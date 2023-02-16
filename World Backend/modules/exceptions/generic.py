from fastapi import HTTPException


class ApiError(Exception):
    def __init__(self, code: int = 400, reason: str = "Generic api error"):
        self.status_code = code
        self.detail = {"ok": False, "reason": reason}
