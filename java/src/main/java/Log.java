import java.util.ArrayList;
import java.util.List;

// The lines the API writes about each request, in order.
class Log {
    final List<String> lines = new ArrayList<>();

    void add(String line) { lines.add(line); }
}
