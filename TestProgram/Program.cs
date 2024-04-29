using ClassLibrary;
using ErrorEventArgs = ClassLibrary.ErrorEventArgs;

namespace TestProgram
{
    class Program
    {
        private static double sharedProgress = 0;
        private static readonly object progressLock = new object();
        private static double[,] matrixA;
        private static double[,] matrixB;
        private static int _size = 500;
        private static double currprog = 0;
        
        public static void Main()
        {
            Console.WriteLine(Environment.ProcessorCount);
            Console.WriteLine("\n\n\n");
            matrixA = _generateRandomMatrix(_size, _size);
            matrixB = _generateRandomMatrix(_size, _size);

            var threadCount = Environment.ProcessorCount;

            for (int i = 0; i < threadCount; i++)
            {
                var bgWorker = new BgWorker();
                
                Console.WriteLine($"Start: {(i == 0 ? 0 : _size / threadCount * i)}, end: {(i < threadCount - 1 ? _size / threadCount * (i + 1) - 1 : _size)}");
                
                InitializeBgWorker(bgWorker);
                bgWorker.RunWorkerAsync(new WorkArgs {
                    MatrixA = matrixA, 
                    MatrixB = matrixB, 
                    StartRow = i == 0 ? 0 : _size / threadCount * i,
                    EndRow = i < threadCount - 1 ? _size / threadCount * (i + 1) - 1 : _size
                });
            }
            
            Console.WriteLine("\n\n\n");
        }
        
        
        private static void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BgWorker worker = sender as BgWorker;
            WorkArgs args = e.Argument as WorkArgs;
            e.Result = _MultiplyMatrix(worker, e, args.MatrixA, args.MatrixB, args.StartRow, args.EndRow);
        }

        private static void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Console.WriteLine("Canceled");
            }
        }
        
        private static void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            lock (progressLock)
            {
                sharedProgress += e.ProgressPercentage;
                sharedProgress /= 2;
                if (sharedProgress > currprog)
                {
                    currprog = Math.Ceiling(sharedProgress);
                    Console.Write("\r" + currprog + "%");
                }
                else
                {
                    Console.Write("\r" + currprog + "%");
                }
                
            }
        }
        

        private static void InitializeBgWorker(BgWorker bgwrk)
        {
            bgwrk.WorkerReportsProgress = true;
            bgwrk.WorkerSupportsCancellation = true;
            bgwrk.DoWorkEvent += BackgroundWorker_DoWork;
            bgwrk.ProgressChangedEvent += BackgroundWorker_ProgressChanged;
            bgwrk.RunWorkerCompletedEvent += BackgroundWorker_RunWorkerCompleted;
            bgwrk.OnError += BgWorker_OnError;
        }
        
        private static void BgWorker_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("error occured: " + e.Error.Message);
        }
        
        private static double[,] _MultiplyMatrix(BgWorker worker, DoWorkEventArgs e, double[,] matrixA, double[,] matrixB, int startRow, int endRow)
        {
            int rA = matrixA.GetLength(0);
            int cA = matrixA.GetLength(1);
            int rB = matrixB.GetLength(0);
            int cB = matrixB.GetLength(1);
            double temp = 0;
            double[,] res = new double[rA, cB];

            for (int i = startRow; i < endRow; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    if (worker.CancellationPending)
                    {   
                        e.Cancel = true;
                    }
                    else
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += matrixA[i, k] * matrixB[k, j];
                        }

                        res[i, j] = temp;
                    }
                }
                double progressPercentage = ((double)(i + 1 - startRow) / (endRow-startRow) * 100);
                worker.ReportProgress(progressPercentage);
            }

            return res;

        }
        
        private static double[,] _generateRandomMatrix(int rows, int cols)
        {
            var rand = new Random();
            var matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rand.NextDouble();
                }
            }
            return matrix;
        }
    }
    public class WorkArgs
    {
        public double[,] MatrixA { get; set; }
        public double[,] MatrixB { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
    }
}

