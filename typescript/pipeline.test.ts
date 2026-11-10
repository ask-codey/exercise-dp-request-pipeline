// The tests for your code. You don't need to change this file.
// Each message says what was expected, so a failing test tells you what to look at.
import assert from "node:assert/strict";
import { test } from "node:test";
import { endpoints, handle, Log, logRequests, Pipeline, signIn, timing, type LibraryRequest, type Middleware } from "./pipeline.ts";
import { FakeClock } from "./fakes.ts";

const LIST_BOOKS: LibraryRequest = { method: "GET", path: "/books", user: "" };
const STRANGER_BORROWS: LibraryRequest = { method: "POST", path: "/loans", user: "" };
const MAYA_BORROWS: LibraryRequest = { method: "POST", path: "/loans", user: "Maya" };

// What the API does today. These pass before you change anything: keep them passing.

test("anyone can list the books", () => {
  const response = handle(LIST_BOOKS, new Log(), new FakeClock());
  assert.equal(`${response.status} ${response.body}`, "200 The Lighthouse Keeper, A Map of Small Things, Rivers of Glass",
    "Keep this working: GET /books answers 200 with the three titles.");
});

test("borrowing needs someone signed in", () => {
  const log = new Log();
  const response = handle(STRANGER_BORROWS, log, new FakeClock());
  assert.equal(`${response.status} ${response.body}`, "401 Please sign in",
    "Keep this working: POST /loans from someone not signed in answers 401 Please sign in.");
  assert.deepEqual(log.lines, ["POST /loans", "POST /loans: 401"],
    "Keep this working: a refused request is logged on the way in and on the way out, and never timed.");
});

test("Maya can borrow", () => {
  const response = handle(MAYA_BORROWS, new Log(), new FakeClock());
  assert.equal(`${response.status} ${response.body}`, "201 Loan 1: Maya, The Lighthouse Keeper",
    "Keep this working: POST /loans from Maya answers 201 with her loan.");
});

test("each request is logged and timed", () => {
  const log = new Log();
  handle(LIST_BOOKS, log, new FakeClock());
  assert.deepEqual(log.lines, ["GET /books", "GET /books took 0 ms", "GET /books: 200"],
    "Keep this working: GET /books writes its line, its time, then its status.");
});

// The ticket's new requirements. These fail until the work is done.

test("the logging step works on its own", () => {
  const log = new Log();
  new Pipeline(endpoints).use(logRequests(log)).handle(LIST_BOOKS);
  assert.deepEqual(log.lines, ["GET /books", "GET /books: 200"],
    "Not yet: a pipeline with only logRequests writes the request on the way in and its status on the way out.");
});

test("the sign-in step stops a stranger before the endpoint", () => {
  let reached = false;
  const pipeline = new Pipeline((request) => { reached = true; return endpoints(request); }).use(signIn());
  const response = pipeline.handle(STRANGER_BORROWS);
  assert.equal(response.status, 401, "Not yet: a pipeline with signIn answers 401 to POST /loans from someone not signed in.");
  assert.equal(reached, false, "Not yet: the sign-in step answers by itself, so the endpoint never runs for a stranger.");
});

test("the timing step times the rest of the line", () => {
  const clock = new FakeClock();
  const log = new Log();
  new Pipeline((request) => { clock.advance(30); return endpoints(request); }).use(timing(clock, log)).handle(LIST_BOOKS);
  assert.deepEqual(log.lines, ["GET /books took 30 ms"],
    "Not yet: the endpoint took 30 ms on the test's clock, so timing writes GET /books took 30 ms.");
});

test("a new check goes in the middle without editing the others", () => {
  const clock = new FakeClock();
  const log = new Log();
  const closedForInventory: Middleware = (request, next) =>
    request.method === "GET" ? next(request) : { status: 503, body: "Closed for inventory" };
  const pipeline = new Pipeline(endpoints).use(logRequests(log)).use(signIn()).use(closedForInventory).use(timing(clock, log));
  const refused = pipeline.handle(MAYA_BORROWS);
  const books = pipeline.handle(LIST_BOOKS);
  assert.equal(`${refused.status} ${books.status}`, "503 200",
    "Not yet: with the inventory check in the line, Maya's POST /loans answers 503 and GET /books still answers 200.");
  assert.deepEqual(log.lines, ["POST /loans", "POST /loans: 503", "GET /books", "GET /books took 0 ms", "GET /books: 200"],
    "Not yet: the steps run in the order they were added, with the new check between sign-in and timing.");
});
