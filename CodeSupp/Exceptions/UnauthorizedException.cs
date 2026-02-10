namespace CodeSupp.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        // Varsayılan mesaj desteği
        public UnauthorizedException()
            : base("Erişim reddedildi. Lütfen giriş yapınız.", 401)
        {
        }

        // Özel mesaj desteği
        // Örn: throw new UnauthorizedException("Token süresi dolmuş.");
        public UnauthorizedException(string message)
            : base(message, 401)
        {
        }
    }
}