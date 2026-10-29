namespace Boovatech.Eventing
{
    public class MessageEntryProcessingContext
    {
        public MessageEntryProcessingContext(Func<Task> ack, Func<string, Task> reject)
        {
            ArgumentNullException.ThrowIfNull(ack);
            ArgumentNullException.ThrowIfNull(reject);

            AckAsync = () =>
            {
                HasBeenMarked = true;
                return ack();
            };
            RejectAsync = reason =>
            {
                HasBeenMarked = true;
                return reject(reason);
            };
        }

        public Func<Task> AckAsync { get; }
        public Func<string, Task> RejectAsync { get; }
        public bool HasBeenMarked { get; internal set; }
    }


}
