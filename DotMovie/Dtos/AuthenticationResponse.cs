namespace DotMovie.Dtos;

public class AuthenticationResponse
{
    public string Token { get; set; }
    public DateTime expirationTime { get; set; }
}