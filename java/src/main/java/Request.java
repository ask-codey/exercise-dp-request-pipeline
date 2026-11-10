// One web request: what's asked for, and who's asking ("" when nobody is signed in).
record Request(String method, String path, String user) { }
