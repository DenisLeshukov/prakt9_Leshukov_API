using prakt8_leshukov.Models;

namespace prakt8_leshukov.Services
{
    public class DBService
    {
        private LeshukovDbContext context;
        private static DBService? instance;
        public LeshukovDbContext Context => context;
        public static DBService Instance
        {
            get
            {

                if (instance == null)
                    instance = new DBService();
                return instance;
            }
        }
        private DBService()
        {
            context = new LeshukovDbContext( );
        }
    }
}
