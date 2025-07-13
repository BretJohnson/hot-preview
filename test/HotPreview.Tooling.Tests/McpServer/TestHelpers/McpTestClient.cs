using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HotPreview.Tooling.Tests.McpServer.TestHelpers;

public class McpTestClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpTestClient> _logger;
    private bool _disposed;

    public McpTestClient(HttpClient httpClient, ILogger<McpTestClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<JsonDocument> SendRequestAsync(object request, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request);
        
        _logger.LogDebug("Sending MCP request: {Request}", json);
        
        // Try different possible MCP endpoints in order of preference
        var endpoints = new[] { "/mcp", "/", "/sse" };
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogDebug("Received MCP response from {Endpoint}: {Response}", endpoint, responseContent);
                    return JsonDocument.Parse(responseContent);
                }
                
                _logger.LogDebug("Endpoint {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to call endpoint {Endpoint}: {Error}", endpoint, ex.Message);
            }
        }
        
        // If all endpoints fail, try the root with a GET (in case it's configured differently)
        try
        {
            var response = await _httpClient.GetAsync("/", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Received response from GET /: {Response}", responseContent);
                
                // If it's HTML or plain text, it means MCP endpoints aren't configured properly
                if (responseContent.Contains("<!DOCTYPE") || responseContent.Contains("<html"))
                {
                    throw new InvalidOperationException("MCP endpoints not properly configured - received HTML response");
                }
                
                return JsonDocument.Parse(responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("GET / also failed: {Error}", ex.Message);
        }
        
        throw new InvalidOperationException("No working MCP endpoint found. The MCP server may not be properly configured.");
    }

    public async Task<T?> SendRequestAsync<T>(object request, CancellationToken cancellationToken = default)
    {
        using var response = await SendRequestAsync(request, cancellationToken);
        return JsonSerializer.Deserialize<T>(response.RootElement);
    }

    public async Task<JsonDocument> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/list",
            @params = new { }
        };
        
        return await SendRequestAsync(request, cancellationToken);
    }

    public async Task<JsonDocument> CallToolAsync(string toolName, object parameters, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments = parameters
            }
        };
        
        return await SendRequestAsync(request, cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}