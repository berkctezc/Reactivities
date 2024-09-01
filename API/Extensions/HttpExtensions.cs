using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace API.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeader(this HttpResponse response, int currentPage,
        int itemsPerPage, int totalItems, int totalPages)
    {
        var paginationHeader = new
        {
            currentPage,
            itemsPerPage,
            totalItems,
            totalPages
        };
        response.Headers.TryAdd("Pagination", JsonSerializer.Serialize(paginationHeader));
        response.Headers.TryAdd("Access-Control-Expose-Headers", "Pagination");
    }
}