using AQ.SharedKernel;
namespace AQ.App.Presentation
{
  public static class GlobalBus
  {
    private static IEventBus _bus = new InMemoryEventBus();
    public static void Install(IEventBus bus) { _bus = bus ?? new InMemoryEventBus(); }
    public static IEventBus Bus => _bus;
  }
}
