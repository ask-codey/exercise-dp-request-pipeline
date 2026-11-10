"""The tests for your code. You don't need to change this file.
Each message says what was expected, so a failing test tells you what to look at."""
from fakes import FakeClock
from pipeline import Log, Pipeline, Request, Response, endpoints, handle, log_requests, sign_in, timing

LIST_BOOKS = Request("GET", "/books", "")
STRANGER_BORROWS = Request("POST", "/loans", "")
MAYA_BORROWS = Request("POST", "/loans", "Maya")


# What the API does today. These pass before you change anything: keep them passing.

def test_anyone_can_list_the_books():
    response = handle(LIST_BOOKS, Log(), FakeClock())
    assert f"{response.status} {response.body}" == "200 The Lighthouse Keeper, A Map of Small Things, Rivers of Glass", \
        "Keep this working: GET /books answers 200 with the three titles."


def test_borrowing_needs_someone_signed_in():
    log = Log()
    response = handle(STRANGER_BORROWS, log, FakeClock())
    assert f"{response.status} {response.body}" == "401 Please sign in", \
        "Keep this working: POST /loans from someone not signed in answers 401 Please sign in."
    assert log.lines == ["POST /loans", "POST /loans: 401"], \
        "Keep this working: a refused request is logged on the way in and on the way out, and never timed."


def test_maya_can_borrow():
    response = handle(MAYA_BORROWS, Log(), FakeClock())
    assert f"{response.status} {response.body}" == "201 Loan 1: Maya, The Lighthouse Keeper", \
        "Keep this working: POST /loans from Maya answers 201 with her loan."


def test_each_request_is_logged_and_timed():
    log = Log()
    handle(LIST_BOOKS, log, FakeClock())
    assert log.lines == ["GET /books", "GET /books took 0 ms", "GET /books: 200"], \
        "Keep this working: GET /books writes its line, its time, then its status."


# The ticket's new requirements. These fail until the work is done.

def test_the_logging_step_works_on_its_own():
    log = Log()
    Pipeline(endpoints).use(log_requests(log)).handle(LIST_BOOKS)
    assert log.lines == ["GET /books", "GET /books: 200"], \
        "Not yet: a pipeline with only log_requests writes the request on the way in and its status on the way out."


def test_the_sign_in_step_stops_a_stranger_before_the_endpoint():
    reached = []

    def endpoint(request):
        reached.append(request)
        return endpoints(request)

    response = Pipeline(endpoint).use(sign_in()).handle(STRANGER_BORROWS)
    assert response.status == 401, "Not yet: a pipeline with sign_in answers 401 to POST /loans from someone not signed in."
    assert reached == [], "Not yet: the sign-in step answers by itself, so the endpoint never runs for a stranger."


def test_the_timing_step_times_the_rest_of_the_line():
    clock = FakeClock()
    log = Log()

    def slow_endpoint(request):
        clock.advance(30)
        return endpoints(request)

    Pipeline(slow_endpoint).use(timing(clock, log)).handle(LIST_BOOKS)
    assert log.lines == ["GET /books took 30 ms"], \
        "Not yet: the endpoint took 30 ms on the test's clock, so timing writes GET /books took 30 ms."


def test_a_new_check_goes_in_the_middle_without_editing_the_others():
    clock = FakeClock()
    log = Log()

    def closed_for_inventory(request, next):
        return next(request) if request.method == "GET" else Response(503, "Closed for inventory")

    pipeline = (Pipeline(endpoints).use(log_requests(log)).use(sign_in())
                .use(closed_for_inventory).use(timing(clock, log)))
    refused = pipeline.handle(MAYA_BORROWS)
    books = pipeline.handle(LIST_BOOKS)
    assert f"{refused.status} {books.status}" == "503 200", \
        "Not yet: with the inventory check in the line, Maya's POST /loans answers 503 and GET /books still answers 200."
    assert log.lines == ["POST /loans", "POST /loans: 503", "GET /books", "GET /books took 0 ms", "GET /books: 200"], \
        "Not yet: the steps run in the order they were added, with the new check between sign-in and timing."
