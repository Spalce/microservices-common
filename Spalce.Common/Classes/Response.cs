using System.Collections.Generic;

namespace Spalce.Common.Classes;

public class Response<T>
{
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
    public bool IsSuccess { get; set; }
}
