# JetStream Publish Tests

Run on 4-core Linux AMD64

## Setup

In a separate terminal, start `nats-server` and leave it running

```bash
nats-server --js
```

## No Latency

```bash
dotnet run -c Release --project Memory.Test
dotnet run -c Release --project Memory.Test.Legacy
```

Results:

```
V2 Avg: 0.090, Min: 0.026, Max: 0.208, Threads: 12
V1 Avg: 0.066, Min: 0.045, Max: 0.122, Threads: 8
```

Analysis: V1 is using sync I/O while V2 is using async I/O.  With no latency over localhost,
sync I/O is very fast and is not a bottleneck.

## 1ms Latency on interface (2ms round-trip)

Note: I had to add `<ThreadPoolMinThreads>20</ThreadPoolMinThreads>` to `Memory.Test.Legacy.csproj` 
to get past sync-over-async deadlocks

```bash
./scripts/add-latency.sh 1
dotnet run -c Release --project Memory.Test

# I had to set ThreadPoolMinThreads for Memory.Test.Legacy to to get past sync-over-async deadlocks
dotnet run -c Release --project Memory.Test.Legacy -p ThreadPoolMinThreads=20
```

Results:

```
V2 Avg: 0.101, Min: 0.035, Max: 0.170, Threads: 10
V1 Avg: 0.395, Min: 0.066, Max: 0.564, Threads: 30
```

Analysis: Sync-over-async calls in V1 block thread pool threads leading to increased contention in thread pool.
Meanwhile V2's async/io allows for it to maintain efficient concurrency.

## 10ms Latency on interface (20ms round-trip)

```
./scripts/add-latency.sh 10
dotnet run -c Release --project Memory.Test
dotnet run -c Release --project Memory.Test.Legacy
```

Results:

```
V2 Avg: 0.199, Min: 0.124, Max: 0.345, Max Threads: 14
V1 Avg: 2.676, Min: 1.215, Max: 3.132, Max Threads: 33
```

Analysis: V1 thread pool is highly contentious now.

## Limited thread pool with 1ms Latency on interface (2ms round-trip)

```
./scripts/add-latency.sh 1
dotnet run -c Release --project Memory.Test -p ThreadPoolMaxThreads=1
dotnet run -c Release --project Memory.Test.Legacy -p ThreadPoolMaxThreads=1
```

Results:

```
V2 Avg: 0.088, Min: 0.056, Max: 0.164, Max Threads: 4
V1: hangs
```

Analysis: V1 completely hangs because it hits a sync-over-async deadlock. V2 does just fine although it looks like the
runtime didn't completely respect our request for 1 thread.

## Cleanup

Remove latency

```bash
./scripts/rm-latency.sh
```
