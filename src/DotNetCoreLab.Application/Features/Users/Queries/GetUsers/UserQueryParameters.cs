namespace DotNetCoreLab.Application.Features.Users.Queries.GetUsers;

public class UserQueryParameters
{
    public int PageNumber { get; set; } = 1;
    
    private int _pageSize = 10;
    const int maxPageSize = 50; 

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
    }

    public string? SearchTerm { get; set; } 
}