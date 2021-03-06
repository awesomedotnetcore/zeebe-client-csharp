using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface IUpdatePayloadCommandStep1
    {
        /// <summary>
        /// Set the new payload of the element instance.
        /// </summary>
        /// 
        /// <param name="payload">the payload (JSON) as String</param>
        /// <returns>the builder for this command. Call {@link #Send()} to complete the command and send it
        ///     to the broker.</returns>
        IUpdatePayloadCommandStep2 Payload(string payload);
    }

    public interface IUpdatePayloadCommandStep2 : IFinalCommandStep<IUpdatePayloadResponse>
    {
        // the place for new optional parameters
    }
}