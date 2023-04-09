namespace DotMovie.Dtos;

public class PaginationDto
{
    public int Page { get; init; } = 1;

    private int recordsPerPage = 10;
    private readonly int maxAmount = 50;

    public int RecordsPerPage
    {
        get
        {
            return recordsPerPage;
        }
        set
        {
            recordsPerPage = (value > maxAmount) ? maxAmount : value;
        }
    }
}