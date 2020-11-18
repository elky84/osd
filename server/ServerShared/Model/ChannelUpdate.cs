using System.Threading.Tasks;

namespace ServerShared.Model
{
    public partial class Channel
    {
        public void UpdateRegen()
        {
            Parallel.ForEach(Sessions, pair =>
            {
                var session = pair.Value;
            });
        }
    }
}