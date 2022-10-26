using SharpC2.Models;

namespace SharpC2.Factories;

public interface IScreenFactory
{
    T GetScreen<T>() where T : Screen;
}