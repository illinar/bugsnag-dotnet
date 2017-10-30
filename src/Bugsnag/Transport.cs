using System;
using System.Net;
using System.Threading;

namespace Bugsnag
{
  public class Transport
  {
    private class TransportState
    {
      public AsyncCallback Callback { get; private set; }

      public object State { get; private set; }

      public Uri Endpoint { get; private set; }

      public byte[] Notification { get; private set; }

      public WebRequest Request { get; private set; }

      public HttpWebResponse Response { get; set; }

      public TransportState(AsyncCallback callback, object state, Uri endpoint, byte[] notification, WebRequest request)
      {
        Callback = callback;
        State = state;
        Endpoint = endpoint;
        Notification = notification;
        Request = request;
      }
    }

    private class TransportAsyncResult : IAsyncResult
    {
      public bool IsCompleted { get { return _innerAsyncResult.IsCompleted; } }

      public WaitHandle AsyncWaitHandle { get { return _innerAsyncResult.AsyncWaitHandle; } }

      public object AsyncState { get { return _state; } }

      public bool CompletedSynchronously { get { return _innerAsyncResult.CompletedSynchronously; } }

      private readonly IAsyncResult _innerAsyncResult;
      private readonly object _state;

      public TransportAsyncResult(IAsyncResult innerAsyncResult, object state)
      {
        _innerAsyncResult = innerAsyncResult;
        _state = state;
      }
    }

    public IAsyncResult BeginSend(Uri endpoint, byte[] notification, AsyncCallback callback, object state)
    {
      var request = WebRequest.Create(endpoint);
      request.Method = "POST";
      request.ContentType = "application/json";
      var internalState = new TransportState(callback, state, endpoint, notification, request);
      var asyncResult = request.BeginGetRequestStream(new AsyncCallback(WriteCallback), internalState);
      return new TransportAsyncResult(asyncResult, state);
    }

    public HttpStatusCode EndSend(IAsyncResult asyncResult)
    {
      var state = (TransportState)asyncResult.AsyncState;

      if (state.Response != null)
      {
        return state.Response.StatusCode;
      }
      else
      {
        return 0;
      }
    }

    private void ReadCallback(IAsyncResult asynchronousResult)
    {
      var state = (TransportState)asynchronousResult.AsyncState;
      try
      {
        state.Response = (HttpWebResponse)state.Request.EndGetResponse(asynchronousResult);
      }
      catch (WebException exception)
      {
        state.Response = exception.Response as HttpWebResponse;
      }

      state.Callback(new TransportAsyncResult(asynchronousResult, state));
    }

    private void WriteCallback(IAsyncResult asynchronousResult)
    {
      var state = (TransportState)asynchronousResult.AsyncState;
      try
      {
        using (var stream = state.Request.EndGetRequestStream(asynchronousResult))
        {
          stream.Write(state.Notification, 0, state.Notification.Length);
        }
      }
      catch (WebException exception)
      {
        state.Response = exception.Response as HttpWebResponse;
      }

      state.Request.BeginGetResponse(new AsyncCallback(ReadCallback), state);
    }
  }
}