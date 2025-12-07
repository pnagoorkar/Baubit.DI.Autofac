namespace Baubit.DI.Autofac.Test.ServiceProviderFactory.Setup
{
    /// <summary>
    /// Test service interface.
    /// </summary>
    public interface ITestService
    {
        string Name { get; }
    }

    /// <summary>
    /// Test service implementation.
    /// </summary>
    public class TestService : ITestService
    {
        public string Name { get; }

        public TestService(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Another test service interface for mixed module testing.
    /// </summary>
    public interface IAnotherService
    {
        string GetMessage();
    }

    /// <summary>
    /// Another test service implementation.
    /// </summary>
    public class AnotherService : IAnotherService
    {
        private readonly string _message;

        public AnotherService(string message)
        {
            _message = message;
        }

        public string GetMessage() => _message;
    }
}
