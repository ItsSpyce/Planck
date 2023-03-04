namespace Planck.Commands.Internal
{
  [Command("navigate")]
  internal struct NavigateCommand
  {
    public string To { get; set; }
  }
}
