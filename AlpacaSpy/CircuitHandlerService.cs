using Microsoft.AspNetCore.Components.Server.Circuits;

namespace AlpacaSpy
{
    public class CircuitHandlerService : CircuitHandler
    {
        private readonly AlpacaSpyLogger logger;
        private readonly Lock _lock = new();
        private readonly List<string> _connections = [];

        public CircuitHandlerService(AlpacaSpyLogger logger)
        {
            this.logger = logger;
        }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogDebug("CircuitHandler", $"Connection {circuit.Id[..12]} is up.  Circuit count: {_connections.Count}");
            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogDebug("CircuitHandler", $"Connection {circuit.Id[..12]} is down.");
            return Task.CompletedTask;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (!_connections.Contains(circuit.Id))
                {
                    _connections.Add(circuit.Id);
                    logger.LogDebug("CircuitHandler", $"Circuit    {circuit.Id[..12]} opened.");
                }
            }
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                _connections.Remove(circuit.Id);
                logger.LogDebug("CircuitHandler", $"Circuit    {circuit.Id[..12]} closed. Circuit count: {_connections.Count}");
            }
            return Task.CompletedTask;
        }
    }
}
