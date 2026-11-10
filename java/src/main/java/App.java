// What the web server calls for every request. Every check is written out in here, in order.
class App {
    static Response handle(Request request, Log log, Clock clock) {
        String what = request.method() + " " + request.path();
        log.add(what);
        Response response;
        if (!request.method().equals("GET") && request.user().isEmpty()) {
            response = new Response(401, "Please sign in");
        } else {
            long start = clock.now();
            response = Endpoints.handle(request);
            log.add(what + " took " + (clock.now() - start) + " ms");
        }
        log.add(what + ": " + response.status());
        return response;
    }
}
