using System;
using System.Net.Http;
using System.Web.Http;
using Xunit;

namespace Bugsnag.AspNet.WebApi.Tests
{
  public class WebHostTests
  {
    public class TestController : ApiController
    {
      [HttpGet]
      public IHttpActionResult Test()
      {
        Request.BugsnagClient().Breadcrumbs.Leave("Bugsnag is great!");
        throw new NotImplementedException("Because it lets me know about exceptions");
      }
    }

    [Fact]
    public async void Test()
    {
      //var bugsnagServer = new TestServer(1);

      //bugsnagServer.Start();

      var configuration = new HttpConfiguration();
      configuration.Routes.MapHttpRoute("Default", "api/{controller}");
      configuration.UseBugsnag(new Configuration("wow"));
      //configuration.UseBugsnag(new Configuration("wow") { Endpoint = bugsnagServer.Endpoint });
      var webApiServer = new HttpServer(configuration);

      var client = new HttpClient(webApiServer);

      var request = new HttpRequestMessage() { RequestUri = new Uri("http://www.bugsnag.com/api/test") };

      var response = await client.SendAsync(request);

      //var responses = await bugsnagServer.Requests();

      //Assert.Single(responses);
      //Assert.Contains("Bugsnag is great!", responses.Single());
    }
  }
}