using System.Collections.Generic;
using System.Linq;

namespace AdvancedEditorTools
{
    public interface IEnumerableMatcheable<T>
    {
        public bool Matches(T obj);
        public bool PartiallyMatches(T obj);
        public T UpdateWith(T obj);
    }

    public static class EnumerableUtils
    {
        public static T[] MatchEnumerables<T>(this IEnumerable<T> newEnumerable, IEnumerable<T> oldEnumerable, bool partialPositionalMatch = false) where T : IEnumerableMatcheable<T>
        {
            int oldLength = oldEnumerable.Count();
            int newLength = newEnumerable.Count();
            T[] result = new T[newLength];

            bool[] usedOldSlots = new bool[oldLength];
            bool[] usedNewSlots = new bool[newLength];

            // Pegar full match en cada posicion final (N^2 search)
            int i = 0;
            int j;
            foreach (var oldItem in oldEnumerable)
            {
                j = 0;
                foreach (var newItem in newEnumerable)
                {
                    if (!usedNewSlots[j] && newItem.Matches(oldItem))
                    {
                        result[j] = newItem.UpdateWith(oldItem);
                        usedOldSlots[i] = true;
                        usedNewSlots[j] = true;
                        break;
                    }
                    j++;
                }
                i++;
            }

            // Rellenar huecos de la posición final
            // Si partial match coincide y no se ha usado ya copiarlo
            // Si se usa positionalMatching solo copiar en caso de que el argumento se encuentre en la misma posición          
            j = 0;
            foreach (var resultItem in result)
            {
                if (usedNewSlots[j])
                {
                    j++;
                    continue;
                }

                var newItem = newEnumerable.ElementAt(j);
                if (partialPositionalMatch)
                {
                    result[j] = usedOldSlots.Length > j && !usedOldSlots[j] && newItem.PartiallyMatches(oldEnumerable.ElementAt(j)) ?
                                    newItem.UpdateWith(oldEnumerable.ElementAt(j)) :
                                    newItem;
                }
                else
                {
                    i = 0;
                    bool itemFound = false;
                    foreach (var oldItem in oldEnumerable)
                    {
                        if (usedOldSlots[i])
                        {
                            i++;
                            continue;
                        }

                        if (newItem.PartiallyMatches(oldItem))
                        {
                            result[j] = newItem.UpdateWith(oldItem);
                            itemFound = true;
                            break;
                        }
                        i++;
                    }
                    if (!itemFound)
                        result[j] = newItem;
                }
                j++;
            }

            return result;
        }

        public static bool FindMatch<T>(this IEnumerable<T> enumerable, ref T itemToMatch) where T : IEnumerableMatcheable<T>
        {
            foreach (var item in enumerable)
            {
                if (item.Matches(itemToMatch))
                {
                    itemToMatch.UpdateWith(item);
                    return true;
                }
            }

            foreach (var item in enumerable)
            {
                if (item.PartiallyMatches(itemToMatch))
                {
                    itemToMatch.UpdateWith(item);
                    return true;
                }
            }

            return false;
        }
    }
}
