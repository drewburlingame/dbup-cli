using System.Data;
using DbUp.Engine;

namespace DbUp.Cli.Tests.RecordingDb;

internal class ScriptReader(SqlScript[] runScripts) : IDataReader
{
    private int currentIndex = -1;

    public bool Read()
    {
        if (runScripts == null)
            return false;
        currentIndex++;
        return runScripts.Length > currentIndex;
    }

    public void Dispose()
    {
    }
    
    // ReSharper disable UnassignedGetOnlyAutoProperty
    public int FieldCount { get; }
    public int Depth { get; }
    public bool IsClosed { get; }
    public int RecordsAffected { get; }

    public object this[int i] => runScripts[currentIndex].Name;
    
    public object this[string name] => throw new NotImplementedException();
    public bool GetBoolean(int i) => throw new NotImplementedException();
    public byte GetByte(int i) => throw new NotImplementedException();
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
    public char GetChar(int i) => throw new NotImplementedException();
    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
    public IDataReader GetData(int i) => throw new NotImplementedException();
    public string GetDataTypeName(int i) => throw new NotImplementedException();
    public DateTime GetDateTime(int i) => throw new NotImplementedException();
    public decimal GetDecimal(int i) => throw new NotImplementedException();
    public double GetDouble(int i) => throw new NotImplementedException();
    public Type GetFieldType(int i) => throw new NotImplementedException();
    public float GetFloat(int i) => throw new NotImplementedException();
    public Guid GetGuid(int i) => throw new NotImplementedException();
    public short GetInt16(int i) => throw new NotImplementedException();
    public int GetInt32(int i) => throw new NotImplementedException();
    public long GetInt64(int i) => throw new NotImplementedException();
    public string GetName(int i) => throw new NotImplementedException();
    public int GetOrdinal(string name) => throw new NotImplementedException();
    public string GetString(int i) => throw new NotImplementedException();
    public object GetValue(int i) => throw new NotImplementedException();
    public int GetValues(object[] values) => throw new NotImplementedException();
    public bool IsDBNull(int i) => throw new NotImplementedException();
    public void Close() => throw new NotImplementedException();
    public DataTable GetSchemaTable() => throw new NotImplementedException();
    public bool NextResult() => throw new NotImplementedException();
}