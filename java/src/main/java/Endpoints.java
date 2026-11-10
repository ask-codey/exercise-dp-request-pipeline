// The library's endpoints: their own work, and no checks.
class Endpoints {
    static Response handle(Request request) {
        return switch (request.method() + " " + request.path()) {
            case "GET /books" -> new Response(200, "The Lighthouse Keeper, A Map of Small Things, Rivers of Glass");
            case "POST /loans" -> new Response(201, "Loan 1: " + request.user() + ", The Lighthouse Keeper");
            default -> new Response(404, "No such page");
        };
    }
}
