using BuildingBlocks.Contracts.Services;
using System.Text.Json;

namespace UserManagement.API.Services;

public class WalletPaymentService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<WalletPaymentService> logger
) : IWalletPaymentService
{
    private readonly string _walletServiceBaseUrl = configuration["ServiceUrls:WalletPayment"] ?? "http://localhost:5233";
    private readonly string _serviceToken = configuration["ServiceAuthentication:InternalToken"] ?? "";

    public async Task<bool> CreateWalletAsync(Guid userId, string defaultCurrency = "IRR")
    {
        try
        {
            logger.LogInformation("Creating wallet for user {UserId} with default currency {Currency}", userId, defaultCurrency);

            var httpClient = httpClientFactory.CreateClient("WalletService");
            httpClient.BaseAddress = new Uri(_walletServiceBaseUrl);

            // ✅ Service-to-Service Authentication
            httpClient.DefaultRequestHeaders.Add("X-Service-Token", _serviceToken);
            httpClient.DefaultRequestHeaders.Add("X-Service-Name", "UserManagement");

            var request = new
            {
                UserId = userId,
                DefaultCurrency = defaultCurrency
            };

            // ✅ Internal endpoint فراخوانی می‌شه
            var response = await httpClient.PostAsJsonAsync("/internal/wallets/create-for-user", request);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Wallet created successfully for user {UserId}", userId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to create wallet for user {UserId}. Status: {StatusCode}, Error: {Error}",
                userId, response.StatusCode, errorContent);

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while creating wallet for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> HasWalletAsync(Guid userId)
    {
        try
        {
            logger.LogInformation("Checking wallet existence for user {UserId}", userId);

            var httpClient = httpClientFactory.CreateClient("WalletService");
            httpClient.BaseAddress = new Uri(_walletServiceBaseUrl);

            // ✅ Service-to-Service Authentication
            httpClient.DefaultRequestHeaders.Add("X-Service-Token", _serviceToken);
            httpClient.DefaultRequestHeaders.Add("X-Service-Name", "UserManagement");

            // ✅ Internal endpoint فراخوانی می‌شه
            var response = await httpClient.GetAsync($"/internal/wallets/user/{userId}/exists");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var existsResponse = JsonSerializer.Deserialize<WalletExistsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return existsResponse?.Exists ?? false;
            }

            logger.LogWarning("Failed to check wallet existence for user {UserId}. Status: {StatusCode}",
                userId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while checking wallet existence for user {UserId}", userId);
            return false;
        }
    }

    // ✅ سایر متدها - این متدها معمولاً از طرف کاربر مستقیماً به Wallet Service فراخوانی می‌شن
    // فقط برای backward compatibility نگه داشته می‌شن
    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        logger.LogWarning("GetBalanceAsync called from UserManagement - This should be called directly from client to WalletService");
        // در production می‌تونیم exception بندازیم یا redirect کنیم
        throw new NotSupportedException("این عملیات باید مستقیماً از کلاینت به سرویس کیف پول فراخوانی شود");
    }

    public async Task<bool> DepositAsync(Guid userId, decimal amount, string description)
    {
        logger.LogWarning("DepositAsync called from UserManagement - This should be called directly from client to WalletService");
        throw new NotSupportedException("این عملیات باید مستقیماً از کلاینت به سرویس کیف پول فراخوانی شود");
    }

    public async Task<bool> WithdrawAsync(Guid userId, decimal amount, string description)
    {
        logger.LogWarning("WithdrawAsync called from UserManagement - This should be called directly from client to WalletService");
        throw new NotSupportedException("این عملیات باید مستقیماً از کلاینت به سرویس کیف پول فراخوانی شود");
    }

    public async Task<bool> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description)
    {
        logger.LogWarning("TransferAsync called from UserManagement - This should be called directly from client to WalletService");
        throw new NotSupportedException("این عملیات باید مستقیماً از کلاینت به سرویس کیف پول فراخوانی شود");
    }

    public async Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(Guid userId, int pageNumber, int pageSize)
    {
        logger.LogWarning("GetTransactionHistoryAsync called from UserManagement - This should be called directly from client to WalletService");
        throw new NotSupportedException("این عملیات باید مستقیماً از کلاینت به سرویس کیف پول فراخوانی شود");
    }
}

public sealed record WalletExistsResponse(bool Exists);