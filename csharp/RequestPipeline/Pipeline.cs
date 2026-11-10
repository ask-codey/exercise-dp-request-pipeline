namespace RequestPipeline;

// Riverside Community Library's web requests, without a framework. Read the ticket in README.md first.

public record Request(string Method, string Path, string User);
public record Response(int Status, string Body);

// Something that answers a request.
public delegate Response Handler(Request request);

// A step in front of the endpoints: it's handed the request and next, the rest of the line,
// and either answers the request itself or calls next(request).
public delegate Response Middleware(Request request, Handler next);

// The lines the API writes about each request, in order.
public class Log
{
    public List<string> Lines { get; } = new();
    public void Add(string line) => Lines.Add(line);
}

// Anything that can say what time it is, in milliseconds.
public interface IClock
{
    long Now { get; }
}

// The library's endpoints: their own work, and no checks.
public static class Endpoints
{
    public static Response Handle(Request request) => $"{request.Method} {request.Path}" switch
    {
        "GET /books" => new Response(200, "The Lighthouse Keeper, A Map of Small Things, Rivers of Glass"),
        "POST /loans" => new Response(201, $"Loan 1: {request.User}, The Lighthouse Keeper"),
        _ => new Response(404, "No such page"),
    };
}

// The platform team's pipeline: the endpoint at the end, and middleware in front of it.
// Other teams already call it. It doesn't run any middleware yet.
public class Pipeline
{
    private readonly Handler endpoint;

    public Pipeline(Handler endpoint) => this.endpoint = endpoint;

    // Adds a step. The first step added is the first a request meets.
    public Pipeline Use(Middleware step) => this;

    public Response Handle(Request request) => endpoint(request);
}

// Today's checks, as the platform team wants them: one piece each. They're named, but they don't do anything yet.
public static class Checks
{
    // Writes "GET /books" on the way in, and "GET /books: 200" on the way back out.
    public static Middleware LogRequests(Log log) => (request, next) => next(request);

    // Answers 401 "Please sign in" to anything but a GET from someone not signed in.
    public static Middleware SignIn() => (request, next) => next(request);

    // Writes "GET /books took 30 ms": how long the rest of the line took, by the clock it's handed.
    public static Middleware Timing(IClock clock, Log log) => (request, next) => next(request);
}

// What the web server calls for every request. Every check is written out in here, in order.
public static class App
{
    public static Response Handle(Request request, Log log, IClock clock)
    {
        string what = $"{request.Method} {request.Path}";
        log.Add(what);
        Response response;
        if (request.Method != "GET" && request.User == "")
        {
            response = new Response(401, "Please sign in");
        }
        else
        {
            long start = clock.Now;
            response = Endpoints.Handle(request);
            log.Add($"{what} took {clock.Now - start} ms");
        }
        log.Add($"{what}: {response.Status}");
        return response;
    }
}
