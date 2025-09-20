using UnityEngine;
using AQ.SharedKernel;
using AQ.SharedKernel.Events;
namespace AQ.App.Presentation
{
  public sealed class EventBusInstaller : MonoBehaviour
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install() { GlobalBus.Install(new InMemoryEventBus()); }
  }
}


