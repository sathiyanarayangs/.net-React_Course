# Week 3 — SecureFileVault

File handling, encryption, hashing, and minimal authentication/authorization,
built up from FileStream basics through AES-CBC, PBKDF2, AES-GCM, HMAC, a
100 MB+ streaming vault, RSA (concept), and a hand-rolled minimal JWT with a
server-side role check.

## Structure

```
Week3.sln
src/
  SecureFileVault.Core/         all crypto/file-handling/auth logic, shared by both apps + tests
    FileIO/          Task 3.1 - chunked FileStream I/O
    Fundamentals/     Task 3.2 - encoding vs hashing vs encryption
    Aes/               Tasks 3.4-3.6 AES-CBC, 3.8-3.9 AES-GCM envelope, 3.12 large-file CBC streaming
    Kdf/                Task 3.7 - PBKDF2 key derivation
    Hashing/           Tasks 3.10-3.11 - SHA256/512, HMAC, constant-time compare
    Streaming/         GcmStreamVault - the SecureFileVault deliverable itself (chunked AES-GCM, 100MB+, tamper-rejected)
    Rsa/                Task 3.13 - RSA/OAEP concept demo
    Auth/               Task 3.15 - PasswordHasher (PBKDF2), MinimalJwt, UserStore
  SecureFileVault.Console/       CLI: `demo` runs every Day 1-5 task; `encrypt`/`decrypt`/`generate-large-file` run the real vault
  SecureFileVault.AuthApi/       minimal Web API: POST /api/auth/login, [Authorize(Roles="Teacher")] on one write endpoint
tests/SecureFileVault.Tests/     xUnit — see Testing Focus below
CRYPTO-NOTES.md                  Day 1/3/5 write-ups (Tasks 3.2, 3.3, 3.13, 3.14)
```

## Run the console demo

```bash
dotnet run --project src/SecureFileVault.Console/SecureFileVault.Console.csproj
```

Runs every Day 1–5 task in sequence and prints pass/fail for each proof
point (IV uniqueness, wrong-key rejection, GCM tamper rejection, PBKDF2
determinism, HMAC key-sensitivity, RSA round-trip, Teacher/Student token
role checks, etc).

### Running the real 100 MB+ vault (Task 3.12 / the deliverable)

```bash
dotnet run --project src/SecureFileVault.Console/SecureFileVault.Console.csproj -- generate-large-file big.bin 100
dotnet run --project src/SecureFileVault.Console/SecureFileVault.Console.csproj -- encrypt big.bin big.bin.sfv MyP@ssw0rd
dotnet run --project src/SecureFileVault.Console/SecureFileVault.Console.csproj -- decrypt big.bin.sfv big.out.bin MyP@ssw0rd
```

Then confirm `big.bin` and `big.out.bin` are byte-identical (e.g. `fc /b`
on Windows, `cmp` on macOS/Linux, or `ChunkedFileCopier.FilesAreByteIdentical`
in a quick script) — that comparison, plus Task Manager/Activity
Monitor showing memory usage stays flat rather than spiking to ~100 MB
during the run, is your evidence for Task 3.12.

To see the tamper-rejection: flip one byte anywhere in `big.bin.sfv` with a
hex editor (or `dotnet run ... -- encrypt` a small file and edit that
instead, to keep it quick) and try to decrypt — it should throw rather than
produce a silently-corrupted output file.

## Run the Auth API

```bash
dotnet run --project src/SecureFileVault.AuthApi/SecureFileVault.AuthApi.csproj
```

Swagger opens at `/swagger`. Seeded accounts:

| Username | Password | Role |
|---|---|---|
| `alice.teacher` | `TeacherPass123!` | Teacher |
| `bob.student` | `StudentPass123!` | Student |

Flow to capture as evidence: `POST /api/auth/login` with each account,
copy the returned token into Swagger's "Authorize" box, then `POST
/api/grades` as each — Teacher gets 200, Student gets 403.

## Run the tests with coverage

```bash
dotnet test tests/SecureFileVault.Tests/SecureFileVault.Tests.csproj --collect:"XPlat Code Coverage"
```

## Testing focus (per the brief)

- PBKDF2 determinism — `Pbkdf2Tests`
- AES-GCM round-trip + tamper rejection — `AesGcmEnvelopeTests`, `GcmStreamVaultTests`
- Constant-time compare — `FileHasherTests`
- Password verify (accept/reject) — `PasswordHasherTests`
- Role check returns 403 for Student, 200 for Teacher — `AuthorizationRoleTests` (needs the real HTTP pipeline, so it uses `WebApplicationFactory<Program>` rather than calling a controller method directly)
- Plus AES-CBC (`AesCbcDemoTests`), hashing/HMAC (`FileHasherTests`), file I/O (`FileIoAndFundamentalsTests`), the minimal JWT itself (`MinimalJwtTests`), and RSA (`RsaDemoTests`)

`Program.cs` in both apps is excluded from the coverage denominator
(`<ExcludeByFile>` in the test csproj) — composition-root/DI wiring isn't
unit-testable logic, same reasoning as Weeks 1–2.

## Notes

- Targets **.NET 8**.
- The minimal JWT (`Auth/MinimalJwt.cs`) is hand-rolled — header.payload.signature,
  HMAC-SHA256 signed — specifically so every part of the mechanism is
  visible, per the brief's "minimal token" framing. A real system should use
  a vetted library instead.
- `AesGcm` doesn't stream through `CryptoStream` the way AES-CBC does, so
  the actual 100 MB+ vault (`Streaming/GcmStreamVault.cs`) chunks the file
  and runs an independent, per-chunk AesGcm encrypt/decrypt with a
  counter-derived nonce — never buffering more than one chunk in memory,
  while still authenticating every chunk. Task 3.12 itself (the literal
  "chain FileStream → CryptoStream" exercise) uses AES-CBC, since that's
  what CryptoStream actually supports; see `Aes/LargeFileStreamCipher.cs`.
