﻿
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Highway.Data
{
	public abstract class SqlCommand : ICommand
	{
		protected Action<SqlConnection> ContextQuery;

		public void Execute(IUnitOfWork context)
		{
			var efContext = context as DbContext;
			using (var conn = new SqlConnection(efContext.Database.Connection.ConnectionString))
			{
				ContextQuery.Invoke(conn);
			}
		}
	}
}