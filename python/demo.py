"""A demo: an inventory weekend at the library, with a check written outside the library's code.
You don't need to change it."""
from fakes import FakeClock
from pipeline import Log, Pipeline, Request, Response, endpoints, log_requests, sign_in, timing

clock = FakeClock()
log = Log()


def closed_for_inventory(request, next):
    """The security team's check, written in their own code."""
    if request.method == "GET" and request.path == "/books":
        return next(request)
    return Response(503, "Closed for inventory")


def slow_endpoints(request):
    clock.advance(12)
    return endpoints(request)


pipeline = (Pipeline(slow_endpoints)
            .use(log_requests(log))
            .use(sign_in())
            .use(closed_for_inventory)
            .use(timing(clock, log)))

for request in [Request("GET", "/books", ""), Request("POST", "/loans", ""), Request("POST", "/loans", "Maya")]:
    response = pipeline.handle(request)
    print(f"{request.method} {request.path}: {response.status} {response.body}")
print("The log:")
for line in log.lines:
    print("  " + line)
