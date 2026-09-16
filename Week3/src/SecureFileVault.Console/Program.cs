using SecureFileVault.Core.Aes;
using SecureFileVault.Core.Auth;
using SecureFileVault.Core.FileIO;
using SecureFileVault.Core.Fundamentals;
using SecureFileVault.Core.Hashing;
using SecureFileVault.Core.Kdf;
using SecureFileVault.Core.Rsa;
using SecureFileVault.Core.Streaming;

if (args.Length == 0 || args[0] == "demo")
{
    RunAllDemos();
    return;
}

switch (args[0])
{
    case "generate-large-file":
        // e.g. SecureFileVault generate-large-file big.bin 100
        GenerateLargeFile(args[1], long.Parse(args[2]));
        break;

    case "encrypt":
        // e.g. SecureFileVault encrypt big.bin big.bin.sfv MyP@ssw0rd
        GcmStreamVault.EncryptFile(args[1], args[2], args[3]);
        Console.WriteLine($"Encrypted '{args[1]}' -> '{args[2]}'.");
        break;

    case "decrypt":
        // e.g. SecureFileVault decrypt big.bin.sfv big.decrypted.bin MyP@ssw0rd
        GcmStreamVault.DecryptFile(args[1], args[2], args[3]);
        Console.WriteLine($"Decrypted '{args[1]}' -> '{args[2]}'.");
        break;

    default:
        Console.WriteLine("Usage:");
        Console.WriteLine("  SecureFileVault demo");
        Console.WriteLine("  SecureFileVault generate-large-file <path> <sizeInMB>");
        Console.WriteLine("  SecureFileVault encrypt <input> <output> <password>");
        Console.WriteLine("  SecureFileVault decrypt <input> <output> <password>");
        break;
}

static void GenerateLargeFile(string path, long sizeInMb)
{
    Console.WriteLine($"Generating {sizeInMb} MB test file at '{path}'...");
    LargeFileStreamCipher.GenerateTestFile(path, sizeInMb * 1024 * 1024);
    Console.WriteLine("Done.");
}

static void RunAllDemos()
{
    Console.WriteLine("===================================================");
    Console.WriteLine(" Week 3 — File Handling, Encryption, Hashing, Auth");
    Console.WriteLine("===================================================\n");

    Day1();
    Console.WriteLine();
    Day2();
    Console.WriteLine();
    Day3();
    Console.WriteLine();
    Day4();
    Console.WriteLine();
    Day5();

    Console.WriteLine("\n===================================================");
    Console.WriteLine(" Done. See CRYPTO-NOTES.md for the Day 1/3/5 write-ups.");
    Console.WriteLine(" Try: SecureFileVault generate-large-file big.bin 100");
    Console.WriteLine("      SecureFileVault encrypt big.bin big.bin.sfv MyP@ssw0rd");
    Console.WriteLine("      SecureFileVault decrypt big.bin.sfv big.out.bin MyP@ssw0rd");
    Console.WriteLine("===================================================");
}

static void Day1()
{
    Console.WriteLine("--- Day 1: File Handling & Fundamentals ---");

    string tempDir = Path.Combine(Path.GetTempPath(), "sfv-demo");
    Directory.CreateDirectory(tempDir);
    string filePath = Path.Combine(tempDir, "task3_1.txt");
    string copyPath = Path.Combine(tempDir, "task3_1_copy.txt");

    // Task 3.1
    ChunkedFileCopier.WriteAllText(filePath, "Line one.\n");
    ChunkedFileCopier.Append(filePath, "Line two, appended.\n");
    string content = ChunkedFileCopier.ReadAllTextChunked(filePath);
    Console.WriteLine($"[3.1] Wrote+appended+read via FileStream. Content:\n{content}");

    ChunkedFileCopier.CopyChunked(filePath, copyPath);
    bool identical = ChunkedFileCopier.FilesAreByteIdentical(filePath, copyPath);
    Console.WriteLine($"[3.1] Chunked copy byte-identical to source? {identical}");

    // Task 3.2
    string original = "correct horse battery staple";
    string encoded = EncodingHashingDemo.Base64Encode(original);
    string decoded = EncodingHashingDemo.Base64Decode(encoded);
    string hash = EncodingHashingDemo.Sha256HashHex(original);
    Console.WriteLine($"[3.2] Base64 round-trip ok? {decoded == original} (encoded: {encoded})");
    Console.WriteLine($"[3.2] SHA-256 (fixed-length, irreversible): {hash} (len={hash.Length})");

    // Task 3.3 — see CRYPTO-NOTES.md for the written note.
    Console.WriteLine("[3.3] ECB vs CBC vs CTR vs GCM comparison — see CRYPTO-NOTES.md.");
}

static void Day2()
{
    Console.WriteLine("--- Day 2: Symmetric Encryption (AES-CBC) ---");

    byte[] key = Pbkdf2KeyDerivation.DeriveKey("demo-password", Pbkdf2KeyDerivation.GenerateSalt());

    // Task 3.4
    var (iv1, ciphertext1) = AesCbcDemo.Encrypt(key, "Meet at dawn.");
    string roundTrip = AesCbcDemo.Decrypt(key, iv1, ciphertext1);
    Console.WriteLine($"[3.4] Round-trip ok? {roundTrip == "Meet at dawn."} (IV: {Convert.ToHexString(iv1)})");

    // Task 3.5
    var (iv2, ciphertext2) = AesCbcDemo.Encrypt(key, "Meet at dawn.");
    bool sameIv = iv1.AsSpan().SequenceEqual(iv2);
    bool sameCiphertext = ciphertext1.AsSpan().SequenceEqual(ciphertext2);
    Console.WriteLine($"[3.5] Same plaintext, fresh IV each time -> same IV? {sameIv}, same ciphertext? {sameCiphertext}");
    Console.WriteLine($"[3.5] Both still decrypt correctly? {AesCbcDemo.Decrypt(key, iv2, ciphertext2) == "Meet at dawn."}");

    // Task 3.6
    byte[] wrongKey = Pbkdf2KeyDerivation.DeriveKey("totally-different-password", Pbkdf2KeyDerivation.GenerateSalt());
    bool threw = AesCbcDemo.DecryptWithWrongKeyThrows(key, wrongKey, "Meet at dawn.");
    Console.WriteLine($"[3.6] Wrong-key decrypt threw CryptographicException? {threw} (CBC has no integrity check — see comment in AesCbcDemo.cs)");
}

static void Day3()
{
    Console.WriteLine("--- Day 3: Authenticated Encryption & Key Derivation ---");

    // Task 3.7
    byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();
    byte[] keyA = Pbkdf2KeyDerivation.DeriveKey("same-password", salt);
    byte[] keyB = Pbkdf2KeyDerivation.DeriveKey("same-password", salt);
    byte[] keyDifferentSalt = Pbkdf2KeyDerivation.DeriveKey("same-password", Pbkdf2KeyDerivation.GenerateSalt());
    Console.WriteLine($"[3.7] Same password+salt -> same key? {keyA.AsSpan().SequenceEqual(keyB)}");
    Console.WriteLine($"[3.7] Different salt -> different key? {!keyA.AsSpan().SequenceEqual(keyDifferentSalt)}");

    // Task 3.8
    byte[] envelope = AesGcmEnvelope.EncryptString("vault-password", "Top secret plans.");
    string decrypted = AesGcmEnvelope.DecryptToString("vault-password", envelope);
    Console.WriteLine($"[3.8] AES-GCM round-trip ok, tag verified? {decrypted == "Top secret plans."}");

    // Task 3.9
    byte[] tampered = (byte[])envelope.Clone();
    tampered[^1] ^= 0x01; // flip the last byte of the ciphertext
    bool rejected;
    try
    {
        AesGcmEnvelope.DecryptToString("vault-password", tampered);
        rejected = false;
    }
    catch (System.Security.Cryptography.CryptographicException)
    {
        rejected = true;
    }
    Console.WriteLine($"[3.9] Single-byte ciphertext tamper rejected by GCM? {rejected} " +
                       "(this is exactly what CBC in Task 3.6 could NOT reliably do)");
}

static void Day4()
{
    Console.WriteLine("--- Day 4: Hashing, HMAC & Streaming Large Files ---");

    string tempDir = Path.Combine(Path.GetTempPath(), "sfv-demo");
    Directory.CreateDirectory(tempDir);
    string filePath = Path.Combine(tempDir, "task3_10.txt");
    File.WriteAllText(filePath, "Some file content for hashing.");

    // Task 3.10
    byte[] sha256a = FileHasher.Sha256File(filePath);
    byte[] sha256b = FileHasher.Sha256File(filePath);
    byte[] hmacKeyA = "key-one"u8.ToArray();
    byte[] hmacKeyB = "key-two"u8.ToArray();
    byte[] hmacA = FileHasher.HmacSha256File(filePath, hmacKeyA);
    byte[] hmacB = FileHasher.HmacSha256File(filePath, hmacKeyB);
    Console.WriteLine($"[3.10] SHA-256 stable across runs? {sha256a.AsSpan().SequenceEqual(sha256b)}");
    Console.WriteLine($"[3.10] HMAC changes when the key changes? {!hmacA.AsSpan().SequenceEqual(hmacB)}");

    // Task 3.11
    bool constantTimeMatch = FileHasher.ConstantTimeEquals(sha256a, sha256b);
    Console.WriteLine($"[3.11] CryptographicOperations.FixedTimeEquals says equal? {constantTimeMatch} " +
                       "(see comment in FileHasher.cs on why == / SequenceEqual leak timing)");

    // Task 3.12 — a small demo run here; use `generate-large-file` + `encrypt`/`decrypt`
    // commands for an actual 100 MB+ run (too slow to do unconditionally on every `demo`).
    string smallInput = Path.Combine(tempDir, "task3_12_small.bin");
    string smallEncrypted = Path.Combine(tempDir, "task3_12_small.enc");
    string smallDecrypted = Path.Combine(tempDir, "task3_12_small.dec");
    LargeFileStreamCipher.GenerateTestFile(smallInput, 1024 * 1024); // 1 MB stand-in for the demo run
    byte[] cbcKey = Pbkdf2KeyDerivation.DeriveKey("stream-password", Pbkdf2KeyDerivation.GenerateSalt());
    byte[] cbcIv = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
    LargeFileStreamCipher.EncryptFile(smallInput, smallEncrypted, cbcKey, cbcIv);
    LargeFileStreamCipher.DecryptFile(smallEncrypted, smallDecrypted, cbcKey, cbcIv);
    bool streamIdentical = ChunkedFileCopier.FilesAreByteIdentical(smallInput, smallDecrypted);
    Console.WriteLine($"[3.12] Stream encrypt/decrypt (1 MB demo run) byte-identical? {streamIdentical}");
    Console.WriteLine("[3.12] For the real 100 MB+ run: `SecureFileVault generate-large-file big.bin 100` " +
                       "then `encrypt`/`decrypt` (uses the chunked AES-GCM vault, not this CBC demo).");
}

static void Day5()
{
    Console.WriteLine("--- Day 5: Asymmetric (concept), Auth Fundamentals & Minimal Token ---");

    // Task 3.13
    var (publicKey, privateKey) = RsaDemo.GenerateKeyPair();
    byte[] rsaCiphertext = RsaDemo.EncryptShortMessage(publicKey, "The eagle lands at dusk.");
    string rsaPlaintext = RsaDemo.DecryptShortMessage(privateKey, rsaCiphertext);
    Console.WriteLine($"[3.13] RSA-OAEP short message round-trip ok? {rsaPlaintext == "The eagle lands at dusk."}");
    Console.WriteLine($"[3.13] Max plaintext for 2048-bit key + OAEP-SHA256: ~{RsaDemo.MaxPlaintextBytes(2048)} bytes " +
                       "(RSA cannot do bulk data — see comment in RsaDemo.cs on hybrid RSA+AES / why TLS uses it).");

    // Task 3.14 — see CRYPTO-NOTES.md for the written paragraphs.
    Console.WriteLine("[3.14] Authn vs authz, session/cookie vs token — see CRYPTO-NOTES.md.");

    // Task 3.15
    var userStore = new UserStore();
    const string jwtSecret = "demo-only-secret-do-not-use-in-real-systems";

    bool teacherLoginOk = TryLogin(userStore, "alice.teacher", "TeacherPass123!", jwtSecret, out string? teacherToken);
    bool studentLoginOk = TryLogin(userStore, "bob.student", "StudentPass123!", jwtSecret, out string? studentToken);
    Console.WriteLine($"[3.15] Teacher login issued a token? {teacherLoginOk}");
    Console.WriteLine($"[3.15] Student login issued a token? {studentLoginOk}");

    bool teacherAllowed = RequireRole(teacherToken!, "Teacher", jwtSecret);
    bool studentDenied = !RequireRole(studentToken!, "Teacher", jwtSecret);
    Console.WriteLine($"[3.15] Teacher token passes [Authorize(Roles=\"Teacher\")] check (200)? {teacherAllowed}");
    Console.WriteLine($"[3.15] Student token is rejected by the same check (403)? {studentDenied}");
    Console.WriteLine("[3.15] The real HTTP 200/403 proof lives in SecureFileVault.AuthApi + the integration tests " +
                       "(AuthorizationRoleTests) — this is the same role-check logic exercised directly.");
}

static bool TryLogin(UserStore userStore, string username, string password, string jwtSecret, out string? token)
{
    token = null;
    var user = userStore.FindByUsername(username);
    if (user is null || !PasswordHasher.Verify(password, user.PasswordHash)) return false;

    token = MinimalJwt.Issue(user.Username, user.Role, TimeSpan.FromHours(1), jwtSecret);
    return true;
}

static bool RequireRole(string token, string requiredRole, string jwtSecret)
{
    if (!MinimalJwt.TryValidate(token, jwtSecret, out var payload) || payload is null) return false;
    return string.Equals(payload.Role, requiredRole, StringComparison.Ordinal);
}
