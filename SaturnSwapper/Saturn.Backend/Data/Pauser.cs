using System.Threading.Tasks;

namespace Saturn.Backend.Data;

public class Pauser
{
    public bool IsPaused;

    public void Pause()
    {
        IsPaused = true;
    }

    public void Unpause()
    {
        IsPaused = false;
    }

    public async Task WaitIfPaused()
    {
        while (IsPaused)
        {
            await Task.Delay(1);
        }
    }
}