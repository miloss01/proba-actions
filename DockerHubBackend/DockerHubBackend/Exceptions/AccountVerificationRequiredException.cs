namespace DockerHubBackend.Exceptions
{
    public class AccountVerificationRequiredException : Exception
    {
        public string VerificationToken { get; set; }
        public AccountVerificationRequiredException(string message, string verificationToken) : base(message)
        {
            VerificationToken = verificationToken;
        }
    }
}
