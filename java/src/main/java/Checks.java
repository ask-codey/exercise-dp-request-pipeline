// Today's checks, as the platform team wants them: one piece each. They're named, but they don't do anything yet.
class Checks {
    // Writes "GET /books" on the way in, and "GET /books: 200" on the way back out.
    static Middleware logRequests(Log log) { return (request, next) -> next.handle(request); }

    // Answers 401 "Please sign in" to anything but a GET from someone not signed in.
    static Middleware signIn() { return (request, next) -> next.handle(request); }

    // Writes "GET /books took 30 ms": how long the rest of the line took, by the clock it's handed.
    static Middleware timing(Clock clock, Log log) { return (request, next) -> next.handle(request); }
}
