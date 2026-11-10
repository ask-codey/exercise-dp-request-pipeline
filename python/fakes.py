"""Written by the test team, for the tests and the demo: a clock that only moves when it's told to."""


class FakeClock:
    def __init__(self):
        self.now = 0

    def advance(self, milliseconds):
        self.now += milliseconds
