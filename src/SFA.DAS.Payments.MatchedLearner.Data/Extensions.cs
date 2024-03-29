﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

// ReSharper disable RedundantNameQualifier

namespace SFA.DAS.Payments.MatchedLearner.Data
{
    public static class Extensions
    {
        public static bool IsUniqueKeyConstraintException(this Exception exception)
        {
            var sqlException = exception.GetException<Microsoft.Data.SqlClient.SqlException>();
            if (sqlException != null)
                return sqlException.Number == 2601 || sqlException.Number == 2627;
            var sqlEx = exception.GetException<SqlException>();
            return sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

        public static T GetException<T>(this Exception e) where T : Exception
        {
            var innerEx = e;
            while (innerEx != null && !(innerEx is T))
            {
                innerEx = innerEx.InnerException;
            }

            return innerEx as T;
        }

        public static bool IsDeadLockException(this Exception exception)
        {
            var sqlException = exception.GetException<Microsoft.Data.SqlClient.SqlException>();
            if (sqlException != null)
                return sqlException.Number == 1205;
            var sqlEx = exception.GetException<SqlException>();
            return sqlEx != null && sqlEx.Number == 1205;
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
        {
            return items
                .Select((item, inx) => new { item, inx })
                .GroupBy(x => x.inx / maxItems)
                .Select(g => g.Select(x => x.item));
        }
    }
}
