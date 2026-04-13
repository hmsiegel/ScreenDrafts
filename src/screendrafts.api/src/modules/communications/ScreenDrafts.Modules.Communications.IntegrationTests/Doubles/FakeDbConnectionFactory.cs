using System.Collections;
using System.Diagnostics.CodeAnalysis;

// CS8765: Nullability of BCL abstract property setters (AllowNull) vs our override —
// we handle null → empty string at runtime; the warning is a false positive for test doubles.
#pragma warning disable CS8765

namespace ScreenDrafts.Modules.Communications.IntegrationTests.Doubles;

/// <summary>
/// In-memory IDbConnectionFactory that lets tests pre-seed query results and
/// inspect executed SQL without a real database. Avoids DataTable (IDisposable)
/// by using plain arrays throughout.
/// </summary>
internal sealed class FakeDbConnectionFactory : IDbConnectionFactory
{
  private readonly Queue<(string[] Columns, object?[][] Rows)> _results = new();
  private readonly List<string> _executedSql = [];

  public bool WasOpened { get; private set; }
  public IReadOnlyList<string> ExecutedSql => _executedSql;

  /// <summary>
  /// Enqueue a result set. The first argument is the column names; each subsequent
  /// argument is one row of values in the same column order.
  /// </summary>
  public void EnqueueQueryResult(string[] columns, params object?[][] rows)
    => _results.Enqueue((columns, rows));

  /// <summary>Enqueue an empty result (zero columns, zero rows).</summary>
  public void EnqueueEmptyResult() => _results.Enqueue(([], []));

#pragma warning disable CA2000 // caller takes ownership and disposes via await using
  public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
  {
    WasOpened = true;
    return ValueTask.FromResult<DbConnection>(new FakeDbConnection(_results, _executedSql));
  }
#pragma warning restore CA2000
}

internal sealed class FakeDbConnection(
  Queue<(string[] Columns, object?[][] Rows)> queryResults,
  List<string> executedSql) : DbConnection
{
  private string _connectionString = string.Empty;

  public override string ConnectionString
  {
    get => _connectionString;
    [param: AllowNull] set => _connectionString = value ?? string.Empty;
  }

  public override string Database => string.Empty;
  public override string DataSource => string.Empty;
  public override string ServerVersion => string.Empty;
  public override ConnectionState State => ConnectionState.Open;

  public override void Open() { }
  public override void Close() { }
  public override Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

  protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    => throw new NotSupportedException();

  public override void ChangeDatabase(string databaseName) { }

  protected override DbCommand CreateDbCommand()
    => new FakeDbCommand(queryResults, executedSql);
}

internal sealed class FakeDbCommand(
  Queue<(string[] Columns, object?[][] Rows)> queryResults,
  List<string> executedSql) : DbCommand
{
  private readonly FakeDbParameterCollection _parameters = new();
  private string _commandText = string.Empty;

  public override string CommandText
  {
    get => _commandText;
    [param: AllowNull] set => _commandText = value ?? string.Empty;
  }

  public override int CommandTimeout { get; set; }
  public override CommandType CommandType { get; set; } = CommandType.Text;
  public override bool DesignTimeVisible { get; set; }
  public override UpdateRowSource UpdatedRowSource { get; set; }
  protected override DbConnection? DbConnection { get; set; }
  protected override DbParameterCollection DbParameterCollection => _parameters;
  protected override DbTransaction? DbTransaction { get; set; }

  public override void Cancel() { }
  public override void Prepare() { }
  protected override DbParameter CreateDbParameter() => new FakeDbParameter();

  public override int ExecuteNonQuery()
  {
    executedSql.Add(_commandText);
    return 1;
  }

  public override object? ExecuteScalar()
  {
    executedSql.Add(_commandText);
    return null;
  }

  protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
  {
    executedSql.Add(_commandText);
    var (columns, rows) = queryResults.Count > 0 ? queryResults.Dequeue() : ([], []);
    return new FakeDbDataReader(columns, rows);
  }
}

internal sealed class FakeDbDataReader(string[] columns, object?[][] rows) : DbDataReader
{
  private int _rowIndex = -1;

  public override int FieldCount => columns.Length;
  public override bool HasRows => rows.Length > 0;
  public override bool IsClosed => false;
  public override int RecordsAffected => -1;
  public override int Depth => 0;

  public override bool Read() => ++_rowIndex < rows.Length;
  public override bool NextResult() => false;

  public override int GetOrdinal(string name)
  {
    for (var i = 0; i < columns.Length; i++)
    {
      if (string.Equals(columns[i], name, StringComparison.OrdinalIgnoreCase))
        return i;
    }

    throw new InvalidOperationException($"Column '{name}' not found in fake result set.");
  }

  public override bool IsDBNull(int ordinal) => rows[_rowIndex][ordinal] is null;
  public override object GetValue(int ordinal) => rows[_rowIndex][ordinal] ?? DBNull.Value;
  public override object this[int ordinal] => GetValue(ordinal);
  public override object this[string name] => GetValue(GetOrdinal(name));

  public override string GetName(int ordinal) => columns[ordinal];

  public override Type GetFieldType(int ordinal) =>
    rows.Length > 0 && rows[0][ordinal] is { } v ? v.GetType() : typeof(object);

  public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

  public override bool GetBoolean(int ordinal) => (bool)rows[_rowIndex][ordinal]!;
  public override byte GetByte(int ordinal) => (byte)rows[_rowIndex][ordinal]!;
  public override char GetChar(int ordinal) => (char)rows[_rowIndex][ordinal]!;
  public override DateTime GetDateTime(int ordinal) => (DateTime)rows[_rowIndex][ordinal]!;
  public override decimal GetDecimal(int ordinal) => (decimal)rows[_rowIndex][ordinal]!;
  public override double GetDouble(int ordinal) => (double)rows[_rowIndex][ordinal]!;
  public override float GetFloat(int ordinal) => (float)rows[_rowIndex][ordinal]!;
  public override Guid GetGuid(int ordinal) => (Guid)rows[_rowIndex][ordinal]!;
  public override short GetInt16(int ordinal) => (short)rows[_rowIndex][ordinal]!;
  public override int GetInt32(int ordinal) => (int)rows[_rowIndex][ordinal]!;
  public override long GetInt64(int ordinal) => (long)rows[_rowIndex][ordinal]!;
  public override string GetString(int ordinal) => (string)rows[_rowIndex][ordinal]!;

  public override int GetValues(object[] values)
  {
    var count = Math.Min(values.Length, columns.Length);
    for (var i = 0; i < count; i++)
      values[i] = GetValue(i);
    return count;
  }

  public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
  public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;

  public override IEnumerator GetEnumerator() => new DbEnumerator(this);
}

internal sealed class FakeDbParameterCollection : DbParameterCollection
{
  private readonly List<DbParameter> _params = [];

  public override int Count => _params.Count;
  public override object SyncRoot => _params;

  public override int Add(object? value) { _params.Add((DbParameter)value!); return _params.Count - 1; }
  public override void AddRange(Array values) { foreach (var v in values) Add(v); }
  public override void Clear() => _params.Clear();
  public override bool Contains(object? value) => _params.Contains((DbParameter)value!);
  public override bool Contains(string value) => _params.Any(p => p.ParameterName == value);
  public override void CopyTo(Array array, int index) => ((ICollection)_params).CopyTo(array, index);
  public override IEnumerator GetEnumerator() => _params.GetEnumerator();
  public override int IndexOf(object? value) => _params.IndexOf((DbParameter)value!);
  public override int IndexOf(string parameterName) => _params.FindIndex(p => p.ParameterName == parameterName);
  public override void Insert(int index, object? value) => _params.Insert(index, (DbParameter)value!);
  public override void Remove(object? value) => _params.Remove((DbParameter)value!);
  public override void RemoveAt(int index) => _params.RemoveAt(index);
  public override void RemoveAt(string parameterName) => _params.RemoveAll(p => p.ParameterName == parameterName);

  protected override DbParameter GetParameter(int index) => _params[index];

  protected override DbParameter GetParameter(string parameterName)
    => _params.First(p => p.ParameterName == parameterName);

  protected override void SetParameter(int index, DbParameter value) => _params[index] = value;

  protected override void SetParameter(string parameterName, DbParameter value)
  {
    var i = _params.FindIndex(p => p.ParameterName == parameterName);
    if (i >= 0)
      _params[i] = value;
  }
}

internal sealed class FakeDbParameter : DbParameter
{
  public override DbType DbType { get; set; }
  public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
  public override bool IsNullable { get; set; }

  private string _parameterName = string.Empty;
  public override string ParameterName
  {
    get => _parameterName;
    [param: AllowNull] set => _parameterName = value ?? string.Empty;
  }

  private string _sourceColumn = string.Empty;
  public override string SourceColumn
  {
    get => _sourceColumn;
    [param: AllowNull] set => _sourceColumn = value ?? string.Empty;
  }

  public override DataRowVersion SourceVersion { get; set; }
  public override object? Value { get; set; }
  public override bool SourceColumnNullMapping { get; set; }
  public override int Size { get; set; }
  public override void ResetDbType() => DbType = DbType.Object;
}
