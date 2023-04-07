namespace Planck.Modules
{
  public interface IModuleService
  {
    object GetModule<T>() where T : Module;
    object GetModule(string name);
  }
}
