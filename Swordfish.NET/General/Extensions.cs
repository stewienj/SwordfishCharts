using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.General
{
  public static class Extensions
  {
    public static IEnumerable<T> GetLatestConsumingEnumerable<T>(this BlockingCollection<T> collection)
    {
      foreach (T pos in collection.GetConsumingEnumerable())
      {
        T currentPos = pos;
        T nextPos;
        while (collection.TryTake(out nextPos))
        {
          currentPos = nextPos;
        }
        yield return currentPos;
      }
    }

  }
}
