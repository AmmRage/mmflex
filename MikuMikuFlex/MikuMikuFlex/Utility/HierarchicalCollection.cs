using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MMF.Utility
{
    /// <summary>
    /// You can get the element in the hierarchical order collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HierarchicalOrderCollection<T>:List<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseList">Of the element you want to sort an array。baseListはインデックス順に並んでいることを前提とする</param>
        /// <param name="solver">Interface is used to retrieve the parent elements</param>
        public HierarchicalOrderCollection(T[] baseList,HierarchicalOrderSolver<T> solver)
        {
             Queue<int> cachedQueue=new Queue<int>();//to queue the parent order from the baseList
            HashSet<int> cachedSet=new HashSet<int>();//To check the element containing the hash set
            cachedSet.Add(-1);
            while (cachedQueue.Count!=baseList.Length)
            {
                foreach (var element in baseList)
                {
                    int index = solver.getIndex(element);
                    int parent = solver.getParentIndex(element);
                    if (cachedSet.Contains(parent)&&!cachedSet.Contains(index))
                    {//If the parent element is already included
                        
                        cachedQueue.Enqueue(index);
                        cachedSet.Add(index);
                    }
                }
            }
            while (cachedQueue.Count!=0)
            {
                Add(baseList[cachedQueue.Dequeue()]);
            }
        }

    }

    public interface HierarchicalOrderSolver<T>
    {
        /// <summary>
        /// Returns the index of the parent
        /// </summary>
        /// <param name="child">Investigators want to child</param>
        /// <returns>To-1 if no parent</returns>
        int getParentIndex(T child);

        /// <summary>
        /// Returns the index of the specified bone
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        int getIndex(T target);
    }
}
