namespace RequestPipeline;

// Written by the test team, for the tests and the demo: a clock that only moves when it's told to.
public class FakeClock : IClock
{
    public long Now { get; private set; }

    public void Advance(long milliseconds) => Now += milliseconds;
}
