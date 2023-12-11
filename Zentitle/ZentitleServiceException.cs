using Newtonsoft.Json;

namespace ZentitleSaaSDemo.Zentitle;

public class ZentitleServiceException
{
    private readonly bool _json;
    private readonly string _response;
    public string FormattedString => FormatJson();
    public int StatusCode { get; private set; }

    public ZentitleServiceException(Exception exception)
    {
        StatusCode = 500;
        _response = exception.Message;
    }

    public ZentitleServiceException(ApiException exception)
    {
        StatusCode = exception.StatusCode;
        _response = exception.Response;
        _json = true;
    }

    private string FormatJson()
    {
        if (!_json) return _response;
        try
        {
            dynamic? parsedJson = JsonConvert.DeserializeObject(_response);
            return parsedJson != null ? JsonConvert.SerializeObject(parsedJson, Formatting.Indented) : string.Empty;
        }
        catch
        {
            //do nothing
        }

        return string.Empty;
    }
}