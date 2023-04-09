namespace DotMovie.Dtos;

public class FilterMoviesDto
{
    public int Page { get; set; }
    public int RecordsPerPage { get; set; }
    public PaginationDto PaginationDto => new PaginationDto() { Page = Page, RecordsPerPage = RecordsPerPage };
    public string Title { get; set; }
    public int GenreId { get; set; }
    public bool InTheaters { get; set; }
    public bool UpcomingReleases { get; set; }
}