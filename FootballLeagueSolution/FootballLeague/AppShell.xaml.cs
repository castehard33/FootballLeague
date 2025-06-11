using FootballLeague.Views; 
namespace FootballLeague
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();


            Routing.RegisterRoute(nameof(AddEditPlayerPage), typeof(AddEditPlayerPage));

        }
    }
}