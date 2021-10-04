using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoolButIsItPrime {
    public class PrimeGenerator {
        private Object lockject = new Object();
        public Task<List<long>> GetPrimesSequentialAsync(long first, long last) {
            return Task.Run(() => GetPrimesSequential(first, last));
        }

        private List<long> GetPrimesSequential(long first, long last) {
            List<long> primes = new List<long>();
            for (long i = first; i <= last; i++) {
                var boundary = (long)Math.Floor(Math.Sqrt(i));
                if (IsPrime(i, boundary)) {
                    primes.Add(i);
                }
            }
            return primes;
        }

        public Task<List<long>> GetPrimesParallelAsync(long first, long last) {
            return Task.Run(() => GetPrimesParallel(first, last));
        }

        private List<long> GetPrimesParallel(long first, long last) {
            List<long> primes = new List<long>();
            // consistently faster than the partitioner version in these ranges,
            // also gets the correct number of primes
            Parallel.For(first, last, (x) => {
                long boundary = (long)Math.Floor(Math.Sqrt(x));
                if (IsPrime(x, boundary)) {
                    lock (lockject) {
                        primes.Add(x);
                    }
                }

            });
            // adds an extra entry for some reason
            //Parallel.ForEach(Partitioner.Create(first, last),
            // (range) => {
            //     for (long i = range.Item1; i <= range.Item2; i++) {
            //         var boundary = (long)Math.Floor(Math.Sqrt(i));
            //         if (IsPrime(i, boundary)) {
            //             lock (lockject) {

            //                 primes.Add(i);
            //             }
            //         }
            //     }
            // });

            return primes;
         
        }

        public static bool IsPrime(long number, long boundary) {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (long i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }
    }
}
