# Exercise: The Request Pipeline

Exercise code for the lesson **Exercise: The Request Pipeline** in the course *Design Patterns, Demystified* on [ask-codey.com](https://www.ask-codey.com).

## The ticket

> **From:** the platform team, Riverside Community Library
>
> Every request to the library's API passes the same checks: it's logged on the way in, anything but a `GET` needs someone signed in, it's timed, and its status is logged on the way out. Today all of that is written out in one function, `App.Handle`, and two other teams are waiting on it.
>
> 1. **We want each of today's checks as its own piece** we can put in any pipeline: `Checks.LogRequests(log)`, `Checks.SignIn()` and `Checks.Timing(clock, log)`. They're already named, because our code calls them, but they don't do anything yet. A step is handed the request and `next`, the rest of the line, and answers the request itself or passes it on.
> 2. **`new Pipeline(endpoint).Use(step)...Handle(request)` has to run the steps,** in the order they were added, in front of the endpoint. It's named too, but today it skips every step and calls the endpoint.
> 3. **The security team is adding its own check** for the library's inventory weekend: everything but a `GET` answers `503 Closed for inventory`. They'll write it in their own code and put it in the line between the sign-in check and the timing, without anyone editing our checks.
>
> `App.Handle` is what the web server calls for every request, and it has to keep doing exactly what it does today.

In Python the pieces are functions in `pipeline.py`: `log_requests(log)`, `sign_in()`, `timing(clock, log)` and `handle(request, log, clock)`; in JavaScript and TypeScript they're `logRequests(log)`, `signIn()`, `timing(clock, log)` and `handle(request, log, clock)` in `pipeline.js` or `pipeline.ts`.

## Pick your language

The same exercise is here in five languages. Work in the folder for the language you set up; you only need one.

| Folder | The API's code | The test team's clock | The tests |
|---|---|---|---|
| `csharp/` | `RequestPipeline/Pipeline.cs` | `RequestPipeline/Fakes.cs` | `RequestPipeline.Tests/PipelineTests.cs` |
| `java/` | `src/main/java/` (`Pipeline.java`, `Checks.java`, `App.java`, `Endpoints.java`, `Middleware.java`, `Handler.java`, `Log.java`, `Request.java`, `Response.java`, `Clock.java`) | `src/main/java/FakeClock.java` | `src/test/java/PipelineTest.java` |
| `python/` | `pipeline.py` | `fakes.py` | `test_pipeline.py` |
| `javascript/` | `pipeline.js` | `fakes.js` | `pipeline.test.js` |
| `typescript/` | `pipeline.ts` | `fakes.ts` | `pipeline.test.ts` |

Every folder also has a small demo program that runs the inventory weekend, with the security team's check written outside the API's code. You don't need to change the tests, the test team's clock or the demo.

## Run it and check your work

Open your language's folder in VS Code (**File > Open Folder**), then open its terminal (**Terminal > New Terminal**) and run these commands there.

**C#**

```bash
dotnet run --project RequestPipeline
dotnet test
```

**Java** (the Maven Wrapper downloads Maven the first time, so the first run takes a minute)

```bash
java src/main/java/Demo.java
./mvnw test
```

On Windows in PowerShell, write `.\mvnw test` instead of `./mvnw test`.

**Python** (once: make a virtual environment and install pytest into it)

```bash
python -m venv .venv
```

Then turn it on: `.venv\Scripts\Activate.ps1` in PowerShell on Windows, or `source .venv/bin/activate` on macOS (where the command is `python3`, not `python`). Then:

```bash
pip install -r requirements.txt
python demo.py
python -m pytest
```

**JavaScript**

```bash
node demo.js
npm test
```

**TypeScript** (once: `npm install`, which installs TypeScript)

```bash
node demo.ts
npm test
```

At first, 4 tests pass and 4 fail. The 4 that pass are what the API does today: keep them green the whole time, and run the tests after every small change. The 4 that fail are the ticket. A failing test's message starts with **Not yet:** (or **Keep this working:** if you broke something that worked) and says what it expected.

## When you're done

All 8 tests are green, and the demo prints:

```text
GET /books: 200 The Lighthouse Keeper, A Map of Small Things, Rivers of Glass
POST /loans: 401 Please sign in
POST /loans: 503 Closed for inventory
The log:
  GET /books
  GET /books took 12 ms
  GET /books: 200
  POST /loans
  POST /loans: 401
  POST /loans
  POST /loans: 503
```

<details>
<summary>Stuck?</summary>

- **For the checks:** `App.Handle` does every job in one function, so none of them can be used, moved or added to on its own. Give each job its own small step, a **middleware**: a function that's handed the request and `next`, and either answers the request itself (as the sign-in check does for a stranger) or calls `next(request)`, doing its own work before it, after it, or both. It's the **Chain of Responsibility** from Pass 3, as every web framework uses it. (*Middleware: The Airport Checkpoints*.)
- **For the pipeline:** keep the steps in a list. To handle a request, wrap them around the endpoint starting from the last one, so the first step added is the first one a request meets. (*Middleware*, the modern version.)
- **For `App.Handle`:** once the pieces work, it can be a pipeline of today's three checks in front of `Endpoints.Handle`, in the order its lines run now: logging, then the sign-in check, then timing.

</details>

## Hand it in

When it works, push your solution to a public repo of your own on GitHub, then paste that repo's address into **Submit your exercise** on the lesson page at [ask-codey.com](https://www.ask-codey.com). That's what completes the lesson. The lesson page shows the Git commands, step by step.
