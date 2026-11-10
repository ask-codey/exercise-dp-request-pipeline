import java.util.List;

// A demo: an inventory weekend at the library, with a check written outside the library's code.
// You don't need to change it. Run it with: java src/main/java/Demo.java
public class Demo {
    public static void main(String[] args) {
        FakeClock clock = new FakeClock();
        Log log = new Log();

        // The security team's check, written in their own code.
        Middleware closedForInventory = (request, next) ->
            request.method().equals("GET") && request.path().equals("/books") ? next.handle(request) : new Response(503, "Closed for inventory");

        Pipeline pipeline = new Pipeline(request -> { clock.advance(12); return Endpoints.handle(request); })
            .use(Checks.logRequests(log))
            .use(Checks.signIn())
            .use(closedForInventory)
            .use(Checks.timing(clock, log));

        for (Request request : List.of(new Request("GET", "/books", ""), new Request("POST", "/loans", ""), new Request("POST", "/loans", "Maya"))) {
            Response response = pipeline.handle(request);
            System.out.println(request.method() + " " + request.path() + ": " + response.status() + " " + response.body());
        }
        System.out.println("The log:");
        for (String line : log.lines) { System.out.println("  " + line); }
    }
}
