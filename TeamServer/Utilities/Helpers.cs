using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TeamServer.Utilities;

public static class Helpers
{
    public static string GenerateShortGuid()
    {
        return Guid.NewGuid()
            .ToString()
            .Replace("-", "")[..10];
    }
    
    public static async Task<byte[]> GetEmbeddedResource(string name)
    {
        var self = Assembly.GetExecutingAssembly();
        await using var rs = self.GetManifestResourceStream($"TeamServer.Resources.{name}");

        if (rs is null)
            return Array.Empty<byte>();

        await using var ms = new MemoryStream();
        await rs.CopyToAsync(ms);

        return ms.ToArray();
    }

    public static async Task CreateCertificate(IPAddress address, string keyFile, string certFile)
    {
        var directory = Directory.GetCurrentDirectory();

        // if already exist, print thumbprint and return
        if (File.Exists(Path.Combine(directory, keyFile)) && File.Exists(Path.Combine(directory, certFile)))
        {
            var raw = await File.ReadAllBytesAsync(Path.Combine(directory, keyFile));
            var x509 = new X509Certificate2(raw);
            
            Console.WriteLine($"Certificate thumbprint: {x509.Thumbprint}");
            return;
        }

        // otherwise, create new self-signed cert
        var distinguishedName = $"CN={address}";

        using var rsa = RSA.Create(2048);

        var request = new CertificateRequest(
            new X500DistinguishedName(distinguishedName), rsa,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature,
            false));

        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false));

        var subjectAlternativeName = new SubjectAlternativeNameBuilder();
        subjectAlternativeName.AddIpAddress(address);

        request.CertificateExtensions.Add(subjectAlternativeName.Build());

        var cert = request.CreateSelfSigned(
            new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
            new DateTimeOffset(DateTime.UtcNow.AddDays(365)));

        await File.WriteAllBytesAsync(
            Path.Combine(directory, keyFile),
            cert.Export(X509ContentType.Pkcs12));

        await File.WriteAllBytesAsync(
            Path.Combine(directory, certFile),
            cert.Export(X509ContentType.Cert));
        
        Console.WriteLine($"Certificate thumbprint: {cert.Thumbprint}");
    }
    
    public static byte[] GeneratePasswordHash(string password, out byte[] salt)
    {
        using var pbkdf = new Rfc2898DeriveBytes(
            password,
            16,
            50000,
            HashAlgorithmName.SHA256);
        
        salt = pbkdf.Salt;
        return pbkdf.GetBytes(32);
    }

    public static byte[] GeneratePasswordHash(string password, byte[] salt)
    {
        using var pbkdf = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            salt,
            50000,
            HashAlgorithmName.SHA256);

        return pbkdf.GetBytes(32);
    }
}