
namespace Application.Common.Models;

public class QueryParam
{
    public string? Search { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public int? Type { get; set; }
    public IList<ApiSortQuery> Sorts { get; set; } = new List<ApiSortQuery>();
}

public class QueryParam2
{
    public IList<ApiSearchQuery>? Search { get; set; } = new List<ApiSearchQuery>();
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public IList<ApiSortQuery> Sorts { get; set; } = new List<ApiSortQuery>();
}

public class ProductFilterParam
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public List<Guid>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }    // giá tối thiểu
    public decimal? MaxPrice { get; set; }    // giá tối đa

    public string? SortBy { get; set; }       // "price" | "name"
    public bool SortAsc { get; set; } = true; // true = tăng dần
}



public class ApiSortQuery
{
    public string Key { get; set; } = "";
    public int Sort { get; set; }
}


public class ApiSearchQuery
{
    public string Key { get; set; } = "";
    public string Value { get; set; }
}


public class Test
{
    public Guid Id { get; set; }
    public List<string> Values { get; set; }
}
