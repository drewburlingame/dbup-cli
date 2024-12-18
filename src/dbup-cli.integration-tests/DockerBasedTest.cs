using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbUp.Cli.IntegrationTests
{
    public class DockerBasedTest : IAsyncDisposable
    {
        internal const string HostIp = DockerContainer.HostIp;

        private DockerContainer container = new();

        protected Task DockerInitialize(
            string imageName, 
            List<string> environmentVariables, 
            string port, 
            Func<DbConnection> createConnection) => 
            DockerInitialize(imageName, environmentVariables, [], port, createConnection);

        protected async Task DockerInitialize(
            string imageName, 
            List<string> environmentVariables, 
            List<string> cmd, 
            string port, 
            Func<DbConnection> createConnection) => 
            await container.Initialize(imageName, environmentVariables, cmd, port, createConnection);

        public async ValueTask DisposeAsync() => await container.DisposeAsync();
    }
}
