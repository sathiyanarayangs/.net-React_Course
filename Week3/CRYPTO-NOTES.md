# CRYPTO-NOTES.md — Week 3 Write-ups

## Day 1

### Task 3.2 — Encoding vs Hashing vs Encryption

**One-line distinction:** encoding is a reversible, keyless representation
change (Base64); hashing is one-way and fixed-length (SHA-256); encryption
is reversible *only* with a key (AES).

Base64-"encoding" a password before storing it is not protection — anyone
can decode it with zero effort, so it provides no secrecy at all. Hashing a
file you need to send to someone and have them read is the wrong tool in
the other direction — there is no "un-hash," so hashing is for proving
integrity/identity, never for anything you need back.

Proven in code (`Fundamentals/EncodingHashingDemo.cs`): a Base64 string
round-trips back to the exact original; a SHA-256 hash is always 64 hex
characters regardless of input length, and there's no `Decode()` for it.

### Task 3.3 — ECB vs CBC vs CTR vs GCM

- **ECB** encrypts each block independently with the same key and no
  chaining, so **identical plaintext blocks always produce identical
  ciphertext blocks** — this is why ECB visibly leaks patterns (the classic
  example is an ECB-encrypted image where you can still see the outline of
  the original picture in the ciphertext). Confidentiality only, and weak
  confidentiality at that.
- **CBC** chains each block's ciphertext into the next block's input (via
  XOR with a random IV for the first block), so identical plaintext blocks
  no longer produce identical ciphertext. Confidentiality, but **no
  integrity** — Task 3.6 shows CBC gives no reliable tamper signal, only an
  occasional padding error.
- **CTR** turns a block cipher into a stream cipher by encrypting an
  incrementing counter and XORing the result with the plaintext. Fast and
  parallelizable, but — like CBC — confidentiality only, no integrity, and
  it is catastrophic to reuse a counter/nonce under the same key (the
  keystream repeats, which leaks the XOR of two plaintexts).
- **GCM** is CTR mode *plus* a built-in authentication tag (GMAC). It is the
  only one of the four that is **authenticated**: confidentiality AND
  integrity in one primitive. Task 3.9 proves this directly — a single
  flipped byte anywhere in a GCM-encrypted payload makes decryption throw,
  where the same tamper under CBC (Task 3.6) would often go undetected.

## Day 3

*(Tasks 3.7-3.9's write-ups live as code comments in `Kdf/Pbkdf2KeyDerivation.cs`
and `Aes/AesGcmEnvelope.cs` rather than duplicated here, since they're most
useful sitting right next to the code they explain.)*

## Day 5

### Task 3.13 — RSA / Asymmetric Encryption (concept)

See the extended comment in `Rsa/RsaDemo.cs` for the full write-up. Summary:
RSA is asymmetric — the **public** key encrypts, the **private** key
decrypts (the opposite pairing from a *signature*, where the private key
signs and the public key verifies). A 2048-bit key with OAEP-SHA256 padding
can only encrypt ~190 bytes in one call, so RSA is never used to encrypt
bulk data directly. Real protocols use **hybrid encryption**: RSA encrypts
a random one-time AES key (small enough for RSA's limit), and AES — fast,
no size limit — encrypts the actual data with that key. TLS uses exactly
this pattern (in its RSA key-exchange cipher suites) to get RSA's
no-shared-secret-needed property for establishing a session key, combined
with AES's speed for the actual traffic.

### Task 3.14 — Authentication vs Authorization, Session/Cookie vs Token

**Authentication vs authorization.** Authentication answers "who are you?"
— proving identity, typically by verifying a password (or a token issued
after a prior successful login). Authorization answers "what are you
allowed to do?" — a separate check, applied *after* identity is known,
against what that specific identity is permitted. Concretely: logging into
the SecureFileVault Auth API with a username/password is authentication
(Task 3.15's `POST /api/auth/login`); the server then checking whether the
logged-in user's role is "Teacher" before letting them post a grade is
authorization (the `[Authorize(Roles = "Teacher")]` attribute). A Student
who logs in successfully is authenticated but not authorized for that
particular action — which is exactly why the correct response is 403
Forbidden ("I know who you are, but no") rather than 401 Unauthorized
("I don't know who you are").

**Session/cookie vs token.** A session/cookie approach has the server keep
authoritative state: on login, the server creates a session record (in
memory, a database, or a cache) and hands the client an opaque cookie that
just points at it; every request the server looks up that session to
decide who's calling. A concrete example is classic ASP.NET Web Forms or
PHP session auth, where logging out means deleting the server-side session
record and the cookie becomes meaningless. A token approach (like this
week's minimal JWT) instead packs the identity/claims directly into a
signed token the client holds and sends with every request — the server
verifies the signature and reads the claims straight from the token,
without needing to look anything up server-side. That statelessness is
why tokens scale well across multiple servers with no shared session
store, but it also means a token can't be "deleted" server-side before it
expires — the SecureFileVault login in Task 3.15 uses a short 1-hour
expiry specifically to bound that risk.
