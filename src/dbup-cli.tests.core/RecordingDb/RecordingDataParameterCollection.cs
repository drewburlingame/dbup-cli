using System.Collections;
using System.Data;

namespace DbUp.Cli.Tests.RecordingDb;

public class RecordingDataParameterCollection(CaptureLogsLogger logger) : IDataParameterCollection
{
    private readonly List<object> backingList = new();
    
    public int Add(object value)
    {
        logger.LogDbOperation($"DB Operation: Add parameter to command: {value}");
        backingList.Add(value);
        return backingList.Count - 1;
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Local
    public int Count { get; private set; }
    public object SyncRoot { get; private set; }
    public bool IsSynchronized { get; private set; }
    public bool IsReadOnly { get; private set; }
    public bool IsFixedSize { get; private set; }

    public IEnumerator GetEnumerator() => throw new NotImplementedException();
    public void CopyTo(Array array, int index) => throw new NotImplementedException();
    public bool Contains(object value) => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public int IndexOf(object value) => throw new NotImplementedException();
    public void Insert(int index, object value) => throw new NotImplementedException();
    public void Remove(object value) => throw new NotImplementedException();
    public void RemoveAt(int index) => throw new NotImplementedException();
    public bool Contains(string parameterName) => throw new NotImplementedException();
    public int IndexOf(string parameterName) => throw new NotImplementedException();
    public void RemoveAt(string parameterName) => throw new NotImplementedException();
    object IList.this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    object IDataParameterCollection.this[string parameterName]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}