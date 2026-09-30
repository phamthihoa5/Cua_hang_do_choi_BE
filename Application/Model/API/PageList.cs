using Microsoft.EntityFrameworkCore;

namespace Application.Model.API;

public class PageList<T>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public IEnumerable<T> Items { get; private set; }
    public bool? HasFileList { get; set; } = false;
    public int? CheckList { get; set; } = 1;
    public ICollection<Guid>? FileIds { get; set; }

    public PageList(IEnumerable<T> items, int count, int pageNumber, int pageSize, bool hasFileList = false, int checkList = 1, ICollection<Guid>? fileIds = null)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
        HasFileList = hasFileList;
        CheckList = checkList;
        FileIds = fileIds;
    }

    public static PageList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();

        if (pageSize <= 0)
        {
            return new PageList<T>(source.ToList(), count, 1, count);
        }
        if (pageNumber < 1) pageNumber = 1;

        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PageList<T>(items, count, pageNumber, pageSize);
    }

    public static async Task<PageList<T>> ToPagedListAsync(IQueryable<T> source, int pageNumber, int pageSize, bool hasFileList = false, int checkList = 1, ICollection<Guid> fileIds = null)
    {
        var count = source.Count();

        if (pageSize <= 0)
        {
            return new PageList<T>(await source.ToListAsync(), count, 1, count);
        }
        if (pageNumber < 1) pageNumber = 1;

        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PageList<T>(items, count, pageNumber, pageSize, hasFileList, checkList, fileIds);
    }
}