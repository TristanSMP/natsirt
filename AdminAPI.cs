using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace Natsirt;

public class AdminAPI : HttpClient
{
    public AdminAPI(IConfiguration configuration)
    {
        DefaultRequestHeaders.Accept.Clear();
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        DefaultRequestHeaders.Add("Authorization", configuration["ADMIN_API_TOKEN"]);
    }
}