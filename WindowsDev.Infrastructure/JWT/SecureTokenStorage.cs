using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using WindowsDev.Application.Identity;

namespace WindowsDev.Infrastructure.JWT;

internal class SecureTokenStorage : ISecureTokenStorage
{
    private readonly string _filePath;

    public SecureTokenStorage()
    {
        string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string companyName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "MyCompany";
        string productName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "MyProduct";
        string appFolder = Path.Combine(roamingPath, companyName, productName);

        Directory.CreateDirectory(appFolder);
        _filePath = Path.Combine(appFolder, "token.dat");

        LoadTokenFromDisk();
    }
    private string? _accessToken;
    public string? AccessToken
    {
        get => _accessToken;
        set
        {
            _accessToken = value;
            SaveTokenToDisk(value);
        }
    }

    public void Clear()
    {
        _accessToken = null;
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    private void SaveTokenToDisk(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return;

        byte[] data = Encoding.UTF8.GetBytes(token);
        byte[] encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);

        File.WriteAllBytes(_filePath, encryptedData);
    }

    private string? LoadTokenFromDisk()
    {
        if (!File.Exists(_filePath)) 
            return null;

        try
        {
            byte[] encryptedData = File.ReadAllBytes(_filePath);
            byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedData);
        }
        catch
        {
            Clear();
            return null;
        }
    }
}
