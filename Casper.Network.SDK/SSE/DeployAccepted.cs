using System.Collections.Generic;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A new <see cref="Deploy">Deploy</see> received for processing in the network.
    /// </summary>
    public class DeployAccepted : Deploy
    {
        //
        // This is an alias of Deploy to be used in SSE.
        //
        public DeployAccepted(string hash, DeployHeader header, ExecutableDeployItem payment,
            ExecutableDeployItem session, List<Approval> approvals)
            : base(hash, header, payment, session, approvals)
        {
        }
    }
}