namespace NetworkShared.Protocols.Request
{
    public class Enter : Header
    {
        public override Id.Request Id => Protocols.Id.Request.Enter;

        public string UserName { get; set; }
    }
}
