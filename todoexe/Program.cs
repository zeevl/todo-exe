using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ToDoLib;
using todoexe.Extensions;

namespace todoexe
{
    class Program
    {
        private static TaskList _taskList;

        static void Main(string[] args)
        {
            _taskList = LoadTaskList();

            if (args.Length == 0)
            {
                ShowActiveTasksByProject();
                return;
            }
            
            switch(args[0])
            {
                case "add":
                case "a":
                    AddTask(args);
                    break;

                case "do":
                    DoTask(args);
                    break;

                case "pri":
                case "p":
                    ChangePriority(args);
                    break;

                default:
                    Console.WriteLine("Unknown command " + args[0]);
                    break;
            }

            ShowActiveTasksByProject();


        }

        static TaskList LoadTaskList()
        {
            var filename = Environment.GetEnvironmentVariable("TODO_FILE");
            if (filename.IsNullOrEmpty())
                filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "todo.txt");

            return new TaskList(filename);
        }

        static void ShowActiveTasksByProject()
        {
            Console.WriteLine();
            var tasks = _taskList.Tasks.Where(t => !t.Completed);

            foreach(var p in _taskList.Projects)
            {
                // the multiple orderby's keeps the tasks with 
                // priorities at the top..
                var projectTasks = tasks.Where(t => t.Projects.Contains(p))
                                        .OrderBy(t => !t.Priority.HasValue())
                                        .ThenBy(t => t.Priority);
                
                if (!projectTasks.Any())
                    continue;

                WriteHeader(p.TrimStart(new char[] {'+'}));
                WriteTaskList(projectTasks);

                Console.WriteLine();
            }

            var noProjectasks = tasks.Where(t => !t.Projects.Any())
                                     .OrderBy(t => !t.Priority.HasValue())
                                     .ThenBy(t => t.Priority);

            if (noProjectasks.Any())
            {
                WriteHeader("Not in project");
                WriteTaskList(noProjectasks);
            }

            Console.WriteLine();

        }

        static void WriteHeader(string header)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("---  " + header + "  ---");
            Console.ResetColor();
            
        }

        static void WriteTaskList(IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                Console.ForegroundColor = GetConsoleColorForTask(task);
                Console.WriteLine(String.Format("{0:00} {1}", task.Id, task.ToString("{1}{2} {4}")));
                Console.ResetColor();
            }
        }

        static ConsoleColor GetConsoleColorForTask(Task task)
        {
            switch (task.Priority)
            {
                case "(A)":
                    return ConsoleColor.Yellow;

                case "(B)":
                    return ConsoleColor.Green;

                case "(C)":
                    return ConsoleColor.Blue;

                case "(D)":
                    return ConsoleColor.Magenta;
            }
            
            return Console.ForegroundColor;
        }

        static void AddTask(string[] args)
        {
            if (args[1].Length == 1)
            {
                if (IsValidPriority(args[1]))
                    // single char, treat it as a priority
                    args[1] = "(" + args[1].ToUpper() + ")";
                
            }

            _taskList.Add(new Task(_taskList.Tasks.Count, args.JoinRange(1)));
        }

        static void DoTask(string[] args)
        {
            var id = args[1].ToNullableInt();
            if (!id.HasValue || id >= _taskList.Tasks.Count)
                throw new TaskException("Don't know task # " + args[1]);

            _taskList.Tasks[id.Value].Completed = true;
            _taskList.Save();

            Console.WriteLine("Task " + id + " completed.");
            Console.WriteLine();

        }

        static void ChangePriority(string[] args)
        {
            var id = args[1].ToNullableInt();
            if (!id.HasValue || id >= _taskList.Tasks.Count)
                throw new TaskException("Don't know task # " + args[1]);

            if (!IsValidPriority(args[2]))
                throw new TaskException("Don't know priority '" + args[2] + "'");

            _taskList.Tasks[id.Value].Priority = "(" + args[2].ToUpper() + ")";
            _taskList.Save();

        }

        static bool IsValidPriority(string priority)
        {
            return priority.Length == 1 && priority.ToUpper()[0] >= 'A' && priority.ToUpper()[0] <= 'Z';
        }

    }
}
