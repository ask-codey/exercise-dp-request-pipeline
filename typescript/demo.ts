// A demo: an inventory weekend at the library, with a check written outside the library's code.
// You don't need to change it.
import { endpoints, Log, logRequests, Pipeline, signIn, timing, type LibraryRequest, type Middleware } from "./pipeline.ts";
import { FakeClock } from "./fakes.ts";

const clock = new FakeClock();
const log = new Log();

// The security team's check, written in their own code.
const closedForInventory: Middleware = (request, next) =>
  request.method === "GET" && request.path === "/books" ? next(request) : { status: 503, body: "Closed for inventory" };

const pipeline = new Pipeline((request) => { clock.advance(12); return endpoints(request); })
  .use(logRequests(log))
  .use(signIn())
  .use(closedForInventory)
  .use(timing(clock, log));

const requests: LibraryRequest[] = [{ method: "GET", path: "/books", user: "" }, { method: "POST", path: "/loans", user: "" }, { method: "POST", path: "/loans", user: "Maya" }];
for (const request of requests) {
  const response = pipeline.handle(request);
  console.log(`${request.method} ${request.path}: ${response.status} ${response.body}`);
}
console.log("The log:");
for (const line of log.lines) { console.log("  " + line); }
