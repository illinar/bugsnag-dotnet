using System.Linq;
using Bugsnag.Payload;

namespace Bugsnag
{
  /// <summary>
  /// Signature for Bugsnag client middleware that can be used to manipulate the
  /// error report before it is sent.
  /// </summary>
  /// <param name="configuration"></param>
  /// <param name="report"></param>
  public delegate void Middleware(IConfiguration configuration, Report report);

  /// <summary>
  /// The middleware that is applied by default by the Bugsnag client.
  /// </summary>
  static class InternalMiddleware
  {
    /// <summary>
    /// Sets the Delivery flag to false if the configuration is setup so that
    /// the report should not be sent based on the release stage information.
    /// </summary>
    public static Middleware ReleaseStageFilter = (c, r) => {
      r.Deliver = r.Deliver && c.ValidReleaseStage();
    };

    /// <summary>
    /// Strips any provided file prefixes from stack trace lines included in the report.
    /// </summary>
    public static Middleware RemoveFilePrefixes = (configuration, report) =>
    {
      if (configuration.FilePrefixes.Any())
      {
        foreach (var @event in report.Events)
        {
          foreach (var exception in @event.Exceptions)
          {
            foreach (var stackTraceLine in exception.StackTrace)
            {
              foreach (var filePrefix in configuration.FilePrefixes)
              {
                if (stackTraceLine.FileName.StartsWith(filePrefix, System.StringComparison.Ordinal))
                {
                  stackTraceLine.FileName = stackTraceLine.FileName.Remove(0, filePrefix.Length);
                }
              }
            }
          }
        }
      }
    };

    /// <summary>
    /// Marks stack trace lines as being 'in project' if they are from a provided namespace.
    /// </summary>
    public static Middleware DetectInProjectNamespaces = (configuration, report) =>
    {
      if (configuration.ProjectNamespaces.Any())
      {
        foreach (var @event in report.Events)
        {
          foreach (var exception in @event.Exceptions)
          {
            foreach (var stackTraceLine in exception.StackTrace)
            {
              foreach (var @namespace in configuration.ProjectNamespaces)
              {
                stackTraceLine.InProject = stackTraceLine.MethodName.StartsWith(@namespace);
              }
            }
          }
        }
      }
    };

    /// <summary>
    /// Strips exceptions from the report if they include any 'ignored classes'
    /// </summary>
    public static Middleware RemoveIgnoredExceptions = (configuration, report) =>
    {
      if (configuration.IgnoreClasses.Any())
      {
        foreach (var @event in report.Events)
        {
          @event.Exceptions = @event.Exceptions.Where(e => !configuration.IgnoreClasses.Any(@class => @class == e.ErrorClass)).ToArray();
          // TODO: if we filter out all of the exceptions should we still send the report?
        }
      }
    };

    /// <summary>
    /// Attaches global metadata if provided by the configuration to each error report.
    /// </summary>
    public static Middleware AttachGlobalMetadata = (configuration, report) =>
    {
      if (configuration.GlobalMetadata != null)
      {
        foreach (var @event in report.Events)
        {
          foreach (var item in configuration.GlobalMetadata)
          {
            @event.Metadata.Add(item.Key, item.Value);
          }
        }
      }
    };
  }
}
