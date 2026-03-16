namespace UniStepRL.Agent
{
    public interface IAgent
    {
        EnvironmentAction Simulate(SimpleEnvironment environment, EnvironmentState rootState);
    }
}
