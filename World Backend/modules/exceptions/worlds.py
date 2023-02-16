from .generic import ApiError


class WorldError(ApiError):
    def __init__(self):
        super().__init__(reason="Generic world error")


class WorldAlreadyExists(ApiError):
    def __init__(self):
        super().__init__(reason="A world with the same map already exists")


class WorldFieldError(ApiError):
    def __init__(self, value: str, field: str = None):
        if len(value) > 16:
            value = value[:13] + "..."

        super().__init__(reason=f"{value!r} is not valid {field or 'field'}")


class WorldTitleError(WorldFieldError):
    def __init__(self, value: str):
        super().__init__(value=value, field="title")


__all__ = "WorldError", "WorldAlreadyExists", "WorldFieldError", "WorldTitleError"
