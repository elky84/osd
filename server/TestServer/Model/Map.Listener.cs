namespace TestServer.Model
{
    public partial class Map
    {
        public interface Listener
        {
            void OnSectorChanged(Object obj);
        }
    }
}
