// Riverside Community Library's web requests, without a framework. Read the ticket in README.md first.

// One web request: what's asked for, and who's asking ("" when nobody is signed in).
export interface LibraryRequest { method: string; path: string; user: string; }
export interface LibraryResponse { status: number; body: string; }

// Something that answers a request.
export type Handler = (request: LibraryRequest) => LibraryResponse;

// A step in front of the endpoints: it's handed the request and next, the rest of the line,
// and either answers the request itself or returns next(request).
export type Middleware = (request: LibraryRequest, next: Handler) => LibraryResponse;

// Anything that can say what time it is, in milliseconds.
export interface Clock { now(): number; }

// The lines the API writes about each request, in order.
export class Log {
  lines: string[] = [];
  add(line: string): void { this.lines.push(line); }
}

// The library's endpoints: their own work, and no checks.
export function endpoints(request: LibraryRequest): LibraryResponse {
  switch (`${request.method} ${request.path}`) {
    case "GET /books": return { status: 200, body: "The Lighthouse Keeper, A Map of Small Things, Rivers of Glass" };
    case "POST /loans": return { status: 201, body: `Loan 1: ${request.user}, The Lighthouse Keeper` };
    default: return { status: 404, body: "No such page" };
  }
}

// The platform team's pipeline: the endpoint at the end, and middleware in front of it.
// Other teams already call it. It doesn't run any middleware yet.
export class Pipeline {
  #endpoint: Handler;

  constructor(endpoint: Handler) { this.#endpoint = endpoint; }

  // Adds a step. The first step added is the first a request meets.
  use(step: Middleware): Pipeline { return this; }

  handle(request: LibraryRequest): LibraryResponse { return this.#endpoint(request); }
}

// Today's checks, as the platform team wants them: one piece each. They're named, but they don't do anything yet.

// Writes "GET /books" on the way in, and "GET /books: 200" on the way back out.
export function logRequests(log: Log): Middleware { return (request, next) => next(request); }

// Answers 401 "Please sign in" to anything but a GET from someone not signed in.
export function signIn(): Middleware { return (request, next) => next(request); }

// Writes "GET /books took 30 ms": how long the rest of the line took, by the clock it's handed.
export function timing(clock: Clock, log: Log): Middleware { return (request, next) => next(request); }

// What the web server calls for every request. Every check is written out in here, in order.
export function handle(request: LibraryRequest, log: Log, clock: Clock): LibraryResponse {
  const what = `${request.method} ${request.path}`;
  log.add(what);
  let response: LibraryResponse;
  if (request.method !== "GET" && request.user === "") {
    response = { status: 401, body: "Please sign in" };
  } else {
    const start = clock.now();
    response = endpoints(request);
    log.add(`${what} took ${clock.now() - start} ms`);
  }
  log.add(`${what}: ${response.status}`);
  return response;
}
