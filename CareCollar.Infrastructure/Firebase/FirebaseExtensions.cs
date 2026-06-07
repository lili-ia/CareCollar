using CareCollar.Application.Contracts;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareCollar.Infrastructure.Firebase;

public static class FirebaseExtensions
{
    public static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        var credentialsPath = configuration["Firebase:CredentialsPath"];

        GoogleCredential credential;
        if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
            credential = GoogleCredential.FromFile(credentialsPath);
        else
            credential = GoogleCredential.GetApplicationDefault();

        FirebaseApp.Create(new AppOptions { Credential = credential });

        services.AddSingleton<IFcmService, FcmService>();
        return services;
    }
}
