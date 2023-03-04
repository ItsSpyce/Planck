namespace Planck.Exceptions
{
  public class PlanckWindowException : Exception
  {
    public PlanckWindowException(string message) : base(message) { }

    public PlanckWindowException(string message, Exception innerException) : base(message, innerException) { }
  }
}
