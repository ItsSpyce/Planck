using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Planck
{
#pragma warning disable CS0618 // Type or member is obsolete
    [ClassInterface(ClassInterfaceType.AutoDual)]
#pragma warning restore CS0618 // Type or member is obsolete
    [ComVisible(true)]
    public abstract class HostObject
    {
        //public sealed object CreateBridgeObject()
        //{
        //  var publicFields = GetType().GetProperties(BindingFlags.Public);
        //  var publicMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        //}
    }
}
