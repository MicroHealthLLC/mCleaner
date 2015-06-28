
namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ActiveXComObjects : ScannerBase
    {
        public ActiveXComObjects() { }
        static ActiveXComObjects _i = new ActiveXComObjects();
        public static ActiveXComObjects I { get { return _i; } }

        public void Clean(bool preview)
        {
            if (preview)
            {
                Clean();
            }
            else
            {
                Preview();
            }
        }

        public void Clean()
        {

        }

        public void Preview()
        {
            this.BadKeys.Clear();
        }
    }
}
