namespace Common.Playable.Input;

public interface IInputGatherer<out T> where T : IInputPackage
{
    public T Gather();
}