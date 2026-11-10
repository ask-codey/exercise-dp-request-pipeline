// The platform team's pipeline: the endpoint at the end, and middleware in front of it.
// Other teams already call it. It doesn't run any middleware yet.
class Pipeline {
    private final Handler endpoint;

    Pipeline(Handler endpoint) { this.endpoint = endpoint; }

    // Adds a step. The first step added is the first a request meets.
    Pipeline use(Middleware step) { return this; }

    Response handle(Request request) { return endpoint.handle(request); }
}
