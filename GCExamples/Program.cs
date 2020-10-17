using System;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace GCExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            // GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            // DumpGcSettings();
            // HardWorker.CriticalTask();
            // GCSettings.LatencyMode = GCLatencyMode.Interactive;
            
            WeakReferenceDemo();
        }

        private static void DumpGcSettings()
        {
            Console.WriteLine($"Mode :        {(GCSettings.IsServerGC ? "Server" : "Workstation")}");
            Console.WriteLine($"LatencyMode : {(GCSettings.LatencyMode)}");
        }

        public const long Kb = 1000;
        public const long Mb = Kb * 1000;

        private static void NoGcRegion()
        {
            var allocator = new MemoryAllocator();
            Console.WriteLine("Press any key to reserve memory and go into noGCRegion");
            Console.ReadLine();
            
            var couldReserveEnoughMemory = System.GC
                .TryStartNoGCRegion(
                    255 * Mb, // just under ephemiral segment in GC
                    true // Should not do a Full blocking GC if there isn't enough space
                );
            
            if (couldReserveEnoughMemory)
            {
                Console.WriteLine("Press any key to start allocating more memory");
                Console.ReadLine();
                Console.WriteLine("Press any key to stop allocating memory");
                var cancellationSource = new CancellationTokenSource();
                Task.Run(() =>
                {
                    while (!cancellationSource.IsCancellationRequested)
                    {
                        allocator.AddMemory(80*Kb);
                        Thread.Sleep(5);
                    }

                }, cancellationSource.Token);
                Console.ReadLine();
                cancellationSource.Cancel();
                
                
                DumpGcSettings(); // LatencyMode = NoGCRegion
                //always check whether mode has not been set back by induced GC
                if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                {
                    Console.WriteLine("Ending NoGCRegion");
                    System.GC.EndNoGCRegion();
                    DumpGcSettings();
                    Console.WriteLine("Press any key to explicitly run GC");
                    Console.ReadLine();
                    GC.Collect(2);
                }
            }
            else
            {
                Console.WriteLine("Could not allocate enough memory");
            }

            Console.ReadLine();
        }

        public static void WeakReferenceDemo()
        {
            var weakArray = MakeWeakArray();
            PrintHasValue(weakArray);
            Console.ReadLine();
            
            // Console.WriteLine("Make the reference strong again!");
            // Console.ReadLine();
            // var arr = (int[])weakArray.Target;
            
            Console.WriteLine("Run GC");
            Console.ReadLine();
            GC.Collect(2);
            PrintHasValue(weakArray);
        }
        
        //prevent inlining as it would ruin this demo
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void PrintHasValue(WeakReference weakReference)
        {
            Console.WriteLine($"WeakArray has value? -> {weakReference.Target != null}");
        }

        //prevent inlining as it would ruin this demo
        [MethodImpl(MethodImplOptions.NoInlining)] 
        public static WeakReference MakeWeakArray()
        {
            var arr = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            return new WeakReference(arr);
        }
        
    }
}