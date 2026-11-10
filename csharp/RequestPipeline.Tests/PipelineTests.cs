using RequestPipeline;

namespace RequestPipeline.Tests;

// The tests for your code. You don't need to change this file.
// Each message says what was expected, so a failing test tells you what to look at.

[TestClass]
public class PipelineTests
{
    private static readonly Request ListBooks = new Request("GET", "/books", "");
    private static readonly Request StrangerBorrows = new Request("POST", "/loans", "");
    private static readonly Request MayaBorrows = new Request("POST", "/loans", "Maya");

    // What the API does today. These pass before you change anything: keep them passing.

    [TestMethod]
    public void AnyoneCanListTheBooks()
    {
        Response response = App.Handle(ListBooks, new Log(), new FakeClock());
        Assert.AreEqual("200 The Lighthouse Keeper, A Map of Small Things, Rivers of Glass", $"{response.Status} {response.Body}",
            "Keep this working: GET /books answers 200 with the three titles.");
    }

    [TestMethod]
    public void BorrowingNeedsSomeoneSignedIn()
    {
        Log log = new Log();
        Response response = App.Handle(StrangerBorrows, log, new FakeClock());
        Assert.AreEqual("401 Please sign in", $"{response.Status} {response.Body}",
            "Keep this working: POST /loans from someone not signed in answers 401 Please sign in.");
        CollectionAssert.AreEqual(new List<string> { "POST /loans", "POST /loans: 401" }, log.Lines,
            "Keep this working: a refused request is logged on the way in and on the way out, and never timed.");
    }

    [TestMethod]
    public void MayaCanBorrow()
    {
        Response response = App.Handle(MayaBorrows, new Log(), new FakeClock());
        Assert.AreEqual("201 Loan 1: Maya, The Lighthouse Keeper", $"{response.Status} {response.Body}",
            "Keep this working: POST /loans from Maya answers 201 with her loan.");
    }

    [TestMethod]
    public void EachRequestIsLoggedAndTimed()
    {
        Log log = new Log();
        App.Handle(ListBooks, log, new FakeClock());
        CollectionAssert.AreEqual(new List<string> { "GET /books", "GET /books took 0 ms", "GET /books: 200" }, log.Lines,
            "Keep this working: GET /books writes its line, its time, then its status.");
    }

    // The ticket's new requirements. These fail until the work is done.

    [TestMethod]
    public void TheLoggingStepWorksOnItsOwn()
    {
        Log log = new Log();
        new Pipeline(Endpoints.Handle).Use(Checks.LogRequests(log)).Handle(ListBooks);
        CollectionAssert.AreEqual(new List<string> { "GET /books", "GET /books: 200" }, log.Lines,
            "Not yet: a pipeline with only Checks.LogRequests writes the request on the way in and its status on the way out.");
    }

    [TestMethod]
    public void TheSignInStepStopsAStrangerBeforeTheEndpoint()
    {
        bool reached = false;
        Pipeline pipeline = new Pipeline(request => { reached = true; return Endpoints.Handle(request); }).Use(Checks.SignIn());
        Response response = pipeline.Handle(StrangerBorrows);
        Assert.AreEqual(401, response.Status, "Not yet: a pipeline with Checks.SignIn answers 401 to POST /loans from someone not signed in.");
        Assert.IsFalse(reached, "Not yet: the sign-in step answers by itself, so the endpoint never runs for a stranger.");
    }

    [TestMethod]
    public void TheTimingStepTimesTheRestOfTheLine()
    {
        FakeClock clock = new FakeClock();
        Log log = new Log();
        new Pipeline(request => { clock.Advance(30); return Endpoints.Handle(request); }).Use(Checks.Timing(clock, log)).Handle(ListBooks);
        CollectionAssert.AreEqual(new List<string> { "GET /books took 30 ms" }, log.Lines,
            "Not yet: the endpoint took 30 ms on the test's clock, so Checks.Timing writes GET /books took 30 ms.");
    }

    [TestMethod]
    public void ANewCheckGoesInTheMiddleWithoutEditingTheOthers()
    {
        FakeClock clock = new FakeClock();
        Log log = new Log();
        Middleware closedForInventory = (request, next) =>
            request.Method == "GET" ? next(request) : new Response(503, "Closed for inventory");
        Pipeline pipeline = new Pipeline(Endpoints.Handle)
            .Use(Checks.LogRequests(log))
            .Use(Checks.SignIn())
            .Use(closedForInventory)
            .Use(Checks.Timing(clock, log));
        Response refused = pipeline.Handle(MayaBorrows);
        Response books = pipeline.Handle(ListBooks);
        Assert.AreEqual("503 200", $"{refused.Status} {books.Status}",
            "Not yet: with the inventory check in the line, Maya's POST /loans answers 503 and GET /books still answers 200.");
        CollectionAssert.AreEqual(
            new List<string> { "POST /loans", "POST /loans: 503", "GET /books", "GET /books took 0 ms", "GET /books: 200" }, log.Lines,
            "Not yet: the steps run in the order they were added, with the new check between sign-in and timing.");
    }
}
