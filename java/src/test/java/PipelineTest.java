import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;

import java.util.List;
import org.junit.jupiter.api.Test;

// The tests for your code. You don't need to change this file.
// Each message says what was expected, so a failing test tells you what to look at.
class PipelineTest {
    private static final Request LIST_BOOKS = new Request("GET", "/books", "");
    private static final Request STRANGER_BORROWS = new Request("POST", "/loans", "");
    private static final Request MAYA_BORROWS = new Request("POST", "/loans", "Maya");

    // What the API does today. These pass before you change anything: keep them passing.

    @Test
    void anyoneCanListTheBooks() {
        Response response = App.handle(LIST_BOOKS, new Log(), new FakeClock());
        assertEquals("200 The Lighthouse Keeper, A Map of Small Things, Rivers of Glass", response.status() + " " + response.body(),
            "Keep this working: GET /books answers 200 with the three titles.");
    }

    @Test
    void borrowingNeedsSomeoneSignedIn() {
        Log log = new Log();
        Response response = App.handle(STRANGER_BORROWS, log, new FakeClock());
        assertEquals("401 Please sign in", response.status() + " " + response.body(),
            "Keep this working: POST /loans from someone not signed in answers 401 Please sign in.");
        assertEquals(List.of("POST /loans", "POST /loans: 401"), log.lines,
            "Keep this working: a refused request is logged on the way in and on the way out, and never timed.");
    }

    @Test
    void mayaCanBorrow() {
        Response response = App.handle(MAYA_BORROWS, new Log(), new FakeClock());
        assertEquals("201 Loan 1: Maya, The Lighthouse Keeper", response.status() + " " + response.body(),
            "Keep this working: POST /loans from Maya answers 201 with her loan.");
    }

    @Test
    void eachRequestIsLoggedAndTimed() {
        Log log = new Log();
        App.handle(LIST_BOOKS, log, new FakeClock());
        assertEquals(List.of("GET /books", "GET /books took 0 ms", "GET /books: 200"), log.lines,
            "Keep this working: GET /books writes its line, its time, then its status.");
    }

    // The ticket's new requirements. These fail until the work is done.

    @Test
    void theLoggingStepWorksOnItsOwn() {
        Log log = new Log();
        new Pipeline(Endpoints::handle).use(Checks.logRequests(log)).handle(LIST_BOOKS);
        assertEquals(List.of("GET /books", "GET /books: 200"), log.lines,
            "Not yet: a pipeline with only Checks.logRequests writes the request on the way in and its status on the way out.");
    }

    @Test
    void theSignInStepStopsAStrangerBeforeTheEndpoint() {
        boolean[] reached = { false };
        Pipeline pipeline = new Pipeline(request -> { reached[0] = true; return Endpoints.handle(request); }).use(Checks.signIn());
        Response response = pipeline.handle(STRANGER_BORROWS);
        assertEquals(401, response.status(), "Not yet: a pipeline with Checks.signIn answers 401 to POST /loans from someone not signed in.");
        assertFalse(reached[0], "Not yet: the sign-in step answers by itself, so the endpoint never runs for a stranger.");
    }

    @Test
    void theTimingStepTimesTheRestOfTheLine() {
        FakeClock clock = new FakeClock();
        Log log = new Log();
        new Pipeline(request -> { clock.advance(30); return Endpoints.handle(request); }).use(Checks.timing(clock, log)).handle(LIST_BOOKS);
        assertEquals(List.of("GET /books took 30 ms"), log.lines,
            "Not yet: the endpoint took 30 ms on the test's clock, so Checks.timing writes GET /books took 30 ms.");
    }

    @Test
    void aNewCheckGoesInTheMiddleWithoutEditingTheOthers() {
        FakeClock clock = new FakeClock();
        Log log = new Log();
        Middleware closedForInventory = (request, next) ->
            request.method().equals("GET") ? next.handle(request) : new Response(503, "Closed for inventory");
        Pipeline pipeline = new Pipeline(Endpoints::handle)
            .use(Checks.logRequests(log))
            .use(Checks.signIn())
            .use(closedForInventory)
            .use(Checks.timing(clock, log));
        Response refused = pipeline.handle(MAYA_BORROWS);
        Response books = pipeline.handle(LIST_BOOKS);
        assertEquals("503 200", refused.status() + " " + books.status(),
            "Not yet: with the inventory check in the line, Maya's POST /loans answers 503 and GET /books still answers 200.");
        assertEquals(List.of("POST /loans", "POST /loans: 503", "GET /books", "GET /books took 0 ms", "GET /books: 200"), log.lines,
            "Not yet: the steps run in the order they were added, with the new check between sign-in and timing.");
    }
}
