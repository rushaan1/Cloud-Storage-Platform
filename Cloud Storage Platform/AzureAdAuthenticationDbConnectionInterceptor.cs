using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Cloud_Storage_Platform
{
    public class AzureAdAuthenticationDbConnectionInterceptor : IDbConnectionInterceptor
    {
        private readonly TokenCredential _credential;
        private readonly TokenRequestContext _tokenRequestContext = new(new[] { "https://database.windows.net/.default" });

        public AzureAdAuthenticationDbConnectionInterceptor(TokenCredential credential)
        {
            _credential = credential;
        }

        // sync open
        public InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            if (connection is SqlConnection sqlConnection)
            {
                var token = _credential.GetToken(_tokenRequestContext, default);
                sqlConnection.AccessToken = token.Token;
            }

            return result;
        }

        // async open
        public async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (connection is SqlConnection sqlConnection)
            {
                var token = await _credential.GetTokenAsync(_tokenRequestContext, cancellationToken).ConfigureAwait(false);
                sqlConnection.AccessToken = token.Token;
            }

            return result;
        }

        // Required interface methods - implement with empty bodies for EF Core 6.0
        public InterceptionResult ConnectionClosing(DbConnection connection, ConnectionEventData eventData, InterceptionResult result) => result;
        public void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData){}
        public Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)=>Task.CompletedTask;
        public ValueTask<InterceptionResult> ConnectionClosingAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)=>ValueTask.FromResult(result);
        public void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData){}
        public Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)=> Task.CompletedTask;
        public void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData){    }
        public Task ConnectionFailedAsync(DbConnection connection, ConnectionErrorEventData eventData, CancellationToken cancellationToken = default)=>Task.CompletedTask;
    }
}
