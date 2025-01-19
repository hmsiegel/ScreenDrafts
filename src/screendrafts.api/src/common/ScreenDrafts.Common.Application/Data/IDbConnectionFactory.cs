﻿namespace ScreenDrafts.Common.Application.Data;

public interface IDbConnectionFactory
{
  ValueTask<DbConnection> OpenConnectionAsync();
}
