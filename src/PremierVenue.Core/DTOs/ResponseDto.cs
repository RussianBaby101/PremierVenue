namespace PremierVenue.Core.DTOs;

public class ResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ResponseDto<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ResponseDto<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

public class PagedResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public static PagedResponseDto<T> Create(List<T> data, int currentPage, int pageSize, int totalCount)
    {
        return new PagedResponseDto<T>
        {
            Success = true,
            Message = "Data retrieved successfully",
            Data = data,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}