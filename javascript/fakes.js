// Written by the test team, for the tests and the demo: a clock that only moves when it's told to.
export class FakeClock {
  #now = 0;
  now() { return this.#now; }
  advance(milliseconds) { this.#now += milliseconds; }
}
