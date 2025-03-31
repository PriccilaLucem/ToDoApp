using System;


namespace WebApplication.Config
{
    public static class Configuration
    {
        public static string PrivateKey
        {
            get
            {
                string privateKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                
                if (string.IsNullOrEmpty(privateKey))
                {
                    throw new InvalidOperationException("JWT_SECRET_KEY is not set.");
                }

                return privateKey;
            }
        }
    }
}
