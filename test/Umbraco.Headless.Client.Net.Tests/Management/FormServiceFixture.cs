using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;
using RichardSzalay.MockHttp;
using Umbraco.Headless.Client.Net.Configuration;
using Umbraco.Headless.Client.Net.Management;
using Xunit;

namespace Umbraco.Headless.Client.Net.Tests.Management
{
    public class FormServiceFixture
    {
         private readonly MockHttpMessageHandler _mockHttp;
         private readonly IHeadlessConfiguration _configuration = new FakeHeadlessConfiguration();

         public FormServiceFixture()
         {
             _mockHttp = new MockHttpMessageHandler();
         }

         [Fact]
         public async Task GetRoot_ReturnsAllForms()
         {
             var httpClient = GetMockedHttpClient("/forms", FormServiceJson.AtRoot);
             var service = CreateService(httpClient);

             var forms = await service.GetAll();

             Assert.NotNull(forms);
             Assert.Single(forms);
         }

         [Fact]
         public async Task GetById_ReturnsSingleForm()
         {
             var id = new Guid("2edaf583-cf66-4d57-930c-f0772c3d1c52");
             var httpClient = GetMockedHttpClient($"/forms/{id}", FormServiceJson.ById);
             var service = CreateService(httpClient);

             var form = await service.GetById(id);

             Assert.NotNull(form);
             Assert.Equal(id, form.Id);
         }

         [Fact]
         public async Task SubmitEntry_PostsObjectDataAsJson()
         {
             var id = new Guid("2edaf583-cf66-4d57-930c-f0772c3d1c52");
             var httpClient = new HttpClient(_mockHttp) { BaseAddress = new Uri(Constants.Urls.BaseApiUrl)};
             var service = CreateService(httpClient);

             var data = new
             {
                 name = "John Smith",
                 email = "johnsmith@example.org",
                 dataConsent = true
             };

             var dataJson = JsonConvert.SerializeObject(data);

             _mockHttp.Expect(HttpMethod.Post, $"/forms/{id}/entries")
                 .WithContent(dataJson)
                 .Respond(HttpStatusCode.Accepted);

             await service.SubmitEntry(id, data);
         }

         [Fact]
         public async Task SubmitEntry_PostsDictionaryAsJson()
         {
             var id = new Guid("2edaf583-cf66-4d57-930c-f0772c3d1c52");
             var httpClient = new HttpClient(_mockHttp) { BaseAddress = new Uri(Constants.Urls.BaseApiUrl)};
             var service = CreateService(httpClient);

             var data = new Dictionary<string, object>
             {
                 {"name", "John Smith"},
                 {"email", "johnsmith@example.org"},
                 {"dataConsent", true}
             };

             var dataJson = JsonConvert.SerializeObject(data);

             _mockHttp.Expect(HttpMethod.Post, $"/forms/{id}/entries")
                 .WithContent(dataJson)
                 .Respond(HttpStatusCode.Accepted);

             await service.SubmitEntry(id, data);
         }

         private HttpClient GetMockedHttpClient(string url, string jsonResponse)
         {
             _mockHttp.When(url).Respond("application/json", jsonResponse);
             var client = new HttpClient(_mockHttp) { BaseAddress = new Uri(Constants.Urls.BaseApiUrl) };
             return client;
         }

         private FormService CreateService(HttpClient client) =>
             new FormService(_configuration, client, new RefitSettings());
    }
}
