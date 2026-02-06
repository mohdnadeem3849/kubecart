namespace Catalog.Api.Models.Dtos;

public sealed class AddProductImageRequest
{
    public string Url { get; set; } = string.Empty;

    // true = primary image (SortOrder = 0)
    public bool IsPrimary { get; set; } = false;
}
