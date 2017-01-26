using System.Windows.Input;

namespace MegaApp.Interfaces
{
    public interface ISetting<T>: ISetting
    {
        new T Value { get; set; }
    }

    public interface ISetting
    {
        string Title { get; set; }

        string Description { get; set; }

        string Key { get; set; }

        object Value { get; set; }

        ICommand ActionCommand { get; }

        void Initialize();
    }
}