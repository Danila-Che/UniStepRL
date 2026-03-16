using System;

namespace UniStepRL.Agent
{
    public class PriorityQueue<T>
    {
        private readonly int m_K;
        private readonly float[] m_Probabilities;
        private readonly T[] m_Items;
        private int m_Count;

        public PriorityQueue(int k)
        {
            m_K = k;
            m_Probabilities = new float[m_K];
            m_Items = new T[m_K];
            m_Count = 0;
        }

        public void Clear()
        {
            m_Count = 0;
        }

        public T[] CopyItems()
        {
            var result = new T[m_Count];
            Array.Copy(m_Items, result, m_Count);

            return result;
        }

        public void Enqueue(float probability, T item)
        {
            // If the heap is not yet full, simply insert the new element.
            if (m_Count < m_K)
            {
                m_Probabilities[m_Count] = probability;
                m_Items[m_Count] = item;
                HeapifyUp(m_Count);
                m_Count++;
            }
            // Otherwise, if the new probability is greater than the current minimum (root),
            // replace the root and restore the heap property.
            else if (probability > m_Probabilities[0])
            {
                m_Probabilities[0] = probability;
                m_Items[0] = item;
                HeapifyDown(0);
            }
            // Else: probability is not high enough – ignore it.
        }

        // Moves the element at index i up until the min‑heap property is restored.
        private void HeapifyUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (m_Probabilities[i] < m_Probabilities[parent])
                {
                    Swap(i, parent);
                    i = parent;
                }
                else
                {
                    break;
                }
            }
        }

        // Moves the element at index i down until the min‑heap property is restored.
        private void HeapifyDown(int i)
        {
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < m_Count && m_Probabilities[left] < m_Probabilities[smallest])
                    smallest = left;
                if (right < m_Count && m_Probabilities[right] < m_Probabilities[smallest])
                    smallest = right;

                if (smallest != i)
                {
                    Swap(i, smallest);
                    i = smallest;
                }
                else
                {
                    break;
                }
            }
        }

        // Swaps the elements (both probability and item) at positions i and j.
        private void Swap(int i, int j)
        {
            float tempProb = m_Probabilities[i];
            m_Probabilities[i] = m_Probabilities[j];
            m_Probabilities[j] = tempProb;

            T tempItem = m_Items[i];
            m_Items[i] = m_Items[j];
            m_Items[j] = tempItem;
        }
    }
}
