using BuildingBlocks.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices;
public class UserManagementServiceClient : IUserManagementService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public UserManagementServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/users/{userId}/details");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDetailDto>(content, _options)
               ?? throw new InvalidOperationException("خطا در دریافت اطلاعات کاربر");
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/users/{userId}/exists");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsUserActiveAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/users/{userId}/is-active");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(content, _options);
    }

    public async Task<bool> ValidateCredentialsAsync(string mobile, string password)
    {
        var request = new { Mobile = mobile, Password = password };
        var response = await _httpClient.PostAsJsonAsync("/auth/validate-credentials", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<TokenResponseDto> AuthenticateAsync(AuthRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponseDto>(_options)
               ?? throw new InvalidOperationException("خطا در احراز هویت");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var request = new { Token = token };
        var response = await _httpClient.PostAsJsonAsync("/auth/validate-token", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode)
    {
        var request = new { UserId = userId, Permission = permissionCode };
        var response = await _httpClient.PostAsJsonAsync("/permissions/check", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(content, _options);
        return result?.IsGranted ?? false;
    }
}
