class Constants:
    MIN_TITLE_LEN = 4
    MAX_TITLE_LEN = 32

    __slots__ = ()

    @staticmethod
    def get_dict():
        return {"MIN_TITLE_LEN": Constants.MIN_TITLE_LEN,
                "MAX_TITLE_LEN": Constants.MAX_TITLE_LEN}

