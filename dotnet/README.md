# C# SDK Skeleton

This folder contains a minimal C# wrapper skeleton for `EliteDriver`.

## Layout

- `EliteRobots.CSharp`: managed wrapper library (`P/Invoke` + `SafeHandle`)
- `EliteRobots.CSharp.Sample`: minimal console example

## Native Dependency

The managed layer depends on native C wrapper library:

- Linux/macOS: `libelite_cs_series_sdk_c.so` / `libelite_cs_series_sdk_c.dylib`
- Windows: `elite_cs_series_sdk_c.dll`

Build native wrapper with CMake option:

```bash
cmake -S . -B build -DELITE_COMPILE_C_WRAPPER=ON
cmake --build build
```

## Managed Build

```bash
dotnet build dotnet/EliteRobots.CSharp/EliteRobots.CSharp.csproj
```

## Notes

- This is a minimal skeleton, not a full API surface.
- The current wrapper covers: create/destroy, connection state, servoj/speedj/speedl/idle, send script, stop control.
