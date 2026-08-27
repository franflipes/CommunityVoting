using Microsoft.UI.Xaml;
using WinRT;

namespace CommunityVoting.Maui.WinUI;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();

        Application.Start((p) =>
        {
            var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);

            new App();
        });
    }
}
