namespace IdlePancake.Features.CoreLoop.Application
{
    public interface IGameTickUseCase
    {
        void Execute(float deltaSeconds);
    }
}
