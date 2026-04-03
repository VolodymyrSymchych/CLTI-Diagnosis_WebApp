using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace CLTI.Diagnosis.Infrastructure.Services;

/// <summary>
/// Сервіс для хешування паролів з використанням сучасних стандартів безпеки.
/// Використовує PBKDF2-SHA256 з конфігурованою кількістю ітерацій.
/// Підтримує міграцію зі старих алгоритмів (BCrypt, MD5) до нового стандарту.
/// 
/// Примітки щодо безпеки:
/// - Використовує криптографічно безпечний генератор випадкових чисел для солі
/// - Constant-time comparison для запобігання timing attacks
/// - Автоматична міграція старих паролів при наступному вході
/// - MD5 підтримується тільки для міграції (застарілий алгоритм)
/// 
/// Примітка: Для нових проектів NIST рекомендує використовувати Argon2id,
/// але PBKDF2-SHA256 з достатньою кількістю ітерацій також відповідає стандартам.
/// </summary>
public interface IPasswordHasherService
{
    /// <summary>
    /// Хешує пароль з використанням PBKDF2-SHA256
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Перевіряє пароль проти хешу. Підтримує автоматичну міграцію зі старих алгоритмів.
    /// </summary>
    bool VerifyPassword(string password, string hash, string? hashType = null);

    /// <summary>
    /// Перевіряє пароль та визначає чи потрібна міграція на новий алгоритм
    /// </summary>
    (bool IsValid, bool NeedsMigration) VerifyPasswordWithMigration(string password, string hash, string? hashType = null);
}

public class PasswordHasherService : IPasswordHasherService
{
    // This value is intentionally configurable. Render's shared CPUs make 1M PBKDF2
    // iterations too slow for interactive login, so we target a lower but still
    // non-trivial work factor and migrate hashes on successful login.
    private const int DefaultIterationCount = 100_000;
    private readonly int _iterationCount;
    private const int SaltSize = 16; // 128 біт (мінімум 16 байт рекомендовано NIST)
    private const int HashSize = 32; // 256 біт (SHA256)

    public PasswordHasherService(IConfiguration configuration)
    {
        var configuredIterations = configuration.GetValue<int?>("PasswordHashing:TargetIterations");
        _iterationCount = configuredIterations.GetValueOrDefault(DefaultIterationCount);
        if (_iterationCount < 50_000)
        {
            _iterationCount = 50_000;
        }
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Генеруємо випадкову сіль
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Хешуємо пароль з PBKDF2-SHA256
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            _iterationCount,
            HashAlgorithmName.SHA256,
            HashSize
        );

        // Format: $pbkdf2-sha256$v=1$iterations=<n>$salt$hash (base64)
        var saltBase64 = Convert.ToBase64String(salt);
        var hashBase64 = Convert.ToBase64String(hash);

        return $"$pbkdf2-sha256$v=1$iterations={_iterationCount}${saltBase64}${hashBase64}";
    }

    public bool VerifyPassword(string password, string hash, string? hashType = null)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        var (isValid, _) = VerifyPasswordWithMigration(password, hash, hashType);
        return isValid;
    }

    public (bool IsValid, bool NeedsMigration) VerifyPasswordWithMigration(
        string password, 
        string hash, 
        string? hashType = null)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return (false, false);

        // Перевіряємо тип хешу
        hashType ??= DetectHashType(hash);

        return hashType switch
        {
            "PBKDF2-SHA256" or null when hash.StartsWith("$pbkdf2-sha256$") => VerifyPbkdf2Hash(password, hash),
            "BCrypt" when hash.StartsWith("$2") => VerifyBcryptHash(password, hash),
            "MD5" => VerifyMd5Hash(password, hash),
            _ => VerifyPbkdf2Hash(password, hash) // За замовчуванням намагаємося PBKDF2
        };
    }

    private (bool IsValid, bool NeedsMigration) VerifyPbkdf2Hash(string password, string hash)
    {
        try
        {
            // Format: $pbkdf2-sha256$v=1$iterations=<n>$salt$hash
            var parts = hash.Split('$');
            if (parts.Length < 6 || parts[1] != "pbkdf2-sha256")
                return (false, false);

            var iterations = int.Parse(parts[3].Split('=')[1]);
            var salt = Convert.FromBase64String(parts[4]);
            var expectedHash = Convert.FromBase64String(parts[5]);

            // Обчислюємо хеш
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                HashSize
            );

            // Порівнюємо хеші (constant-time comparison для запобігання timing attacks)
            var isValid = CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
            
            // Rehash whenever the stored work factor differs from the current target.
            var needsMigration = isValid && iterations != _iterationCount;

            return (isValid, needsMigration);
        }
        catch
        {
            return (false, false);
        }
    }

    private (bool IsValid, bool NeedsMigration) VerifyBcryptHash(string password, string hash)
    {
        try
        {
            // Використовуємо BCrypt для перевірки (для міграції старих паролів)
            // Тимчасова залежність BCrypt.Net-Next буде видалена після завершення міграції
            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            
            // Якщо пароль валідний, потрібна міграція на PBKDF2
            return (isValid, isValid);
        }
        catch
        {
            return (false, false);
        }
    }

    private (bool IsValid, bool NeedsMigration) VerifyMd5Hash(string password, string hash)
    {
        // MD5 застарілий та небезпечний - використовується тільки для міграції
        // ВАЖЛИВО: MD5 не має constant-time comparison, але це прийнятно тільки для міграції
        try
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = md5.ComputeHash(inputBytes);
            var computedHashBytes = Encoding.UTF8.GetBytes(Convert.ToHexString(hashBytes).ToLower());
            var expectedHashBytes = Encoding.UTF8.GetBytes(hash.ToLower());

            // Використовуємо constant-time comparison для безпеки
            // Порівнюємо байти замість рядків
            var isValid = computedHashBytes.Length == expectedHashBytes.Length &&
                         CryptographicOperations.FixedTimeEquals(
                             computedHashBytes, 
                             expectedHashBytes);

            return (isValid, isValid); // Якщо MD5, завжди потрібна міграція
        }
        catch
        {
            return (false, false);
        }
    }

    private static string DetectHashType(string hash)
    {
        if (hash.StartsWith("$pbkdf2-sha256$"))
            return "PBKDF2-SHA256";
        
        if (hash.StartsWith("$2") || hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2x$") || hash.StartsWith("$2y$"))
            return "BCrypt";
        
        if (hash.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(hash, @"^[a-f0-9]{32}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "MD5";
        
        return "Unknown";
    }

}
