﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Highway.Data
{
	/// <summary>
	///     The base implementation for Queries that return collections
	/// </summary>
	/// <typeparam name="T">The Type being requested</typeparam>
	public class Query<T> : QueryBase, IQuery<T>
	{
		/// <summary>
		///     This holds the expression that will be used to create the <see cref="IQueryable{T}" /> when executed on the context
		/// </summary>
		protected Func<IReadOnlyUnitOfWork, IQueryable<T>> ContextQuery { get; set; }

		/// <summary>
		///     This executes the expression in ContextQuery on the context that is passed in, resulting in a
		///     <see cref="IQueryable{T}" /> that is returned as an <see cref="IEnumerable{T}" />
		/// </summary>
		/// <param name="context">the data context that the query should be executed against</param>
		/// <returns>
		///     <see cref="IEnumerable{T}" />
		/// </returns>
		public virtual IEnumerable<T> Execute(IReadOnlyUnitOfWork context)
		{
			IQueryable<T> task = PrepareQuery(context);
			return task;
		}

		/// <summary>
		///     This executes the expression against the passed in context to generate the SQL statement, but doesn't execute the
		///     IQueryable<typeparamref name="T" /> against the data context
		/// </summary>
		/// <param name="context">The data context that the query is evaluated and the SQL is generated against</param>
		/// <returns></returns>
		public virtual string OutputSQLStatement(IReadOnlyUnitOfWork context)
		{
			return OutputQuery(context);
		}

		public virtual string OutputQuery(IReadOnlyUnitOfWork context)
		{
			IQueryable<T> query = PrepareQuery(context);
			return query.ToString();
		}

		/// <summary>
		///     This method allows for the extension of Ordering and Grouping on the prebuild Query
		/// </summary>
		/// <returns>an <see cref="IQueryable{T}" /></returns>
		protected virtual IQueryable<T> ExtendQuery()
		{
			try
			{
				return ContextQuery(UnitOfWork);
			}
			catch (Exception)
			{
				throw; //just here to catch while debugging
			}
		}

		/// <summary>
		///     Gives the ability to apend an <see cref="IQueryable" /> onto the current query
		/// </summary>
		/// <param name="query">The query containing the expressions to append</param>
		/// <returns>The combined query</returns>
		protected virtual IQueryable<T> AppendExpressions(IQueryable<T> query)
		{
			IQueryable<T> source = query;
			foreach (var exp in ExpressionList)
			{
				List<Expression> newParams = exp.Item2.ToList();
				newParams.Insert(0, source.Expression);
				source = source.Provider.CreateQuery<T>(Expression.Call(null, exp.Item1, newParams));
			}
			return source;
		}

		protected virtual IQueryable<T> PrepareQuery(IReadOnlyUnitOfWork context)
		{
			UnitOfWork = context;
			CheckContextAndQuery(ContextQuery);
			IQueryable<T> query = ExtendQuery();
			return AppendExpressions(query);
		}
	}
}