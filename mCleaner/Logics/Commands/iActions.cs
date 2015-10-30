using mCleaner.Model;

namespace mCleaner.Logics.Commands
{
    public interface iActions
    {
        action Action { get; set; }
        void Enqueue(bool apply = false);
    }
}
