using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Bugsnag
{
  /// <summary>
  /// Used by Bugsnag clients to send serialized error reports to an endpoint.
  /// </summary>
  public interface ITransport
  {
    void Send(IPayload payload);
  }

  public interface IPayload : IDictionary
  {
    Uri Endpoint { get; }

    IWebProxy Proxy { get; }

    KeyValuePair<string, string>[] Headers { get; }
  }
}
