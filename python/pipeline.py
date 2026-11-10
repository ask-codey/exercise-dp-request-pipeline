"""Riverside Community Library's web requests, without a framework. Read the ticket in README.md first.

A handler is a function that answers a request: handler(request) gives a Response.
A middleware is a step in front of the endpoints: middleware(request, next) is handed the request and next,
the rest of the line, and either answers the request itself or returns next(request)."""
from dataclasses import dataclass


@dataclass
class Request:
    """One web request: what's asked for, and who's asking ("" when nobody is signed in)."""

    method: str
    path: str
    user: str


@dataclass
class Response:
    status: int
    body: str


class Log:
    """The lines the API writes about each request, in order."""

    def __init__(self):
        self.lines = []

    def add(self, line):
        self.lines.append(line)


def endpoints(request):
    """The library's endpoints: their own work, and no checks."""
    match f"{request.method} {request.path}":
        case "GET /books":
            return Response(200, "The Lighthouse Keeper, A Map of Small Things, Rivers of Glass")
        case "POST /loans":
            return Response(201, f"Loan 1: {request.user}, The Lighthouse Keeper")
        case _:
            return Response(404, "No such page")


class Pipeline:
    """The platform team's pipeline: the endpoint at the end, and middleware in front of it.
    Other teams already call it. It doesn't run any middleware yet."""

    def __init__(self, endpoint):
        self._endpoint = endpoint

    def use(self, step):
        """Adds a step. The first step added is the first a request meets."""
        return self

    def handle(self, request):
        return self._endpoint(request)


# Today's checks, as the platform team wants them: one piece each. They're named, but they don't do anything yet.

def log_requests(log):
    """Writes "GET /books" on the way in, and "GET /books: 200" on the way back out."""
    return lambda request, next: next(request)


def sign_in():
    """Answers 401 "Please sign in" to anything but a GET from someone not signed in."""
    return lambda request, next: next(request)


def timing(clock, log):
    """Writes "GET /books took 30 ms": how long the rest of the line took, by the clock it's handed."""
    return lambda request, next: next(request)


def handle(request, log, clock):
    """What the web server calls for every request. Every check is written out in here, in order."""
    what = f"{request.method} {request.path}"
    log.add(what)
    if request.method != "GET" and request.user == "":
        response = Response(401, "Please sign in")
    else:
        start = clock.now
        response = endpoints(request)
        log.add(f"{what} took {clock.now - start} ms")
    log.add(f"{what}: {response.status}")
    return response
