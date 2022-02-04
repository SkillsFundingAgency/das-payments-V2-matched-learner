using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityAuthenticationDbConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly ISqlAzureIdentityTokenProvider _tokenProvider;
        private static bool _connectionNeedsAccessToken = true;

        public SqlAzureIdentityAuthenticationDbConnectionInterceptor(ISqlAzureIdentityTokenProvider tokenProvider, bool connectionNeedsAccessToken)
        {
            _tokenProvider = tokenProvider;
            _connectionNeedsAccessToken = connectionNeedsAccessToken;
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            var sqlConnection = (SqlConnection)connection;
            if (_connectionNeedsAccessToken)
            {
                var token = _tokenProvider.GetAccessToken();
                sqlConnection.AccessToken = token;
            }

            return base.ConnectionOpening(connection, eventData, result);
        }

        public override async Task<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            var sqlConnection = (SqlConnection)connection;
            if (_connectionNeedsAccessToken)
            {
                var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
                sqlConnection.AccessToken = token;
            }

            return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
        }

        //private static bool ConnectionNeedsAccessToken(SqlConnection connection)
        //{
        //    //
        //    // Only try to get a token from AAD if
        //    //  - We connect to an Azure SQL instance; and
        //    //  - The connection doesn't specify a username.
        //    //
        //    //var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
        //    //
        //    //return connectionStringBuilder.DataSource.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(connectionStringBuilder.UserID);
        //}
    }
}