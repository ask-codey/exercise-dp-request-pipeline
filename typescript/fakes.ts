// Written by the test team, for the tests and the demo: a clock that only moves when it's told to.
import type { Clock } from "./pipeline.ts";

export class FakeClock implements Clock {
  #now = 0;
  now(): number { return this.#now; }
  advance(milliseconds: number): void { this.#now += milliseconds; }
}
