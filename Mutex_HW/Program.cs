using System;
using System.Threading;

class WorkerThread
{
    private Thread thread;
    private ManualResetEvent startEvent;
    private Mutex mutex;
    private int id;
    private ThreadManager manager;

    public WorkerThread(int id, ManualResetEvent startEvent, Mutex mutex, ThreadManager manager)
    {
        this.id = id;
        this.startEvent = startEvent;
        this.mutex = mutex;
        this.manager = manager;

        thread = new Thread(DoWork);
        thread.Name = $"Thread {id}";
    }

   
    public void Start()
    {
        thread.Start();
    }

    
    public void Join()
    {
        thread.Join();
    }

   
    private void DoWork()
    {
        Console.WriteLine($"{Thread.CurrentThread.Name} waiting for start...");

       
        startEvent.WaitOne();

       
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} tries to access resource...");

      
            mutex.WaitOne();
            try
            {
                
                Console.WriteLine($"{Thread.CurrentThread.Name} access granted.");
                manager.AccessSharedResource(id);
            }
            finally
            {
               
                mutex.ReleaseMutex();
                Console.WriteLine($"{Thread.CurrentThread.Name} left resource.");
            }

      
            Thread.Sleep(new Random().Next(500, 1000));
        }
    }
}

class ThreadManager
{
    private ManualResetEvent startEvent;
    private Mutex mutex;
    private int sharedResource = 0;
    private WorkerThread[] workers;
    private int threadCount;

    public ThreadManager(int threadCount)
    {
        this.threadCount = threadCount;

       
        startEvent = new ManualResetEvent(false);
        mutex = new Mutex();

      
        workers = new WorkerThread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            workers[i] = new WorkerThread(i + 1, startEvent, mutex, this);
        }
    }

   
    public void StartThreads()
    {
        Console.WriteLine("Main thread: preparing...");
        foreach (var worker in workers)
        {
            worker.Start();
        }

       
        Thread.Sleep(2000);

      
        Console.WriteLine("Main thread: start!");
        startEvent.Set();

 
        foreach (var worker in workers)
        {
            worker.Join();
        }

        Console.WriteLine("Main thread: all threads completed.");
    }


    public void AccessSharedResource(int threadId)
    {
        sharedResource++;
        Console.WriteLine($"Thread {threadId}: sharedResource = {sharedResource}");
        Thread.Sleep(1000); 
    }
}

class Program
{
    static void Main(string[] args)
    {
        ThreadManager manager = new ThreadManager(5);
        manager.StartThreads();
    }
}
