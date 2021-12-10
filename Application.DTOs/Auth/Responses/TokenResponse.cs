public record TokenResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);
//同等下方
//    public class TokenResponse()
//    {
//        public string Token { get; set; }
//    public string RefreshToken { get; set; }  
//    public DateTime RefreshTokenExpiryTime { get; set; }
//}