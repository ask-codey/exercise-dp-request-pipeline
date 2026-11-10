// Written by the test team, for the tests and the demo: a clock that only moves when it's told to.
class FakeClock implements Clock {
    private long now;

    public long now() { return now; }

    void advance(long milliseconds) { now += milliseconds; }
}
