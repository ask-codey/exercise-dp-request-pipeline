// A step in front of the endpoints: it's handed the request and next, the rest of the line,
// and either answers the request itself or calls next.handle(request).
interface Middleware {
    Response handle(Request request, Handler next);
}
