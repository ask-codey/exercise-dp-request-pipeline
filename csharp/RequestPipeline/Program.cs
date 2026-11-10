using RequestPipeline;

// A demo: an inventory weekend at the library, with a check written outside the library's code.
// You don't need to change it.
FakeClock clock = new FakeClock();
Log log = new Log();

// The security team's check, written in their own code.
Middleware closedForInventory = (request, next) =>
    request.Method == "GET" && request.Path == "/books" ? next(request) : new Response(503, "Closed for inventory");

Pipeline pipeline = new Pipeline(request => { clock.Advance(12); return Endpoints.Handle(request); })
    .Use(Checks.LogRequests(log))
    .Use(Checks.SignIn())
    .Use(closedForInventory)
    .Use(Checks.Timing(clock, log));

foreach (Request request in new[] { new Request("GET", "/books", ""), new Request("POST", "/loans", ""), new Request("POST", "/loans", "Maya") })
{
    Response response = pipeline.Handle(request);
    Console.WriteLine($"{request.Method} {request.Path}: {response.Status} {response.Body}");
}
Console.WriteLine("The log:");
foreach (string line in log.Lines) { Console.WriteLine("  " + line); }
